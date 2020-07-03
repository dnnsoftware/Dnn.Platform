// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.OutputCache
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    using DotNetNuke.ComponentModel;

    public abstract class OutputCachingProvider
    {
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

        public abstract int GetItemCount(int tabId);

        public abstract byte[] GetOutput(int tabId, string cacheKey);

        public abstract OutputCacheResponseFilter GetResponseFilter(int tabId, int maxVaryByCount, Stream responseFilter, string cacheKey, TimeSpan cacheDuration);

        public abstract void Remove(int tabId);

        public abstract void SetOutput(int tabId, string cacheKey, TimeSpan duration, byte[] output);

        public abstract bool StreamOutput(int tabId, string cacheKey, HttpContext context);

        public virtual string GenerateCacheKey(int tabId, StringCollection includeVaryByKeys, StringCollection excludeVaryByKeys, SortedDictionary<string, string> varyBy)
        {
            var cacheKey = new StringBuilder();
            if (varyBy != null)
            {
                SortedDictionary<string, string>.Enumerator varyByParms = varyBy.GetEnumerator();
                while (varyByParms.MoveNext())
                {
                    string key = varyByParms.Current.Key.ToLowerInvariant();
                    if (includeVaryByKeys.Contains(key) && !excludeVaryByKeys.Contains(key))
                    {
                        cacheKey.Append(string.Concat(key, "=", varyByParms.Current.Value, "|"));
                    }
                }
            }

            return this.GenerateCacheKeyHash(tabId, cacheKey.ToString());
        }

        public virtual void PurgeCache(int portalId)
        {
        }

        public virtual void PurgeExpiredItems(int portalId)
        {
        }

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
                return string.Concat(tabId.ToString(), "_", this.ByteArrayToString(hash));
            }
        }

        protected void WriteStreamAsText(HttpContext context, Stream stream, long offset, long length)
        {
            if (length < 0)
            {
                length = stream.Length - offset;
            }

            if (length > 0)
            {
                if (offset > 0)
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
    }
}
