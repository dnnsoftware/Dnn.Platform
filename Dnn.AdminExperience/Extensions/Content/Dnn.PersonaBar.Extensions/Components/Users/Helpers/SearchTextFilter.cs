using System.Text;
using System.Text.RegularExpressions;

namespace Dnn.PersonaBar.Users.Components.Helpers
{
    public class SearchTextFilter
    {

        public static string CleanWildcards(string searchText)
        {
            if (string.IsNullOrEmpty(searchText) || searchText.Equals("*") || searchText.Equals("%"))
            {
                return null;
            }
            var pattern = "";

            var prefixWildcard = searchText.StartsWith("%") || searchText.StartsWith("*");
            var suffixWildcard = searchText.EndsWith("%") || searchText.EndsWith("*");

            bool IN_STRING = true == prefixWildcard && true == suffixWildcard;
            bool PREFIX = true == prefixWildcard && false == suffixWildcard;
            bool SUFFIX = true == suffixWildcard && false == prefixWildcard;
            bool EXACT = false == suffixWildcard && false == prefixWildcard;

            if (EXACT)
            {
                pattern = searchText.Replace("*", "").Replace("%", "");
            }
            else
            {

                if (IN_STRING == true)
                {
                    pattern = GetInStringSearchPattern(searchText);
                }
                else if (PREFIX == true)
                {
                    pattern = GetPrefixSearchPattern(searchText);
                }
                else if (SUFFIX == true)
                {
                    pattern = GetSuffixSearchPattern(searchText);
                }
            }

            return pattern;
        }

        private static string GetInStringSearchPattern(string searchText)
        {
            var pattern = new StringBuilder();
            var regexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;
            var inStringRegex = "^(\\*|%)?([\\w\\-_\\s\\*\\%\\.\\@]+)(\\*|%)$";
            var regex = new Regex(inStringRegex, regexOptions);
            var matches = regex.Matches(searchText);

            if (matches.Count > 0)
            {
                var matchText = matches[0].Groups[2].Value;
                if (matchText != null && !string.IsNullOrEmpty(matchText))
                {
                    pattern.Append("%");
                    pattern.Append(matchText.Replace("*", "").Replace("%", ""));
                    pattern.Append("%");

                }
            }
            return pattern.ToString();
        }

        private static string GetPrefixSearchPattern(string searchText)
        {
            var regexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;
            var pattern = new StringBuilder();
            var prefixRegex = "^(\\*|%)?([\\w\\-_\\s\\*\\%\\.\\@]+)";
            var regex = new Regex(prefixRegex, regexOptions);
            var matches = regex.Matches(searchText);

            if (matches.Count > 0)
            {
                var matchText = matches[0].Groups[2].Value;
                if (matchText != null && !string.IsNullOrEmpty(matchText))
                {
                    pattern.Append("%");
                    pattern.Append(matchText.Replace("*", "").Replace("%", ""));
                }
            }
            return pattern.ToString();
        }

        private static string GetSuffixSearchPattern(string searchText)
        {
            var pattern = new StringBuilder();
            var regexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;
            var suffixRegex = "([\\w\\-_\\*\\s\\%\\.\\@]+)(\\*|%)$";
            var regex = new Regex(suffixRegex, regexOptions);
            var matches = regex.Matches(searchText);

            if (matches.Count > 0)
            {
                var matchText = matches[0].Groups[1].Value;
                if (matchText != null && !string.IsNullOrEmpty(matchText))
                {
                    pattern.Append(matchText.Replace("*", "").Replace("%", ""));
                    pattern.Append("%");
                }
            }
            return pattern.ToString();
        }
    }
}