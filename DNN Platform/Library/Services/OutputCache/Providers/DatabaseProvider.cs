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
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using DotNetNuke.Data;

namespace DotNetNuke.Services.OutputCache.Providers
{
    /// <summary>
    /// DatabaseProvider implements the OutputCachingProvider for database storage.
    /// </summary>
    public class DatabaseProvider : OutputCachingProvider
    {
        #region Abstract Method Implementation

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
                    if (! (dr.Read()))
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
                    if (! (dr.Read()))
                    {
                        return false;
                    }

                	var expireTime = Convert.ToDateTime(dr["Expiration"]);
					if(expireTime < DateTime.UtcNow)
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

        #endregion
    }
}