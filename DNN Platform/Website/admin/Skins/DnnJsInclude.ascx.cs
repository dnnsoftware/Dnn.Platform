// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>A control which causes JavaScript to be included on the page.</summary>
    public partial class DnnJsInclude : SkinObjectBase
    {
        public string FilePath
        {
            get => this.ctlInclude.FilePath;
            set => this.ctlInclude.FilePath = value;
        }

        public string PathNameAlias
        {
            get => this.ctlInclude.PathNameAlias;
            set => this.ctlInclude.PathNameAlias = value;
        }

        public int Priority
        {
            get => this.ctlInclude.Priority;
            set => this.ctlInclude.Priority = value;
        }

        public bool AddTag
        {
            get => this.ctlInclude.AddTag;
            set => this.ctlInclude.AddTag = value;
        }

        public string Name
        {
            get => this.ctlInclude.Name;
            set => this.ctlInclude.Name = value;
        }

        public string Version
        {
            get => this.ctlInclude.Version;
            set => this.ctlInclude.Version = value;
        }

        public bool ForceVersion
        {
            get => this.ctlInclude.ForceVersion;
            set => this.ctlInclude.ForceVersion = value;
        }

        public string ForceProvider
        {
            get => this.ctlInclude.ForceProvider;
            set => this.ctlInclude.ForceProvider = value;
        }

        [Obsolete("Deprecated in DotNetNuke 10.2.0. Bundling is no longer supported, there is no replacement within DNN for this functionality. Scheduled removal in v12.0.0.")]
        public bool ForceBundle { get; set; }

        /// <summary>Gets or sets the CDN URL of the resource.</summary>
        public string CdnUrl
        {
            get => this.ctlInclude.CdnUrl;
            set => this.ctlInclude.CdnUrl = value;
        }

        /// <summary>Gets or sets a value indicating whether to render the <c>blocking</c> attribute.</summary>
        public bool Blocking
        {
            get => this.ctlInclude.Blocking;
            set => this.ctlInclude.Blocking = value;
        }

        /// <summary>Gets or sets the integrity hash of the resource.</summary>
        public string Integrity
        {
            get => this.ctlInclude.Integrity;
            set => this.ctlInclude.Integrity = value;
        }

        /// <summary>Gets or sets the value of the <c>crossorigin</c> attribute.</summary>
        public CrossOrigin CrossOrigin
        {
            get => this.ctlInclude.CrossOrigin;
            set => this.ctlInclude.CrossOrigin = value;
        }

        /// <summary>Gets or sets the value of the <c>fetchpriority</c> attribute.</summary>
        public FetchPriority FetchPriority
        {
            get => this.ctlInclude.FetchPriority;
            set => this.ctlInclude.FetchPriority = value;
        }

        /// <summary>Gets or sets the value of the <c>referrerpolicy</c> attribute.</summary>
        public ReferrerPolicy ReferrerPolicy
        {
            get => this.ctlInclude.ReferrerPolicy;
            set => this.ctlInclude.ReferrerPolicy = value;
        }

        /// <inheritdoc cref="IScriptResource.Async" />
        public bool Async
        {
            get => this.ctlInclude.Async;
            set => this.ctlInclude.Async = value;
        }

        /// <inheritdoc cref="IScriptResource.Defer" />
        public bool Defer
        {
            get => this.ctlInclude.Defer;
            set => this.ctlInclude.Defer = value;
        }

        /// <inheritdoc cref="IScriptResource.NoModule" />
        public bool NoModule
        {
            get => this.ctlInclude.NoModule;
            set => this.ctlInclude.NoModule = value;
        }
    }
}
