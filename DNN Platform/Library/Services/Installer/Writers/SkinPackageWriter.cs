// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.UI.Skins;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SkinPackageWriter class.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SkinPackageWriter : PackageWriterBase
    {
        private readonly SkinPackageInfo _SkinPackage;
        private readonly string _SubFolder;

        public SkinPackageWriter(PackageInfo package)
            : base(package)
        {
            this._SkinPackage = SkinController.GetSkinByPackageID(package.PackageID);
            this.SetBasePath();
        }

        public SkinPackageWriter(SkinPackageInfo skinPackage, PackageInfo package)
            : base(package)
        {
            this._SkinPackage = skinPackage;
            this.SetBasePath();
        }

        public SkinPackageWriter(SkinPackageInfo skinPackage, PackageInfo package, string basePath)
            : base(package)
        {
            this._SkinPackage = skinPackage;
            this.BasePath = basePath;
        }

        public SkinPackageWriter(SkinPackageInfo skinPackage, PackageInfo package, string basePath, string subFolder)
            : base(package)
        {
            this._SkinPackage = skinPackage;
            this._SubFolder = subFolder;
            this.BasePath = Path.Combine(basePath, subFolder);
        }

        public override bool IncludeAssemblies
        {
            get
            {
                return false;
            }
        }

        protected SkinPackageInfo SkinPackage
        {
            get
            {
                return this._SkinPackage;
            }
        }

        public void SetBasePath()
        {
            if (this._SkinPackage.SkinType == "Skin")
            {
                this.BasePath = Path.Combine("Portals\\_default\\Skins", this.SkinPackage.SkinName);
            }
            else
            {
                this.BasePath = Path.Combine("Portals\\_default\\Containers", this.SkinPackage.SkinName);
            }
        }

        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
            // Call base class method with includeAppCode = false
            base.GetFiles(includeSource, false);
        }

        protected override void ParseFiles(DirectoryInfo folder, string rootPath)
        {
            // Add the Files in the Folder
            FileInfo[] files = folder.GetFiles();
            foreach (FileInfo file in files)
            {
                string filePath = folder.FullName.Replace(rootPath, string.Empty);
                if (filePath.StartsWith("\\"))
                {
                    filePath = filePath.Substring(1);
                }

                if (!file.Extension.Equals(".dnn", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (string.IsNullOrEmpty(this._SubFolder))
                    {
                        this.AddFile(Path.Combine(filePath, file.Name));
                    }
                    else
                    {
                        filePath = Path.Combine(filePath, file.Name);
                        this.AddFile(filePath, Path.Combine(this._SubFolder, filePath));
                    }
                }
            }
        }

        protected override void WriteFilesToManifest(XmlWriter writer)
        {
            var skinFileWriter = new SkinComponentWriter(this.SkinPackage.SkinName, this.BasePath, this.Files, this.Package);
            if (this.SkinPackage.SkinType == "Skin")
            {
                skinFileWriter = new SkinComponentWriter(this.SkinPackage.SkinName, this.BasePath, this.Files, this.Package);
            }
            else
            {
                skinFileWriter = new ContainerComponentWriter(this.SkinPackage.SkinName, this.BasePath, this.Files, this.Package);
            }

            skinFileWriter.WriteManifest(writer);
        }
    }
}
