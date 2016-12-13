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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

using DotNetNuke.ComponentModel;

#endregion

namespace DotNetNuke.Services.OutputCache
{
    public abstract class OutputCachingProvider
    {
        #region "Protected Methods"

        protected string ByteArrayToString(byte[] arrInput)
        {
            int i = 0;
            var sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i <= arrInput.Length - 1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }

        protected string GenerateCacheKeyHash(int tabId, string cacheKey)
        {
            byte[] hash = Encoding.ASCII.GetBytes(cacheKey);
            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                hash = sha256.ComputeHash(hash);
                return string.Concat(tabId.ToString(), "_", ByteArrayToString(hash));
            }
        }

        protected void WriteStreamAsText(HttpContext context, Stream stream, long offset, long length)
        {
            if ((length < 0))
            {
                length = (stream.Length - offset);
            }

            if ((length > 0))
            {
                if ((offset > 0))
                {
                    stream.Seek(offset, SeekOrigin.Begin);
                }
                var buffer = new byte[Convert.ToInt32(length)];
                int count = stream.Read(buffer, 0, Convert.ToInt32(length));
                char[] output = Encoding.UTF8.GetChars(buffer, 0, count);
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.Output.Write(output);
            }
        }

        #endregion

        #region "Shared/Static Methods"

        public static Dictionary<string, OutputCachingProvider> GetProviderList()
        {
            return ComponentFactory.GetComponents<OutputCachingProvider>();
        }

        public static OutputCachingProvider Instance(string FriendlyName)
        {
            return ComponentFactory.GetComponent<OutputCachingProvider>(FriendlyName);
        }

        public static void RemoveItemFromAllProviders(int tabId)
        {
            foreach (KeyValuePair<string, OutputCachingProvider> kvp in GetProviderList())
            {
                kvp.Value.Remove(tabId);
            }
        }

        #endregion

        #region "Abstract Methods"

        public abstract int GetItemCount(int tabId);

        public abstract byte[] GetOutput(int tabId, string cacheKey);

        public abstract OutputCacheResponseFilter GetResponseFilter(int tabId, int maxVaryByCount, Stream responseFilter, string cacheKey, TimeSpan cacheDuration);

        public abstract void Remove(int tabId);

        public abstract void SetOutput(int tabId, string cacheKey, TimeSpan duration, byte[] output);

        public abstract bool StreamOutput(int tabId, string cacheKey, HttpContext context);

        #endregion

        #region "Virtual Methods"

        public virtual string GenerateCacheKey(int tabId, StringCollection includeVaryByKeys, StringCollection excludeVaryByKeys, SortedDictionary<string, string> varyBy)
        {
            var cacheKey = new StringBuilder();
            if (varyBy != null)
            {
                SortedDictionary<string, string>.Enumerator varyByParms = varyBy.GetEnumerator();
                while ((varyByParms.MoveNext()))
                {
                    string key = varyByParms.Current.Key.ToLower();
                    if (includeVaryByKeys.Contains(key) && !excludeVaryByKeys.Contains(key))
                    {
                        cacheKey.Append(string.Concat(key, "=", varyByParms.Current.Value, "|"));
                    }
                }
            }
            return GenerateCacheKeyHash(tabId, cacheKey.ToString());
        }

        public virtual void PurgeCache(int portalId)
        {
        }

        public virtual void PurgeExpiredItems(int portalId)
        {
        }

        #endregion

        #region "Obsolete Methods"

        [Obsolete("This method was deprecated in 5.2.1")]
        public virtual void PurgeCache()
        {
        }

        [Obsolete("This method was deprecated in 5.2.1")]
        public virtual void PurgeExpiredItems()
        {
        }

        #endregion
    }
}