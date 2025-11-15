// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System.Collections.Generic;
    using System.Linq;
    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Manages the entire Content Security Policy.
    /// </summary>
    public class ContentSecurityPolicy : IContentSecurityPolicy
    {
        private string nonce;

        /// <summary>Initializes a new instance of the <see cref="ContentSecurityPolicy"/> class.</summary>
        public ContentSecurityPolicy()
        {
        }

        /// <summary>
        /// Gets a cryptographically secure random nonce value for use in CSP policies.
        /// </summary>
        public string Nonce
        {
            get
            {
                if (this.nonce == null)
                {
                    var nonceBytes = new byte[32];
                    using (var generator = System.Security.Cryptography.RandomNumberGenerator.Create())
                    {
                        generator.GetBytes(nonceBytes);
                    }

                    this.nonce = System.Convert.ToBase64String(nonceBytes);
                }

                return this.nonce;
            }
        }

        /// <summary>
        /// Gets the default source contributor for managing default-src directives.
        /// </summary>
        public SourceCspContributor DefaultSource
        {
            get
            {
                return this.GetOrCreateDirective(CspDirectiveType.DefaultSrc);
            }
        }

        /// <summary>
        /// Gets the script source contributor for managing script-src directives.
        /// </summary>
        public SourceCspContributor ScriptSource
        {
            get
            {
                return this.GetOrCreateDirective(CspDirectiveType.ScriptSrc);
            }
        }

        /// <summary>
        /// Gets the style source contributor for managing style-src directives.
        /// </summary>
        public SourceCspContributor StyleSource
        {
            get
            {
                return this.GetOrCreateDirective(CspDirectiveType.StyleSrc);
            }
        }

        /// <summary>
        /// Gets the image source contributor for managing img-src directives.
        /// </summary>
        public SourceCspContributor ImgSource
        {
            get
            {
                return this.GetOrCreateDirective(CspDirectiveType.ImgSrc);
            }
        }

        /// <summary>
        /// Gets the connect source contributor for managing connect-src directives.
        /// </summary>
        public SourceCspContributor ConnectSource
        {
            get
            {
                return this.GetOrCreateDirective(CspDirectiveType.ConnectSrc);
            }
        }

        /// <summary>
        /// Gets the frame ancestors contributor for managing frame-ancestors directives.
        /// </summary>
        public SourceCspContributor FrameAncestors
        {
            get
            {
                return this.GetOrCreateDirective(CspDirectiveType.FrameAncestors);
            }
        }

        /// <summary>
        /// Gets the font source contributor for managing font-src directives.
        /// </summary>
        public SourceCspContributor FontSource
        {
            get
            {
                return this.GetOrCreateDirective(CspDirectiveType.FontSrc);
            }
        }

        /// <summary>
        /// Gets the object source contributor for managing object-src directives.
        /// </summary>
        public SourceCspContributor ObjectSource
        {
            get
            {
                return this.GetOrCreateDirective(CspDirectiveType.ObjectSrc);
            }
        }

        /// <summary>
        /// Gets the media source contributor for managing media-src directives.
        /// </summary>
        public SourceCspContributor MediaSource
        {
            get
            {
                return this.GetOrCreateDirective(CspDirectiveType.MediaSrc);
            }
        }

        /// <summary>
        /// Gets the frame source contributor for managing frame-src directives.
        /// </summary>
        public SourceCspContributor FrameSource
        {
            get
            {
                return this.GetOrCreateDirective(CspDirectiveType.FrameSrc);
            }
        }

        /// <summary>
        /// Gets the form action source contributor for managing form-action directives.
        /// </summary>
        public SourceCspContributor FormAction
        {
            get
            {
                return this.GetOrCreateDirective(CspDirectiveType.FormAction);
            }
        }

        /// <summary>
        /// Gets the base URI source contributor for managing base-uri directives.
        /// </summary>
        public SourceCspContributor BaseUriSource
        {
            get
            {
                return this.GetOrCreateDirective(CspDirectiveType.BaseUri);
            }
        }

        /// <summary>
        /// Gets collection of CSP contributors for content security policy directives.
        /// </summary>
        private List<BaseCspContributor> ContentSecurityPolicyContributors { get; } = new List<BaseCspContributor>();

        /// <summary>
        /// Gets collection of CSP contributors for reporting endpoints directives.
        /// </summary>
        private List<BaseCspContributor> ReportingEndpointsContributors { get; } = new List<BaseCspContributor>();

        /// <summary>
        /// Parses a CSP header string into a ContentSecurityPolicy object.
        /// </summary>
        /// <param name="cspHeader">The CSP header string to parse.</param>
        /// <returns>A ContentSecurityPolicy object representing the parsed header.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the CSP header is invalid or cannot be parsed.</exception>
        public IContentSecurityPolicy AddHeader(string cspHeader)
        {
            var parser = new ContentSecurityPolicyParser(this);
            parser.Parse(cspHeader);
            return this;
        }

        /// <summary>
        /// Adds a reporting directive to the policy.
        /// </summary>
        /// <param name="header">The reporting directive to add.</param>
        /// <returns>A ContentSecurityPolicy object representing the parsed header.</returns>
        public IContentSecurityPolicy AddReportEndpointHeader(string header)
        {
            if (!string.IsNullOrEmpty(header))
            {
                var parser = new ContentSecurityPolicyParser(this);
                parser.ParseReportingEndpoints(header);
            }

            return this;
        }

        /// <summary>
        /// Removes script sources of the specified type from the CSP policy.
        /// </summary>
        /// <param name="cspSourceType">The CSP source type to remove.</param>
        public void RemoveScriptSources(CspSourceType cspSourceType)
        {
            this.RemoveSources(CspDirectiveType.ScriptSrc, cspSourceType);
        }

        /// <summary>
        /// Adds allowed plugin types to the CSP policy.
        /// </summary>
        /// <param name="value">The plugin type to allow.</param>
        public void AddPluginTypes(string value)
        {
            this.AddDocumentDirective(CspDirectiveType.PluginTypes, value);
        }

        /// <summary>
        /// Adds a sandbox directive to the CSP policy.
        /// </summary>
        /// <param name="value">The sandbox directive value.</param>
        public void AddSandbox(string value)
        {
            this.SetDocumentDirective(CspDirectiveType.SandboxDirective, value);
        }

        /// <summary>
        /// Adds a form-action directive to the CSP policy.
        /// </summary>
        /// <param name="sourceType">The CSP source type to add.</param>
        /// <param name="value">The value associated with the source.</param>
        public void AddFormAction(CspSourceType sourceType, string value)
        {
            this.AddSource(CspDirectiveType.FormAction, sourceType, value);
        }

        /// <summary>
        /// Adds a frame-ancestors directive to the CSP policy.
        /// </summary>
        /// <param name="sourceType">The CSP source type to add.</param>
        /// <param name="value">The value associated with the source.</param>
        public void AddFrameAncestors(CspSourceType sourceType, string value)
        {
            this.AddSource(CspDirectiveType.FrameAncestors, sourceType, value);
        }

        /// <summary>
        /// Adds a report URI to the CSP policy.
        /// </summary>
        /// <param name="name">The name where violation reports will be sent.</param>
        /// <param name="value">The URI where violation reports will be sent.</param>
        public void AddReportEndpoint(string name, string value)
        {
            // this.AddReportingDirective(CspDirectiveType.ReportUri, value);
            this.AddReportingEndpointsDirective(name, value);
        }

        /// <summary>
        /// Adds a report endpoint to the CSP policy.
        /// </summary>
        /// <param name="value">The endpoint where reports will be sent.</param>
        public void AddReportTo(string value)
        {
            this.AddReportingDirective(CspDirectiveType.ReportTo, value);
        }

        /// <summary>
        /// Adds the upgrade-insecure-requests directive to upgrade HTTP requests to HTTPS.
        /// </summary>
        public void UpgradeInsecureRequests()
        {
            this.SetDocumentDirective(CspDirectiveType.UpgradeInsecureRequests, string.Empty);
        }

        /// <summary>
        /// Generates the complete Content Security Policy.
        /// </summary>
        /// <returns>The complete Content Security Policy.</returns>
        public string GeneratePolicy()
        {
            return string.Join(
                "; ",
                this.ContentSecurityPolicyContributors
                    .Select(c => c.GenerateDirective())
                    .Where(d => !string.IsNullOrEmpty(d)));
        }

        /// <summary>
        /// Generates the complete security policy.
        /// </summary>
        /// <returns>Reporting Endpoints as a string.</returns>
        public string GenerateReportingEndpoints()
        {
            return string.Join(
                  "; ",
                  this.ReportingEndpointsContributors
                      .Select(c => c.GenerateDirective())
                      .Where(d => !string.IsNullOrEmpty(d)));
        }

        /// <summary>
        /// Clear Content Security Policy Contributors.
        /// </summary>
        public void ClearContentSecurityPolicyContributors()
        {
            this.ContentSecurityPolicyContributors.Clear();
        }

        /// <summary>
        /// Clear Reporting Endpoints Contributors.
        /// </summary>
        public void ClearReportingEndpointsContributors()
        {
            this.ReportingEndpointsContributors.Clear();
        }

        /// <summary>
        /// Gets an existing directive contributor or creates a new one if it doesn't exist.
        /// </summary>
        /// <param name="directiveType">The type of directive to get or create.</param>
        /// <returns>A SourceCspContributor for the specified directive type.</returns>
        private SourceCspContributor GetOrCreateDirective(CspDirectiveType directiveType)
        {
            var directive = this.ContentSecurityPolicyContributors.FirstOrDefault(c => c.DirectiveType == directiveType) as SourceCspContributor;
            if (directive == null)
            {
                directive = new SourceCspContributor(directiveType);
                this.AddContributor(directive);
            }

            return directive;
        }

        /// <summary>
        /// Adds a contributor to the content security policy.
        /// </summary>
        /// <param name="contributor">The contributor to add to the policy.</param>
        private void AddContributor(BaseCspContributor contributor)
        {
            // Remove any existing contributor of the same directive type
            this.ContentSecurityPolicyContributors.RemoveAll(c => c.DirectiveType == contributor.DirectiveType);
            this.ContentSecurityPolicyContributors.Add(contributor);
        }

        /// <summary>
        /// Adds a contributor to the reporting endpoints collection.
        /// </summary>
        /// <param name="contributor">The contributor to add to the reporting endpoints.</param>
        private void AddReportingEndpointsContributors(BaseCspContributor contributor)
        {
            // Remove any existing contributor of the same directive type
            this.ReportingEndpointsContributors.RemoveAll(c => c.DirectiveType == contributor.DirectiveType);
            this.ReportingEndpointsContributors.Add(contributor);
        }

        /// <summary>
        /// Adds a source to the specified directive type.
        /// </summary>
        /// <param name="directiveType">The directive type to add the source to.</param>
        /// <param name="sourceType">The type of source to add.</param>
        /// <param name="value">The value associated with the source. If null and sourceType is Nonce, uses the generated nonce.</param>
        private void AddSource(CspDirectiveType directiveType, CspSourceType sourceType, string value = null)
        {
            var contributor = this.ContentSecurityPolicyContributors.FirstOrDefault(c => c.DirectiveType == directiveType) as SourceCspContributor;
            if (contributor == null)
            {
                contributor = new SourceCspContributor(directiveType);
                this.AddContributor(contributor);
            }

            if (sourceType == CspSourceType.Nonce && string.IsNullOrEmpty(value))
            {
                value = this.Nonce;
            }

            contributor.AddSource(new CspSource(sourceType, value));
        }

        /// <summary>
        /// Removes sources of the specified type from the directive.
        /// </summary>
        /// <param name="directiveType">The directive type to remove sources from.</param>
        /// <param name="sourceType">The type of sources to remove.</param>
        private void RemoveSources(CspDirectiveType directiveType, CspSourceType sourceType)
        {
            var contributor = this.ContentSecurityPolicyContributors.FirstOrDefault(c => c.DirectiveType == directiveType) as SourceCspContributor;
            if (contributor == null)
            {
                contributor = new SourceCspContributor(directiveType);
                this.AddContributor(contributor);
            }

            contributor.RemoveSources(sourceType);
        }

        /// <summary>
        /// Sets a document directive value, replacing any existing value.
        /// </summary>
        /// <param name="directiveType">The directive type to set.</param>
        /// <param name="value">The value to set for the directive.</param>
        private void SetDocumentDirective(CspDirectiveType directiveType, string value)
        {
            var contributor = this.ContentSecurityPolicyContributors.FirstOrDefault(c => c.DirectiveType == directiveType) as DocumentCspContributor;
            if (contributor == null)
            {
                contributor = new DocumentCspContributor(directiveType, value);
                this.AddContributor(contributor);
            }

            contributor.SetDirectiveValue(value);
        }

        /// <summary>
        /// Adds a document directive with the specified value.
        /// </summary>
        /// <param name="directiveType">The directive type to add.</param>
        /// <param name="value">The value for the directive.</param>
        private void AddDocumentDirective(CspDirectiveType directiveType, string value)
        {
            var contributor = this.ContentSecurityPolicyContributors.FirstOrDefault(c => c.DirectiveType == directiveType) as DocumentCspContributor;
            if (contributor == null)
            {
                contributor = new DocumentCspContributor(directiveType, value);
                this.AddContributor(contributor);
            }

            contributor.SetDirectiveValue(value);
        }

        /// <summary>
        /// Adds a reporting directive with the specified value.
        /// </summary>
        /// <param name="directiveType">The directive type to add.</param>
        /// <param name="value">The value for the reporting directive.</param>
        private void AddReportingDirective(CspDirectiveType directiveType, string value)
        {
            var contributor = this.ContentSecurityPolicyContributors.FirstOrDefault(c => c.DirectiveType == directiveType) as ReportingCspContributor;
            if (contributor == null)
            {
                contributor = new ReportingCspContributor(directiveType);
                this.AddContributor(contributor);
            }

            contributor.AddReportingEndpoint(value);
        }

        /// <summary>
        /// Adds a reporting endpoints directive with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the reporting endpoint.</param>
        /// <param name="value">The URI value for the reporting endpoint.</param>
        private void AddReportingEndpointsDirective(string name, string value)
        {
            var contributor = this.ReportingEndpointsContributors.FirstOrDefault(c => c.DirectiveType == CspDirectiveType.ReportUri) as ReportingEndpointContributor;
            if (contributor == null)
            {
                contributor = new ReportingEndpointContributor(CspDirectiveType.ReportUri);
                this.AddReportingEndpointsContributors(contributor);
            }

            contributor.AddReportingEndpoint(name, value);
        }
    }
}
