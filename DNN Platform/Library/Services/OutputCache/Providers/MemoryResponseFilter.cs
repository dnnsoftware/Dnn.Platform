// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.OutputCache.Providers;

using System;
using System.IO;
using System.Web;
using System.Web.Caching;

/// <summary>FileResponseFilter implements the OutputCacheRepsonseFilter to capture the response into memory.</summary>
public class MemoryResponseFilter : OutputCacheResponseFilter
{
    // Private _content As StringBuilder
    private static System.Web.Caching.Cache runtimeCache;

    /// <summary>Initializes a new instance of the <see cref="MemoryResponseFilter"/> class.</summary>
    /// <param name="itemId"></param>
    /// <param name="maxVaryByCount"></param>
    /// <param name="filterChain"></param>
    /// <param name="cacheKey"></param>
    /// <param name="cacheDuration"></param>
    internal MemoryResponseFilter(int itemId, int maxVaryByCount, Stream filterChain, string cacheKey, TimeSpan cacheDuration)
        : base(filterChain, cacheKey, cacheDuration, maxVaryByCount)
    {
        if (maxVaryByCount > -1 && Services.OutputCache.Providers.MemoryProvider.GetCacheKeys(itemId).Count >= maxVaryByCount)
        {
            this.HasErrored = true;
            return;
        }

        this.CaptureStream = new MemoryStream();
    }

    protected static System.Web.Caching.Cache Cache
    {
        get
        {
            // create singleton of the cache object
            if (runtimeCache == null)
            {
                runtimeCache = HttpRuntime.Cache;
            }

            return runtimeCache;
        }
    }

    /// <inheritdoc/>
    protected override void AddItemToCache(int itemId, string output)
    {
        Cache.Insert(this.CacheKey, output, null, DateTime.UtcNow.Add(this.CacheDuration), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
    }

    /// <inheritdoc/>
    protected override void RemoveItemFromCache(int itemId)
    {
        Cache.Remove(this.CacheKey);
    }
}
