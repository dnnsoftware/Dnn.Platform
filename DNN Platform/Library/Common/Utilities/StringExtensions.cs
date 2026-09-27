// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>Extension methods for <see cref="string"/>.</summary>
    public static class StringExtensions
    {
        private static readonly Encoding Iso8859Encoding = Encoding.GetEncoding("iso-8859-8");

        /// <summary>Returns a value indicating whether the specified <paramref name="value"/> occurs within the <paramref name="source"/>, using the specified comparison rules.</summary>
        /// <param name="source">The source <see cref="string"/>.</param>
        /// <param name="value">The value to seek within the <paramref name="source"/>.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="source"/> and <paramref name="value"/> are compared.</param>
        /// <returns><see langword="true"/> if the <paramref name="value"/> parameter occurs within this string, or if <paramref name="value"/> is the empty string (<c>""</c>); otherwise, <see langword="false"/>.</returns>
        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.IndexOf(value, comparisonType) > -1;
        }

        /// <summary>Trims <paramref name="source"/> to the specified <paramref name="maxLength"/>.</summary>
        /// <param name="source">The source value.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>The trimmed value.</returns>
        public static string TrimToLength(this string source, int maxLength)
        {
            return source == null || source.Length <= maxLength
                ? source :
                source.Substring(0, maxLength);
        }

        /// <summary>Appends <paramref name="stringToLink"/> to <paramref name="stringValue"/>, using <paramref name="delimiter"/> as a separator (but only if both are not empty).</summary>
        /// <param name="stringValue">The beginning value.</param>
        /// <param name="stringToLink">The value to append.</param>
        /// <param name="delimiter">A separator to use if both <paramref name="stringValue"/> and <paramref name="stringToLink"/> are not empty.</param>
        /// <returns>The combined <see cref="string"/>.</returns>
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

        /// <summary>Gets the <paramref name="stringValue"/> or <see cref="string.Empty"/> if <paramref name="stringValue"/> is <see langword="null"/>.</summary>
        /// <param name="stringValue">The value.</param>
        /// <returns>A <see cref="string"/> value that is not <see langword="null"/>.</returns>
        public static string ValueOrEmpty(this string stringValue)
        {
            return stringValue ?? string.Empty;
        }

        /// <summary>Takes a string and removes any diacritics.</summary>
        /// <param name="input">String to normalize.</param>
        /// <returns>String without diacritics.</returns>
        public static string NormalizeString(this string input)
        {
            return string.IsNullOrEmpty(input)
                ? input
                : Iso8859Encoding.GetString(Encoding.Convert(Encoding.UTF8, Iso8859Encoding, Encoding.UTF8.GetBytes(input))).ToLowerInvariant();
        }

        /// <summary>Alternative to <see cref="string.Replace(string, string)"/> that supports case-insensitive replacement.</summary>
        /// <param name="source">The source.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>The <paramref name="source"/> with <paramref name="oldValue"/> replaced by <paramref name="newValue"/>.</returns>
        public static string ReplaceIgnoreCase(this string source, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldValue) || oldValue.Equals(newValue, StringComparison.OrdinalIgnoreCase))
            {
                return source;
            }

            return Regex.Replace(source, Regex.Escape(oldValue), newValue, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Ensures that the <paramref name="source"/> string ends with the specified <paramref name="value"/>.
        /// If <paramref name="source"/> already ends with <paramref name="value"/> (using <see cref="StringComparison.Ordinal"/>),
        /// the original <paramref name="source"/> is returned unchanged; otherwise the <paramref name="value"/> is appended and the resulting string is returned.
        /// </summary>
        /// <param name="source">The source string to check. Cannot be <see langword="null"/>.</param>
        /// <param name="value">The string that <paramref name="source"/> must end with. Cannot be <see langword="null"/>.</param>
        /// <returns>
        /// The original <paramref name="source"/> if it already ends with <paramref name="value"/>; otherwise
        /// a new string consisting of <paramref name="source"/> followed by <paramref name="value"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Comparison is performed using <see cref="StringComparison.Ordinal"/> (case-sensitive).
        /// This method does not trim or alter the input strings other than appending <paramref name="value"/> when necessary.
        /// </remarks>
        public static string EnsureEndsWith(this string source, string value)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return source.EndsWith(value, StringComparison.Ordinal) ? source : source + value;
        }
    }
}
