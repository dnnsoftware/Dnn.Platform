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
using System.Web.Caching;

#endregion

namespace DotNetNuke.ComponentModel.DataAnnotations
{
    public class CacheableAttribute : Attribute
    {
        /// <summary>
        /// Construct a new CacheableAttribute
        /// </summary>
        public CacheableAttribute()
        {
            CachePriority = CacheItemPriority.Default;
            CacheTimeOut = 20;
        }

        /// <summary>
        /// Construct a new CacheableAttribute
        /// </summary>
        /// <param name="cacheKey">The cacheKey to use</param>
        public CacheableAttribute(string cacheKey) : this(cacheKey, CacheItemPriority.Default, 20) {}

        /// <summary>
        /// Construct a new CacheableAttribute
        /// </summary>
        /// <param name="cacheKey">The cacheKey to use</param>
        /// <param name="priority">The priority of the cached item</param>
        public CacheableAttribute(string cacheKey, CacheItemPriority priority) : this(cacheKey, priority, 20) { }

        /// <summary>
        /// Construct a new CacheableAttribute
        /// </summary>
        /// <param name="cacheKey">The cacheKey to use</param>
        /// <param name="priority">The priority of the cached item</param>
        /// <param name="timeOut">The timeout multiplier used to cache the item</param>
        public CacheableAttribute(string cacheKey, CacheItemPriority priority, int timeOut)
        {
            CacheKey = cacheKey;
            CachePriority = priority;
            CacheTimeOut = timeOut;
        }

        /// <summary>
        /// The root key to use for the cache
        /// </summary>
        public string CacheKey { get; set; }

        /// <summary>
        /// The priority of the cached item.  The default value is CacheItemPriority.Default
        /// </summary>
        public CacheItemPriority CachePriority { get; set; }

        /// <summary>
        /// The timeout multiplier used to cache the item. This value is multiple by the Host 
        /// Performance Setting to determine the actual timeout value. The default value is 20. 
        /// </summary>
        public int CacheTimeOut { get; set; }
    }
}