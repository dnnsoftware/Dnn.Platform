// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ModuleCache
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Web.Caching;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Cache;

    public class MemoryProvider : ModuleCachingProvider
    {
        private const string CachePrefix = "ModuleCache:";

        /// <inheritdoc/>
        public override string GenerateCacheKey(int tabModuleId, SortedDictionary<string, string> varyBy)
        {
            var cacheKey = new StringBuilder();
            if (varyBy != null)
            {
                foreach (KeyValuePair<string, string> kvp in varyBy)
                {
                    cacheKey.Append($"{kvp.Key.ToLowerInvariant()}={kvp.Value}|");
                }
            }

            return $"{CachePrefix}|{tabModuleId.ToString(CultureInfo.InvariantCulture)}|{cacheKey}";
        }

        /// <inheritdoc/>
        public override int GetItemCount(int tabModuleId)
        {
            return GetCacheKeys(tabModuleId).Count;
        }

        /// <inheritdoc/>
        public override byte[] GetModule(int tabModuleId, string cacheKey)
        {
            return DataCache.GetCache<byte[]>(cacheKey);
        }

        /// <inheritdoc/>
        public override void PurgeCache(int portalId)
        {
            DataCache.ClearCache(CachePrefix);
        }

        /// <inheritdoc/>
        public override void Remove(int tabModuleId)
        {
            DataCache.ClearCache($"{CachePrefix}|{tabModuleId.ToString(CultureInfo.InvariantCulture)}");
        }

        /// <inheritdoc/>
        public override void SetModule(int tabModuleId, string cacheKey, TimeSpan duration, byte[] moduleOutput)
        {
            DNNCacheDependency dep = null;
            DataCache.SetCache(cacheKey, moduleOutput, dep, DateTime.UtcNow.Add(duration), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        /// <inheritdoc/>
        public override void PurgeExpiredItems(int portalId)
        {
            // throw new NotSupportedException();
        }

        private static List<string> GetCacheKeys(int tabModuleId)
        {
            var keys = new List<string>();
            IDictionaryEnumerator cacheEnum = CachingProvider.Instance().GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                if (cacheEnum.Key.ToString().StartsWith($"{CachePrefix}|{tabModuleId.ToString(CultureInfo.InvariantCulture)}|", StringComparison.Ordinal))
                {
                    keys.Add(cacheEnum.Key.ToString());
                }
            }

            return keys;
        }
    }
}
