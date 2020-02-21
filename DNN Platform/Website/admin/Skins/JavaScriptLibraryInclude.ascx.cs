// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Framework.JavaScriptLibraries;

namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    public partial class JavaScriptLibraryInclude : SkinObjectBase
    {
        public string Name { get; set; }
        public Version Version { get; set; }
        public SpecificVersion? SpecificVersion { get; set; }

        protected override void OnInit(EventArgs e)
        {
            if (this.Version == null) 
            {
                JavaScript.RequestRegistration(this.Name);
            }
            else if (this.SpecificVersion == null)
            {
                JavaScript.RequestRegistration(this.Name, this.Version);
            }
            else
            {
                JavaScript.RequestRegistration(this.Name, this.Version, this.SpecificVersion.Value);
            }
        }
    }
}
