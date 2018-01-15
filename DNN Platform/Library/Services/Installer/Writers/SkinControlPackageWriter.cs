#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
            BasePath = Path.Combine("DesktopModules", package.Name.ToLower()).Replace("/", "\\");
            AppCodePath = Path.Combine("App_Code", package.Name.ToLower()).Replace("/", "\\");
        }

        public SkinControlPackageWriter(SkinControlInfo skinControl, PackageInfo package) : base(package)
        {
            SkinControl = skinControl;
            BasePath = Path.Combine("DesktopModules", package.Name.ToLower()).Replace("/", "\\");
            AppCodePath = Path.Combine("App_Code", package.Name.ToLower()).Replace("/", "\\");
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

            BasePath = Path.Combine("DesktopModules", Package.Name.ToLower()).Replace("/", "\\");
            AppCodePath = Path.Combine("App_Code", Package.Name.ToLower()).Replace("/", "\\");
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
                    SkinControl.ControlSrc = Path.Combine(Path.Combine("DesktopModules", Package.Name.ToLower()), Util.ReadElement(controlNav, "src")).Replace("\\", "/");
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
