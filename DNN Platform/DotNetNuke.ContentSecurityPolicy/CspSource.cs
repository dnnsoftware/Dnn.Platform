﻿// Licensed to the .NET Foundation under one or more agreements.
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
        /// Initializes a new instance of the <see cref="CspSource"/> class.
        /// </summary>
        /// <param name="type">Type of the source.</param>
        /// <param name="value">Value of the source.</param>
        public CspSource(CspSourceType type, string value = null)
        {
            this.Type = type;
            this.Value = this.ValidateSource(type, value);
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
        private string ValidateSource(CspSourceType type, string value)
        {
            switch (type)
            {
                case CspSourceType.Host:
                    return this.ValidateHostSource(value);
                case CspSourceType.Scheme:
                    return this.ValidateSchemeSource(value);
                case CspSourceType.Nonce:
                    return this.ValidateNonceSource(value);
                case CspSourceType.Hash:
                    return this.ValidateHashSource(value);
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
        private string ValidateHostSource(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Host source cannot be empty");
            }

            // Basic domain validation
            var domainRegex = new Regex(@"^(https?://)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}(:\d+)?(/.*)?$");
            if (!domainRegex.IsMatch(value))
            {
                throw new ArgumentException($"Invalid host source: {value}");
            }

            return value.StartsWith("http") ? value : $"https://{value}";
        }

        /// <summary>
        /// Validates scheme source (protocol).
        /// </summary>
        private string ValidateSchemeSource(string value)
        {
            string[] validSchemes = { "http:", "https:", "data:", "blob:", "filesystem:" };
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

            // Basic nonce validation (base64 encoded)
            if (!this.IsBase64String(value))
            {
                throw new ArgumentException("Invalid nonce format");
            }

            return $"'nonce-{value}'";
        }

        /// <summary>
        /// Validates hash source.
        /// </summary>
        private string ValidateHashSource(string value)
        {
            string[] hashPrefixes = { "sha256-", "sha384-", "sha512-" };

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Hash cannot be empty");
            }

            // Check if the value starts with a valid hash prefix and has a base64 encoded value
            bool isValidHash = hashPrefixes.Any(prefix =>
                value.StartsWith(prefix) && this.IsBase64String(value.Substring(prefix.Length)));

            if (!isValidHash)
            {
                throw new ArgumentException($"Invalid hash format: {value}");
            }

            return $"'{value}'";
        }

        /// <summary>
        /// Checks if a string is a valid Base64 string.
        /// </summary>
        private bool IsBase64String(string value)
        {
            try
            {
                Convert.FromBase64String(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
