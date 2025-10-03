// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Contributor for reporting directives.
    /// </summary>
    public class ReportingCspContributor : BaseCspContributor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingCspContributor"/> class.
        /// </summary>
        /// <param name="directiveType">Le type de directive de rapport (ReportUri ou ReportTo).</param>
        public ReportingCspContributor(CspDirectiveType directiveType)
        {
            if (directiveType != CspDirectiveType.ReportUri && directiveType != CspDirectiveType.ReportTo)
            {
                throw new ArgumentException("Invalid reporting directive type");
            }

            this.DirectiveType = directiveType;
        }

        /// <summary>
        /// Gets collection of reporting endpoints.
        /// </summary>
        private List<string> ReportingEndpoints { get; } = new List<string>();

        /// <summary>
        /// Adds a reporting endpoint.
        /// </summary>
        /// <param name="endpoint">L'URL de l'endpoint où envoyer les rapports.</param>
        public void AddReportingEndpoint(string endpoint)
        {
            this.ValidateReportingEndpoint(endpoint);
            if (!this.ReportingEndpoints.Contains(endpoint))
            {
                this.ReportingEndpoints.Add(endpoint);
            }
        }

        /// <summary>
        /// Removes a reporting endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to remove.</param>
        public void RemoveReportingEndpoint(string endpoint)
        {
            this.ReportingEndpoints.Remove(endpoint);
        }

        /// <summary>
        /// Generates the directive string.
        /// </summary>
        /// <returns>The directive string.</returns>
        public override string GenerateDirective()
        {
            if (!this.ReportingEndpoints.Any())
            {
                return string.Empty;
            }

            return $"{CspDirectiveNameMapper.GetDirectiveName(this.DirectiveType)} {string.Join(" ", this.ReportingEndpoints)}";
        }

        /// <summary>
        /// Validates reporting endpoint.
        /// </summary>
        private void ValidateReportingEndpoint(string value)
        {
            switch (this.DirectiveType)
            {
                case CspDirectiveType.ReportUri:
                    this.ValidateReportUri(value);
                    break;
                case CspDirectiveType.ReportTo:
                    this.ValidateReportTo(value);
                    break;

                    // Add more specific validations as needed
            }
        }

        private void ValidateReportTo(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Reporting to cannot be empty");
            }
        }

        private void ValidateReportUri(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Reporting endpoint cannot be empty");
            }

            // URL validation regex
            var urlRegex = new Regex(@"^(https?://)?([a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*)?(:\d+)?(/.*)?$");
            if (!urlRegex.IsMatch(endpoint))
            {
                throw new ArgumentException($"Invalid reporting endpoint: {endpoint}");
            }
        }
    }
}
