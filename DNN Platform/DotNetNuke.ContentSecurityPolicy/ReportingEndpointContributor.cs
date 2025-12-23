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
    public class ReportingEndpointContributor : BaseCspContributor
    {
        private bool checkSyntax;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingEndpointContributor"/> class.
        /// </summary>
        /// <param name="directiveType">The reporting directive type (ReportUri).</param>
        /// <param name="checkSyntax">Check syntax of value.</param>
        public ReportingEndpointContributor(CspDirectiveType directiveType, bool checkSyntax)
        {
            if (directiveType != CspDirectiveType.ReportUri)
            {
                throw new ArgumentException("Invalid reporting directive type");
            }

            this.DirectiveType = directiveType;
            this.checkSyntax = checkSyntax;
        }

        /// <summary>
        /// Gets collection of reporting endpoints.
        /// </summary>
        private Dictionary<string, string> ReportingEndpoints { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Adds a reporting endpoint.
        /// </summary>
        /// <param name="name">The name of the endpoint where reports will be sent.</param>
        /// <param name="endpoint">The URL of the endpoint where reports will be sent.</param>
        public void AddReportingEndpoint(string name, string endpoint)
        {
            if (this.checkSyntax)
            {
                this.ValidateReportingEndpoint(endpoint);
            }

            if (!this.ReportingEndpoints.ContainsKey(name))
            {
                this.ReportingEndpoints.Add(name, endpoint);
            }
        }

        /// <summary>
        /// Removes a reporting endpoint.
        /// </summary>
        /// <param name="name">The endpoint to remove.</param>
        public void RemoveReportingEndpoint(string name)
        {
            this.ReportingEndpoints.Remove(name);
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

            var endpoints = this.ReportingEndpoints.Select(ep => $"{ep.Key}=\"{ep.Value}\"").ToList();
            return $"{string.Join(" ", endpoints)}";
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
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri uriResult))
            {
                throw new ArgumentException($"Invalid reporting endpoint: {endpoint}");
            }
        }
    }
}
