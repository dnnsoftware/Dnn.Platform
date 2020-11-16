// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.XPath;

    using ClientDependency.Core;
    using ClientDependency.Core.CompositeFiles.Providers;
    using ClientDependency.Core.Config;
    using DotNetNuke;
    using DotNetNuke.Instrumentation;

    /// <summary>
    /// Provides the ability to request that client resources (JavaScript and CSS) be loaded on the client browser.
    /// </summary>
    public class ClientResourceManager
    {
        internal const string DefaultCssProvider = "DnnPageHeaderProvider";
        internal const string DefaultJsProvider = "DnnBodyProvider";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ClientResourceManager));

        private static Dictionary<string, bool> _fileExistsCache = new Dictionary<string, bool>();
        private static ReaderWriterLockSlim _lockFileExistsCache = new ReaderWriterLockSlim();

        /// <summary>
        /// Adds the neccessary configuration to website root web.config to use the Client Depenedecny componenet.
        /// </summary>
        public static void AddConfiguration()
        {
            var configPath = HostingEnvironment.MapPath("~/web.config");
            if (!string.IsNullOrEmpty(configPath))
            {
                var xmlDoc = new XmlDocument { XmlResolver = null };
                xmlDoc.Load(configPath);
                XmlDocumentFragment xmlFrag;

                // Config Sections
                var sectionsConfig = xmlDoc.DocumentElement.SelectSingleNode("configSections");
                if (sectionsConfig != null)
                {
                    var clientDependencySectionConfig = sectionsConfig.SelectSingleNode("section[@name='clientDependency']");
                    if (clientDependencySectionConfig == null)
                    {
                        xmlFrag = xmlDoc.CreateDocumentFragment();
                        xmlFrag.InnerXml = "<section name=\"clientDependency\" type=\"ClientDependency.Core.Config.ClientDependencySection, ClientDependency.Core\" requirePermission=\"false\" />";
                        xmlDoc.DocumentElement.SelectSingleNode("configSections").AppendChild(xmlFrag);
                    }
                }

                // Module Config
                var systemWebServerModulesConfig = xmlDoc.DocumentElement.SelectSingleNode("system.webServer/modules");
                if (systemWebServerModulesConfig != null)
                {
                    var moduleConfig = systemWebServerModulesConfig.SelectSingleNode("add[@name=\"ClientDependencyModule\"]");
                    if (moduleConfig == null)
                    {
                        xmlFrag = xmlDoc.CreateDocumentFragment();
                        xmlFrag.InnerXml = "<add name=\"ClientDependencyModule\" type=\"ClientDependency.Core.Module.ClientDependencyModule, ClientDependency.Core\"  preCondition=\"managedHandler\" />";
                        xmlDoc.DocumentElement.SelectSingleNode("system.webServer/modules").AppendChild(xmlFrag);
                    }
                }

                // Handler Config
                var systemWebServerHandlersConfig = xmlDoc.DocumentElement.SelectSingleNode("system.webServer/handlers");
                if (systemWebServerHandlersConfig != null)
                {
                    var handlerConfig = systemWebServerHandlersConfig.SelectSingleNode("add[@name=\"ClientDependencyHandler\"]");
                    if (handlerConfig == null)
                    {
                        xmlFrag = xmlDoc.CreateDocumentFragment();
                        xmlFrag.InnerXml = "<add name=\"ClientDependencyHandler\" verb=\"*\" path=\"DependencyHandler.axd\" type=\"ClientDependency.Core.CompositeFiles.CompositeDependencyHandler, ClientDependency.Core\" preCondition=\"integratedMode\" />";
                        xmlDoc.DocumentElement.SelectSingleNode("system.webServer/handlers").AppendChild(xmlFrag);
                    }
                }

                // HttpModules Config
                var systemWebServerHttpModulesConfig = xmlDoc.DocumentElement.SelectSingleNode("system.web/httpModules");
                if (systemWebServerHttpModulesConfig != null)
                {
                    var httpModuleConfig = systemWebServerHttpModulesConfig.SelectSingleNode("add[@name=\"ClientDependencyModule\"]");
                    if (httpModuleConfig == null)
                    {
                        xmlFrag = xmlDoc.CreateDocumentFragment();
                        xmlFrag.InnerXml = "<add name=\"ClientDependencyModule\" type=\"ClientDependency.Core.Module.ClientDependencyModule, ClientDependency.Core\" />";
                        xmlDoc.DocumentElement.SelectSingleNode("system.web/httpModules").AppendChild(xmlFrag);
                    }
                }

                // HttpHandler Config
                var systemWebServerHttpHandlersConfig = xmlDoc.DocumentElement.SelectSingleNode("system.web/httpHandlers");
                if (systemWebServerHttpHandlersConfig != null)
                {
                    var httpHandlerConfig = systemWebServerHttpHandlersConfig.SelectSingleNode("add[@type=\"ClientDependency.Core.CompositeFiles.CompositeDependencyHandler, ClientDependency.Core\"]");
                    if (httpHandlerConfig == null)
                    {
                        xmlFrag = xmlDoc.CreateDocumentFragment();
                        xmlFrag.InnerXml = "<add verb=\"*\" path=\"DependencyHandler.axd\" type=\"ClientDependency.Core.CompositeFiles.CompositeDependencyHandler, ClientDependency.Core\" />";
                        xmlDoc.DocumentElement.SelectSingleNode("system.web/httpHandlers").AppendChild(xmlFrag);
                    }
                }

                // ClientDependency Config
                var clientDependencyConfig = xmlDoc.DocumentElement.SelectSingleNode("clientDependency");
                if (clientDependencyConfig == null)
                {
                    xmlFrag = xmlDoc.CreateDocumentFragment();
                    xmlFrag.InnerXml = @"<clientDependency version=""0"" fileDependencyExtensions="".js,.css"">
                                            <fileRegistration defaultProvider=""DnnPageHeaderProvider"">
                                              <providers>
                                                <add name=""DnnBodyProvider"" type=""DotNetNuke.Web.Client.Providers.DnnBodyProvider, DotNetNuke.Web.Client"" enableCompositeFiles=""false"" />
                                                <add name=""DnnPageHeaderProvider"" type=""DotNetNuke.Web.Client.Providers.DnnPageHeaderProvider, DotNetNuke.Web.Client"" enableCompositeFiles=""false"" />
                                                <add name=""DnnFormBottomProvider"" type=""DotNetNuke.Web.Client.Providers.DnnFormBottomProvider, DotNetNuke.Web.Client"" enableCompositeFiles=""false"" />
                                                <add name=""PageHeaderProvider"" type=""ClientDependency.Core.FileRegistration.Providers.PageHeaderProvider, ClientDependency.Core"" enableCompositeFiles=""false""/>
                                                <add name=""LazyLoadProvider"" type=""ClientDependency.Core.FileRegistration.Providers.LazyLoadProvider, ClientDependency.Core"" enableCompositeFiles=""false""/>
                                                <add name=""LoaderControlProvider"" type=""ClientDependency.Core.FileRegistration.Providers.LoaderControlProvider, ClientDependency.Core"" enableCompositeFiles=""false""/>
                                              </providers>
                                            </fileRegistration>
                                            <compositeFiles defaultFileProcessingProvider=""DnnCompositeFileProcessor"" compositeFileHandlerPath=""~/DependencyHandler.axd"">
                                              <fileProcessingProviders>
                                                <!-- For webfarms update the urlType attribute to Base64QueryStrings, default setting is MappedId -->
                                                <add name=""DnnCompositeFileProcessor"" type=""DotNetNuke.Web.Client.Providers.DnnCompositeFileProcessingProvider, DotNetNuke.Web.Client"" enableCssMinify=""false"" enableJsMinify=""true"" persistFiles=""true"" compositeFilePath=""~/App_Data/ClientDependency"" bundleDomains="""" urlType=""MappedId"" />
                                              </fileProcessingProviders>
                                            </compositeFiles>
                                          </clientDependency>";

                    xmlDoc.DocumentElement.AppendChild(xmlFrag);
                }

                // Save Config
                xmlDoc.Save(configPath);
            }
        }

        public static bool IsInstalled()
        {
            var configPath = HostingEnvironment.MapPath("~/web.config");
            var installed = false;

            if (!string.IsNullOrEmpty(configPath))
            {
                var xmlDoc = new XmlDocument { XmlResolver = null };
                xmlDoc.Load(configPath);

                // Config Sections
                var sectionsConfig = xmlDoc.DocumentElement.SelectSingleNode("configSections");
                if (sectionsConfig != null)
                {
                    var clientDependencySectionConfig = sectionsConfig.SelectSingleNode("section[@name='clientDependency']");
                    installed = clientDependencySectionConfig != null;
                }
            }

            return installed;
        }

        public static void RegisterAdminStylesheet(Page page, string filePath)
        {
            RegisterStyleSheet(page, filePath, FileOrder.Css.AdminCss);
        }

        public static void RegisterDefaultStylesheet(Page page, string filePath)
        {
            RegisterStyleSheet(page, filePath, (int)FileOrder.Css.DefaultCss, DefaultCssProvider, "dnndefault", "7.0.0");
        }

        public static void RegisterFeatureStylesheet(Page page, string filePath)
        {
            RegisterStyleSheet(page, filePath, FileOrder.Css.FeatureCss);
        }

        public static void RegisterIEStylesheet(Page page, string filePath)
        {
            var browser = HttpContext.Current.Request.Browser;
            if (browser.Browser == "Internet Explorer" || browser.Browser == "IE")
            {
                RegisterStyleSheet(page, filePath, FileOrder.Css.IeCss);
            }
        }

        /// <summary>
        /// Requests that a JavaScript file be registered on the client browser.
        /// </summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        public static void RegisterScript(Page page, string filePath)
        {
            RegisterScript(page, filePath, FileOrder.Js.DefaultPriority);
        }

        /// <summary>
        /// Requests that a JavaScript file be registered on the client browser.
        /// </summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public static void RegisterScript(Page page, string filePath, int priority)
        {
            RegisterScript(page, filePath, priority, DefaultJsProvider);
        }

        /// <summary>
        /// Requests that a JavaScript file be registered on the client browser.
        /// </summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public static void RegisterScript(Page page, string filePath, FileOrder.Js priority)
        {
            RegisterScript(page, filePath, (int)priority, DefaultJsProvider);
        }

        /// <summary>
        /// Requests that a JavaScript file be registered on the client browser.
        /// </summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        public static void RegisterScript(Page page, string filePath, FileOrder.Js priority, string provider)
        {
            RegisterScript(page, filePath, (int)priority, provider);
        }

        /// <summary>
        /// Requests that a JavaScript file be registered on the client browser.
        /// </summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        public static void RegisterScript(Page page, string filePath, int priority, string provider)
        {
            RegisterScript(page, filePath, priority, provider, string.Empty, string.Empty);
        }

        /// <summary>
        /// Requests that a JavaScript file be registered on the client browser.
        /// </summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
        /// <param name="version">Version nr of framework.</param>
        public static void RegisterScript(Page page, string filePath, int priority, string provider, string name, string version)
        {
            var include = new DnnJsInclude { ForceProvider = provider, Priority = priority, FilePath = filePath, Name = name, Version = version };
            var loader = page.FindControl("ClientResourceIncludes");
            if (loader != null)
            {
                loader.Controls.Add(include);
            }
        }

        /// <summary>
        /// Requests that a CSS file be registered on the client browser.
        /// </summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        public static void RegisterStyleSheet(Page page, string filePath)
        {
            RegisterStyleSheet(page, filePath, Constants.DefaultPriority, DefaultCssProvider);
        }

        /// <summary>
        /// Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.
        /// </summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public static void RegisterStyleSheet(Page page, string filePath, int priority)
        {
            RegisterStyleSheet(page, filePath, priority, DefaultCssProvider);
        }

        /// <summary>
        /// Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.
        /// </summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public static void RegisterStyleSheet(Page page, string filePath, FileOrder.Css priority)
        {
            RegisterStyleSheet(page, filePath, (int)priority, DefaultCssProvider);
        }

        /// <summary>
        /// Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.
        /// </summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The provider name to be used to render the css file on the page.</param>
        public static void RegisterStyleSheet(Page page, string filePath, int priority, string provider)
        {
            RegisterStyleSheet(page, filePath, priority, provider, string.Empty, string.Empty);
        }

        /// <summary>
        /// Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.
        /// </summary>
        /// <param name="page">The current page. Used to get a reference to the client resource loader.</param>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The provider name to be used to render the css file on the page.</param>
        /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
        /// <param name="version">Version nr of framework.</param>
        public static void RegisterStyleSheet(Page page, string filePath, int priority, string provider, string name, string version)
        {
            var fileExists = false;

            // Some "legacy URLs" could be using their own query string versioning scheme (and we've forced them to use the new API through re-routing PageBase.RegisterStyleSheet
            // Ensure that physical CSS files with query strings have their query strings removed
            if (filePath.Contains(".css?"))
            {
                var filePathSansQueryString = RemoveQueryString(filePath);
                if (File.Exists(page.Server.MapPath(filePathSansQueryString)))
                {
                    fileExists = true;
                    filePath = filePathSansQueryString;
                }
            }
            else if (filePath.Contains("WebResource.axd"))
            {
                fileExists = true;
            }

            if (fileExists || FileExists(page, filePath))
            {
                var include = new DnnCssInclude { ForceProvider = provider, Priority = priority, FilePath = filePath, Name = name, Version = version };
                var loader = page.FindControl("ClientResourceIncludes");

                if (loader != null)
                {
                    loader.Controls.Add(include);
                }
            }
        }

        /// <summary>
        /// This is a utility method that can be called to update the version of the composite files.
        /// </summary>
        [Obsolete("This method is not required anymore. The CRM vesion is now managed in host settings and site settings.. Scheduled removal in v11.0.0.")]
        public static void UpdateVersion()
        {
        }

        /// <summary>
        /// Clear the default compisite files so that it can be generated next time.
        /// </summary>
        public static void ClearCache()
        {
            var provider = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider;
            if (provider is CompositeFileProcessingProvider)
            {
                try
                {
                    var folder = provider.CompositeFilePath;
                    if (folder.Exists)
                    {
                        var files = folder.GetFiles("*.cd?");
                        foreach (var file in files)
                        {
                            file.Delete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        public static void ClearFileExistsCache(string path)
        {
            _lockFileExistsCache.EnterWriteLock();
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    _fileExistsCache.Clear();
                }
                else
                {
                    _fileExistsCache.Remove(path.ToLowerInvariant());
                }
            }
            finally
            {
                _lockFileExistsCache.ExitWriteLock();
            }
        }

        public static void EnableAsyncPostBackHandler()
        {
            if (HttpContext.Current != null && !HttpContext.Current.Items.Contains("AsyncPostBackHandlerEnabled"))
            {
                HttpContext.Current.Items.Add("AsyncPostBackHandlerEnabled", true);
            }
        }

        private static bool FileExists(Page page, string filePath)
        {
            // remove query string for the file exists check, won't impact the absoluteness, so just do it either way.
            filePath = RemoveQueryString(filePath);
            var cacheKey = filePath.ToLowerInvariant();

            // cache css file paths
            if (!_fileExistsCache.ContainsKey(cacheKey))
            {
                // appply lock after IF, locking is more expensive than worst case scenario (check disk twice)
                _lockFileExistsCache.EnterWriteLock();
                try
                {
                    _fileExistsCache[cacheKey] = IsAbsoluteUrl(filePath) || File.Exists(page.Server.MapPath(filePath));
                }
                finally
                {
                    _lockFileExistsCache.ExitWriteLock();
                }
            }

            // return if file exists from cache
            _lockFileExistsCache.EnterReadLock();
            try
            {
                return _fileExistsCache[cacheKey];
            }
            finally
            {
                _lockFileExistsCache.ExitReadLock();
            }
        }

        private static bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result);
        }

        private static string RemoveQueryString(string filePath)
        {
            var queryStringPosition = filePath.IndexOf("?", StringComparison.Ordinal);
            return queryStringPosition != -1 ? filePath.Substring(0, queryStringPosition) : filePath;
        }
    }
}
