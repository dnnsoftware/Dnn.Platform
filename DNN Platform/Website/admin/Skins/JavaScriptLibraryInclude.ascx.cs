// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Framework.JavaScriptLibraries;

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
