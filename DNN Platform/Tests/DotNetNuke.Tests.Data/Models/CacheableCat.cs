using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Tests.Utilities;

namespace DotNetNuke.Tests.Data.Models
{
    [Cacheable(Constants.CACHE_CatsKey, Constants.CACHE_Priority, Constants.CACHE_TimeOut)]
    [Scope(Constants.CACHE_ScopeModule)]
    public class CacheableCat
    {
        public int? Age { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
    }
}
