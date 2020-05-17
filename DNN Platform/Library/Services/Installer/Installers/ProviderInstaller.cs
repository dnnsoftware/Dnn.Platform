﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Xml.XPath;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProviderInstaller installs Provider Components to a DotNetNuke site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ProviderInstaller : ComponentInstallerBase
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
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
            Completed = true;
        }

        public override void Install()
        {
            Completed = true;
        }

        public override void ReadManifest(XPathNavigator manifestNav)
        {
        }

        public override void Rollback()
        {
            Completed = true;
        }

        public override void UnInstall()
        {
            Completed = true;
        }
    }
}
