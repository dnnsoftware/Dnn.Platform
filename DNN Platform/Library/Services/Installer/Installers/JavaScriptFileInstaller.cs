// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Xml.XPath;

    using DotNetNuke.Common;

    public class JavaScriptFileInstaller : FileInstaller
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
                return "js";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("jsfiles").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "jsfiles";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("jsfile").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "jsfile";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadCustomManifest method reads the custom manifest items (that subclasses
        /// of FileInstaller may need).
        /// </summary>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        /// -----------------------------------------------------------------------------
        protected override void ReadCustomManifest(XPathNavigator nav)
        {
            XPathNavigator libraryNav = nav.SelectSingleNode("libraryFolderName");
            if (libraryNav != null)
            {
                this.BasePath = string.Format("Resources\\Libraries\\{0}\\{1}", libraryNav.Value, Globals.FormatVersion(this.Package.Version, "00", 3, "_"));
            }
        }
    }
}
