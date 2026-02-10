// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Helpers
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    public class SearchTextFilter
    {
        public static string CleanWildcards(string searchText)
        {
            if (string.IsNullOrEmpty(searchText) || searchText.Equals("*", StringComparison.Ordinal) || searchText.Equals("%", StringComparison.Ordinal))
            {
                return null;
            }

            var pattern = string.Empty;

            var prefixWildcard = searchText.StartsWith("%", StringComparison.Ordinal) || searchText.StartsWith("*", StringComparison.Ordinal);
            var suffixWildcard = searchText.EndsWith("%", StringComparison.Ordinal) || searchText.EndsWith("*", StringComparison.Ordinal);

            bool inString = prefixWildcard && suffixWildcard;
            bool prefix = prefixWildcard && !suffixWildcard;
            bool suffix = suffixWildcard && !prefixWildcard;
            bool exact = !suffixWildcard && !prefixWildcard;

            if (exact)
            {
                pattern = searchText.Replace("*", string.Empty).Replace("%", string.Empty);
            }
            else if (inString)
            {
                pattern = GetInStringSearchPattern(searchText);
            }
            else if (prefix)
            {
                pattern = GetPrefixSearchPattern(searchText);
            }
            else
            {
                pattern = GetSuffixSearchPattern(searchText);
            }

            return pattern;
        }

        private static string GetInStringSearchPattern(string searchText)
        {
            var pattern = new StringBuilder();
            var regex = new Regex(@"^(\*|%)?([\w\-_\s\*\%\.\@]+)(\*|%)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var matches = regex.Matches(searchText);

            if (matches.Count > 0)
            {
                var matchText = matches[0].Groups[2].Value;
                if (matchText != null && !string.IsNullOrEmpty(matchText))
                {
                    pattern.Append('%');
                    pattern.Append(matchText.Replace("*", string.Empty).Replace("%", string.Empty));
                    pattern.Append('%');
                }
            }

            return pattern.ToString();
        }

        private static string GetPrefixSearchPattern(string searchText)
        {
            var pattern = new StringBuilder();
            var regex = new Regex(@"^(\*|%)?([\w\-_\s\*\%\.\@]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var matches = regex.Matches(searchText);

            if (matches.Count > 0)
            {
                var matchText = matches[0].Groups[2].Value;
                if (matchText != null && !string.IsNullOrEmpty(matchText))
                {
                    pattern.Append('%');
                    pattern.Append(matchText.Replace("*", string.Empty).Replace("%", string.Empty));
                }
            }

            return pattern.ToString();
        }

        private static string GetSuffixSearchPattern(string searchText)
        {
            var pattern = new StringBuilder();
            var regex = new Regex(@"([\w\-_\*\s\%\.\@]+)(\*|%)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var matches = regex.Matches(searchText);

            if (matches.Count > 0)
            {
                var matchText = matches[0].Groups[1].Value;
                if (matchText != null && !string.IsNullOrEmpty(matchText))
                {
                    pattern.Append(matchText.Replace("*", string.Empty).Replace("%", string.Empty));
                    pattern.Append('%');
                }
            }

            return pattern.ToString();
        }
    }
}
