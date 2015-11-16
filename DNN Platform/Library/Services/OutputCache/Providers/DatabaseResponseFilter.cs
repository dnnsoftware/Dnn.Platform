#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2015
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.IO;
using DotNetNuke.Data;

namespace DotNetNuke.Services.OutputCache.Providers
{
    // helper class to capture the response into memory

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