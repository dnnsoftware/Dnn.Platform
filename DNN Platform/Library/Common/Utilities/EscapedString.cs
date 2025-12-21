// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public static class EscapedString
    {
        private const char EscapeSequence = '\\';
        private const string DoubleEscapeSequence = @"\\";
        private const char DefaultSeparator = ',';

        /// <summary>Combine the string values of the enumerable into an escaped string.</summary>
        /// <param name="enumerable">An IEnumerable of values to combine.</param>
        /// <returns>An escaped string that is separated using the specified character.  The escape character is <c>'\'</c>.
        /// The string returned by <see cref="object.ToString"/> is used as the value of each item in the <paramref name="enumerable"/>.</returns>
        /// <remarks>The separator char is <c>','</c>.</remarks>
        public static string Combine(IEnumerable enumerable)
        {
            return Combine(enumerable, DefaultSeparator);
        }

        /// <summary>Combine the string values of the enumerable into an escaped string.</summary>
        /// <param name="enumerable">An IEnumerable of values to combine.</param>
        /// <param name="seperator">The character to use as a separator.</param>
        /// <returns>An escaped string that is separated using the specified character.  The escape character is <c>'\'</c>.
        /// The string returned by <see cref="object.ToString"/> is used as the value of each item in the <paramref name="enumerable"/>.</returns>
        public static string Combine(IEnumerable enumerable, char seperator)
        {
            string result = string.Empty;

            foreach (var item in enumerable)
            {
                var s = item.ToString();
                s = s.Replace(EscapeSequence.ToString(), EscapeSequence.ToString() + EscapeSequence);
                s = s.Replace(seperator.ToString(), EscapeSequence.ToString() + seperator);
                result += s + seperator;
            }

            return string.IsNullOrEmpty(result) ? string.Empty : result.Substring(0, result.Length - 1);
        }

        /// <summary>Takes an escaped string and splits it into an IEnumerable of separate strings.</summary>
        /// <param name="combinedString">The string to separate.</param>
        /// <returns>IEnumerable of all the separated strings.</returns>
        /// <remarks>The escape character is <c>'\'</c>, the separator char is <c>','</c>.</remarks>
        public static IEnumerable<string> Seperate(string combinedString)
        {
            return Seperate(combinedString, DefaultSeparator);
        }

        /// <summary>Takes an escaped string and splits it into an IEnumerable of separate strings.</summary>
        /// <param name="combinedString">The string to separate.</param>
        /// <param name="trimWhitespaces">Trims whitespaces.</param>
        /// <returns>IEnumerable of all the separated strings.</returns>
        /// <remarks>The escape character is <c>'\'</c>, the separator char is <c>','</c>.</remarks>
        public static IEnumerable<string> Seperate(string combinedString, bool trimWhitespaces)
        {
            return Seperate(combinedString, DefaultSeparator, trimWhitespaces);
        }

        /// <summary>Takes an escaped string and splits it into an IEnumerable of separate strings.</summary>
        /// <param name="combinedString">The string to separate.</param>
        /// <param name="separator">The character on which to split.</param>
        /// <returns>IEnumerable of all the separated strings.</returns>
        /// <remarks>The escape character is <c>'\'</c>, the separator char is <c>','</c>.</remarks>
        public static IEnumerable<string> Seperate(string combinedString, char separator)
        {
            return Seperate(combinedString, separator, false);
        }

        /// <summary>Takes an escaped string and splits it into an IEnumerable of separate strings.</summary>
        /// <param name="combinedString">The string to separate.</param>
        /// <param name="seperator">The character on which to split.</param>
        /// <param name="trimWhitespaces">Trims whitespaces.</param>
        /// <returns>IEnumerable of all the separated strings.</returns>
        /// <remarks>The escape character is <c>'\'</c>.</remarks>
        public static IEnumerable<string> Seperate(string combinedString, char seperator, bool trimWhitespaces)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(combinedString))
            {
                return result;
            }

            var segments = combinedString.Split(new[] { seperator });

            for (int i = 0; i < segments.Length; i++)
            {
                var current = trimWhitespaces ? segments[i].Trim() : segments[i];

                while (current.EndsWith(EscapeSequence.ToString(), StringComparison.Ordinal))
                {
                    if (EndsInEscapeMode(current))
                    {
                        i++;
                        current = current.Substring(0, current.Length - 1) + seperator + segments[i];
                    }
                    else
                    {
                        break;
                    }
                }

                result.Add(current.Replace(DoubleEscapeSequence, EscapeSequence.ToString()));
            }

            return result;
        }

        private static bool EndsInEscapeMode(string s)
        {
            int escapeCount = 0;

            // count the number of escape chars on end of string
            for (int i = s.Length - 1; i > -1; i--)
            {
                if (s.Substring(i, 1) == EscapeSequence.ToString())
                {
                    escapeCount++;
                }
                else
                {
                    break;
                }
            }

            return escapeCount % 2 == 1; // odd count means escape mode is active
        }
    }
}
