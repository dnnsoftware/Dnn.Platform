// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;
    using System.Linq;

    /// <summary>
    /// Contributor for document-level directives.
    /// </summary>
    public class DocumentCspContributor : BaseCspContributor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentCspContributor"/> class.
        /// </summary>
        /// <param name="directiveType">The directive type to create the contributor for.</param>
        /// <param name="value">The value of the directive.</param>
        public DocumentCspContributor(CspDirectiveType directiveType, string value)
        {
            this.DirectiveType = directiveType;
            this.SetDirectiveValue(value);
        }

        /// <summary>
        /// Gets value of the document directive.
        /// </summary>
        public string DirectiveValue { get; private set; }

        /// <summary>
        /// Sets the directive value with validation.
        /// </summary>
        /// <param name="value">The value to set for the directive.</param>
        public void SetDirectiveValue(string value)
        {
            this.ValidateDirectiveValue(this.DirectiveType, value);
            this.DirectiveValue = value;
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

                    // Add more specific validations as needed
            }
        }

        /// <summary>
        /// Validates plugin types.
        /// </summary>
        private void ValidatePluginTypes(string value)
        {
            string[] validPluginTypes = { "application/pdf", "image/svg+xml" };
            var types = value.Split(' ');

            if (types.Any(t => !validPluginTypes.Contains(t)))
            {
                throw new ArgumentException("Invalid plugin type");
            }
        }

        /// <summary>
        /// Validates sandbox directive values.
        /// </summary>
        private void ValidateSandboxDirective(string value)
        {
            string[] validSandboxValues =
            {
                "allow-forms",
                "allow-scripts",
                "allow-same-origin",
                "allow-top-navigation",
                "allow-popups",
            };

            var values = value.Split(' ');

            if (values.Any(v => !validSandboxValues.Contains(v)))
            {
                throw new ArgumentException("Invalid sandbox directive value");
            }
        }
    }
}
