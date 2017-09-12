using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ClientDependency.Core.Config;
using System.IO;
using System.Web;
using System.Net;
using System.Configuration.Provider;
using System.Web.Hosting;

namespace ClientDependency.Core.CompositeFiles.Providers
{
    public abstract class BaseCompositeFileProcessingProvider : ProviderBase, IHttpProvider
    {

        private const string DefaultDependencyPath = "~/App_Data/ClientDependency";

        /// <summary>
        /// Defines the UrlType default value, this can be set at startup
        /// </summary>
        public static CompositeUrlType UrlTypeDefault = CompositeUrlType.MappedId;

        /// <summary>
        /// The path specified in the config
        /// </summary>
        internal string CompositeFilePathAsString;

        /// <summary>
        /// constructor sets defaults
        /// </summary>
        protected BaseCompositeFileProcessingProvider()
        {
            PersistCompositeFiles = true;
            EnableCssMinify = true;
            EnableJsMinify = true;
            UrlType = UrlTypeDefault;
            PathBasedUrlFormat = "{dependencyId}/{version}/{type}";
            CompositeFilePathAsString = DefaultDependencyPath;
            BundleDomains = new List<string>();
        }

        #region Public Properties

        /// <summary>
        /// Flags whether or not to enable composite file script creation/persistence.
        /// Composite file persistence will increase performance in the case of cache turnover or application
        /// startup since the files are already combined and compressed.
        /// This also allows for the ability to easily clear the cache so the files are refreshed.
        /// </summary>
        public bool PersistCompositeFiles { get; set; }
        public bool EnableCssMinify { get; set; }
        public bool EnableJsMinify { get; set; }

        /// <summary>
        /// The Url type to use for the dependency handler 
        /// </summary>
        public CompositeUrlType UrlType { get; protected set; }

        /// <summary>
        /// The format of a path based URL (either a MappedId or a Base64Paths URL). The string format is a tokenized string such as:
        /// {dependencyId}.{version}.{type} 
        /// or
        /// {dependencyId}/{version}/{type}
        /// </summary>
        /// <remarks>
        /// By defaut this is just:
        /// {dependencyId}.{version}.{type} 
        /// but Cassini doesn't support '.' in the URL so some people may want to change this to use '/'
        /// </remarks>
        public string PathBasedUrlFormat { get; protected set; }

        /// <summary>
        /// Returns the CompositeFilePath
        /// </summary>
        /// <returns></returns>
        public DirectoryInfo CompositeFilePath { get; protected set; }

        /// <summary>
        /// Returns the set of white listed domains
        /// </summary>
        public IList<string> BundleDomains { get; protected set; }

        #endregion

        #region IHttpProvider Members

        public void Initialize(HttpContextBase http)
        {
            CompositeFilePath = new DirectoryInfo(http.Server.MapPath(CompositeFilePathAsString));
        }

        #endregion

        public abstract FileInfo SaveCompositeFile(byte[] fileContents, ClientDependencyType type, HttpServerUtilityBase server);
        public abstract byte[] CombineFiles(string[] filePaths, HttpContextBase context, ClientDependencyType type, out List<CompositeFileDefinition> fileDefs);
        public abstract byte[] CompressBytes(CompressionType type, byte[] fileBytes);

        protected bool CanProcessLocally(HttpContextBase context, string filePath, out IVirtualFileWriter virtualFileWriter)
        {
            //First check if there's any virtual file providers that can handle this
            var writer = FileWriters.GetVirtualWriterForFile(filePath);
            if (writer == null)
            {
                var ext = Path.GetExtension(filePath);
                writer = FileWriters.GetVirtualWriterForExtension(ext);                
            }
            if (writer != null)
            {
                virtualFileWriter = writer;
                return writer.FileProvider.FileExists(filePath);
            }

            //can process if it exists locally
            virtualFileWriter = null;
            return File.Exists(context.Server.MapPath(filePath));
        }

        protected virtual VirtualFile GetVirtualFile(HttpContextBase context, string filePath)
        {
            var vf = HostingEnvironment.VirtualPathProvider.GetFile(filePath);
            if (vf == null) return null;
            if (vf.IsDirectory) return null;
            return vf;
        }

