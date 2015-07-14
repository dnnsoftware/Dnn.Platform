using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.Config;
using System.IO;
using System.Web;
using System.Net;
using System.Configuration.Provider;

namespace ClientDependency.Core.CompositeFiles.Providers
{
    public abstract class BaseCompositeFileProcessingProvider : ProviderBase, IHttpProvider
    {

        private const string DefaultDependencyPath = "~/App_Data/ClientDependency";
        private readonly string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        /// <summary>
        /// The path specified in the config
        /// </summary>
        private string _compositeFilePath;

        /// <summary>
        /// constructor sets defaults
        /// </summary>
        protected BaseCompositeFileProcessingProvider()
        {
            PersistCompositeFiles = true;
            EnableCssMinify = true;
            EnableJsMinify = true;
            UrlType = CompositeUrlType.MappedId;

            _compositeFilePath = DefaultDependencyPath;
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
            CompositeFilePath = new DirectoryInfo(http.Server.MapPath(_compositeFilePath));
        }

        #endregion

        public abstract FileInfo SaveCompositeFile(byte[] fileContents, ClientDependencyType type, HttpServerUtilityBase server, int version);
        public abstract byte[] CombineFiles(string[] filePaths, HttpContextBase context, ClientDependencyType type, out List<CompositeFileDefinition> fileDefs);
        public abstract byte[] CompressBytes(CompressionType type, byte[] fileBytes);

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
            if (!dependencies.Any())
                return new string[] { };

            switch (UrlType)
            {
                case CompositeUrlType.MappedId:
                    
                    //use the file mapper to create us a file key/id for the file set
                    var fileKey = ClientDependencySettings.Instance.DefaultFileMapProvider.CreateNewMap(http, dependencies, GetVersion());

                    //create the url
                    return new[] { GetCompositeFileUrl(fileKey, type, http, CompositeUrlType.MappedId) };

                default:
                    
                    //build the combined composite list urls          
                    
                    var files = new List<string>();
                    var currBuilder = new StringBuilder();
                    var base64Builder = new StringBuilder();
                    var builderCount = 1;
                    var stringType = type.ToString();
                    foreach (var a in dependencies)
                    {
                        //update the base64 output to get the length
                        base64Builder.Append(a.FilePath.EncodeTo64());

                        //test if the current base64 string exceeds the max length, if so we need to split
                        if ((base64Builder.Length + ClientDependencySettings.Instance.CompositeFileHandlerPath.Length + stringType.Length + 10) 
                            >= (CompositeDependencyHandler.MaxHandlerUrlLength))
                        {
                            //add the current output to the array
                            files.Add(currBuilder.ToString());
                            //create some new output
                            currBuilder = new StringBuilder();
                            base64Builder = new StringBuilder();
                            builderCount++;
                        }

                        //update the normal builder
                        currBuilder.Append(a.FilePath + ";");
                    }

                    if (builderCount > files.Count)
                    {
                        files.Add(currBuilder.ToString());
                    }

                    //now, compress each url
                    for (var i = 0; i < files.Count; i++)
                    {
                        //append our version to the combined url 
                        var encodedFile = files[i].EncodeTo64Url();
                        files[i] = GetCompositeFileUrl(encodedFile, type, http, UrlType);
                    }

                    return files.ToArray();
            }            
        }

        public virtual int GetVersion()
        {
            return ClientDependencySettings.Instance.Version;
        }

