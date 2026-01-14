// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Utilities.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Services.Cache;

    using Moq;

    public class FakeCachingProvider : CachingProvider
    {
        private Dictionary<string, object> _dictionary;

        public FakeCachingProvider(Dictionary<string, object> dictionary)
            : base(Mock.Of<IHostSettings>())
        {
            this._dictionary = dictionary;
        }

        public override void Insert(string cacheKey, object itemToCache, DNNCacheDependency dependency, DateTime absoluteExpiration,
            TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            this._dictionary[cacheKey] = itemToCache;
        }

        public override object GetItem(string cacheKey)
        {
            return this._dictionary.ContainsKey(cacheKey) ? this._dictionary[cacheKey] : null;
        }
    }
}
