using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Tests.Utilities;

namespace DotNetNuke.Tests.Data.Models
{
    [Cacheable(Constants.CACHE_DogsKey, Constants.CACHE_Priority, Constants.CACHE_TimeOut)]
    [Scope(Constants.CACHE_ScopeAll)]
    public class CacheableDog
    {
        public int? Age { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