        /// <summary>
        /// Returns the url for the composite file handler for the filePath specified.
        /// </summary>
        /// <param name="fileKey">The Base64 encoded file paths or the file map key used to lookup the required dependencies</param>
        /// <param name="type"></param>
        /// <param name="http"></param>
        /// <param name="urlType"></param>
        /// <returns></returns>
        public virtual string GetCompositeFileUrl(
            string fileKey, 
            ClientDependencyType type, 
            HttpContextBase http, 
            CompositeUrlType urlType)
        {
            var url = new StringBuilder();
            int version = GetVersion();
            switch (urlType)
            {
                case CompositeUrlType.Base64QueryStrings:

                    //Create a URL with a base64 query string

                    const string handler = "{0}?s={1}&t={2}";
                    url.Append(string.Format(handler,
                                             ClientDependencySettings.Instance.CompositeFileHandlerPath,
                                             http.Server.UrlEncode(fileKey), type));
                    url.Append("&cdv=");
                    url.Append(version.ToString());
                    break;
                default:

                    //Create a URL based on base64 paths instead of a query string


                    //this used to be a "." but this causes problems with mvc and ignore routes for 
                    //some very strange reason, so we'll just use normal paths.
                    var versionDelimiter = ".";

                    url.Append(ClientDependencySettings.Instance.CompositeFileHandlerPath);
                    int pos = 0;
                    while (fileKey.Length > pos)
                    {
                        url.Append("/");
                        int len = Math.Min(fileKey.Length - pos, 240);
                        url.Append(fileKey.Substring(pos, len));
                        pos += 240;
                    }
                    url.Append(versionDelimiter);
                    url.Append(version.ToString());
                    switch (type)
                    {
                        case ClientDependencyType.Css:
                            url.Append(versionDelimiter);
                            url.Append("css");
                            break;
                        case ClientDependencyType.Javascript:
                            url.Append(versionDelimiter);
                            url.Append("js");
                            break;
                    }
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
                bool enableCssMinify;
                if (bool.TryParse(config["enableCssMinify"], out enableCssMinify))
                    EnableCssMinify = enableCssMinify;
            }
            if (config["enableJsMinify"] != null)
            {
                bool enableJsMinify;
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

            _compositeFilePath = config["compositeFilePath"] ?? DefaultDependencyPath;

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
        protected virtual string MinifyFile(string fileContents, ClientDependencyType type)
        {
            switch (type)
            {
                case ClientDependencyType.Css:
                    return EnableCssMinify ? CssMin.CompressCSS(fileContents) : fileContents;
                case ClientDependencyType.Javascript:
                    return EnableJsMinify ? JSMin.CompressJS(fileContents) : fileContents;
                default:
                    return fileContents;
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
                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                fileContents = CssFileUrlFormatter.TransformCssFile(fileContents, uri.MakeAbsoluteUri(http));
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
        protected bool TryReadUri(string url, out string requestContents, HttpContextBase http)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
            {
                //flag of whether or not to make a request to get the external resource (used below)
                bool bundleExternalUri = false;

                //if its a relative path, then check if we should execute/retreive contents,
                //otherwise change it to an absolute path and try to request it.
                if (!uri.IsAbsoluteUri)
                {
                    //if this is an ASPX page, we should execute it instead of http getting it.
                    if (uri.ToString().EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var sw = new StringWriter();
                        try
                        {
                            http.Server.Execute(url, sw);
                            requestContents = sw.ToString();
                            sw.Close();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            ClientDependencySettings.Instance.Logger.Error(string.Format("Could not load file contents from {0}. EXCEPTION: {1}", url, ex.Message), ex);
                            requestContents = "";
                            return false;
                        }
                    }
                    
                    //if this is a call for a web resource, we should http get it
                    if (url.StartsWith(http.Request.ApplicationPath.TrimEnd('/') + "/webresource.axd", StringComparison.InvariantCultureIgnoreCase))
                    {
                        bundleExternalUri = true;
                    }
                }

                try
                {
                    //we've gotten this far, make the URI absolute and try to load it
                    uri = uri.MakeAbsoluteUri(http);

                    //if this isn't a web resource, we need to check if its approved
                    if (!bundleExternalUri)
                    {
                        // get the domain to test, with starting dot and trailing port, then compare with
                        // declared (authorized) domains. the starting dot is here to allow for subdomain
                        // approval, eg '.maps.google.com:80' will be approved by rule '.google.com:80', yet
                        // '.roguegoogle.com:80' will not.
                        var domain = string.Format(".{0}:{1}", uri.Host, uri.Port);

                        foreach (string bundleDomain in BundleDomains)
                        {
                            if (domain.EndsWith(bundleDomain))
                            {
                                bundleExternalUri = true;
                                break;
                            }
                        }
                    }
                    
                    if (bundleExternalUri)
                    {
                        requestContents = GetXmlResponse(uri);
                        return true;
                    }
                    else
                    {
                        ClientDependencySettings.Instance.Logger.Error(string.Format("Could not load file contents from {0}. Domain is not white-listed.", url), null);
                    }
                }
                catch (Exception ex)
                {
                    ClientDependencySettings.Instance.Logger.Error(string.Format("Could not load file contents from {0}. EXCEPTION: {1}", url, ex.Message), ex);
                }


            }
            requestContents = "";
            return false;
        }

        /// <summary>
        /// Gets the web response and ensures that the BOM is not present not matter what encoding is specified.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private string GetXmlResponse(Uri resource)
        {
            string xml;

            using (var client = new WebClient())
            {
                client.Credentials = CredentialCache.DefaultNetworkCredentials;
                client.Encoding = Encoding.UTF8;
                xml = client.DownloadString(resource);
            }

            if (xml.StartsWith(_byteOrderMarkUtf8))
            {
                xml = xml.Remove(0, _byteOrderMarkUtf8.Length - 1);
            }

            return xml;
        }
    }
}
