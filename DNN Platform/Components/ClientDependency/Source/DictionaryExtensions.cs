using System;
using System.Collections.Generic;
using System.Linq;

namespace ClientDependency.Core
{
    public static class DictionaryExtensions
    {

        /// <summary>
        /// Returns the dictionary as formatted html attributes for use in an html tag
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToHtmlAttributes(this IDictionary<string, string> d)
        {
            return string.Join(" ", d.Select(x => x.Key + "=\"" + x.Value + "\"").ToArray());
        }

        /// <summary>
        /// Determines if 2 dictionaries contain the exact same keys/values
        /// </summary>
        /// <param name="d"></param>
        /// <param name="compareTo"></param>
        /// <returns></returns>
        public static bool IsEqualTo(this IDictionary<string, string> d, IDictionary<string, string> compareTo)
        {
            if (d.Count != compareTo.Count)
                return false;

            foreach(var i in d)
            {
                if (!compareTo.ContainsKey(i.Key))
                    return false;
                if (!compareTo[i.Key].Equals(i.Value, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            return true;
        }

    }
}