#region Usings

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Compilation;

using DotNetNuke.Framework.Providers;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;

#endregion

namespace DotNetNuke.ComponentModel
{
    public class ProviderInstaller : IComponentInstaller
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ProviderInstaller));
        private readonly ComponentLifeStyleType _ComponentLifeStyle;
        private readonly Type _ProviderInterface;
        private readonly string _ProviderType;
        private Type _defaultProvider;

        public ProviderInstaller(string providerType, Type providerInterface)
        {
            _ComponentLifeStyle = ComponentLifeStyleType.Singleton;
            _ProviderType = providerType;
            _ProviderInterface = providerInterface;
        }

        public ProviderInstaller(string providerType, Type providerInterface, Type defaultProvider)
        {
            _ComponentLifeStyle = ComponentLifeStyleType.Singleton;
            _ProviderType = providerType;
            _ProviderInterface = providerInterface;
            _defaultProvider = defaultProvider;
        }

        public ProviderInstaller(string providerType, Type providerInterface, ComponentLifeStyleType lifeStyle)
        {
            _ComponentLifeStyle = lifeStyle;
            _ProviderType = providerType;
            _ProviderInterface = providerInterface;
        }

        #region IComponentInstaller Members

        public void InstallComponents(IContainer container)
        {
            ProviderConfiguration config = ProviderConfiguration.GetProviderConfiguration(_ProviderType);
            //Register the default provider first (so it is the first component registered for its service interface
			if (config != null)
            {
                InstallProvider(container, (Provider) config.Providers[config.DefaultProvider]);

                //Register the others
                foreach (Provider provider in config.Providers.Values)
                {
					//Skip the default because it was registered above
                    if (!config.DefaultProvider.Equals(provider.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        InstallProvider(container, provider);
                    }
                }
            }
        }

        #endregion

        private void InstallProvider(IContainer container, Provider provider)
        {
            if (provider != null)
            {
                Type type = null;

                //Get the provider type
                try
                {
                    type = BuildManager.GetType(provider.Type, false, true);
                 }
                catch (TypeLoadException)
                {
                    if (_defaultProvider != null)
                    {
                        type = _defaultProvider;
                    }
                }

                if (type == null)
                {
                    Logger.Error(new ConfigurationErrorsException(string.Format("Could not load provider {0}", provider.Type)));
                }
                else
                {
                    //Register the component
                    container.RegisterComponent(provider.Name, _ProviderInterface, type, _ComponentLifeStyle);

                    //Load the settings into a dictionary
                    var settingsDict = new Dictionary<string, string> { { "providerName", provider.Name } };
                    foreach (string key in provider.Attributes.Keys)
                    {
                        settingsDict.Add(key, provider.Attributes.Get(key));
                    }
                    //Register the settings as dependencies
                    container.RegisterComponentSettings(type.FullName, settingsDict);
                }
            }
        }
    }
}
