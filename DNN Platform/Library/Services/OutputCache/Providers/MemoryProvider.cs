#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace DotNetNuke.Services.OutputCache.Providers
{
    /// <summary>
    /// MemoryResponseFilter implements the OutputCachingProvider for memory storage.
    /// </summary>
    public class MemoryProvider : OutputCachingProvider
    {
        protected const string cachePrefix = "DNN_OUTPUT:";
        private static System.Web.Caching.Cache runtimeCache;

        #region Friend Properties

        internal static System.Web.Caching.Cache Cache
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

    	internal static string CachePrefix
    	{
    		get
    		{
    			return cachePrefix;
    		}
    	}

        #endregion

        #region Private Methods

        private string GetCacheKey(string CacheKey)
        {
            if (string.IsNullOrEmpty(CacheKey))
            {
                throw new ArgumentException("Argument cannot be null or an empty string", "CacheKey");
            }
            return string.Concat(cachePrefix, CacheKey);
        }

        #endregion

        #region Friend Methods

        internal static List<string> GetCacheKeys()
        {
            var keys = new List<string>();
            IDictionaryEnumerator CacheEnum = Cache.GetEnumerator();
            while (CacheEnum.MoveNext())
            {
                if (CacheEnum.Key.ToString().StartsWith(string.Concat(cachePrefix)))
                {
                    keys.Add(CacheEnum.Key.ToString());
                }
            }
            return keys;
        }

        internal static List<string> GetCacheKeys(int tabId)
        {
            var keys = new List<string>();
            IDictionaryEnumerator CacheEnum = Cache.GetEnumerator();
            while (CacheEnum.MoveNext())
            {
                if (CacheEnum.Key.ToString().StartsWith(string.Concat(cachePrefix, tabId.ToString(), "_")))
                {
                    keys.Add(CacheEnum.Key.ToString());
                }
            }
            return keys;
        }

        #endregion

        #region Abstract Method Implementation

        public override string GenerateCacheKey(int tabId, System.Collections.Specialized.StringCollection includeVaryByKeys, System.Collections.Specialized.StringCollection excludeVaryByKeys, SortedDictionary<string, string> varyBy)
        {
            return GetCacheKey(base.GenerateCacheKey(tabId, includeVaryByKeys, excludeVaryByKeys, varyBy));
        }

        public override int GetItemCount(int tabId)
        {
            return GetCacheKeys().Count();
        }

        public override byte[] GetOutput(int tabId, string cacheKey)
        {
            object output = Cache[cacheKey];
            if (output != null)
            {
                return (byte[]) output;
            }
            else
            {
                return null;
            }
        }

        public override OutputCacheResponseFilter GetResponseFilter(int tabId, int maxVaryByCount, Stream responseFilter, string cacheKey, TimeSpan cacheDuration)
        {
            return new MemoryResponseFilter(tabId, maxVaryByCount, responseFilter, cacheKey, cacheDuration);
        }

        public override void PurgeCache(int portalId)
        {
            foreach (string key in GetCacheKeys())
            {
                Cache.Remove(key);
            }
        }

        public override void PurgeExpiredItems(int portalId)
        {
            throw new NotSupportedException();
        }

        public override void Remove(int tabId)
        {
            foreach (string key in GetCacheKeys(tabId))
            {
                Cache.Remove(key);
            }
        }

        public override void SetOutput(int tabId, string cacheKey, TimeSpan duration, byte[] output)
        {
            Cache.Insert(cacheKey, output, null, DateTime.UtcNow.Add(duration), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        public override bool StreamOutput(int tabId, string cacheKey, HttpContext context)
        {
            if (Cache[cacheKey] == null)
            {
                return false;
            }

			context.Response.BinaryWrite(Encoding.Default.GetBytes(Cache[cacheKey].ToString()));
        	return true;
        }

        #endregion
    }
}