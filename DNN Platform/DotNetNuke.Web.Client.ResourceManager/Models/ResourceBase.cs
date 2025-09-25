// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Base class for all resource types.
    /// </summary>
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
        public string Key { get; set; }

        /// <inheritdoc />
        public string CdnUrl { get; set; }

        /// <inheritdoc />
        public int Priority { get; set; }

        /// <inheritdoc />
        public string Provider { get; set; } = ClientResourceProviders.DnnPageHeaderProvider;

        /// <inheritdoc />
        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(this.name) ? this.FilePath : this.name;
            }
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
        public bool Preload { get; set; } = false;

        /// <inheritdoc />
        public string Integrity { get; set; } = string.Empty;

        /// <inheritdoc />
        public bool Blocking { get; set; } = false;

        /// <inheritdoc />
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        /// <inheritdoc />
        public void Register()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public string Render(int crmVersion, bool useCdn, string applicationPath)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the versioned path.
        /// </summary>
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
            else if (path.ToLowerInvariant().StartsWith("http"))
            {
                return path;
            }
            else if (path.StartsWith("~"))
            {
                if (string.IsNullOrEmpty(applicationPath))
                {
                    return path.TrimStart('~');
                }
                else
                {
                    return path.Replace("~", applicationPath);
                }
            }
            else if (path.StartsWith("/"))
            {
                return $"{path}?cdv={crmVersion}";
            }

            return $"{applicationPath}/{path}?cdv={crmVersion}";
        }

        /// <summary>
        /// Renders the blocking attribute.
        /// </summary>
        /// <returns>The blocking attribute.</returns>
        protected string RenderBlocking()
        {
            if (this.Blocking)
            {
                return " blocking=\"render\"";
            }

            return string.Empty;
        }

        /// <summary>
        /// Renders the cross origin attribute.
        /// </summary>
        /// <returns>The cross origin attribute.</returns>
        protected string RenderCrossOriginAttribute()
        {
            if (this.CrossOrigin != CrossOrigin.None)
            {
                var crossOrigin = "anonymous";
                if (this.CrossOrigin == CrossOrigin.UseCredentials)
                {
                    crossOrigin = "use-credentials";
                }

                return $" crossorigin=\"{crossOrigin}\"";
            }

            return string.Empty;
        }

        /// <summary>
        /// Renders the fetch priority attribute.
        /// </summary>
        /// <returns>The fetch priority attribute.</returns>
        protected string RenderFetchPriority()
        {
            if (this.FetchPriority != FetchPriority.Auto)
            {
                var fetchPriority = "low";
                if (this.FetchPriority == FetchPriority.High)
                {
                    fetchPriority = "high";
                }

                return $" fetchpriority=\"{fetchPriority}\"";
            }

            return string.Empty;
        }

        /// <summary>
        /// Renders the integrity attribute.
        /// </summary>
        /// <returns>The integrity attribute.</returns>
        protected string RenderIntegrity()
        {
            if (!string.IsNullOrEmpty(this.Integrity))
            {
                return $" integrity=\"{this.Integrity}\"";
            }

            return string.Empty;
        }

        /// <summary>
        /// Renders the referrer policy attribute.
        /// </summary>
        /// <returns>The referrer policy attribute.</returns>
        protected string RenderReferrerPolicy()
        {
            if (this.ReferrerPolicy != ReferrerPolicy.None)
            {
                switch (this.ReferrerPolicy)
                {
                    case ReferrerPolicy.NoReferrer:
                        return " referrerpolicy=\"no-referrer\"";
                    case ReferrerPolicy.NoReferrerWhenDowngrade:
                        return " referrerpolicy=\"no-referrer-when-downgrade\"";
                    case ReferrerPolicy.Origin:
                        return " referrerpolicy=\"origin\"";
                    case ReferrerPolicy.OriginWhenCrossOrigin:
                        return " referrerpolicy=\"origin-when-cross-origin\"";
                    case ReferrerPolicy.SameOrigin:
                        return " referrerpolicy=\"same-origin\"";
                    case ReferrerPolicy.StrictOrigin:
                        return " referrerpolicy=\"strict-origin\"";
                    case ReferrerPolicy.StrictOriginWhenCrossOrigin:
                        return " referrerpolicy=\"strict-origin-when-cross-origin\"";
                    case ReferrerPolicy.UnsafeUrl:
                        return " referrerpolicy=\"unsafe-url\"";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Renders the attributes.
        /// </summary>
        /// <returns>The attributes.</returns>
        protected string RenderAttributes()
        {
            var htmlString = string.Empty;
            foreach (var attribute in this.Attributes)
            {
                htmlString += $" {attribute.Key}=\"{attribute.Value}\"";
            }

            return htmlString;
        }
    }
}
