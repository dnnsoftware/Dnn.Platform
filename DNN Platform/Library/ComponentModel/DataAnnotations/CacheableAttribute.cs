// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
