// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Xml.XPath;

using DotNetNuke.Common;

namespace DotNetNuke.Services.Installer.Installers
{
    public class JavaScriptFileInstaller : FileInstaller
    {
 		#region Protected Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("jsfiles")
        /// </summary>
        /// <value>A String</value>
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
        /// Gets the name of the Item Node ("jsfile")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "jsfile";
            }
        }

		#endregion  
 
        #region Public Properties

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
                return "js";
            }
        }


        #endregion

        #region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadCustomManifest method reads the custom manifest items (that subclasses
        /// of FileInstaller may need)
        /// </summary>
        /// <param name="nav">The XPathNavigator representing the node</param>
        /// -----------------------------------------------------------------------------
        protected override void ReadCustomManifest(XPathNavigator nav)
        {
            XPathNavigator libraryNav = nav.SelectSingleNode("libraryFolderName");
            if (libraryNav != null)
            {
                BasePath = String.Format("Resources\\Libraries\\{0}\\{1}", libraryNav.Value, Globals.FormatVersion(Package.Version, "00", 3, "_"));
            }
        }


        #endregion

    }
}
