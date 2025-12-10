// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.Cdf;

    /// <summary>A control which causes CSS to be included on the page.</summary>
    public partial class DnnCssInclude : SkinObjectBase
    {
        public CssMediaType CssMedia { get; set; } = CssMediaType.None;

        public string FilePath { get; set; }

        public string PathNameAlias { get; set; }

        public int Priority { get; set; }

        public bool AddTag { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public bool ForceVersion { get; set; }

        public string ForceProvider { get; set; }

        [Obsolete("Deprecated in DotNetNuke 10.2.0. Bundling is no longer supported, there is no replacement within DNN for this functionality. Scheduled removal in v12.0.0.")]
        public bool ForceBundle { get; set; }

        /// <summary>Gets or sets the CDN URL of the resource.</summary>
        public string CdnUrl { get; set; }

        /// <summary>Gets or sets a value indicating whether to render the <c>blocking</c> attribute.</summary>
        public bool Blocking { get; set; }

        /// <summary>Gets or sets the integrity hash of the resource.</summary>
        public string Integrity { get; set; }

        /// <summary>Gets or sets the value of the <c>crossorigin</c> attribute.</summary>
        public CrossOrigin CrossOrigin { get; set; }

        /// <summary>Gets or sets the value of the <c>fetchpriority</c> attribute.</summary>
        public FetchPriority FetchPriority { get; set; }

        /// <summary>Gets or sets the value of the <c>referrerpolicy</c> attribute.</summary>
        public ReferrerPolicy ReferrerPolicy { get; set; }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.ctlInclude.AddTag = this.AddTag;
            this.ctlInclude.FilePath = this.FilePath;
            this.ctlInclude.ForceProvider = this.ForceProvider;
            this.ctlInclude.ForceVersion = this.ForceVersion;
            this.ctlInclude.Name = this.Name;
            this.ctlInclude.PathNameAlias = this.PathNameAlias;
            this.ctlInclude.Priority = this.Priority;
            this.ctlInclude.Version = this.Version;
            this.ctlInclude.CdnUrl = this.CdnUrl;
            this.ctlInclude.Blocking = this.Blocking;
            this.ctlInclude.Integrity = this.Integrity;
            this.ctlInclude.CrossOrigin = this.CrossOrigin;
            this.ctlInclude.FetchPriority = this.FetchPriority;
            this.ctlInclude.ReferrerPolicy = this.ReferrerPolicy;
            if (this.CssMedia != CssMediaType.None)
            {
                this.ctlInclude.CssMedia = this.CssMedia.ToString().ToLowerInvariant();
            }
        }
    }
}
