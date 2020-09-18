// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Web.Compilation;

    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;

    public class ProviderInstaller : IComponentInstaller
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ProviderInstaller));
        private readonly ComponentLifeStyleType _ComponentLifeStyle;
        private readonly Type _ProviderInterface;
        private readonly string _ProviderType;
        private Type _defaultProvider;

        public ProviderInstaller(string providerType, Type providerInterface)
        {
            this._ComponentLifeStyle = ComponentLifeStyleType.Singleton;
            this._ProviderType = providerType;
            this._ProviderInterface = providerInterface;
        }

        public ProviderInstaller(string providerType, Type providerInterface, Type defaultProvider)
        {
            this._ComponentLifeStyle = ComponentLifeStyleType.Singleton;
            this._ProviderType = providerType;
            this._ProviderInterface = providerInterface;
            this._defaultProvider = defaultProvider;
        }

        public ProviderInstaller(string providerType, Type providerInterface, ComponentLifeStyleType lifeStyle)
        {
            this._ComponentLifeStyle = lifeStyle;
            this._ProviderType = providerType;
            this._ProviderInterface = providerInterface;
        }

        public void InstallComponents(IContainer container)
        {
            ProviderConfiguration config = ProviderConfiguration.GetProviderConfiguration(this._ProviderType);

            // Register the default provider first (so it is the first component registered for its service interface
            if (config != null)
            {
                this.InstallProvider(container, (Provider)config.Providers[config.DefaultProvider]);

                // Register the others
                foreach (Provider provider in config.Providers.Values)
                {
                    // Skip the default because it was registered above
                    if (!config.DefaultProvider.Equals(provider.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        this.InstallProvider(container, provider);
                    }
                }
            }
        }

        private void InstallProvider(IContainer container, Provider provider)
        {
            if (provider != null)
            {
                Type type = null;

                // Get the provider type
                try
                {
                    type = BuildManager.GetType(provider.Type, false, true);
                }
                catch (TypeLoadException)
                {
                    if (this._defaultProvider != null)
                    {
                        type = this._defaultProvider;
                    }
                }

                if (type == null)
                {
                    Logger.Error(new ConfigurationErrorsException(string.Format("Could not load provider {0}", provider.Type)));
                }
                else
                {
                    // Register the component
                    container.RegisterComponent(provider.Name, this._ProviderInterface, type, this._ComponentLifeStyle);

                    // Load the settings into a dictionary
                    var settingsDict = new Dictionary<string, string> { { "providerName", provider.Name } };
                    foreach (string key in provider.Attributes.Keys)
                    {
                        settingsDict.Add(key, provider.Attributes.Get(key));
                    }

                    // Register the settings as dependencies
                    container.RegisterComponentSettings(type.FullName, settingsDict);
                }
            }
        }
    }
}
