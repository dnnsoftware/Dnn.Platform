// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.IO;
using DotNetNuke.Data;

namespace DotNetNuke.Services.OutputCache.Providers
{
    /// <summary>
    /// FileResponseFilter implements the OutputCacheRepsonseFilter to capture the response into database.
    /// </summary>
    public class DatabaseResponseFilter : OutputCacheResponseFilter
    {
        internal DatabaseResponseFilter(int itemId, int maxVaryByCount, Stream filterChain, string cacheKey, TimeSpan cacheDuration) : base(filterChain, cacheKey, cacheDuration, maxVaryByCount)
        {
            if (maxVaryByCount > -1 && DataProvider.Instance().GetOutputCacheItemCount(itemId) >= maxVaryByCount)
            {
                HasErrored = true;
                return;
            }

            CaptureStream = new MemoryStream();
        }

        protected override void AddItemToCache(int itemId, string output)
        {
            DataProvider.Instance().AddOutputCacheItem(itemId, CacheKey, output, DateTime.UtcNow.Add(CacheDuration));
        }

        protected override void RemoveItemFromCache(int itemId)
        {
            DataProvider.Instance().RemoveOutputCacheItem(itemId);
        }
    }
}
