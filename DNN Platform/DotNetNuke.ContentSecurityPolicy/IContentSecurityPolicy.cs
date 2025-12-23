// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;

    /// <summary>
    /// Interface defining Content Security Policy management operations.
    /// </summary>
    public interface IContentSecurityPolicy
    {
        /// <summary>
        /// Gets a cryptographically secure nonce value for the CSP policy.
        /// </summary>
        string Nonce { get; }

        /// <summary>
        /// Gets the default source contributor.
        /// </summary>
        SourceCspContributor DefaultSource { get; }

        /// <summary>
        /// Gets the script source contributor.
        /// </summary>
        SourceCspContributor ScriptSource { get; }

        /// <summary>
        /// Gets the style source contributor.
        /// </summary>
        SourceCspContributor StyleSource { get; }

        /// <summary>
        /// Gets the image source contributor.
        /// </summary>
        SourceCspContributor ImgSource { get; }

        /// <summary>
        /// Gets the connect source contributor.
        /// </summary>
        SourceCspContributor ConnectSource { get; }

        /// <summary>
        /// Gets the font source contributor.
        /// </summary>
        SourceCspContributor FontSource { get; }

        /// <summary>
        /// Gets the object source contributor.
        /// </summary>
        SourceCspContributor ObjectSource { get; }

        /// <summary>
        /// Gets the media source contributor.
        /// </summary>
        SourceCspContributor MediaSource { get; }

        /// <summary>
        /// Gets the frame source contributor.
        /// </summary>
        SourceCspContributor FrameSource { get; }

        /// <summary>
        /// Gets the frame ancestors contributor.
        /// </summary>
        SourceCspContributor FrameAncestors { get; }

        /// <summary>
        /// Gets the Form action source contributor.
        /// </summary>
        SourceCspContributor FormAction { get; }

        /// <summary>
        /// Gets the base URI source contributor.
        /// </summary>
        SourceCspContributor BaseUriSource { get; }

        /// <summary>
        /// Removes a script source from the policy.
        /// </summary>
        /// <param name="cspSourceType">The CSP source type to remove.</param>
        void RemoveScriptSources(CspSourceType cspSourceType);

        /// <summary>
        /// Adds plugin types to the policy.
        /// </summary>
        /// <param name="value">The plugin type to allow.</param>
        void AddPluginTypes(string value);

        /// <summary>
        /// Adds a sandbox directive to the policy.
        /// </summary>
        /// <param name="value">The sandbox directive options.</param>
        void AddSandbox(string value);

        /// <summary>
        /// Adds a form action to the policy.
        /// </summary>
        /// <param name="sourceType">The CSP source type to add.</param>
        /// <param name="value">The allowed URL for form submission.</param>
        void AddFormAction(CspSourceType sourceType, string value);

        /// <summary>
        /// Adds frame ancestors to the policy.
        /// </summary>
        /// <param name="sourceType">The CSP source type to add.</param>
        /// <param name="value">The allowed URL as a frame ancestor.</param>
        void AddFrameAncestors(CspSourceType sourceType, string value);

        /// <summary>
        /// Adds a report URI to the policy.
        /// </summary>
        /// <param name="name">The name where violation reports will be sent.</param>
        /// <param name="value">The URI where violation reports will be sent.</param>
        public void AddReportEndpoint(string name, string value);

        /// <summary>
        /// Adds a report destination to the policy.
        /// </summary>
        /// <param name="value">The endpoint where reports will be sent.</param>
        void AddReportTo(string value);

        /// <summary>
        /// Parses a CSP header string into a ContentSecurityPolicy object.
        /// </summary>
        /// <param name="cspHeader">The CSP header string to parse.</param>
        /// <returns>A ContentSecurityPolicy object representing the parsed header.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the CSP header is invalid or cannot be parsed.</exception>
        IContentSecurityPolicy AddHeader(string cspHeader);

        /// <summary>
        /// Adds a report directive to the policy.
        /// </summary>
        /// <param name="header">The report directive to add.</param>
        /// <returns>A ContentSecurityPolicy object representing the parsed header.</returns>
        IContentSecurityPolicy AddReportEndpointHeader(string header);

        /// <summary>
        /// Generates the complete security policy.
        /// </summary>
        /// <returns>The complete security policy as a string.</returns>
        string GeneratePolicy();

        /// <summary>
        /// Generates the reporting endpoints.
        /// </summary>
        /// <returns>Reporting Endpoints as a string.</returns>
        string GenerateReportingEndpoints();

        /// <summary>
        /// Upgrade Insecure Requests.
        /// </summary>
        void UpgradeInsecureRequests();

        /// <summary>
        /// Clear Content Security Policy Contributors.
        /// </summary>
        void ClearContentSecurityPolicyContributors();

        /// <summary>
        /// Clear Reporting Endpoints Contributors.
        /// </summary>
        void ClearReportingEndpointsContributors();
    }
}
