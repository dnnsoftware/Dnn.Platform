// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System.Text;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        private static readonly Encoding Iso8859Encoding = Encoding.GetEncoding("iso-8859-8");

        public static string TrimToLength(this string source, int maxLength)
        {
            return source == null || source.Length <= maxLength
                ? source :
                source.Substring(0, maxLength);
        }

        public static string Append(this string stringValue, string stringToLink, string delimiter)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                return stringToLink;
            }

            if (string.IsNullOrEmpty(stringToLink))
            {
                return stringValue;
            }

            return stringValue + delimiter + stringToLink;
        }

        public static string ValueOrEmpty(this string stringValue)
        {
            return stringValue ?? string.Empty;
        }

        /// <summary>
        /// Takes a string and removes any diacritics.
        /// </summary>
        /// <param name="input">String to normalize.</param>
        /// <returns>String without diacritics.</returns>
        public static string NormalizeString(this string input)
        {
            return string.IsNullOrEmpty(input)
                ? input
                : Iso8859Encoding.GetString(Encoding.Convert(Encoding.UTF8, Iso8859Encoding, Encoding.UTF8.GetBytes(input))).ToLowerInvariant();
        }

        /// <summary>
        /// Alternative to <see cref="string.Replace(string, string)"/> that supports case insensitive replacement.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns></returns>
        public static string ReplaceIgnoreCase(this string source, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldValue) || oldValue.Equals(newValue, System.StringComparison.OrdinalIgnoreCase))
            {
                return source;
            }

            return Regex.Replace(source, Regex.Escape(oldValue), newValue, RegexOptions.IgnoreCase);
        }
    }
}