        /// <summary>
        /// Writes a given path to the stream
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path">The path could be a local url or an absolute url</param>
        /// <param name="context"></param>
        /// <param name="sw"></param>
        /// <returns>If successful returns a CompositeFileDefinition, otherwise returns null</returns>
        public CompositeFileDefinition WritePathToStream(ClientDependencyType type, string path, HttpContextBase context, StreamWriter sw)
        {
            CompositeFileDefinition def = null;
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    //var fi = new FileInfo(context.Server.MapPath(path));

                    var extension = Path.GetExtension(path);

                    //all config based extensions and all extensions registered by file writers
                    var fileBasedExtensions = ClientDependencySettings.Instance.FileBasedDependencyExtensionList
                                                                      .Union(FileWriters.GetRegisteredExtensions());

                    if (fileBasedExtensions.Contains(extension.ToUpper()))
                    {
                        IVirtualFileWriter virtualWriter;
                        if (CanProcessLocally(context, path, out virtualWriter))
                        {
                            //internal request
                            if (virtualWriter != null)
                            {
                                var vf = virtualWriter.FileProvider.GetFile(path);
                                WriteVirtualFileToStream(sw, vf, virtualWriter, type, context);
                            }
                            else
                            {
                                var fi = new FileInfo(context.Server.MapPath(path));
                                WriteFileToStream(sw, fi, type, path, context);
                            }                            
                        }
                        else
                        {
                            //external request
                            def = WriteFileToStream(sw, path, type, context);
                        }
                    }
                    else
                    {
                        //if it's not a file based dependency, try to get the request output.
                        def = WriteFileToStream(sw, path, type, context);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is NotSupportedException
                        || ex is ArgumentException
                        || ex is HttpException)
                    {
                        //could not parse the string into a fileinfo or couldn't mappath, so we assume it is a URI

                        //before we try to load it by URI, we want to check if the URI is a local request, we'll try to detect if it is and
                        // then try to load it from the file system, if the file isn't there then we'll continue trying to load it via the URI.
                        Uri uri;
                        if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out uri) && uri.IsLocalUri(context))
                        {
                            var localPath = uri.PathAndQuery;
                            var fi = new FileInfo(context.Server.MapPath(localPath));
                            if (fi.Exists)
                            {
                                try
                                {
                                    WriteFileToStream(sw, fi, type, path, context); //internal request
                                }
                                catch (Exception ex1)
                                {
                                    ClientDependencySettings.Instance.Logger.Error($"Could not load file contents from {path}. EXCEPTION: {ex1.Message}", ex1);
                                }
                            }
                        }

