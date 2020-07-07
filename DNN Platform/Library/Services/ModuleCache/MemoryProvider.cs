// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ModuleCache
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Web.Caching;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Cache;

    public class MemoryProvider : ModuleCachingProvider
    {
        private const string cachePrefix = "ModuleCache:";

        public override string GenerateCacheKey(int tabModuleId, SortedDictionary<string, string> varyBy)
        {
            var cacheKey = new StringBuilder();
            if (varyBy != null)
            {
                foreach (KeyValuePair<string, string> kvp in varyBy)
                {
                    cacheKey.Append(string.Concat(kvp.Key.ToLowerInvariant(), "=", kvp.Value, "|"));
                }
            }

            return string.Concat(cachePrefix, "|", tabModuleId.ToString(), "|", cacheKey.ToString());
        }

        public override int GetItemCount(int tabModuleId)
        {
            return this.GetCacheKeys(tabModuleId).Count;
        }

        public override byte[] GetModule(int tabModuleId, string cacheKey)
        {
            return DataCache.GetCache<byte[]>(cacheKey);
        }

        public override void PurgeCache(int portalId)
        {
            DataCache.ClearCache(cachePrefix);
        }

        public override void Remove(int tabModuleId)
        {
            DataCache.ClearCache(string.Concat(cachePrefix, "|", tabModuleId.ToString()));
        }

        public override void SetModule(int tabModuleId, string cacheKey, TimeSpan duration, byte[] moduleOutput)
        {
            DNNCacheDependency dep = null;
            DataCache.SetCache(cacheKey, moduleOutput, dep, DateTime.UtcNow.Add(duration), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        public override void PurgeExpiredItems(int portalId)
        {
            // throw new NotSupportedException();
        }

        private List<string> GetCacheKeys(int tabModuleId)
        {
            var keys = new List<string>();
            IDictionaryEnumerator CacheEnum = CachingProvider.Instance().GetEnumerator();
            while (CacheEnum.MoveNext())
            {
                if (CacheEnum.Key.ToString().StartsWith(string.Concat(cachePrefix, "|", tabModuleId.ToString(), "|")))
                {
                    keys.Add(CacheEnum.Key.ToString());
                }
            }

            return keys;
        }
    }
}
