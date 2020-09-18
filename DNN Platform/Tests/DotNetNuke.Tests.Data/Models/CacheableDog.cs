// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data.Models
{
    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Tests.Utilities;

    [Cacheable(Constants.CACHE_DogsKey, Constants.CACHE_Priority, Constants.CACHE_TimeOut)]
    [Scope(Constants.CACHE_ScopeAll)]
    public class CacheableDog
    {
        public int? Age { get; set; }

        public int ID { get; set; }

        public string Name { get; set; }
    }
}
