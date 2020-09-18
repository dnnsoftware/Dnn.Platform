// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Installer.Packages;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SkinControlPackageWriter class.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SkinControlPackageWriter : PackageWriterBase
    {
        public SkinControlPackageWriter(PackageInfo package)
            : base(package)
        {
            this.SkinControl = SkinControlController.GetSkinControlByPackageID(package.PackageID);
            this.BasePath = Path.Combine("DesktopModules", package.Name.ToLowerInvariant()).Replace("/", "\\");
            this.AppCodePath = Path.Combine("App_Code", package.Name.ToLowerInvariant()).Replace("/", "\\");
        }

        public SkinControlPackageWriter(SkinControlInfo skinControl, PackageInfo package)
            : base(package)
        {
            this.SkinControl = skinControl;
            this.BasePath = Path.Combine("DesktopModules", package.Name.ToLowerInvariant()).Replace("/", "\\");
            this.AppCodePath = Path.Combine("App_Code", package.Name.ToLowerInvariant()).Replace("/", "\\");
        }

        public SkinControlPackageWriter(XPathNavigator manifestNav, InstallerInfo installer)
        {
            this.SkinControl = new SkinControlInfo();

            // Create a Package
            this.Package = new PackageInfo(installer);

            this.ReadLegacyManifest(manifestNav, true);

            this.Package.Description = Null.NullString;
            this.Package.Version = new Version(1, 0, 0);
            this.Package.PackageType = "SkinObject";
            this.Package.License = Util.PACKAGE_NoLicense;

            this.BasePath = Path.Combine("DesktopModules", this.Package.Name.ToLowerInvariant()).Replace("/", "\\");
            this.AppCodePath = Path.Combine("App_Code", this.Package.Name.ToLowerInvariant()).Replace("/", "\\");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the associated SkinControl.
        /// </summary>
        /// <value>A SkinControlInfo object.</value>
        /// -----------------------------------------------------------------------------
        public SkinControlInfo SkinControl { get; set; }

        protected override void WriteManifestComponent(XmlWriter writer)
        {
            // Start component Element
            writer.WriteStartElement("component");
            writer.WriteAttributeString("type", "SkinObject");

            // Write SkinControl Manifest
            CBO.SerializeObject(this.SkinControl, writer);

            // End component Element
            writer.WriteEndElement();
        }

        private void ReadLegacyManifest(XPathNavigator legacyManifest, bool processModule)
        {
            XPathNavigator folderNav = legacyManifest.SelectSingleNode("folders/folder");

            if (processModule)
            {
                this.Package.Name = Util.ReadElement(folderNav, "name");
                this.Package.FriendlyName = this.Package.Name;

                // Process legacy controls Node
                foreach (XPathNavigator controlNav in folderNav.Select("modules/module/controls/control"))
                {
                    this.SkinControl.ControlKey = Util.ReadElement(controlNav, "key");
                    this.SkinControl.ControlSrc = Path.Combine(Path.Combine("DesktopModules", this.Package.Name.ToLowerInvariant()), Util.ReadElement(controlNav, "src")).Replace("\\", "/");
                    string supportsPartialRendering = Util.ReadElement(controlNav, "supportspartialrendering");
                    if (!string.IsNullOrEmpty(supportsPartialRendering))
                    {
                        this.SkinControl.SupportsPartialRendering = bool.Parse(supportsPartialRendering);
                    }
                }
            }

            // Process legacy files Node
            foreach (XPathNavigator fileNav in folderNav.Select("files/file"))
            {
                string fileName = Util.ReadElement(fileNav, "name");
                string filePath = Util.ReadElement(fileNav, "path");

                this.AddFile(Path.Combine(filePath, fileName), fileName);
            }

            // Process resource file Node
            if (!string.IsNullOrEmpty(Util.ReadElement(folderNav, "resourcefile")))
            {
                this.AddFile(Util.ReadElement(folderNav, "resourcefile"));
            }
        }
    }
}
