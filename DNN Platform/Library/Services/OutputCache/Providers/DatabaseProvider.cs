// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.OutputCache.Providers
{
    using System;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Web;

    using DotNetNuke.Data;

    /// <summary>
    /// DatabaseProvider implements the OutputCachingProvider for database storage.
    /// </summary>
    public class DatabaseProvider : OutputCachingProvider
    {
        public override int GetItemCount(int tabId)
        {
            return DataProvider.Instance().GetOutputCacheItemCount(tabId);
        }

        public override byte[] GetOutput(int tabId, string cacheKey)
        {
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().GetOutputCacheItem(cacheKey);
                if (dr == null)
                {
                    return null;
                }
                else
                {
                    if (!dr.Read())
                    {
                        return null;
                    }

                    return Encoding.UTF8.GetBytes(dr["Data"].ToString());
                }
            }
            finally
            {
                if (dr != null)
                {
                    dr.Close();
                }
            }
        }

        public override OutputCacheResponseFilter GetResponseFilter(int tabId, int maxVaryByCount, Stream responseFilter, string cacheKey, TimeSpan cacheDuration)
        {
            return new DatabaseResponseFilter(tabId, maxVaryByCount, responseFilter, cacheKey, cacheDuration);
        }

        public override void PurgeCache(int portalId)
        {
            DataProvider.Instance().PurgeOutputCache();
        }

        public override void PurgeExpiredItems(int portalId)
        {
            DataProvider.Instance().PurgeExpiredOutputCacheItems();
        }

        public override void Remove(int tabId)
        {
            DataProvider.Instance().RemoveOutputCacheItem(tabId);
        }

        public override void SetOutput(int tabId, string cacheKey, TimeSpan duration, byte[] output)
        {
            string data = Encoding.UTF8.GetString(output);
            DataProvider.Instance().AddOutputCacheItem(tabId, cacheKey, data, DateTime.UtcNow.Add(duration));
        }

        public override bool StreamOutput(int tabId, string cacheKey, HttpContext context)
        {
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().GetOutputCacheItem(cacheKey);
                if (dr == null)
                {
                    return false;
                }
                else
                {
                    if (!dr.Read())
                    {
                        return false;
                    }

                    var expireTime = Convert.ToDateTime(dr["Expiration"]);
                    if (expireTime < DateTime.UtcNow)
                    {
                        DataProvider.Instance().RemoveOutputCacheItem(tabId);
                        return false;
                    }

                    context.Response.BinaryWrite(Encoding.Default.GetBytes(dr["Data"].ToString()));
                    return true;
                }
            }
            finally
            {
                if (dr != null)
                {
                    dr.Close();
                }
            }
        }
    }
}
