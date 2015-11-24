using System;
using System.Text.RegularExpressions;
using System.Web.Caching;
using DotNetNuke.Services.Cache;

namespace DotNetNuke.Common.Utilities
{
    public class RegexUtils
    {
        /// <summary>
        /// Creates and caches a Regex object for later use and retrieves it in a later call if it is cacehd
        /// </summary>
        public static Regex GetCahcedRegex(string pattern, RegexOptions options = RegexOptions.None)
        {
            var key = pattern;
            if ((options & RegexOptions.IgnoreCase) != 0)
            {
                key = (options & RegexOptions.CultureInvariant) != 0
                    ? pattern.ToUpperInvariant()
                    : pattern.ToUpper();
            }

            key = string.Join(":", "REGEX_ITEM", options.ToString("X"), key);

            var cache = CachingProvider.Instance();
            var regex = cache.GetItem(key) as Regex;
            if (regex == null)
            {
                regex = new Regex(pattern, options & ~RegexOptions.Compiled, TimeSpan.FromSeconds(1)); // should not compile a dynamic regex
                cache.Insert(key, regex, (DNNCacheDependency)null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(30), CacheItemPriority.BelowNormal, null);
            }

            return regex;
        }
    }
}