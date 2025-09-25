// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using System.Xml;

    using ClientDependency.Core;
    using ClientDependency.Core.CompositeFiles.Providers;
    using ClientDependency.Core.Config;
    using ClientDependency.Core.Mvc;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>Provides the ability to request that client resources (JavaScript and CSS) be loaded on the client browser.</summary>
    public partial class MvcClientResourceManager
    {
        /// <summary>The default css provider.</summary>
        internal const string DefaultCssProvider = "DnnPageHeaderProvider";

        /// <summary>The default javascript provider.</summary>
        internal const string DefaultJsProvider = "DnnBodyProvider";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ClientResourceManager));
        private static readonly Dictionary<string, bool> FileExistsCache = new Dictionary<string, bool>();
        private static readonly ReaderWriterLockSlim LockFileExistsCache = new ReaderWriterLockSlim();

        /// <summary>Adds the necessary configuration to website root <c>web.config</c> to use the Client Dependency component.</summary>
        public static void AddConfiguration()
        {
            var configPath = HostingEnvironment.MapPath("~/web.config");
            if (string.IsNullOrEmpty(configPath))
            {
                return;
            }

            var xmlDoc = new XmlDocument { XmlResolver = null };
            xmlDoc.Load(configPath);
            XmlDocumentFragment xmlFrag;

            // Config Sections
            var sectionsConfig = xmlDoc.DocumentElement?.SelectSingleNode("configSections");
            if (sectionsConfig != null)
            {
                var clientDependencySectionConfig = sectionsConfig.SelectSingleNode("section[@name='clientDependency']");
                if (clientDependencySectionConfig == null)
                {
                    xmlFrag = xmlDoc.CreateDocumentFragment();
                    xmlFrag.InnerXml = "<section name=\"clientDependency\" type=\"ClientDependency.Core.Config.ClientDependencySection, ClientDependency.Core\" requirePermission=\"false\" />";
                    xmlDoc.DocumentElement.SelectSingleNode("configSections")?.AppendChild(xmlFrag);
                }
            }

            // Module Config
            var systemWebServerModulesConfig = xmlDoc.DocumentElement?.SelectSingleNode("system.webServer/modules");
            if (systemWebServerModulesConfig != null)
            {
                var moduleConfig = systemWebServerModulesConfig.SelectSingleNode("add[@name=\"ClientDependencyModule\"]");
                if (moduleConfig == null)
                {
                    xmlFrag = xmlDoc.CreateDocumentFragment();
                    xmlFrag.InnerXml = "<add name=\"ClientDependencyModule\" type=\"ClientDependency.Core.Module.ClientDependencyModule, ClientDependency.Core\"  preCondition=\"managedHandler\" />";
                    xmlDoc.DocumentElement.SelectSingleNode("system.webServer/modules")?.AppendChild(xmlFrag);
                }
            }

            // Handler Config
            var systemWebServerHandlersConfig = xmlDoc.DocumentElement?.SelectSingleNode("system.webServer/handlers");
            if (systemWebServerHandlersConfig != null)
            {
                var handlerConfig = systemWebServerHandlersConfig.SelectSingleNode("add[@name=\"ClientDependencyHandler\"]");
                if (handlerConfig == null)
                {
                    xmlFrag = xmlDoc.CreateDocumentFragment();
                    xmlFrag.InnerXml = "<add name=\"ClientDependencyHandler\" verb=\"*\" path=\"DependencyHandler.axd\" type=\"ClientDependency.Core.CompositeFiles.CompositeDependencyHandler, ClientDependency.Core\" preCondition=\"integratedMode\" />";
                    xmlDoc.DocumentElement.SelectSingleNode("system.webServer/handlers")?.AppendChild(xmlFrag);
                }
            }

            // ClientDependency Config
            var clientDependencyConfig = xmlDoc.DocumentElement?.SelectSingleNode("clientDependency");
            if (clientDependencyConfig == null)
            {
                xmlFrag = xmlDoc.CreateDocumentFragment();
                xmlFrag.InnerXml = @"<clientDependency version=""0"" fileDependencyExtensions="".js,.css"">
                                            <fileRegistration defaultProvider=""DnnControllerContextHeaderProvider"">
                                              <providers>
                                                <add name=""DnnBodyProvider"" type=""DotNetNuke.Web.Client.Providers.DnnBodyProvider, DotNetNuke.Web.Client"" enableCompositeFiles=""false"" />
                                                <add name=""DnnControllerContextHeaderProvider"" type=""DotNetNuke.Web.Client.Providers.DnnControllerContextHeaderProvider, DotNetNuke.Web.Client"" enableCompositeFiles=""false"" />
                                                <add name=""DnnFormBottomProvider"" type=""DotNetNuke.Web.Client.Providers.DnnFormBottomProvider, DotNetNuke.Web.Client"" enableCompositeFiles=""false"" />
                                                <add name=""ControllerContextHeaderProvider"" type=""ClientDependency.Core.FileRegistration.Providers.ControllerContextHeaderProvider, ClientDependency.Core"" enableCompositeFiles=""false""/>
                                                <add name=""LazyLoadProvider"" type=""ClientDependency.Core.FileRegistration.Providers.LazyLoadProvider, ClientDependency.Core"" enableCompositeFiles=""false""/>
                                                <add name=""LoaderControlProvider"" type=""ClientDependency.Core.FileRegistration.Providers.LoaderControlProvider, ClientDependency.Core"" enableCompositeFiles=""false""/>
                                              </providers>
                                            </fileRegistration>
                                             <mvc defaultRenderer=""DnnStandardRenderer"">
                                                  <renderers>
                                                    <add name=""DnnStandardRenderer"" type=""DotNetNuke.Web.Client.Providers.DnnStandardRenderer, DotNetNuke.Web.Client"" enableCompositeFiles=""false"" />
                                                    <add name=""LazyLoadRenderer"" type=""ClientDependency.Core.FileRegistration.Providers.LazyLoadRenderer, ClientDependency.Core"" enableCompositeFiles=""false"" />
                                                  </renderers>
                                             </mvc>
                                            <compositeFiles defaultFileProcessingProvider=""DnnCompositeFileProcessor"" compositeFileHandlerPath=""~/DependencyHandler.axd"">
                                              <fileProcessingProviders>
                                                <!-- For webfarms update the urlType attribute to Base64QueryStrings, default setting is MappedId -->
                                                <add name=""DnnCompositeFileProcessor"" type=""DotNetNuke.Web.Client.Providers.DnnCompositeFileProcessingProvider, DotNetNuke.Web.Client"" enableCssMinify=""false"" enableJsMinify=""true"" persistFiles=""true"" compositeFilePath=""~/App_Data/ClientDependency"" bundleDomains="""" urlType=""MappedId"" />
                                              </fileProcessingProviders>
                                            </compositeFiles>
                                          </clientDependency>";

                xmlDoc.DocumentElement?.AppendChild(xmlFrag);
            }

            // Save Config
            xmlDoc.Save(configPath);
        }

        /// <summary>Checks if ClientDependency is installed.</summary>
        /// <returns>A value indicating whether the ClientDependency provider is installed.</returns>
        public static bool IsInstalled()
        {
            var configPath = HostingEnvironment.MapPath("~/web.config");
            if (string.IsNullOrEmpty(configPath))
            {
                return false;
            }

            var xmlDoc = new XmlDocument { XmlResolver = null };
            xmlDoc.Load(configPath);

            return xmlDoc.DocumentElement?.SelectSingleNode("configSections")?.SelectSingleNode("section[@name='clientDependency']") != null;
        }

        /// <summary>Registers a stylesheet that has an admin level priority.</summary>
        /// <param name="page">The page on which to register the style.</param>
        /// <param name="filePath">The path to the CSS stylesheet.</param>
        public static void RegisterAdminStylesheet(ControllerContext page, string filePath)
        {
            RegisterStyleSheet(page, filePath, FileOrder.Css.AdminCss);
        }

        /// <summary>Registers the <c>default.css</c> stylesheet.</summary>
        /// <param name="page">The page on which to register the style.</param>
        /// <param name="filePath">The path to the CSS stylesheet.</param>
        public static void RegisterDefaultStylesheet(ControllerContext page, string filePath)
        {
            RegisterStyleSheet(page, filePath, (int)FileOrder.Css.DefaultCss, DefaultCssProvider, "dnndefault", "10.0.0");
        }

        /// <summary>Registers a stylesheet for a specific feature.</summary>
        /// <param name="page">The page on which to register the style.</param>
        /// <param name="filePath">The path to the CSS stylesheet.</param>
        public static void RegisterFeatureStylesheet(ControllerContext page, string filePath)
        {
            RegisterStyleSheet(page, filePath, FileOrder.Css.FeatureCss);
        }

        /// <summary>Registers a stylesheet specific for Internet Explorer.</summary>
        /// <param name="page">The page on which to register the style.</param>
        /// <param name="filePath">The path to the CSS stylesheet.</param>
        public static void RegisterIEStylesheet(ControllerContext page, string filePath)
        {
            var browser = HttpContext.Current.Request.Browser;
            if (browser.Browser == "Internet Explorer" || browser.Browser == "IE")
            {
                RegisterStyleSheet(page, filePath, FileOrder.Css.IeCss);
            }
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        public static void RegisterScript(ControllerContext page, string filePath)
        {
            RegisterScript(page, filePath, htmlAttributes: null);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>script</c> tag. The key being the attribute name and the value its value.</param>
        public static void RegisterScript(ControllerContext page, string filePath, IDictionary<string, string> htmlAttributes)
        {
            RegisterScript(page, filePath, FileOrder.Js.DefaultPriority, htmlAttributes);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public static void RegisterScript(ControllerContext page, string filePath, int priority)
        {
            RegisterScript(page, filePath, priority, htmlAttributes: null);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>script</c> tag. The key being the attribute name and the value its value.</param>
        public static void RegisterScript(ControllerContext page, string filePath, int priority, IDictionary<string, string> htmlAttributes)
        {
            RegisterScript(page, filePath, priority, DefaultJsProvider, htmlAttributes);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public static void RegisterScript(ControllerContext page, string filePath, FileOrder.Js priority)
        {
            RegisterScript(page, filePath, priority, htmlAttributes: null);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>script</c> tag. The key being the attribute name and the value its value.</param>
        public static void RegisterScript(ControllerContext page, string filePath, FileOrder.Js priority, IDictionary<string, string> htmlAttributes)
        {
            RegisterScript(page, filePath, (int)priority, DefaultJsProvider, htmlAttributes);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        public static void RegisterScript(ControllerContext page, string filePath, FileOrder.Js priority, string provider)
        {
            RegisterScript(page, filePath, priority, provider, htmlAttributes: null);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        /// /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>script</c> tag. The key being the attribute name and the value its value.</param>
        public static void RegisterScript(ControllerContext page, string filePath, FileOrder.Js priority, string provider, IDictionary<string, string> htmlAttributes)
        {
            RegisterScript(page, filePath, (int)priority, provider, htmlAttributes);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        public static void RegisterScript(ControllerContext page, string filePath, int priority, string provider)
        {
            RegisterScript(page, filePath, priority, provider, htmlAttributes: null);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>script</c> tag. The key being the attribute name and the value its value.</param>
        public static void RegisterScript(ControllerContext page, string filePath, int priority, string provider, IDictionary<string, string> htmlAttributes)
        {
            RegisterScript(page, filePath, priority, provider, string.Empty, string.Empty, htmlAttributes);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
        /// <param name="version">Version number of framework.</param>
        public static void RegisterScript(ControllerContext page, string filePath, int priority, string provider, string name, string version)
        {
            RegisterScript(page, filePath, priority, provider, name, version, htmlAttributes: null);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
        /// <param name="version">Version number of framework.</param>
        /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>script</c> tag. The key being the attribute name and the value its value.</param>
        public static void RegisterScript(
            ControllerContext page,
            string filePath,
            int priority,
            string provider,
            string name,
            string version,
            IDictionary<string, string> htmlAttributes)
        {
            // var include = new DnnJsInclude { ForceProvider = provider, Priority = priority, FilePath = filePath, Name = name, Version = version, };
            var include = new BasicFile(ClientDependencyType.Javascript)
            {
                FilePath = filePath,
                Priority = priority,

                // ForceProvider = provider,
                Group = Constants.DefaultGroup,
                Name = name,
                Version = version,
            };
            if (htmlAttributes != null)
            {
                foreach (var attribute in htmlAttributes)
                {
                    include.HtmlAttributes[attribute.Key] = attribute.Value;
                }
            }

            // include.HtmlAttributes["defer"] = "defer";
            include.HtmlAttributes["nonce"] = HttpContext.Current.Items["CSP-NONCE"].ToString();
            var loader = page.GetLoader();
            loader.RegisterDependency(include, include.HtmlAttributes);

            // page.FindControl("ClientResourceIncludes")?.Controls.Add(include);
        }

        /// <summary>Requests that a CSS file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        public static void RegisterStyleSheet(ControllerContext page, string filePath)
        {
            RegisterStyleSheet(page, filePath, Constants.DefaultPriority, DefaultCssProvider, htmlAttributes: null);
        }

        /// <summary>Requests that a CSS file be registered on the client browser.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>link</c> tag. The key being the attribute name and the value its value.</param>
        public static void RegisterStyleSheet(ControllerContext page, string filePath, IDictionary<string, string> htmlAttributes)
        {
            RegisterStyleSheet(page, filePath, Constants.DefaultPriority, DefaultCssProvider, htmlAttributes);
        }

        /// <summary>Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public static void RegisterStyleSheet(ControllerContext page, string filePath, int priority)
        {
            RegisterStyleSheet(page, filePath, priority, DefaultCssProvider, htmlAttributes: null);
        }

        /// <summary>Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>link</c> tag. The key being the attribute name and the value its value.</param>
        public static void RegisterStyleSheet(ControllerContext page, string filePath, int priority, IDictionary<string, string> htmlAttributes)
        {
            RegisterStyleSheet(page, filePath, priority, DefaultCssProvider, htmlAttributes);
        }

        /// <summary>Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public static void RegisterStyleSheet(ControllerContext page, string filePath, FileOrder.Css priority)
        {
            RegisterStyleSheet(page, filePath, (int)priority, DefaultCssProvider, htmlAttributes: null);
        }

        /// <summary>Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>link</c> tag. The key being the attribute name and the value its value.</param>
        public static void RegisterStyleSheet(ControllerContext page, string filePath, FileOrder.Css priority, IDictionary<string, string> htmlAttributes)
        {
            RegisterStyleSheet(page, filePath, (int)priority, DefaultCssProvider, htmlAttributes);
        }

        /// <summary>Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The provider name to be used to render the css file on the page.</param>
        public static void RegisterStyleSheet(ControllerContext page, string filePath, int priority, string provider)
        {
            RegisterStyleSheet(page, filePath, priority, provider, string.Empty, string.Empty, htmlAttributes: null);
        }

        /// <summary>Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The provider name to be used to render the css file on the page.</param>
        /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>link</c> tag. The key being the attribute name and the value its value.</param>
        public static void RegisterStyleSheet(ControllerContext page, string filePath, int priority, string provider, IDictionary<string, string> htmlAttributes)
        {
            RegisterStyleSheet(page, filePath, priority, provider, string.Empty, string.Empty, htmlAttributes);
        }

        /// <summary>Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The provider name to be used to render the css file on the page.</param>
        /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
        /// <param name="version">Version number of framework.</param>
        public static void RegisterStyleSheet(ControllerContext page, string filePath, int priority, string provider, string name, string version)
        {
            RegisterStyleSheet(page, filePath, priority, provider, name, version, htmlAttributes: null);
        }

        /// <summary>Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.</summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The provider name to be used to render the css file on the page.</param>
        /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
        /// <param name="version">Version number of framework.</param>
        /// <param name="htmlAttributes">A dictionary of HTML attributes to use for the <c>link</c> tag. The key being the attribute name and the value its value.</param>
        public static void RegisterStyleSheet(ControllerContext page, string filePath, int priority, string provider, string name, string version, IDictionary<string, string> htmlAttributes)
        {
            var fileExists = false;

            // Some "legacy URLs" could be using their own query string versioning scheme (and we've forced them to use the new API through re-routing ControllerContextBase.RegisterStyleSheet
            // Ensure that physical CSS files with query strings have their query strings removed
            // Ignore absolute urls, they will not exist locally
            if (!Uri.TryCreate(filePath, UriKind.Absolute, out _) && filePath.Contains(".css?"))
            {
                var filePathSansQueryString = RemoveQueryString(filePath);
                if (File.Exists(HttpContext.Current.Server.MapPath(filePathSansQueryString)))
                {
                    fileExists = true;
                    filePath = filePathSansQueryString;
                }
            }
            else if (filePath.Contains("WebResource.axd"))
            {
                fileExists = true;
            }

            if (!fileExists && !FileExists(page, filePath))
            {
                return;
            }

            // var include = new DnnCssInclude { ForceProvider = provider, Priority = priority, FilePath = filePath, Name = name, Version = version };
            var include = new BasicFile(ClientDependencyType.Css)
            {
                FilePath = filePath,
                Priority = priority,

                // ForceProvider = provider,
                Group = Constants.DefaultGroup,
                Name = name,
                Version = version,
            };
            if (htmlAttributes != null)
            {
                foreach (var attribute in htmlAttributes)
                {
                    include.HtmlAttributes[attribute.Key] = attribute.Value;
                }
            }

            var loader = page.GetLoader();
            loader.RegisterDependency(include);

            // page.FindControl("ClientResourceIncludes")?.Controls.Add(include);
        }

        /// <summary>This is a utility method that can be called to update the version of the composite files.</summary>
        [DnnDeprecated(8, 0, 1, "This method is not required anymore. The CRM version is now managed in host settings and site settings", RemovalVersion = 10)]
        public static partial void UpdateVersion()
        {
        }

        /// <summary>Clear the default composite files so that it can be generated next time.</summary>
        public static void ClearCache()
        {
            var provider = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider;
            if (!(provider is CompositeFileProcessingProvider))
            {
                return;
            }

            try
            {
                var folder = provider.CompositeFilePath;
                if (!folder.Exists)
                {
                    return;
                }

                var files = folder.GetFiles("*.cd?");
                foreach (var file in files)
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>Clears the cache used for file existence.</summary>
        /// <param name="path">The path for the file.</param>
        public static void ClearFileExistsCache(string path)
        {
            LockFileExistsCache.EnterWriteLock();
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    FileExistsCache.Clear();
                }
                else
                {
                    FileExistsCache.Remove(path.ToLowerInvariant());
                }
            }
            finally
            {
                LockFileExistsCache.ExitWriteLock();
            }
        }

        /// <summary>Enables the async postback handler.</summary>
        public static void EnableAsyncPostBackHandler()
        {
            if (HttpContext.Current != null && !HttpContext.Current.Items.Contains("AsyncPostBackHandlerEnabled"))
            {
                HttpContext.Current.Items.Add("AsyncPostBackHandlerEnabled", true);
            }
        }

        private static bool FileExists(ControllerContext page, string filePath)
        {
            // remove query string for the file exists check, won't impact the absoluteness, so just do it either way.
            filePath = RemoveQueryString(filePath);
            var cacheKey = filePath.ToLowerInvariant();

            // cache css file paths
            if (!FileExistsCache.ContainsKey(cacheKey))
            {
                // apply lock after IF, locking is more expensive than worst case scenario (check disk twice)
                LockFileExistsCache.EnterWriteLock();
                try
                {
                    FileExistsCache[cacheKey] = IsAbsoluteUrl(filePath) || File.Exists(HttpContext.Current.Server.MapPath(filePath));
                }
                finally
                {
                    LockFileExistsCache.ExitWriteLock();
                }
            }

            // return if file exists from cache
            LockFileExistsCache.EnterReadLock();
            try
            {
                return FileExistsCache[cacheKey];
            }
            finally
            {
                LockFileExistsCache.ExitReadLock();
            }
        }

        private static bool IsAbsoluteUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        private static string RemoveQueryString(string filePath)
        {
            var queryStringPosition = filePath.IndexOf("?", StringComparison.Ordinal);
            return queryStringPosition != -1 ? filePath.Substring(0, queryStringPosition) : filePath;
        }
    }
}
