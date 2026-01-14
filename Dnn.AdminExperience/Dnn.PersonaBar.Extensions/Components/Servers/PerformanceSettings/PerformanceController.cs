// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.PerformanceSettings
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Services.ModuleCache;
    using DotNetNuke.Services.OutputCache;

    public class PerformanceController
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public object GetPageStatePersistenceOptions()
        {
            return new[]
            {
                new KeyValuePair<string, string>("Page", "P"),
                new KeyValuePair<string, string>("Memory", "M"),
            };
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IEnumerable<KeyValuePair<string, string>> GetModuleCacheProviders()
        {
            return GetFilteredProviders(ModuleCachingProvider.GetProviderList(), "ModuleCachingProvider");
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IEnumerable<KeyValuePair<string, string>> GetPageCacheProviders()
        {
            return GetFilteredProviders(OutputCachingProvider.GetProviderList(), "OutputCachingProvider");
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IEnumerable<KeyValuePair<string, string>> GetCachingProviderOptions()
        {
            var providers = ProviderConfiguration.GetProviderConfiguration("caching").Providers;

            return (from object key in providers.Keys select new KeyValuePair<string, string>((string)key, (string)key)).ToList();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public object GetCacheSettingOptions()
        {
            return new[]
            {
                new KeyValuePair<string, int>("NoCaching", 0),
                new KeyValuePair<string, int>("LightCaching", 1),
                new KeyValuePair<string, int>("ModerateCaching", 3),
                new KeyValuePair<string, int>("HeavyCaching", 6),
            };
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public object GetCacheabilityOptions()
        {
            return new[]
            {
                new KeyValuePair<string, int>("NoCache", 1),
                new KeyValuePair<string, int>("Private", 2),
                new KeyValuePair<string, int>("ServerAndNoCache", 3),
                new KeyValuePair<string, int>("Public", 4),
                new KeyValuePair<string, int>("ServerAndPrivate", 5),
            };
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string GetCachingProvider()
        {
            return ProviderConfiguration.GetProviderConfiguration("caching").DefaultProvider;
        }

        private static IEnumerable<KeyValuePair<string, string>> GetFilteredProviders<T>(Dictionary<string, T> providerList, string keyFilter)
        {
            return
                from provider in providerList
                let filteredKey = provider.Key.Replace(keyFilter, string.Empty)
                select new KeyValuePair<string, string>(filteredKey, provider.Key);
        }
    }
}
