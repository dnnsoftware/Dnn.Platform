// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using ClientDependency.Core;
    using ClientDependency.Core.Controls;

    public partial class DnnCssInclude : SkinObjectBase
    {
        public CssMediaType CssMedia { get; set; }

        public string FilePath { get; set; }

        public string PathNameAlias { get; set; }

        public int Priority { get; set; }

        public bool AddTag { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public bool ForceVersion { get; set; }

        public string ForceProvider { get; set; }

        public bool ForceBundle { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.ctlInclude.AddTag = this.AddTag;
            this.ctlInclude.CssMedia = this.CssMedia;
            this.ctlInclude.FilePath = this.FilePath;
            this.ctlInclude.ForceBundle = this.ForceBundle;
            this.ctlInclude.ForceProvider = this.ForceProvider;
            this.ctlInclude.ForceVersion = this.ForceVersion;
            this.ctlInclude.Name = this.Name;
            this.ctlInclude.PathNameAlias = this.PathNameAlias;
            this.ctlInclude.Priority = this.Priority;
            this.ctlInclude.Version = this.Version;
        }
    }
}
