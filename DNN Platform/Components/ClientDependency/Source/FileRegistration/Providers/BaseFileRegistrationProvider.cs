using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Configuration.Provider;
using System.Web;
using System.Linq;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Controls;
using ClientDependency.Core.Config;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public abstract class BaseFileRegistrationProvider : ProviderBase
    {
        /// <summary>
        /// Constructor sets defaults
        /// </summary>
        protected BaseFileRegistrationProvider()
        {
            EnableCompositeFiles = true;
            DisableCompositeBundling = false;
            EnableDebugVersionQueryString = true;
        }

        /// <summary>
        /// By default this is true but can be overriden (in either config or code). 
        /// Composite files are never enabled with compilation debug="true" however.
        /// </summary>
        //*** DNN related change *** begin
        public virtual bool EnableCompositeFiles { get; set; }
        //*** DNN related change *** end

        /// <summary>
        /// By default this is false. Enabling this setting will output each dependeny as its own file in the markup instead of bundling them
        /// as a single composite bundle.
        /// </summary>
        public bool DisableCompositeBundling { get; set; }

        /// <summary>
        /// By default this is true but can be disabled (in either config or code). When this
        /// is enabled a query string like ?cdv=1235 of the current CDF version will be appended
        /// to dependencies when debugging is enabled or when composite files are disabled
        /// </summary>
        public bool EnableDebugVersionQueryString { get; set; }

        #region Abstract methods/properties

        /// <summary>
        /// This is called when rendering many js dependencies
        /// </summary>
        /// <param name="jsDependencies"></param>
        /// <param name="http"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        protected abstract string RenderJsDependencies(IEnumerable<IClientDependencyFile> jsDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes);

        /// <summary>
        /// This is called when rendering many css dependencies
        /// </summary>
        /// <param name="cssDependencies"></param>
        /// <param name="http"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        protected abstract string RenderCssDependencies(IEnumerable<IClientDependencyFile> cssDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes);

        /// <summary>
        /// Called to render a single js file
        /// </summary>
        /// <param name="js"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        protected abstract string RenderSingleJsFile(string js, IDictionary<string, string> htmlAttributes);

        /// <summary>
        /// Called to render a single css file
        /// </summary>
        /// <param name="css"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        protected abstract string RenderSingleCssFile(string css, IDictionary<string, string> htmlAttributes);

        #endregion

		#region CompositeFileHandlerPath config

        //set the default
        private string _compositeFileHandlerPath = "~/DependencyHandler.axd";
        private volatile bool _compositeFileHandlerPathInitialized = false;

        [Obsolete("The GetCompositeFileHandlerPath should be retrieved from the compositeFiles element in config: ClientDependencySettings.Instance.CompositeFileHandlerPath")]
		protected internal string GetCompositeFileHandlerPath(HttpContextBase http)
		{
		    if (!_compositeFileHandlerPathInitialized)
		    {
                lock (this)
                {
                    //double check
                    if (!_compositeFileHandlerPathInitialized)
                    {                        
                        //we may need to convert this to a real path
                        if (_compositeFileHandlerPath.StartsWith("~/"))
                        {
                            _compositeFileHandlerPath = VirtualPathUtility.ToAbsolute(_compositeFileHandlerPath, http.Request.ApplicationPath);    
                        }
                        //set the flag, we're done
                        _compositeFileHandlerPathInitialized = true;
                    }                    
                }
		    }
            return _compositeFileHandlerPath;
		}

		#endregion

		#region Provider Initialization

		public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            if (config != null && config["enableCompositeFiles"] != null && !string.IsNullOrEmpty(config["enableCompositeFiles"]))
            {
                bool enableCompositeFiles;
                if (bool.TryParse(config["enableCompositeFiles"], out enableCompositeFiles))
                {
                    EnableCompositeFiles = enableCompositeFiles;
                }
            }

            if (config != null && config["disableCompositeBundling"] != null && !string.IsNullOrEmpty(config["disableCompositeBundling"]))
            {
                bool disableCompositeBundling;
                if (bool.TryParse(config["disableCompositeBundling"], out disableCompositeBundling))
                {
                    DisableCompositeBundling = disableCompositeBundling;
                }
            }

            if (config != null && config["enableDebugVersionQueryString"] != null && !string.IsNullOrEmpty(config["enableDebugVersionQueryString"]))
            {
                bool enableDebugVersionQueryString;
                if (bool.TryParse(config["disableCompositeBundling"], out enableDebugVersionQueryString))
                {
                    EnableDebugVersionQueryString = enableDebugVersionQueryString;
                }
            }
		}

        #endregion

        #region Protected Methods

        private void StaggerOnDifferentAttributes(
            HttpContextBase http,
            StringBuilder builder,
            IEnumerable<IClientDependencyFile> list,
            Func<IEnumerable<IClientDependencyFile>, HttpContextBase, IDictionary<string, string>, string> renderCompositeFiles)
        {
            var sameAttributes = new List<IClientDependencyFile>();
            var currHtmlAttr = GetHtmlAttributes(list.ElementAt(0));
            foreach (var c in list)
            {
                if (!currHtmlAttr.IsEqualTo(GetHtmlAttributes(c)))
                {
                    //if the attributes are different we need to stagger
                    if (sameAttributes.Any())
                    {
                        //render the current buffer
                        builder.Append(renderCompositeFiles(sameAttributes, http, currHtmlAttr));
                        //clear the buffer
                        sameAttributes.Clear();
                    }
                }

                //add the item to the buffer and set the current html attributes
                sameAttributes.Add(c);
                currHtmlAttr = GetHtmlAttributes(c);
            }

            //if there's anything in the buffer then write the remaining
            if (sameAttributes.Any())
                builder.Append(renderCompositeFiles(sameAttributes, http, currHtmlAttr));
        }

        /// <summary>
        /// Because we can have both internal and external dependencies rendered, we need to stagger the script tag output... if they are external, we need to stop the compressing/combining
        /// and write out the external dependency, then resume the compressing/combining handler.
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="http"></param>
        /// <param name="builder"></param>
        /// <param name="renderCompositeFiles"></param>
        /// <param name="renderSingle"></param>
        protected void WriteStaggeredDependencies(
            IEnumerable<IClientDependencyFile> dependencies, 
            HttpContextBase http, 
            StringBuilder builder,
            Func<IEnumerable<IClientDependencyFile>, HttpContextBase, IDictionary<string, string>, string> renderCompositeFiles,
            Func<string, IDictionary<string, string>, string> renderSingle)
        {
            var fileBasedExtensions = ClientDependencySettings.Instance.FileBasedDependencyExtensionList
                                                              .Union(FileWriters.GetRegisteredExtensions())
                                                              .ToArray();

            var currNonRemoteFiles = new List<IClientDependencyFile>();
            foreach (var f in dependencies)
            {
                //need to parse out the request's extensions and remove query strings
                //need to force non-bundled lines for items with query parameters or a hash value.
                var extension = f.FilePath.Contains('?') || f.FilePath.Contains('#') ? "" : Path.GetExtension(f.FilePath);
                var stringExt = "";
                if (!string.IsNullOrWhiteSpace(extension))
                {
                    stringExt = extension.ToUpper().Split(new[] {'?'}, StringSplitOptions.RemoveEmptyEntries)[0];
                }

                //if this is a protocol-relative/protocol-less uri, then we need to add the protocol for the remaining
                // logic to work properly
                if (f.FilePath.StartsWith("//"))
                {
                    f.FilePath = Regex.Replace(f.FilePath, @"^\/\/", http.Request.Url.GetLeftPart(UriPartial.Scheme));
                }
                

                // if it is an external resource OR
                // if it is a non-standard JS/CSS resource (i.e. a server request)
                // then we need to break the sequence
                // unless it has been explicitely required that the dependency be bundled
                if ((!http.IsAbsolutePath(f.FilePath) && !fileBasedExtensions.Contains(stringExt))
                    //now check for external resources
                    || (http.IsAbsolutePath(f.FilePath)
                        //remote dependencies aren't local
                        && !new Uri(f.FilePath, UriKind.RelativeOrAbsolute).IsLocalUri(http)
                        // not required to be bundled
                        && !f.ForceBundle))
                {
                    //we've encountered an external dependency, so we need to break the sequence and restart it after
                    //we output the raw script tag
                    if (currNonRemoteFiles.Count > 0)
                    {
                        //render the current buffer
                        StaggerOnDifferentAttributes(http, builder, currNonRemoteFiles, renderCompositeFiles);
                        //clear the buffer
                        currNonRemoteFiles.Clear();
                    }
                    //write out the single script tag
                    builder.Append(renderSingle(f.FilePath, GetHtmlAttributes(f)));
                }
                else
                {
                    //its a normal registration, add to the buffer
                    currNonRemoteFiles.Add(f);
                }
            }
            //now check if there's anything in the buffer to render
            if (currNonRemoteFiles.Count > 0)
            {
                //render the current buffer
                StaggerOnDifferentAttributes(http, builder, currNonRemoteFiles, renderCompositeFiles);
            }
        }

        /// <summary>
        /// Ensures the correctly resolved file path is set for each dependency (i.e. so that ~ are taken care of) and also
        /// prefixes the file path with the correct base path specified for the PathNameAlias if specified.
        /// </summary>
        /// <param name="dependencies">The dependencies list for which file paths will be updated</param>
        /// <param name="folderPaths"></param>
        /// <param name="http"></param>
        protected virtual void UpdateFilePaths(IEnumerable<IClientDependencyFile> dependencies,
            HashSet<IClientDependencyPath> folderPaths, HttpContextBase http)
        {
            var paths = folderPaths.ToList();
            foreach (var dependency in dependencies)
            {
                if (!string.IsNullOrEmpty(dependency.PathNameAlias))
                {                    
                    var d = dependency;
                    var path = paths.Find(
                        (p) => p.Name == d.PathNameAlias);
                    if (path == null)
                    {
                        throw new NullReferenceException("The PathNameAlias specified for dependency " + dependency.FilePath + " does not exist in the ClientDependencyPathCollection");
                    }
                    var resolvedPath = path.ResolvePath(http);
                    var basePath = resolvedPath.EndsWith("/") ? resolvedPath : resolvedPath + "/";
                    dependency.FilePath = basePath + dependency.FilePath;
                    dependency.ForceBundle = (dependency.ForceBundle | path.ForceBundle);
                }
                else
                {
                    dependency.FilePath = dependency.ResolveFilePath(http);
                }

                //append query strings to each file if we are in debug mode
                if (EnableDebugVersionQueryString &&
                    (http.IsDebuggingEnabled || !EnableCompositeFiles))
                {
                    dependency.FilePath = AppendVersion(dependency.FilePath, http);
                }
            }
        }


        /// <summary>
        /// This will ensure that no duplicates have made it into the collection.
        /// Duplicates WILL occur if the same dependency is registered in 2 different ways: 
        /// one with a global path and one with a full path. This is because paths may not be defined
        /// until we render so we cannot de-duplicate at the time of registration.
        /// De-duplication will remove the dependency with a lower priority or later in the list.
        /// This also must be called after UpdatePaths are called since we need to full path filled in.
        /// </summary>
        /// <param name="dependencies">The dependencies list for which duplicates will be removed</param>
        /// <param name="folderPaths"></param>
        protected virtual void EnsureNoDuplicates(List<IClientDependencyFile> dependencies, HashSet<IClientDependencyPath> folderPaths)
        {
            var toKeep = new HashSet<IClientDependencyFile>();

            foreach (var d in dependencies)
            {
                //check if it is a duplicate
                if (dependencies.Count(x => x.FilePath.ToUpper().Trim().Equals(d.FilePath.ToUpper().Trim())) > 1)
                {
                    //find the dups and return an object with the associated index
                    var dups = dependencies
                        .Where(x => x.FilePath.ToUpper().Trim().Equals(d.FilePath.ToUpper().Trim()))
                        .Select((x, index) => new { Index = index, File = x })
                        .ToList();

                    var priorities = dups.Select(x => x.File.Priority).Distinct().ToList();

                    //if there's more than 1 priority defined, we know we need to remove by priority
                    //instead of by index
                    if (priorities.Count() > 1)
                    {
                        toKeep.Add(dups.First(x => x.File.Priority == priorities.Min()).File);
                    }
                    else
                    {
                        //if not by priority, we just need to keep the first on in the list
                        toKeep.Add(dups.First(x => x.Index == dups.Select(p => p.Index).Min()).File);
                    }
                }
                else
                {
                    //if there's only one found, then just add it to our output
                    toKeep.Add(d);
                }
            }

            dependencies.Clear();
            dependencies.AddRange(toKeep);


        }

        private string AppendVersion(string url, HttpContextBase http)
        {
            if (ClientDependencySettings.Instance.Version == 0)
                return url;
            if ((http.IsDebuggingEnabled || !EnableCompositeFiles)
                || ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.UrlType == CompositeUrlType.Base64QueryStrings)
            {
                //don't append if it is already there!
                if (url.Contains("cdv=" + ClientDependencySettings.Instance.Version))
                    return url;

                //move hash to the end of the file name if present. Eg: /s/myscript.js?cdv=3#myhash
                var hash = url.Contains("#") ? "#" + url.Split(new[] { '#' }, 2)[1] : "";
                if (!String.IsNullOrEmpty(hash))
                    url = url.Split(new[] { '#' }, 2)[0];

                //ensure there's not duplicated query string syntax
                url += url.Contains('?') ? "&" : "?";
                //append a version
                url += "cdv=" + ClientDependencySettings.Instance.Version + hash;
            }
            else
            {
                if (url.EndsWith(ClientDependencySettings.Instance.Version.ToString()))
                    return url;

                //the URL should end with a '0'
                url = url.TrimEnd('0') + ClientDependencySettings.Instance.Version;
            }
            return url;
        }

        #endregion        

        /// <summary>
        /// Checks if the current file implements the html attribute interfaces and returns the appropriate html attributes
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static IDictionary<string, string> GetHtmlAttributes(IClientDependencyFile file)
        {
            IDictionary<string, string> attributes = new Dictionary<string, string>();

            var htmlAttributes = file as IHaveHtmlAttributes;
            if (htmlAttributes != null)
            {
                var fileWithAttributes = htmlAttributes;
                attributes = fileWithAttributes.HtmlAttributes;

                var parsing = file as IRequiresHtmlAttributesParsing;
                if (parsing != null)
                {
                    //we need to parse the attributes into the dictionary
                    HtmlAttributesStringParser.ParseIntoDictionary(parsing.HtmlAttributesAsString, attributes);                    
                }                
            }

            //now we must ensure that the correct js/css attribute exist!
            switch(file.DependencyType)
            {
                case ClientDependencyType.Javascript:
                    if (!attributes.ContainsKey("type"))
                        attributes.Add("type", "text/javascript");
                    if (attributes.ContainsKey("src"))
                        attributes.Remove("src");
                    break;
                case ClientDependencyType.Css:
                    if (!attributes.ContainsKey("type"))
                        attributes.Add("type", "text/css");
                    if (!attributes.ContainsKey("rel"))
                        attributes.Add("rel", "stylesheet");
                    if (attributes.ContainsKey("href"))
                        attributes.Remove("href");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //just return an empty dictionary
            return attributes;
        }

        

        /// <summary>
        /// Called to write the js and css to string output
        /// </summary>
        /// <param name="allDependencies"></param>
        /// <param name="paths"></param>
        /// <param name="jsOutput"></param>
        /// <param name="cssOutput"></param>
        /// <param name="http"></param>
        internal void WriteDependencies(List<IClientDependencyFile> allDependencies,
            HashSet<IClientDependencyPath> paths,
            out string jsOutput,
            out string cssOutput,
            HttpContextBase http)
        {
            //create the hash to see if we've already stored it
            var hashCodeCombiner = new HashCodeCombiner();
            foreach (var d in allDependencies)
            {
                hashCodeCombiner.AddObject(d);
            }
            var hash = hashCodeCombiner.GetCombinedHashCode();
            
            //we may have already processed this so don't do it again
            if (http.Items["BaseRenderer.RegisterDependencies." + hash] == null)
            {
                var folderPaths = paths;
                UpdateFilePaths(allDependencies, folderPaths, http);
                EnsureNoDuplicates(allDependencies, folderPaths);

                //now we regenerate the hash since dependencies have been removed/etc.. 
                // and update the context items so it's not run again
                hashCodeCombiner = new HashCodeCombiner();
                foreach (var d in allDependencies)
                {
                    hashCodeCombiner.AddObject(d);
                }
                hash = hashCodeCombiner.GetCombinedHashCode();
                http.Items["BaseRenderer.RegisterDependencies." + hash] = true;
            }

            var cssBuilder = new StringBuilder();
            var jsBuilder = new StringBuilder();

            //group by the group and order by the value
            foreach (var group in allDependencies.GroupBy(x => x.Group).OrderBy(x => x.Key))
            {
                //sort both the js and css dependencies properly

                //*** DNN related change *** begin
                var jsDependencies = DependencySorter.SortItems(DependencySorter.FilterDependencies(
                    group.Where(x => x.DependencyType == ClientDependencyType.Javascript).ToList()));

                var cssDependencies = DependencySorter.SortItems(DependencySorter.FilterDependencies(
                    group.Where(x => x.DependencyType == ClientDependencyType.Css).ToList()));
                //*** DNN related change *** end

                //render
                WriteStaggeredDependencies(cssDependencies, http, cssBuilder, RenderCssDependencies, RenderSingleCssFile);
                WriteStaggeredDependencies(jsDependencies, http, jsBuilder, RenderJsDependencies, RenderSingleJsFile);
            }

            cssOutput = cssBuilder.ToString();
            jsOutput = jsBuilder.ToString();
        }

        protected virtual void RenderJsComposites(HttpContextBase http, IDictionary<string, string> htmlAttributes, StringBuilder sb, IEnumerable<IClientDependencyFile> dependencies)
        {
            var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(
                    dependencies,
                    ClientDependencyType.Javascript, 
                    http,
                    ClientDependencySettings.Instance.CompositeFileHandlerPath);

            foreach (var s in comp)
            {
                sb.Append(RenderSingleJsFile(s, htmlAttributes));
            }
        }

        protected virtual void RenderCssComposites(HttpContextBase http, IDictionary<string, string> htmlAttributes, StringBuilder sb, IEnumerable<IClientDependencyFile> dependencies)
        {
            var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(
                    dependencies,
                    ClientDependencyType.Css,
                    http,
                    ClientDependencySettings.Instance.CompositeFileHandlerPath);

            foreach (var s in comp)
            {
                sb.Append(RenderSingleCssFile(s, htmlAttributes));
            }
        }
    }
}
