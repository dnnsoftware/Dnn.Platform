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
        public static Regex GetCachedRegex(string pattern, RegexOptions options = RegexOptions.None, int timeoutSeconds = 2)
        {
            Requires.NotNull("pattern", pattern);
            var key = pattern;
            if ((options & RegexOptions.IgnoreCase) != 0)
            {
                key = (options & RegexOptions.CultureInvariant) != 0
                    ? pattern.ToUpperInvariant()
                    : pattern.ToUpper();
            }

            // // should not allow for compiled dynamic regex object
            options &= ~RegexOptions.Compiled;
            key = string.Join(":", "REGEX_ITEM", options.ToString("X"), key.GetHashCode().ToString("X"));

            // limit timeout between 1 and 10 seconds
            if (timeoutSeconds < 1) timeoutSeconds = 1;
            else if (timeoutSeconds > 10) timeoutSeconds = 10;

            var cache = CachingProvider.Instance();
            var regex = cache.GetItem(key) as Regex;
            if (regex == null)
            {
                regex = new Regex(pattern, options & ~RegexOptions.Compiled, TimeSpan.FromSeconds(timeoutSeconds));
                cache.Insert(key, regex, (DNNCacheDependency)null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10), CacheItemPriority.BelowNormal, null);
            }

            return regex;
        }
    }
}