﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System.Collections.Generic;
    using System.Linq;

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
                    var generator = System.Security.Cryptography.RandomNumberGenerator.Create();
                    generator.GetBytes(nonceBytes);
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
        /// Gets collection of CSP contributors.
        /// </summary>
        private List<BaseCspContributor> Contributors { get; } = new List<BaseCspContributor>();

        /// <summary>
        /// Generates the complete Content Security Policy.
        /// </summary>
        /// <returns>The complete Content Security Policy.</returns>
        public string GeneratePolicy()
        {
            return string.Join(
                "; ",
                this.Contributors
                    .Select(c => c.GenerateDirective())
                    .Where(d => !string.IsNullOrEmpty(d)));
        }

        /// <summary>
        /// Supprime les sources de script du type spécifié de la politique CSP.
        /// </summary>
        /// <param name="cspSourceType">Le type de source CSP à supprimer.</param>
        public void RemoveScriptSources(CspSourceType cspSourceType)
        {
            this.RemoveSources(CspDirectiveType.ScriptSrc, cspSourceType);
        }

        /// <summary>
        /// Ajoute des types de plugins autorisés à la politique CSP.
        /// </summary>
        /// <param name="value">Le type de plugin à autoriser.</param>
        public void AddPluginTypes(string value)
        {
            this.AddDocumentDirective(CspDirectiveType.PluginTypes, value);
        }

        /// <summary>
        /// Ajoute une directive sandbox à la politique CSP.
        /// </summary>
        /// <param name="value">La valeur de la directive sandbox.</param>
        public void AddSandboxDirective(string value)
        {
            this.SetDocumentDirective(CspDirectiveType.SandboxDirective, value);
        }

        /// <summary>
        /// Ajoute une directive form-action à la politique CSP.
        /// </summary>
        /// <param name="sourceType">Le type de source CSP à ajouter.</param>
        /// <param name="value">La valeur associée à la source.</param>
        public void AddFormAction(CspSourceType sourceType, string value)
        {
            this.AddSource(CspDirectiveType.FormAction, sourceType, value);
        }

        /// <summary>
        /// Ajoute une directive frame-ancestors à la politique CSP.
        /// </summary>
        /// <param name="sourceType">Le type de source CSP à ajouter.</param>
        /// <param name="value">La valeur associée à la source.</param>
        public void AddFrameAncestors(CspSourceType sourceType, string value)
        {
            this.AddSource(CspDirectiveType.FrameAncestors, sourceType, value);
        }

        /// <summary>
        /// Ajoute une URI de rapport à la politique CSP.
        /// </summary>
        /// <param name="value">L'URI où les rapports de violation seront envoyés.</param>
        public void AddReportUri(string value)
        {
            this.AddReportingDirective(CspDirectiveType.ReportUri, value);
        }

        /// <summary>
        /// Ajoute un endpoint de rapport à la politique CSP.
        /// </summary>
        /// <param name="value">L'endpoint où les rapports seront envoyés.</param>
        public void AddReportTo(string value)
        {
            this.AddReportingDirective(CspDirectiveType.ReportTo, value);
        }

        private SourceCspContributor GetOrCreateDirective(CspDirectiveType directiveType)
        {
            var directive = this.Contributors.FirstOrDefault(c => c.DirectiveType == directiveType) as SourceCspContributor;
            if (directive == null)
            {
                directive = new SourceCspContributor(directiveType);
                this.AddContributor(directive);
            }

            return directive;
        }

        /// <summary>
        /// Adds a contributor to the policy.
        /// </summary>
        private void AddContributor(BaseCspContributor contributor)
        {
            // Remove any existing contributor of the same directive type
            this.Contributors.RemoveAll(c => c.DirectiveType == contributor.DirectiveType);
            this.Contributors.Add(contributor);
        }

        private void AddSource(CspDirectiveType directiveType, CspSourceType sourceType, string value = null)
        {
            var contributor = this.Contributors.FirstOrDefault(c => c.DirectiveType == directiveType) as SourceCspContributor;
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

        private void RemoveSources(CspDirectiveType directiveType, CspSourceType sourceType)
        {
            var contributor = this.Contributors.FirstOrDefault(c => c.DirectiveType == directiveType) as SourceCspContributor;
            if (contributor == null)
            {
                contributor = new SourceCspContributor(directiveType);
                this.AddContributor(contributor);
            }

            contributor.RemoveSources(sourceType);
        }

        private void SetDocumentDirective(CspDirectiveType directiveType, string value)
        {
            var contributor = this.Contributors.FirstOrDefault(c => c.DirectiveType == directiveType) as DocumentCspContributor;
            if (contributor == null)
            {
                contributor = new DocumentCspContributor(directiveType, value);
                this.AddContributor(contributor);
            }

            contributor.SetDirectiveValue(value);
        }

        private void AddDocumentDirective(CspDirectiveType directiveType, string value)
        {
            var contributor = this.Contributors.FirstOrDefault(c => c.DirectiveType == directiveType) as DocumentCspContributor;
            if (contributor == null)
            {
                contributor = new DocumentCspContributor(directiveType, value);
                this.AddContributor(contributor);
            }

            contributor.SetDirectiveValue(value);
        }

        private void AddReportingDirective(CspDirectiveType directiveType, string value)
        {
            var contributor = this.Contributors.FirstOrDefault(c => c.DirectiveType == directiveType) as ReportingCspContributor;
            if (contributor == null)
            {
                contributor = new ReportingCspContributor(directiveType);
                this.AddContributor(contributor);
            }

            contributor.AddReportingEndpoint(value);
        }
    }
}
