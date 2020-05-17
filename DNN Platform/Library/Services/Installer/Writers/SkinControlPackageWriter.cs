﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SkinControlPackageWriter class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SkinControlPackageWriter : PackageWriterBase
    {
		#region "Constructors"
		
        public SkinControlPackageWriter(PackageInfo package) : base(package)
        {
            SkinControl = SkinControlController.GetSkinControlByPackageID(package.PackageID);
            BasePath = Path.Combine("DesktopModules", package.Name.ToLowerInvariant()).Replace("/", "\\");
            AppCodePath = Path.Combine("App_Code", package.Name.ToLowerInvariant()).Replace("/", "\\");
        }

        public SkinControlPackageWriter(SkinControlInfo skinControl, PackageInfo package) : base(package)
        {
            SkinControl = skinControl;
            BasePath = Path.Combine("DesktopModules", package.Name.ToLowerInvariant()).Replace("/", "\\");
            AppCodePath = Path.Combine("App_Code", package.Name.ToLowerInvariant()).Replace("/", "\\");
        }

        public SkinControlPackageWriter(XPathNavigator manifestNav, InstallerInfo installer)
        {
            SkinControl = new SkinControlInfo();

            //Create a Package
            Package = new PackageInfo(installer);

            ReadLegacyManifest(manifestNav, true);

            Package.Description = Null.NullString;
            Package.Version = new Version(1, 0, 0);
            Package.PackageType = "SkinObject";
            Package.License = Util.PACKAGE_NoLicense;

            BasePath = Path.Combine("DesktopModules", Package.Name.ToLowerInvariant()).Replace("/", "\\");
            AppCodePath = Path.Combine("App_Code", Package.Name.ToLowerInvariant()).Replace("/", "\\");
        }
		
		#endregion

		#region "Public Properties"

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets the associated SkinControl
		/// </summary>
		/// <value>A SkinControlInfo object</value>
		/// -----------------------------------------------------------------------------
        public SkinControlInfo SkinControl { get; set; }
		
		#endregion

        private void ReadLegacyManifest(XPathNavigator legacyManifest, bool processModule)
        {
            XPathNavigator folderNav = legacyManifest.SelectSingleNode("folders/folder");

            if (processModule)
            {
                Package.Name = Util.ReadElement(folderNav, "name");
                Package.FriendlyName = Package.Name;

                //Process legacy controls Node
                foreach (XPathNavigator controlNav in folderNav.Select("modules/module/controls/control"))
                {
                    SkinControl.ControlKey = Util.ReadElement(controlNav, "key");
                    SkinControl.ControlSrc = Path.Combine(Path.Combine("DesktopModules", Package.Name.ToLowerInvariant()), Util.ReadElement(controlNav, "src")).Replace("\\", "/");
                    string supportsPartialRendering = Util.ReadElement(controlNav, "supportspartialrendering");
                    if (!string.IsNullOrEmpty(supportsPartialRendering))
                    {
                        SkinControl.SupportsPartialRendering = bool.Parse(supportsPartialRendering);
                    }
                }
            }
			
            //Process legacy files Node
            foreach (XPathNavigator fileNav in folderNav.Select("files/file"))
            {
                string fileName = Util.ReadElement(fileNav, "name");
                string filePath = Util.ReadElement(fileNav, "path");

                AddFile(Path.Combine(filePath, fileName), fileName);
            }
			
            //Process resource file Node
            if (!string.IsNullOrEmpty(Util.ReadElement(folderNav, "resourcefile")))
            {
                AddFile(Util.ReadElement(folderNav, "resourcefile"));
            }
        }

		#region "Protected Methods"
		
        protected override void WriteManifestComponent(XmlWriter writer)
        {
			//Start component Element
            writer.WriteStartElement("component");
            writer.WriteAttributeString("type", "SkinObject");

            //Write SkinControl Manifest
            CBO.SerializeObject(SkinControl, writer);

            //End component Element
            writer.WriteEndElement();
        }
		
		#endregion
    }
}
