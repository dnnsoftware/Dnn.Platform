using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace ClientDependency.Core.CompositeFiles
{
    public class CompositeDependencyHandler : IHttpHandler
    {


        private readonly static object Lock = new object();

        //*** DNN related change *** begin
        private static DnnConfiguration dnnConfig = new DnnConfiguration();
        //*** DNN related change *** end

        /// <summary>
        /// When building composite includes, it creates a Base64 encoded string of all of the combined dependency file paths
        /// for a given composite group. If this group contains too many files, then the file path with the query string will be very long.
        /// This is the maximum allowed number of characters that there is allowed, otherwise an exception is thrown.
        /// </summary>
        /// <remarks>
        /// If this handler path needs to change, it can be changed by setting it in the global.asax on application start
        /// </remarks>
        public static int MaxHandlerUrlLength = 2048;

        /// <summary>
        /// Re-usable is true, this is a thread safe class
        /// </summary>
        bool IHttpHandler.IsReusable
        {
            get { return true; }
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            var contextBase = new HttpContextWrapper(context);

            ClientDependencyType type;
            string fileKey;
            int version = 0;

            if (string.IsNullOrEmpty(context.Request.PathInfo))
            {
                var decodedUrl = HttpUtility.HtmlDecode(context.Request.Url.OriginalString);
                var query = decodedUrl.Split(new char[] { '?' });
                if (query.Length < 2)
                {
                    throw new ArgumentException("No query string found in request");
                }
                var queryStrings = HttpUtility.ParseQueryString(query[1]);

                // querystring format
                fileKey = queryStrings["s"];
                var clientDepdendencyVersion = queryStrings["cdv"].TrimEnd('/');
                if (!string.IsNullOrEmpty(clientDepdendencyVersion) && !Int32.TryParse(clientDepdendencyVersion, out version))
                    throw new ArgumentException("Could not parse the version in the request");
                try
                {
                    type = (ClientDependencyType)Enum.Parse(typeof(ClientDependencyType), queryStrings["t"], true);
                }
                catch
                {
                    throw new ArgumentException("Could not parse the type set in the request");
                }
            }
            else
            {

                //get path to parse
                var path = context.Request.PathInfo.TrimStart('/');
                var pathFormat = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.PathBasedUrlFormat;
                //parse using the parser
                if (!PathBasedUrlFormatter.Parse(pathFormat, path, out fileKey, out type, out version))
                {
                    //*** DNN related change *** begin
                    if (context.IsDebuggingEnabled || dnnConfig.IsDebugMode())
                    {
                        throw new FormatException("Could not parse the URL path: " + path + " with the format specified: " + pathFormat);
                    }

                    throw new HttpException(404, "Path not found");
                    //*** DNN related change *** end
                }
            }

            fileKey = context.Server.UrlDecode(fileKey);

            if (string.IsNullOrEmpty(fileKey))
                throw new ArgumentException("Must specify a fileset in the request");

            // don't process if the version doesn't match - this would be nice to do but people will get errors if 
            // their html pages are cached and are referencing an old version
            //if (version != ClientDependencySettings.Instance.Version)
            //    throw new ArgumentException("Configured version does not match request");

            byte[] outputBytes = null;

            //create the webforms page to perform the server side output cache, ensure
            // the parameters are the same that we are going to use when setting our own custom
            // caching parameters. Unfortunately server side output cache is tied so directly to 
            // webforms this seems to be the only way to to this.
            var page = new OutputCachedPage(new OutputCacheParameters
            {
                Duration = Convert.ToInt32(TimeSpan.FromDays(10).TotalSeconds),
                Enabled = true,
                VaryByParam = "t;s;cdv",
                VaryByContentEncoding = "gzip;deflate",
                VaryByHeader = "Accept-Encoding",
                Location = OutputCacheLocation.Any
            });

            //retry up to 5 times... this is only here due to a bug found in another website that was returning a blank 
            //result. To date, it can't be replicated in VS, but we'll leave it here for error handling support... can't hurt
            for (int i = 0; i < 5; i++)
            {
                outputBytes = ProcessRequestInternal(contextBase, fileKey, type, version, outputBytes, page);
                if (outputBytes != null && outputBytes.Length > 0)
                    break;

                ClientDependencySettings.Instance.Logger.Error(string.Format("No bytes were returned, this is attempt {0}. Fileset: {1}, Type: {2}, Version: {3}", i, fileKey, type, version), null);
            }

            if (outputBytes == null || outputBytes.Length == 0)
            {
                ClientDependencySettings.Instance.Logger.Fatal(string.Format("No bytes were returned after 5 attempts. Fileset: {0}, Type: {1}, Version: {2}", fileKey, type, version), null);
                List<CompositeFileDefinition> fDefs;
                outputBytes = GetCombinedFiles(contextBase, fileKey, type, out fDefs);
            }

            context.Response.ContentType = type == ClientDependencyType.Javascript ? "application/x-javascript" : "text/css";
            context.Response.OutputStream.Write(outputBytes, 0, outputBytes.Length);

            //dispose the webforms page used to do ensure server side output cache
            page.Dispose();
        }

        internal byte[] ProcessRequestInternal(HttpContextBase context, string fileset, ClientDependencyType type, int version, byte[] outputBytes, OutputCachedPage page)
        {
            //get the compression type supported
            var clientCompression = context.GetClientCompression();

            var x1 = ClientDependencySettings.Instance;
            if (x1 == null) throw new Exception("x1");
            var x2 = x1.DefaultFileMapProvider;
            if (x2 == null) throw new Exception("x2");

            //get the map to the composite file for this file set, if it exists.
            var map = ClientDependencySettings.Instance.DefaultFileMapProvider.GetCompositeFile(fileset, version, clientCompression.ToString());

            string compositeFileName = "";
            if (map != null && map.HasFileBytes)
            {
                ProcessFromFile(context, map, out compositeFileName, out outputBytes);
            }
            else
            {
                lock (Lock)
                {
                    //check again...
                    if (map != null && map.HasFileBytes)
                    {
                        //there's files there now, so process them
                        ProcessFromFile(context, map, out compositeFileName, out outputBytes);
                    }
                    else
                    {
                        List<CompositeFileDefinition> fileDefinitions;
                        byte[] fileBytes;

                        if (ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.UrlType == CompositeUrlType.MappedId)
                        {
                            //need to try to find the map by it's id/version (not compression)
                            var filePaths = ClientDependencySettings.Instance.DefaultFileMapProvider.GetDependentFiles(fileset, version);

                            if (filePaths == null)
                            {
                                //*** DNN related change *** begin
                                if (context.IsDebuggingEnabled || dnnConfig.IsDebugMode())
                                {
                                    throw new KeyNotFoundException("no map was found for the dependency key: " + fileset +
                                                               " ,CompositeUrlType.MappedId requires that a map is found");
                                }

                                throw new HttpException(404, "Path not found");
                                //*** DNN related change *** end
                            }

                            var filePathArray = filePaths.ToArray();
                            // sanity check/fix for file type
                            type = ValidateTypeFromFileNames(context, type, filePathArray);
                            //combine files and get the definition types of them (internal vs external resources)
                            fileBytes = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider
                                .CombineFiles(filePathArray, context, type, out fileDefinitions);
                        }
                        else
                        {
                            //need to do the combining, etc... and save the file map                            
                            fileBytes = GetCombinedFiles(context, fileset, type, out fileDefinitions);
                        }

                        //compress data                        
                        outputBytes = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.CompressBytes(clientCompression, fileBytes);
                        context.AddCompressionResponseHeader(clientCompression);

                        //save combined file
                        var compositeFile = ClientDependencySettings.Instance
                            .DefaultCompositeFileProcessingProvider
                            .SaveCompositeFile(outputBytes, type, context.Server);

                        if (compositeFile != null)
                        {
                            compositeFileName = compositeFile.FullName;
                            if (!string.IsNullOrEmpty(compositeFileName))
                            {
                                //Update the XML file map
                                ClientDependencySettings.Instance.DefaultFileMapProvider.CreateUpdateMap(fileset, clientCompression.ToString(),
                                    fileDefinitions.Select(x => new BasicFile(type) { FilePath = x.Uri }),
                                        compositeFileName,
                                        //TODO: We should probably use the passed in version param?
                                        ClientDependencySettings.Instance.Version);
                            }
                        }
                    }
                }
            }

            //set our caching params 
            SetCaching(context, compositeFileName, fileset, clientCompression, page);

            return outputBytes;
        }

        /// <summary>
        /// Validate ClientDependencyType and return one which is derived from file extension of first file in bundle. We currently see bug in production where
        /// js bundles are saved as cdC files and are improperly CSS-minifed due to this. Without this validation one can force wrong {type} via http request to /DependencyHandler.axd/{id}/{version}/{type}
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        private ClientDependencyType ValidateTypeFromFileNames(HttpContextBase context, ClientDependencyType type, string[] filePaths)
        {
            if (filePaths.Length > 0)
            {
                // sanity check of file types, we see a bug when javascript bundles are saved as cdC bundles and are minified as CSS which breaks them completely
                if (filePaths[0].EndsWith(".js", StringComparison.OrdinalIgnoreCase) && type == ClientDependencyType.Css)
                {
                    type = ClientDependencyType.Javascript;
                    ClientDependencySettings.Instance.Logger.Error(string.Format("Mismatched ClientDependencyType, attempt to treat .js files bundle as css, forcing js type, Url={0}, UserAgent={1}", context.Request.Url, context.Request.UserAgent), null);
                }
                if (filePaths[0].EndsWith(".css", StringComparison.OrdinalIgnoreCase) && type == ClientDependencyType.Javascript)
                {
                    type = ClientDependencyType.Css;
                    ClientDependencySettings.Instance.Logger.Error(string.Format("Mismatched ClientDependencyType, attempt to treat .css files bundle as js, forcing css type, Url={0}, UserAgent={1}", context.Request.Url, context.Request.UserAgent), null);
                }
            }
            return type;
        }

        private byte[] GetCombinedFiles(HttpContextBase context, string fileset, ClientDependencyType type, out List<CompositeFileDefinition> fDefs)
        {
            //get the file list
            string[] filePaths = fileset.DecodeFrom64Url().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            // sanity check/fix for file type
            type = ValidateTypeFromFileNames(context, type, filePaths);
            //combine files and get the definition types of them (internal vs external resources)
            return ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.CombineFiles(filePaths, context, type, out fDefs);
        }

        private void ProcessFromFile(HttpContextBase context, CompositeFileMap map, out string compositeFileName, out byte[] outputBytes)
        {
            //the saved file's bytes are already compressed.
            outputBytes = map.GetCompositeFileBytes();
            compositeFileName = map.CompositeFileName;
            var cType = (CompressionType)Enum.Parse(typeof(CompressionType), map.CompressionType);
            context.AddCompressionResponseHeader(cType);
        }

        /// <summary>
        /// Sets the output cache parameters and also the client side caching parameters
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileName">The name of the file that has been saved to disk</param>
        /// <param name="fileset">The Base64 encoded string supplied in the query string for the handler</param>
        /// <param name="compressionType"></param>
        /// <param name="page">The outputcache page - ensures server side output cache is stored</param>
        private void SetCaching(HttpContextBase context, string fileName, string fileset, CompressionType compressionType, OutputCachedPage page)
        {
            //this initializes the webforms page part to get outputcaching working server side
            page.ProcessRequest(HttpContext.Current);

            // in any case, cache already varies by pathInfo (build-in) so for path formats, we do not need anything
            // just add params for querystring format, just in case...
            context.SetClientCachingResponse(
                //the e-tag to use
                (fileset + compressionType.ToString()).GenerateHash(),
                //10 days
                10,
                //vary-by params
                new[] { "t", "s", "cdv" });

            //make this output cache dependent on the file if there is one.
            if (!string.IsNullOrEmpty(fileName))
                context.Response.AddFileDependency(fileName);
        }

        internal sealed class OutputCachedPage : Page
        {
            private readonly OutputCacheParameters _cacheSettings;

            public OutputCachedPage(OutputCacheParameters cacheSettings)
            {
                // Tracing requires Page IDs to be unique.
                ID = Guid.NewGuid().ToString();
                _cacheSettings = cacheSettings;
            }

            protected override void FrameworkInitialize()
            {
                // when you put the <%@ OutputCache %> directive on a page, the generated code calls InitOutputCache() from here
                base.FrameworkInitialize();
                InitOutputCache(_cacheSettings);
            }
        }

    }
}

