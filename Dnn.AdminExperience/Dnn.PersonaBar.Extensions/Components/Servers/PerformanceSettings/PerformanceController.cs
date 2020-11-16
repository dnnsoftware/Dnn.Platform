// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.PerformanceSettings
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Services.ModuleCache;
    using DotNetNuke.Services.OutputCache;

    public class PerformanceController
    {
        public object GetPageStatePersistenceOptions()
        {
            return new[]
            {
                new KeyValuePair<string, string>("Page", "P"),
                new KeyValuePair<string, string>("Memory", "M")
            };
        }

        public IEnumerable<KeyValuePair<string, string>> GetModuleCacheProviders()
        {
            return this.GetFilteredProviders(ModuleCachingProvider.GetProviderList(), "ModuleCachingProvider");
        }

        public IEnumerable<KeyValuePair<string, string>> GetPageCacheProviders()
        {
            return this.GetFilteredProviders(OutputCachingProvider.GetProviderList(), "OutputCachingProvider");
        }

        public IEnumerable<KeyValuePair<string, string>> GetCachingProviderOptions()
        {
            var providers = ProviderConfiguration.GetProviderConfiguration("caching").Providers;

            return (from object key in providers.Keys select new KeyValuePair<string, string>((string)key, (string)key)).ToList();
        }

        public object GetCacheSettingOptions()
        {
            return new[]
            {
                new KeyValuePair<string, int>("NoCaching", 0),
                new KeyValuePair<string, int>("LightCaching", 1),
                new KeyValuePair<string, int>("ModerateCaching", 3),
                new KeyValuePair<string, int>("HeavyCaching", 6)
            };
        }

        public object GetCacheabilityOptions()
        {
            return new[]
            {
                new KeyValuePair<string, string>("NoCache", "0"),
                new KeyValuePair<string, string>("Private", "1"),
                new KeyValuePair<string, string>("Public", "2"),
                new KeyValuePair<string, string>("Server", "3"),
                new KeyValuePair<string, string>("ServerAndNoCache", "4"),
                new KeyValuePair<string, string>("ServerAndPrivate", "5")
            };
        }

        public string GetCachingProvider()
        {
            return ProviderConfiguration.GetProviderConfiguration("caching").DefaultProvider;
        }

        private IEnumerable<KeyValuePair<string, string>> GetFilteredProviders<T>(Dictionary<string, T> providerList, string keyFilter)
        {
            var providers = from provider in providerList let filteredkey = provider.Key.Replace(keyFilter, string.Empty) select new KeyValuePair<string, string>(filteredkey, provider.Key);
            return providers;
        }
    }
}
