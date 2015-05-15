using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using DotNetNuke.Services.Cache;

namespace DotNetNuke.Tests.Utilities.Fakes
{
    public class FakeCachingProvider : CachingProvider
    {
        private Dictionary<string, object> _dictionary;

        public FakeCachingProvider(Dictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public override void Insert(string cacheKey, object itemToCache, DNNCacheDependency dependency, DateTime absoluteExpiration,
            TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            _dictionary[cacheKey] = itemToCache;
        }

        public override object GetItem(string cacheKey)
        {
            return _dictionary.ContainsKey(cacheKey) ? _dictionary[cacheKey] : null;
        }
    }
}
