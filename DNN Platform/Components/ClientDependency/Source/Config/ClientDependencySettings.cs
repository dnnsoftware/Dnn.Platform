using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Configuration.Provider;
using System.Web;
using System.Configuration;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Logging;
using System.IO;

namespace ClientDependency.Core.Config
{
    public class ClientDependencySettings
    {
        /// <summary>
        /// used for singleton
        /// </summary>
        private static ClientDependencySettings _settings;
        private static readonly object Lock = new object();

        /// <summary>
        /// Default constructor, for use with a web context app
        /// </summary>
        internal ClientDependencySettings()
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException(
                    "HttpContext.Current must exist when using the empty constructor for ClientDependencySettings, otherwise use the alternative constructor");
            }

            LoadProviders((ClientDependencySection)ConfigurationManager.GetSection("clientDependency"), new HttpContextWrapper(HttpContext.Current));
            
        }

        internal ClientDependencySettings(FileInfo configFile, HttpContextBase ctx)
        {
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile.FullName };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            LoadProviders((ClientDependencySection)configuration.GetSection("clientDependency"), ctx);            
        }

        /// <summary>
        /// Singleton, used for web apps
        /// </summary>
        public static ClientDependencySettings Instance
        {
            get
            {
                if (_settings == null)
                {
                    lock(Lock)
                    {
                        //double check
                        if (_settings == null)
                        {
                            _settings = new ClientDependencySettings();
                        }
                    }
                }
                return _settings;
            }
        }

        /// <summary>
        /// The file extensions of Client Dependencies that are file based as opposed to request based.
        /// Any file that doesn't have the extensions listed here will be request based, request based is
        /// more overhead for the server to process.
        /// </summary>
        /// <example>
        /// A request based JavaScript file may be  a .ashx that dynamically creates JavaScript server side.
        /// </example>
        /// <remarks>
        /// If this is not explicitly set, then the extensions 'js' and 'css' are the defaults.
        /// </remarks>
        public List<string> FileBasedDependencyExtensionList { get; set; }
        
        public int Version { get; set; }

        public ILogger Logger { get; private set; }

        /// <summary>
        /// Returns the default MVC renderer
        /// </summary>
        public BaseRenderer DefaultMvcRenderer { get; private set; }

        /// <summary>
        /// Returns the MVC renderer provider collection
        /// </summary>
        public RendererCollection MvcRendererCollection { get; private set; }

        /// <summary>
        /// Returns the default file registration provider
        /// </summary>
        public WebFormsFileRegistrationProvider DefaultFileRegistrationProvider { get; private set; }

        /// <summary>
        /// Returns the file registration provider collection
        /// </summary>
        public FileRegistrationProviderCollection FileRegistrationProviderCollection { get; private set; }

        /// <summary>
        /// Returns the default composite file processing provider
        /// </summary>
        public BaseCompositeFileProcessingProvider DefaultCompositeFileProcessingProvider { get; private set; }

        /// <summary>
        /// Returns the composite file processing provider collection
        /// </summary>
        public CompositeFileProcessingProviderCollection CompositeFileProcessingProviderCollection { get; private set; }

        /// <summary>
        /// Returns the default file map provider
        /// </summary>
        public BaseFileMapProvider DefaultFileMapProvider { get; private set; }

        /// <summary>
        /// Returns the collection of file map providers
        /// </summary>
        public FileMapProviderCollection FileMapProviderCollection { get; private set; }

        public ClientDependencySection ConfigSection { get; private set; }
       
        public string CompositeFileHandlerPath { get; set; }
       
        internal void LoadProviders(ClientDependencySection section, HttpContextBase http)
        {
         
            ConfigSection = section;

            FileRegistrationProviderCollection = new FileRegistrationProviderCollection();
            CompositeFileProcessingProviderCollection = new CompositeFileProcessingProviderCollection();
            MvcRendererCollection = new RendererCollection();
            FileMapProviderCollection = new FileMapProviderCollection();

            // if there is no section found, then create one
            if (ConfigSection == null)
            {
                //create a new section with the default settings
                ConfigSection = new ClientDependencySection();                            
            }

            //need to check if it's an http path or a lambda path
            var path = ConfigSection.CompositeFileElement.CompositeFileHandlerPath;
            CompositeFileHandlerPath = path.StartsWith("~/")
                ? VirtualPathUtility.ToAbsolute(ConfigSection.CompositeFileElement.CompositeFileHandlerPath, http.Request.ApplicationPath)
                : ConfigSection.CompositeFileElement.CompositeFileHandlerPath;
            Version = ConfigSection.Version;
            FileBasedDependencyExtensionList = ConfigSection.FileBasedDependencyExtensionList.ToList();

            //load the providers from the config, if there isn't config sections then add default providers
            // and then load the defaults.
            
            LoadDefaultCompositeFileConfig(ConfigSection, http);

            DefaultCompositeFileProcessingProvider = CompositeFileProcessingProviderCollection[ConfigSection.CompositeFileElement.DefaultFileProcessingProvider];
            if (DefaultCompositeFileProcessingProvider == null)
                throw new ProviderException("Unable to load default composite file provider");

            LoadDefaultFileMapConfig(ConfigSection, http);

            DefaultFileMapProvider = FileMapProviderCollection[ConfigSection.CompositeFileElement.DefaultFileMapProvider];
            if (DefaultFileMapProvider == null)
                throw new ProviderException("Unable to load default file map provider");

            LoadDefaultMvcFileConfig(ConfigSection);

            DefaultMvcRenderer = MvcRendererCollection[ConfigSection.MvcElement.DefaultRenderer];
            if (DefaultMvcRenderer == null)
                throw new ProviderException("Unable to load default mvc renderer");

            LoadDefaultFileRegConfig(ConfigSection);

            DefaultFileRegistrationProvider = FileRegistrationProviderCollection[ConfigSection.FileRegistrationElement.DefaultProvider];
            if (DefaultFileRegistrationProvider == null)
                throw new ProviderException("Unable to load default file registration provider");

            if (string.IsNullOrEmpty(ConfigSection.LoggerType))
            {
                Logger = new NullLogger();
            }
            else
            {
                var t = Type.GetType(ConfigSection.LoggerType);
                if (!typeof(ILogger).IsAssignableFrom(t))
                {
                    throw new ArgumentException("The loggerType '" + ConfigSection.LoggerType + "' does not inherit from ClientDependency.Core.Logging.ILogger");
                }

                Logger = (ILogger)Activator.CreateInstance(t);
            }
                    
        }

        private void LoadDefaultFileRegConfig(ClientDependencySection section)
        {
            if (section.FileRegistrationElement.Providers.Count == 0)
            {
                //create new providers
                var php = new PageHeaderProvider();
                php.Initialize(PageHeaderProvider.DefaultName, null);
                FileRegistrationProviderCollection.Add(php);

                var csrp = new LazyLoadProvider();
                csrp.Initialize(LazyLoadProvider.DefaultName, null);
                FileRegistrationProviderCollection.Add(csrp);

                var lcp = new LoaderControlProvider();
                lcp.Initialize(LoaderControlProvider.DefaultName, null);
                FileRegistrationProviderCollection.Add(lcp);
            }
            else
            {
                ProvidersHelper.InstantiateProviders(section.FileRegistrationElement.Providers, FileRegistrationProviderCollection, typeof(BaseFileRegistrationProvider));
            }

        }

        private void LoadDefaultFileMapConfig(ClientDependencySection section, HttpContextBase http)
        {
            if (section.CompositeFileElement.FileMapProviders.Count == 0)
            {
                //if not specified, create default
                var fmp = new XmlFileMapper();
                fmp.Initialize(XmlFileMapper.DefaultName, null);
                fmp.Initialize(http);
                FileMapProviderCollection.Add(fmp);
            }
            else
            {
                ProvidersHelper.InstantiateProviders(section.CompositeFileElement.FileMapProviders, FileMapProviderCollection, typeof(BaseFileMapProvider));
                //since the BaseFileMapProvider is an IHttpProvider, we need to do the http init
                foreach (var p in FileMapProviderCollection.Cast<BaseFileMapProvider>())
                {
                    p.Initialize(http);
                }
            }

        }

        private void LoadDefaultCompositeFileConfig(ClientDependencySection section, HttpContextBase http)
        {
            if (section.CompositeFileElement.FileProcessingProviders.Count == 0)
            {
                var cfpp = new CompositeFileProcessingProvider();
                cfpp.Initialize(CompositeFileProcessingProvider.DefaultName, null);
                cfpp.Initialize(http);
                CompositeFileProcessingProviderCollection.Add(cfpp);
            }
            else
            {
                ProvidersHelper.InstantiateProviders(section.CompositeFileElement.FileProcessingProviders, CompositeFileProcessingProviderCollection, typeof(BaseCompositeFileProcessingProvider));
                //since the BaseCompositeFileProcessingProvider is an IHttpProvider, we need to do the http init
                foreach(var p in CompositeFileProcessingProviderCollection.Cast<BaseCompositeFileProcessingProvider>())
                {
                    p.Initialize(http);
                }
            }
            
        }

        private void LoadDefaultMvcFileConfig(ClientDependencySection section)
        {
            if (section.MvcElement.Renderers.Count == 0)
            {
                var mvc = new StandardRenderer();
                mvc.Initialize(StandardRenderer.DefaultName, null);
                MvcRendererCollection.Add(mvc);
            }
            else
            {
                ProvidersHelper.InstantiateProviders(section.MvcElement.Renderers, MvcRendererCollection, typeof(BaseRenderer));
            }

        }
    }
}

