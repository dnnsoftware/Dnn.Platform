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
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Cache;

#endregion

namespace DotNetNuke.Services.ModuleCache
{
    public class MemoryProvider : ModuleCachingProvider
    {
        private const string cachePrefix = "ModuleCache:";

        private List<string> GetCacheKeys(int tabModuleId)
        {
            var keys = new List<string>();
            IDictionaryEnumerator CacheEnum = CachingProvider.Instance().GetEnumerator();
            while ((CacheEnum.MoveNext()))
            {
                if (CacheEnum.Key.ToString().StartsWith(string.Concat(cachePrefix, "|", tabModuleId.ToString(), "|")))
                {
                    keys.Add(CacheEnum.Key.ToString());
                }
            }
            return keys;
        }

        public override string GenerateCacheKey(int tabModuleId, SortedDictionary<string, string> varyBy)
        {
            var cacheKey = new StringBuilder();
            if (varyBy != null)
            {
                foreach (KeyValuePair<string, string> kvp in varyBy)
                {
                    cacheKey.Append(string.Concat(kvp.Key.ToLower(), "=", kvp.Value, "|"));
                }
            }
            return string.Concat(cachePrefix, "|", tabModuleId.ToString(), "|", cacheKey.ToString());
        }

        public override int GetItemCount(int tabModuleId)
        {
            return GetCacheKeys(tabModuleId).Count;
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
            //throw new NotSupportedException();
        }
    }
}