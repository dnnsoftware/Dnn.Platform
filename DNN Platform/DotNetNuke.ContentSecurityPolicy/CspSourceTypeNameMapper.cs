// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;

    /// <summary>
    /// Utility class for converting source types to their string representations.
    /// </summary>
    public static class CspSourceTypeNameMapper
    {
        /// <summary>
        /// Gets the source type name string.
        /// </summary>
        /// <param name="sourceType">The source type to get the name for.</param>
        /// <returns>The source type name string.</returns>
        public static string GetSourceTypeName(CspSourceType sourceType)
        {
            return sourceType switch
            {
                CspSourceType.Host => "host",
                CspSourceType.Scheme => "scheme",
                CspSourceType.Self => "'self'",
                CspSourceType.Inline => "'unsafe-inline'",
                CspSourceType.Eval => "'unsafe-eval'",
                CspSourceType.Nonce => "nonce",
                CspSourceType.Hash => "hash",
                CspSourceType.None => "'none'",
                CspSourceType.StrictDynamic => "'strict-dynamic'",
                _ => throw new ArgumentException("Unknown source type")
            };
        }

        /// <summary>
        /// Gets the source type from a source name string.
        /// </summary>
        /// <param name="sourceName">The source name to get the type for.</param>
        /// <returns>The source type.</returns>
        /// <exception cref="ArgumentException">Thrown when the source name is unknown.</exception>
        public static CspSourceType GetSourceType(string sourceName)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                throw new ArgumentException("Source name cannot be null or empty", nameof(sourceName));
            }

            return sourceName.ToLowerInvariant() switch
            {
                "'self'" => CspSourceType.Self,
                "'unsafe-inline'" => CspSourceType.Inline,
                "'unsafe-eval'" => CspSourceType.Eval,
                "'none'" => CspSourceType.None,
                "'strict-dynamic'" => CspSourceType.StrictDynamic,
                _ => throw new ArgumentException($"Unknown source name: {sourceName}")
            };
        }

        /// <summary>
        /// Tries to get the source type from a source name string.
        /// </summary>
        /// <param name="sourceName">The source name to get the type for.</param>
        /// <param name="sourceType">The source type, or default if parsing failed.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public static bool TryGetSourceType(string sourceName, out CspSourceType sourceType)
        {
            sourceType = default;

            try
            {
                sourceType = GetSourceType(sourceName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a source string represents a quoted keyword.
        /// </summary>
        /// <param name="source">The source string to check.</param>
        /// <returns>True if the source is a quoted keyword, false otherwise.</returns>
        public static bool IsQuotedKeyword(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            return source.StartsWith("'") && source.EndsWith("'");
        }

        /// <summary>
        /// Checks if a source string represents a nonce value.
        /// </summary>
        /// <param name="source">The source string to check.</param>
        /// <returns>True if the source is a nonce value, false otherwise.</returns>
        public static bool IsNonceSource(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            return source.StartsWith("'nonce-") && source.EndsWith("'");
        }

        /// <summary>
        /// Checks if a source string represents a hash value.
        /// </summary>
        /// <param name="source">The source string to check.</param>
        /// <returns>True if the source is a hash value, false otherwise.</returns>
        public static bool IsHashSource(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            return source.StartsWith("'") && source.EndsWith("'") &&
                   (source.Contains("sha256-") || source.Contains("sha384-") || source.Contains("sha512-"));
        }
    }
}
