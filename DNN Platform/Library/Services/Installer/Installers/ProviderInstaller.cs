// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System.Xml.XPath;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProviderInstaller installs Provider Components to a DotNetNuke site.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ProviderInstaller : ComponentInstallerBase
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List).
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
                return "ashx, aspx, ascx, vb, cs, resx, css, js, resources, config, xml, htc, html, htm, text, vbproj, csproj, sln";
            }
        }

        public override void Commit()
        {
            this.Completed = true;
        }

        public override void Install()
        {
            this.Completed = true;
        }

        public override void ReadManifest(XPathNavigator manifestNav)
        {
        }

        public override void Rollback()
        {
            this.Completed = true;
        }

        public override void UnInstall()
        {
            this.Completed = true;
        }
    }
}
