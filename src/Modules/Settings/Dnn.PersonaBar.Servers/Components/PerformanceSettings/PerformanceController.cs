#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Services.ModuleCache;
using DotNetNuke.Services.OutputCache;

namespace Dnn.PersonaBar.Servers.Components.PerformanceSettings
{
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
            return GetFilteredProviders(ModuleCachingProvider.GetProviderList(), "ModuleCachingProvider");
        }

        public IEnumerable<KeyValuePair<string, string>> GetPageCacheProviders()
        {
            return GetFilteredProviders(OutputCachingProvider.GetProviderList(), "OutputCachingProvider");
        }

        public IEnumerable<KeyValuePair<string, string>> GetCachingProviderOptions()
        {
            var providers = ProviderConfiguration.GetProviderConfiguration("caching").Providers;

            return (from object key in providers.Keys select new KeyValuePair<string, string>((string) key, (string) key)).ToList();
        }

        public object GetCacheSettingOptions()
        {
            return new []
            {
                new KeyValuePair<string, int>("NoCaching", 0),
                new KeyValuePair<string, int>("LightCaching", 1),
                new KeyValuePair<string, int>("ModerateCaching", 3),
                new KeyValuePair<string, int>("HeavyCaching", 6)
            };
        }

        public object GetCacheabilityOptions()
        {
            return new []
            {
                new KeyValuePair<string, string>("NoCache", "0"),
                new KeyValuePair<string, string>("Private", "1"),
                new KeyValuePair<string, string>("Public", "2"),
                new KeyValuePair<string, string>("Server", "3"),
                new KeyValuePair<string, string>("ServerAndNoCache", "4"),
                new KeyValuePair<string, string>("ServerAndPrivate", "5")
            };
        }

        private IEnumerable<KeyValuePair<string, string>> GetFilteredProviders<T>(Dictionary<string, T> providerList, string keyFilter)
        {
            var providers = from provider in providerList let filteredkey = provider.Key.Replace(keyFilter, string.Empty) select new KeyValuePair<string, string> (filteredkey, provider.Key);
            return providers;
        }

        public string GetCachingProvider()
        {
            return ProviderConfiguration.GetProviderConfiguration("caching").DefaultProvider;
        }
    }
}