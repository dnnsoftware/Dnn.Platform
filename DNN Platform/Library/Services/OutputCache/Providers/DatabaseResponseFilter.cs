﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.OutputCache.Providers
{
    using System;
    using System.IO;

    using DotNetNuke.Data;

    /// <summary>FileResponseFilter implements the OutputCacheRepsonseFilter to capture the response into database.</summary>
    public class DatabaseResponseFilter : OutputCacheResponseFilter
    {
        /// <summary>Initializes a new instance of the <see cref="DatabaseResponseFilter"/> class.</summary>
        /// <param name="itemId"></param>
        /// <param name="maxVaryByCount"></param>
        /// <param name="filterChain"></param>
        /// <param name="cacheKey"></param>
        /// <param name="cacheDuration"></param>
        internal DatabaseResponseFilter(int itemId, int maxVaryByCount, Stream filterChain, string cacheKey, TimeSpan cacheDuration)
            : base(filterChain, cacheKey, cacheDuration, maxVaryByCount)
        {
            if (maxVaryByCount > -1 && DataProvider.Instance().GetOutputCacheItemCount(itemId) >= maxVaryByCount)
            {
                this.HasErrored = true;
                return;
            }

            this.CaptureStream = new MemoryStream();
        }

        /// <inheritdoc/>
        protected override void AddItemToCache(int itemId, string output)
        {
            DataProvider.Instance().AddOutputCacheItem(itemId, this.CacheKey, output, DateTime.UtcNow.Add(this.CacheDuration));
        }

        /// <inheritdoc/>
        protected override void RemoveItemFromCache(int itemId)
        {
            DataProvider.Instance().RemoveOutputCacheItem(itemId);
        }
    }
}