                        def = WriteFileToStream(sw, path, type, context);
                    }
                    else
                    {
                        //if this fails, log the exception, but continue
                        ClientDependencySettings.Instance.Logger.Error($"Could not load file contents from {path}. EXCEPTION: {ex.Message}", ex);
                    }
                }
            }

            if (type == ClientDependencyType.Javascript)
            {
                sw.Write(";;;"); //write semicolons in case the js isn't formatted correctly. This also helps for debugging.
            }

            return def;
        }

        /// <summary>
        /// Writes the output of an external request to the stream
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="url"></param>
        /// <param name="type"></param>
        /// <param name="http"></param>
        protected virtual CompositeFileDefinition WriteFileToStream(StreamWriter sw, string url, ClientDependencyType type, HttpContextBase http)
        {
            string requestOutput;
            Uri resultUri;
            var rVal = RequestHelper.TryReadUri(url, http, BundleDomains, out requestOutput, out resultUri);
            if (!rVal) return null;

            //write the contents of the external request.
            DefaultFileWriter.WriteContentToStream(this, sw, requestOutput, type, http, url);
            return new CompositeFileDefinition(url, false);
        }

        /// <summary>
        /// Writes the output of a local file to the stream
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="fi"></param>
        /// <param name="type"></param>
        /// <param name="origUrl"></param>
        /// <param name="http"></param>
        protected virtual CompositeFileDefinition WriteFileToStream(StreamWriter sw, FileInfo fi, ClientDependencyType type, string origUrl, HttpContextBase http)
        {
            //get a writer for the file, first check if there's a specific file writer
            //then check for an extension writer.
            var writer = FileWriters.GetWriterForFile(origUrl);
            if (writer is DefaultFileWriter)
            {
                writer = FileWriters.GetWriterForExtension(fi.Extension);
                if (writer == null) return null;
            }
            return writer.WriteToStream(this, sw, fi, type, origUrl, http)
                ? new CompositeFileDefinition(origUrl, true)
                : null;
        }

        /// <summary>
        /// Writes the output of a local file to the stream
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="vf"></param>
        /// <param name="virtualWriter"></param>
        /// <param name="type"></param>
        /// <param name="http"></param>
        protected virtual CompositeFileDefinition WriteVirtualFileToStream(StreamWriter sw, IVirtualFile vf, IVirtualFileWriter virtualWriter, ClientDependencyType type, HttpContextBase http)
        {
            if (virtualWriter == null) throw new ArgumentNullException("virtualWriter");
            if (vf == null) return null;

            return virtualWriter.WriteToStream(this, sw, vf, type, vf.Path, http)
                ? new CompositeFileDefinition(vf.Path, true)
                : null;
        }

        /// <summary>
        /// Returns a URL used to return a compbined/compressed/optimized version of all dependencies.
        /// <remarks>
        /// The full url with the encoded query strings for the handler which will process the composite list
        /// of dependencies. The handler will compbine, compress, minify, and output cache the results
        /// on the base64 encoded string.
        /// </remarks>        
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="type"></param>
        /// <param name="http"></param>
        /// <returns>An array containing the list of composite file URLs. This will generally only contain 1 value unless
        /// the number of files registered exceeds the maximum length, then it will return more than one file.</returns>
        public virtual string[] ProcessCompositeList(
            IEnumerable<IClientDependencyFile> dependencies,
            ClientDependencyType type,
            HttpContextBase http)
        {
            return ProcessCompositeList(dependencies, type, http, null);
        }

        /// <summary>
        /// When the path type is one of the base64 paths, this will create the composite file urls for all of the dependencies. 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dependencies"></param>
        /// <param name="compositeFileHandlerPath"></param>
        /// <param name="http"></param>
        /// <param name="maxLength">the max length each url can be</param>
        /// <param name="version">the current cdf version</param>
        /// <returns></returns>
        /// <remarks>
        /// Generally there will only be one path returned but his depends on how many dependencies there are and whether the base64 created path will exceed the max url length parameter.
        /// If the string length exceeds it, then we need to creaet multiple paths all of which must be less length than the maximum provided.
        /// </remarks>
        internal IEnumerable<string> GetCompositeFileUrls(
            ClientDependencyType type,
            IClientDependencyFile[] dependencies,
            string compositeFileHandlerPath,
            HttpContextBase http,
            int maxLength,
            int version)
        {
            var files = new List<string>();
            var currBuilder = new StringBuilder();
            var base64Builder = new StringBuilder();
            var builderCount = 1;
            var stringType = type.ToString();

            var remaining = new Queue<IClientDependencyFile>(dependencies);
            while (remaining.Any())
            {
                var current = remaining.Peek();

                //update the base64 output to get the length
                base64Builder.Append(current.FilePath.EncodeTo64());

                //test if the current base64 string exceeds the max length, if so we need to split
                if ((base64Builder.Length
                     + compositeFileHandlerPath.Length
                     + stringType.Length
                     + version.ToString(CultureInfo.InvariantCulture).Length
                    //this number deals with the ampersands, etc...
                     + 10)
                    >= (maxLength))
                {
                    //we need to do a check here, this is the first one and it's already exceeded the max length we cannot continue
                    if (currBuilder.Length == 0)
                    {
                        throw new InvalidOperationException("The path for the single dependency: '" + current.FilePath + "' exceeds the max length (" + maxLength + "), either reduce the single dependency's path length or increase the CompositeDependencyHandler.MaxHandlerUrlLength value");
                    }

                    //flush the current output to the array
                    files.Add(currBuilder.ToString());
                    //create some new output
                    currBuilder = new StringBuilder();
                    base64Builder = new StringBuilder();
                    builderCount++;
                }
                else
                {
                    //update the normal builder
                    currBuilder.Append(current.FilePath + ";");
                    //remove from the queue
                    remaining.Dequeue();
                }
            }

            //foreach (var a in dependencies)
            //{
            //    //update the base64 output to get the length
            //    base64Builder.Append(a.FilePath.EncodeTo64());

            //    //test if the current base64 string exceeds the max length, if so we need to split
            //    if ((base64Builder.Length
            //        + compositeFileHandlerPath.Length
            //        + stringType.Length
            //        + version.Length
            //        + 10)
            //        >= (maxLength))
            //    {
            //        //add the current output to the array
            //        files.Add(currBuilder.ToString());
            //        //create some new output
            //        currBuilder = new StringBuilder();
            //        base64Builder = new StringBuilder();
            //        builderCount++;
            //    }

            //    //update the normal builder
            //    currBuilder.Append(a.FilePath + ";");
            //}

            if (builderCount > files.Count)
            {
                files.Add(currBuilder.ToString());
            }

            //now, compress each url
            for (var i = 0; i < files.Count; i++)
            {
                //append our version to the combined url 
                var encodedFile = files[i].EncodeTo64Url();
                files[i] = GetCompositeFileUrl(encodedFile, type, http, UrlType, compositeFileHandlerPath, version);
            }

            return files.ToArray();
        }

        public virtual string[] ProcessCompositeList(
            IEnumerable<IClientDependencyFile> dependencies,
            ClientDependencyType type,
            HttpContextBase http,
            string compositeFileHandlerPath)
        {
            var asArray = dependencies.ToArray();

            if (!asArray.Any())
                return new string[] { };

            compositeFileHandlerPath = compositeFileHandlerPath ?? ClientDependencySettings.Instance.CompositeFileHandlerPath;

            switch (UrlType)
            {
                case CompositeUrlType.MappedId:

                    //use the file mapper to create us a file key/id for the file set
                    var fileKey = ClientDependencySettings.Instance.DefaultFileMapProvider.CreateNewMap(
                        http,
                        asArray,
                        ClientDependencySettings.Instance.Version);

                    //create the url
                    return new[] { GetCompositeFileUrl(
                        fileKey, 
                        type, 
                        http, 
                        CompositeUrlType.MappedId, 
                        compositeFileHandlerPath,
                        ClientDependencySettings.Instance.Version) };

                default:

                    //build the combined composite list urls          

                    return GetCompositeFileUrls(type, asArray, compositeFileHandlerPath, http, CompositeDependencyHandler.MaxHandlerUrlLength, ClientDependencySettings.Instance.Version).ToArray();
            }
        }

        /// <summary>
        /// Returns the url for the composite file handler for the filePath specified.
        /// </summary>
        /// <param name="fileKey">The Base64 encoded file paths or the file map key used to lookup the required dependencies</param>
        /// <param name="type"></param>
        /// <param name="http"></param>
        /// <param name="urlType"></param>
        /// <param name="compositeFileHandlerPath"> </param>
        /// <param name="version"> </param>
        /// <returns></returns>
        public virtual string GetCompositeFileUrl(
            string fileKey,
            ClientDependencyType type,
            HttpContextBase http,
            CompositeUrlType urlType,
            string compositeFileHandlerPath,
            int version)
        {
            var url = new StringBuilder();
            switch (urlType)
            {
                case CompositeUrlType.Base64QueryStrings:

                    //Create a URL with a base64 query string

                    const string handler = "{0}?s={1}&t={2}";
                    url.Append(string.Format(handler,
                                             compositeFileHandlerPath,
                                             http.Server.UrlEncode(fileKey), type));
                    url.Append("&cdv=");
                    url.Append(version.ToString());
                    break;
                default:

                    //Create a URL based on base64 paths instead of a query string

                    url.Append(compositeFileHandlerPath);
                    url.Append('/');

                    //create the path based on the path format...
                    var pathUrl = PathBasedUrlFormatter.CreatePath(PathBasedUrlFormat, fileKey, type, version);

                    //append the path formatted
                    url.Append(pathUrl);

                    break;
            }

            return url.ToString();
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            if (config == null)
                return;

            if (config["enableCssMinify"] != null)
            {
                bool enableCssMinify = true;
                if (bool.TryParse(config["enableCssMinify"], out enableCssMinify))
                    EnableCssMinify = enableCssMinify;
            }
            if (config["enableJsMinify"] != null)
            {
                bool enableJsMinify = true;
                if (bool.TryParse(config["enableJsMinify"], out enableJsMinify))
                    EnableJsMinify = enableJsMinify;
            }

            if (config["persistFiles"] != null)
            {
                bool persistFiles;
                if (bool.TryParse(config["persistFiles"], out persistFiles))
                    PersistCompositeFiles = persistFiles;
            }

            if (config["urlType"] != null)
            {
                try
                {
                    UrlType = (CompositeUrlType)Enum.Parse(typeof(CompositeUrlType), config["urlType"]);
                }
                catch (ArgumentException)
                {
                    //swallow exception, we've set the default
                }
            }
            if (config["pathUrlFormat"] != null)
            {
                PathBasedUrlFormat = config["pathUrlFormat"];
                PathBasedUrlFormatter.Validate(PathBasedUrlFormat);
            }

            CompositeFilePathAsString = config["compositeFilePath"] ?? DefaultDependencyPath;

            string bundleDomains = config["bundleDomains"];
            if (bundleDomains != null)
                bundleDomains = bundleDomains.Trim();
            if (string.IsNullOrEmpty(bundleDomains))
            {
                BundleDomains = new List<string>();
            }
            else
            {
                string[] domains = bundleDomains.Split(new char[] { ',' });
                for (int i = 0; i < domains.Length; i++)
                {
                    // make sure we have a starting dot and a trailing port
                    // ie 'maps.google.com' will be stored as '.maps.google.com:80'
                    if (domains[i].IndexOf(':') < 0)
                        domains[i] = domains[i] + ":80";
                    if (!domains[i].StartsWith("."))
                        domains[i] = "." + domains[i];
                }
                BundleDomains = new List<string>(domains);
            }
        }

        /// <summary>
        /// Minifies the file
        /// </summary>
        /// <param name="fileContents"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual string MinifyFile(string fileContents, ClientDependencyType type)
        {
            switch (type)
            {
                case ClientDependencyType.Css:
                    return EnableCssMinify ? CssHelper.MinifyCss(fileContents) : fileContents;
                case ClientDependencyType.Javascript:
                    return EnableJsMinify ? JSMin.CompressJS(fileContents) : fileContents;
                default:
                    return fileContents;
            }
        }

        /// <summary>
        /// Minifies the file
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual string MinifyFile(Stream fileStream, ClientDependencyType type)
        {
            Func<Stream, string> streamToString = stream =>
            {
                if (!stream.CanRead)
                    throw new InvalidOperationException("Cannot read input stream");

                if (stream.CanSeek)
                    stream.Position = 0;

                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            };

            switch (type)
            {
                case ClientDependencyType.Css:
                    return EnableCssMinify ? CssHelper.MinifyCss(fileStream) : streamToString(fileStream);
                case ClientDependencyType.Javascript:
                    return EnableJsMinify ? JSMin.CompressJS(fileStream) : streamToString(fileStream);
                default:
                    return streamToString(fileStream);
            }
        }

        /// <summary>
        /// This ensures that all paths (i.e. images) in a CSS file have their paths change to absolute paths.
        /// </summary>
        /// <param name="fileContents"></param>
        /// <param name="type"></param>
        /// <param name="url"></param>
        /// <param name="http"></param>
        /// <returns></returns>
        protected virtual string ParseCssFilePaths(string fileContents, ClientDependencyType type, string url, HttpContextBase http)
        {
            //if it is a CSS file we need to parse the URLs
            if (type == ClientDependencyType.Css)
            {
                fileContents = CssHelper.ReplaceUrlsWithAbsolutePaths(fileContents, url, http);
            }
            return fileContents;
        }

        /// <summary>
        /// Tries to convert the url to a uri, then read the request into a string and return it.
        /// This takes into account relative vs absolute URI's
        /// </summary>
        /// <param name="url"></param>
        /// <param name="requestContents"></param>
        /// <param name="http"></param>
        /// <returns>true if successful, false if not successful</returns>
        /// <remarks>
        /// if the path is a relative local path, the we use Server.Execute to get the request output, otherwise
        /// if it is an absolute path, a WebClient request is made to fetch the contents.
        /// </remarks>
        [Obsolete("This is no longer used in the codebase and will be removed in future versions")]
        protected bool TryReadUri(string url, out string requestContents, HttpContextBase http)
        {
            Uri uri;
            return RequestHelper.TryReadUri(url, http, BundleDomains, out requestContents, out uri);
        }

    }
}
