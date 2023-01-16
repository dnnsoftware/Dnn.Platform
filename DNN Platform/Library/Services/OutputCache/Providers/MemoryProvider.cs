// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.OutputCache.Providers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Caching;

    /// <summary>MemoryResponseFilter implements the OutputCachingProvider for memory storage.</summary>
    public class MemoryProvider : OutputCachingProvider
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1303:ConstFieldNamesMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        protected const string cachePrefix = "DNN_OUTPUT:";

        private static System.Web.Caching.Cache runtimeCache;

        internal static System.Web.Caching.Cache Cache
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

        internal static string CachePrefix
        {
            get
            {
                return cachePrefix;
            }
        }

        /// <inheritdoc/>
        public override string GenerateCacheKey(int tabId, System.Collections.Specialized.StringCollection includeVaryByKeys, System.Collections.Specialized.StringCollection excludeVaryByKeys, SortedDictionary<string, string> varyBy)
        {
            return this.GetCacheKey(base.GenerateCacheKey(tabId, includeVaryByKeys, excludeVaryByKeys, varyBy));
        }

        /// <inheritdoc/>
        public override int GetItemCount(int tabId)
        {
            return GetCacheKeys(tabId).Count();
        }

        /// <inheritdoc/>
        public override byte[] GetOutput(int tabId, string cacheKey)
        {
            object output = Cache[cacheKey];
            if (output != null)
            {
                return (byte[])output;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public override OutputCacheResponseFilter GetResponseFilter(int tabId, int maxVaryByCount, Stream responseFilter, string cacheKey, TimeSpan cacheDuration)
        {
            return new MemoryResponseFilter(tabId, maxVaryByCount, responseFilter, cacheKey, cacheDuration);
        }

        /// <inheritdoc/>
        public override void PurgeCache(int portalId)
        {
            foreach (string key in GetCacheKeys())
            {
                Cache.Remove(key);
            }
        }

        /// <inheritdoc/>
        public override void PurgeExpiredItems(int portalId)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Remove(int tabId)
        {
            foreach (string key in GetCacheKeys(tabId))
            {
                Cache.Remove(key);
            }
        }

        /// <inheritdoc/>
        public override void SetOutput(int tabId, string cacheKey, TimeSpan duration, byte[] output)
        {
            Cache.Insert(cacheKey, output, null, DateTime.UtcNow.Add(duration), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        /// <inheritdoc/>
        public override bool StreamOutput(int tabId, string cacheKey, HttpContext context)
        {
            if (Cache[cacheKey] == null)
            {
                return false;
            }

            context.Response.BinaryWrite(Encoding.Default.GetBytes(Cache[cacheKey].ToString()));
            return true;
        }

        internal static List<string> GetCacheKeys()
        {
            var keys = new List<string>();
            IDictionaryEnumerator cacheEnum = Cache.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                if (cacheEnum.Key.ToString().StartsWith(string.Concat(cachePrefix)))
                {
                    keys.Add(cacheEnum.Key.ToString());
                }
            }

            return keys;
        }

        internal static List<string> GetCacheKeys(int tabId)
        {
            var keys = new List<string>();
            IDictionaryEnumerator cacheEnum = Cache.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                if (cacheEnum.Key.ToString().StartsWith(string.Concat(cachePrefix, tabId.ToString(), "_")))
                {
                    keys.Add(cacheEnum.Key.ToString());
                }
            }

            return keys;
        }

        private string GetCacheKey(string cacheKey)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new ArgumentException("Argument cannot be null or an empty string", "CacheKey");
            }

            return string.Concat(cachePrefix, cacheKey);
        }
    }
}
