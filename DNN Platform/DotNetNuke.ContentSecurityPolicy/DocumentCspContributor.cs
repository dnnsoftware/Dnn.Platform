// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Contributor for document-level directives.
    /// </summary>
    public class DocumentCspContributor : BaseCspContributor
    {
        /// <summary>
        /// Compiled regex for validating MIME type format (type/subtype).
        /// </summary>
        private static readonly Regex MimeTypeRegex = new Regex(@"^[a-zA-Z0-9][a-zA-Z0-9!#$&\-\^_.+]*/[a-zA-Z0-9][a-zA-Z0-9!#$&\-\^_.+]*$", RegexOptions.Compiled);

        private string directiveValue;

        private bool checkSyntax;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentCspContributor"/> class.
        /// </summary>
        /// <param name="directiveType">The directive type to create the contributor for.</param>
        /// <param name="value">The value of the directive.</param>
        /// <param name="checkSyntax">Check syntax of value.</param>
        public DocumentCspContributor(CspDirectiveType directiveType, string value, bool checkSyntax)
        {
            this.checkSyntax = checkSyntax;
            this.DirectiveType = directiveType;
            this.DirectiveValue = value;
        }

        /// <summary>
        /// Gets or Sets value of the document directive.
        /// </summary>
        public string DirectiveValue
        {
            get
            {
                return this.directiveValue;
            }

            set
            {
                if (this.checkSyntax)
                {
                    this.ValidateDirectiveValue(this.DirectiveType, value);
                }

                this.directiveValue = value;
            }
        }

        /// <summary>
        /// Generates the directive string.
        /// </summary>
        /// <returns>The directive string.</returns>
        public override string GenerateDirective()
        {
            if (this.DirectiveType == CspDirectiveType.UpgradeInsecureRequests)
            {
                return $"{CspDirectiveNameMapper.GetDirectiveName(this.DirectiveType)}";
            }

            if (string.IsNullOrWhiteSpace(this.DirectiveValue))
            {
                return string.Empty;
            }

            return $"{CspDirectiveNameMapper.GetDirectiveName(this.DirectiveType)} {this.DirectiveValue}";
        }

        /// <summary>
        /// Validates directive value based on directive type.
        /// </summary>
        private void ValidateDirectiveValue(CspDirectiveType type, string value)
        {
            switch (type)
            {
                case CspDirectiveType.PluginTypes:
                    this.ValidatePluginTypes(value);
                    break;
                case CspDirectiveType.SandboxDirective:
                    this.ValidateSandboxDirective(value);
                    break;
            }
        }

        /// <summary>
        /// Validates plugin types (MIME types).
        /// </summary>
        private void ValidatePluginTypes(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Plugin types cannot be empty");
            }

            var types = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (types.Any(t => !MimeTypeRegex.IsMatch(t)))
            {
                throw new ArgumentException($"Invalid MIME type format: {value}");
            }
        }

        /// <summary>
        /// Validates sandbox directive values.
        /// </summary>
        private void ValidateSandboxDirective(string value)
        {
            var validSandboxValues = new string[]
            {
                "allow-forms",
                "allow-modals",
                "allow-orientation-lock",
                "allow-pointer-lock",
                "allow-popups",
                "allow-popups-to-escape-sandbox",
                "allow-presentation",
                "allow-same-origin",
                "allow-scripts",
                "allow-top-navigation",
                "allow-top-navigation-by-user-activation",
            };

            var values = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (values.Any(v => !validSandboxValues.Contains(v)))
            {
                throw new ArgumentException($"Invalid sandbox directive value: {value}");
            }
        }
    }
}
