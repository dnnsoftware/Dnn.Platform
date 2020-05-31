// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.IO;
using System.Web;
using System.Web.Caching;

namespace DotNetNuke.Services.OutputCache.Providers
{
    /// <summary>
    /// FileResponseFilter implements the OutputCacheRepsonseFilter to capture the response into memory.
    /// </summary>
    public class MemoryResponseFilter : OutputCacheResponseFilter
    {
        //Private _content As StringBuilder
        private static System.Web.Caching.Cache runtimeCache;

        internal MemoryResponseFilter(int itemId, int maxVaryByCount, Stream filterChain, string cacheKey, TimeSpan cacheDuration) : base(filterChain, cacheKey, cacheDuration, maxVaryByCount)
        {
            if (maxVaryByCount > -1 && Services.OutputCache.Providers.MemoryProvider.GetCacheKeys(itemId).Count >= maxVaryByCount)
            {
                HasErrored = true;
                return;
            }
            CaptureStream = new MemoryStream();
        }

        protected static System.Web.Caching.Cache Cache
        {
            get
            {
                //create singleton of the cache object
                if (runtimeCache == null)
                {
                    runtimeCache = HttpRuntime.Cache;
                }
                return runtimeCache;
            }
        }

        protected override void AddItemToCache(int itemId, string output)
        {
            Cache.Insert(CacheKey, output, null, DateTime.UtcNow.Add(CacheDuration), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        protected override void RemoveItemFromCache(int itemId)
        {
            Cache.Remove(CacheKey);
        }
    }
}
