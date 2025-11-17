// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a single source in a Content Security Policy.
    /// </summary>
    public class CspSource
    {
        /// <summary>
        /// Compiled regex for validating host sources (domain with wildcard, IPv4 and IPv6).
        /// </summary>
        private static readonly Regex DomainRegex = new Regex(@"^(https?://)?(([a-zA-Z0-9-*]+\.)+[a-zA-Z]{2,}|\d{1,3}(\.\d{1,3}){3}|\[?[0-9a-fA-F:]+\]?)(:\d+)?(/.*)?$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="CspSource"/> class.
        /// </summary>
        /// <param name="type">Type of the source.</param>
        /// <param name="value">Value of the source.</param>
        /// <param name="checkSyntax">Check syntax of value.</param>
        public CspSource(CspSourceType type, string value = null, bool checkSyntax = false)
        {
            this.Type = type;
            this.Value = this.ValidateSource(type, value, checkSyntax);
        }

        /// <summary>
        /// Gets type of the CSP source.
        /// </summary>
        public CspSourceType Type { get; }

        /// <summary>
        /// Gets the actual source value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Returns the string representation of the source.
        /// </summary>
        /// <returns>The string representation of the source.</returns>
        public override string ToString() => this.Value ?? CspSourceTypeNameMapper.GetSourceTypeName(this.Type);

        /// <summary>
        /// Validates the source based on its type.
        /// </summary>
        private string ValidateSource(CspSourceType type, string value, bool checkSyntax)
        {
            switch (type)
            {
                case CspSourceType.Host:
                    return this.ValidateHostSource(value, checkSyntax);
                case CspSourceType.Scheme:
                    return this.ValidateSchemeSource(value, checkSyntax);
                case CspSourceType.Nonce:
                    return this.ValidateNonceSource(value);
                case CspSourceType.Hash:
                    return this.ValidateHashSource(value, checkSyntax);
                case CspSourceType.Self:
                    return "'self'";
                case CspSourceType.Inline:
                case CspSourceType.Eval:
                    return "'unsafe-" + type.ToString().ToLowerInvariant() + "'";
                case CspSourceType.None:
                    return "'none'";
                case CspSourceType.StrictDynamic:
                    return "'strict-dynamic'";
                default:
                    throw new ArgumentException("Invalid source type");
            }
        }

        /// <summary>
        /// Validates host source (domain or IP).
        /// </summary>
        private string ValidateHostSource(string value, bool checkSyntax)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Host source cannot be empty");
            }

            // domain with wildcard, ip4 and ip6 validation
            if (checkSyntax && !DomainRegex.IsMatch(value))
            {
                throw new ArgumentException($"Invalid host source: {value}");
            }

            return value;
        }

        /// <summary>
        /// Validates scheme source (protocol).
        /// </summary>
        private string ValidateSchemeSource(string value, bool checkSyntax)
        {
            var validSchemes = new string[] { "http:", "https:", "data:", "blob:", "filesystem:", "wss:", "ws:" };
            if (!validSchemes.Contains(value))
            {
                throw new ArgumentException($"Invalid scheme: {value}");
            }

            return value;
        }

        /// <summary>
        /// Validates nonce source.
        /// </summary>
        private string ValidateNonceSource(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Nonce cannot be empty");
            }

            // Basic nonce validation - allow any non-empty string for flexibility
            // In real-world scenarios, nonces might not always be strict base64
            return $"'nonce-{value}'";
        }

        /// <summary>
        /// Validates hash source.
        /// </summary>
        private string ValidateHashSource(string value, bool checkSyntax)
        {
            var hashPrefixes = new string[] { "sha256-", "sha384-", "sha512-" };

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Hash cannot be empty");
            }

            // Check if the value starts with a valid hash prefix
            // Allow any string after the prefix for flexibility in parsing scenarios
            bool hasValidPrefix = hashPrefixes.Any(prefix => value.StartsWith(prefix));

            if (!hasValidPrefix)
            {
                throw new ArgumentException($"Invalid hash format: {value}");
            }

            return $"'{value}'";
        }
    }
}
