// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>Base class for all resource types.</summary>
    public abstract class ResourceBase : IResource
    {
        private string name;

        /// <inheritdoc />
        public string FilePath { get; set; }

        /// <inheritdoc />
        public string PathNameAlias { get; set; }

        /// <inheritdoc />
        public string ResolvedPath { get; set; }

        /// <inheritdoc />
        public string CdnUrl { get; set; }

        /// <inheritdoc />
        public int Priority { get; set; }

        /// <inheritdoc />
        public string Provider { get; set; } = ClientResourceProviders.DnnPageHeaderProvider;

        /// <inheritdoc />
        public string Name
        {
            get => string.IsNullOrEmpty(this.name) ? this.FilePath : this.name;
            set => this.name = value;
        }

        /// <inheritdoc />
        public string Version { get; set; }

        /// <inheritdoc />
        public bool ForceVersion { get; set; }

        /// <inheritdoc />
        public CrossOrigin CrossOrigin { get; set; } = CrossOrigin.None;

        /// <inheritdoc />
        public FetchPriority FetchPriority { get; set; } = FetchPriority.Auto;

        /// <inheritdoc />
        public ReferrerPolicy ReferrerPolicy { get; set; } = ReferrerPolicy.None;

        /// <inheritdoc />
        public string Integrity { get; set; } = string.Empty;

        /// <inheritdoc />
        public string Type { get; set; } = string.Empty;

        /// <inheritdoc />
        public bool Blocking { get; set; }

        /// <inheritdoc />
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        /// <inheritdoc />
        public abstract void Register();

        /// <inheritdoc />
        public abstract string Render(int crmVersion, bool useCdn, string applicationPath);

        /// <summary>Gets the versioned path.</summary>
        /// <param name="crmVersion">The CRM version.</param>
        /// <param name="useCdn">Whether to use the CDN url if available.</param>
        /// <param name="applicationPath">The application path to use for resolving relative paths.</param>
        /// <returns>The versioned path.</returns>
        internal string GetVersionedPath(int crmVersion, bool useCdn, string applicationPath)
        {
            var path = this.ResolvedPath;
            if (useCdn && !string.IsNullOrEmpty(this.CdnUrl))
            {
                return this.CdnUrl;
            }
            else if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }
            else if (path.StartsWith("~", StringComparison.Ordinal))
            {
                if (string.IsNullOrEmpty(applicationPath))
                {
                    return $"{path.TrimStart('~')}?cdv={crmVersion}";
                }
                else
                {
                    return $"{path.Replace("~", applicationPath)}?cdv={crmVersion}";
                }
            }
            else if (path.StartsWith("/", StringComparison.Ordinal))
            {
                return $"{path}?cdv={crmVersion}";
            }

            return $"{applicationPath}/{path}?cdv={crmVersion}";
        }

        /// <summary>Renders the <c>blocking</c> attribute.</summary>
        /// <param name="htmlString">The HTML string builder to append to.</param>
        protected void RenderBlocking(StringBuilder htmlString)
        {
            if (this.Blocking)
            {
                htmlString.Append(" blocking=\"render\"");
            }
        }

        /// <summary>Renders the <c>crossorigin</c> attribute.</summary>
        /// <param name="htmlString">The HTML string builder to append to.</param>
        protected void RenderCrossOriginAttribute(StringBuilder htmlString)
        {
            switch (this.CrossOrigin)
            {
                case CrossOrigin.UseCredentials:
                    htmlString.Append($" crossorigin=\"use-credentials\"");
                    return;
                case CrossOrigin.Anonymous:
                    htmlString.Append($" crossorigin=\"anonymous\"");
                    return;
                case CrossOrigin.None:
                    return;
                default:
                    throw new InvalidOperationException($"Unexpected CrossOrigin value: {this.CrossOrigin}");
            }
        }

        /// <summary>Renders the <c>fetchpriority</c> attribute.</summary>
        /// <param name="htmlString">The HTML string builder to append to.</param>
        protected void RenderFetchPriority(StringBuilder htmlString)
        {
            switch (this.FetchPriority)
            {
                case FetchPriority.High:
                    htmlString.Append($" fetchpriority=\"high\"");
                    return;
                case FetchPriority.Low:
                    htmlString.Append($" fetchpriority=\"low\"");
                    return;
                case FetchPriority.Auto:
                    return;
                default:
                    throw new InvalidOperationException($"Unexpected FetchPriority value: {this.FetchPriority}");
            }
        }

        /// <summary>Renders the <c>integrity</c> attribute.</summary>
        /// <param name="htmlString">The HTML string builder to append to.</param>
        protected void RenderIntegrity(StringBuilder htmlString)
        {
            if (!string.IsNullOrEmpty(this.Integrity))
            {
                htmlString.Append($" integrity=\"{WebUtility.HtmlEncode(this.Integrity)}\"");
            }
        }

        /// <summary>Renders the <c>referrerpolicy</c> attribute.</summary>
        /// <param name="htmlString">The HTML string builder to append to.</param>
        protected void RenderReferrerPolicy(StringBuilder htmlString)
        {
            switch (this.ReferrerPolicy)
            {
                case ReferrerPolicy.NoReferrer:
                    htmlString.Append(" referrerpolicy=\"no-referrer\"");
                    break;
                case ReferrerPolicy.NoReferrerWhenDowngrade:
                    htmlString.Append(" referrerpolicy=\"no-referrer-when-downgrade\"");
                    break;
                case ReferrerPolicy.Origin:
                    htmlString.Append(" referrerpolicy=\"origin\"");
                    break;
                case ReferrerPolicy.OriginWhenCrossOrigin:
                    htmlString.Append(" referrerpolicy=\"origin-when-cross-origin\"");
                    break;
                case ReferrerPolicy.SameOrigin:
                    htmlString.Append(" referrerpolicy=\"same-origin\"");
                    break;
                case ReferrerPolicy.StrictOrigin:
                    htmlString.Append(" referrerpolicy=\"strict-origin\"");
                    break;
                case ReferrerPolicy.StrictOriginWhenCrossOrigin:
                    htmlString.Append(" referrerpolicy=\"strict-origin-when-cross-origin\"");
                    break;
                case ReferrerPolicy.UnsafeUrl:
                    htmlString.Append(" referrerpolicy=\"unsafe-url\"");
                    break;
                case ReferrerPolicy.None:
                    return;
                default:
                    throw new InvalidOperationException($"Unexpected ReferrerPolicy value: {this.ReferrerPolicy}");
            }
        }

        /// <summary>Renders the <c>type</c> attribute.</summary>
        /// <param name="htmlString">The HTML string builder to append to.</param>
        protected void RenderType(StringBuilder htmlString)
        {
            if (!string.IsNullOrEmpty(this.Type))
            {
                htmlString.Append($" type=\"{WebUtility.HtmlEncode(this.Type)}\"");
            }
        }

        /// <summary>Renders the attributes.</summary>
        /// <param name="htmlString">The HTML string builder to append to.</param>
        protected void RenderAttributes(StringBuilder htmlString)
        {
            foreach (var attribute in this.Attributes)
            {
                htmlString.Append($" {attribute.Key}=\"{WebUtility.HtmlEncode(attribute.Value)}\"");
            }
        }
    }
}
