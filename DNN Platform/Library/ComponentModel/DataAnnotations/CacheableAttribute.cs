// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel.DataAnnotations
{
    using System;
    using System.Web.Caching;

    public class CacheableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheableAttribute"/> class.
        /// Construct a new CacheableAttribute.
        /// </summary>
        public CacheableAttribute()
        {
            this.CachePriority = CacheItemPriority.Default;
            this.CacheTimeOut = 20;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheableAttribute"/> class.
        /// Construct a new CacheableAttribute.
        /// </summary>
        /// <param name="cacheKey">The cacheKey to use.</param>
        public CacheableAttribute(string cacheKey)
            : this(cacheKey, CacheItemPriority.Default, 20)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheableAttribute"/> class.
        /// Construct a new CacheableAttribute.
        /// </summary>
        /// <param name="cacheKey">The cacheKey to use.</param>
        /// <param name="priority">The priority of the cached item.</param>
        public CacheableAttribute(string cacheKey, CacheItemPriority priority)
            : this(cacheKey, priority, 20)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheableAttribute"/> class.
        /// Construct a new CacheableAttribute.
        /// </summary>
        /// <param name="cacheKey">The cacheKey to use.</param>
        /// <param name="priority">The priority of the cached item.</param>
        /// <param name="timeOut">The timeout multiplier used to cache the item.</param>
        public CacheableAttribute(string cacheKey, CacheItemPriority priority, int timeOut)
        {
            this.CacheKey = cacheKey;
            this.CachePriority = priority;
            this.CacheTimeOut = timeOut;
        }

        /// <summary>
        /// Gets or sets the root key to use for the cache.
        /// </summary>
        public string CacheKey { get; set; }

        /// <summary>
        /// Gets or sets the priority of the cached item.  The default value is CacheItemPriority.Default.
        /// </summary>
        public CacheItemPriority CachePriority { get; set; }

        /// <summary>
        /// Gets or sets the timeout multiplier used to cache the item. This value is multiple by the Host
        /// Performance Setting to determine the actual timeout value. The default value is 20.
        /// </summary>
        public int CacheTimeOut { get; set; }
    }
}
