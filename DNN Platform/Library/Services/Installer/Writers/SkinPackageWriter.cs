// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.IO;
using System.Xml;

using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SkinPackageWriter class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SkinPackageWriter : PackageWriterBase
    {
		#region "Private Members"

        private readonly SkinPackageInfo _SkinPackage;
        private readonly string _SubFolder;
		
		#endregion

		#region "Constructors"

        public SkinPackageWriter(PackageInfo package) : base(package)
        {
            _SkinPackage = SkinController.GetSkinByPackageID(package.PackageID);
            SetBasePath();
        }

        public SkinPackageWriter(SkinPackageInfo skinPackage, PackageInfo package) : base(package)
        {
            _SkinPackage = skinPackage;
            SetBasePath();
        }

        public SkinPackageWriter(SkinPackageInfo skinPackage, PackageInfo package, string basePath) : base(package)
        {
            _SkinPackage = skinPackage;
            BasePath = basePath;
        }

        public SkinPackageWriter(SkinPackageInfo skinPackage, PackageInfo package, string basePath, string subFolder) : base(package)
        {
            _SkinPackage = skinPackage;
            _SubFolder = subFolder;
            BasePath = Path.Combine(basePath, subFolder);
        }
		
		#endregion

		#region "Protected Properties"
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
                return _SkinPackage;
            }
        }
		
		#endregion

        public void SetBasePath()
        {
            if (_SkinPackage.SkinType == "Skin")
            {
                BasePath = Path.Combine("Portals\\_default\\Skins", SkinPackage.SkinName);
            }
            else
            {
                BasePath = Path.Combine("Portals\\_default\\Containers", SkinPackage.SkinName);
            }
        }

        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
			//Call base class method with includeAppCode = false
            base.GetFiles(includeSource, false);
        }

        protected override void ParseFiles(DirectoryInfo folder, string rootPath)
        {
			//Add the Files in the Folder
            FileInfo[] files = folder.GetFiles();
            foreach (FileInfo file in files)
            {
                string filePath = folder.FullName.Replace(rootPath, "");
                if (filePath.StartsWith("\\"))
                {
                    filePath = filePath.Substring(1);
                }
                if (!file.Extension.Equals(".dnn", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (string.IsNullOrEmpty(_SubFolder))
                    {
                        AddFile(Path.Combine(filePath, file.Name));
                    }
                    else
                    {
                        filePath = Path.Combine(filePath, file.Name);
                        AddFile(filePath, Path.Combine(_SubFolder, filePath));
                    }
                }
            }
        }

        protected override void WriteFilesToManifest(XmlWriter writer)
        {
            var skinFileWriter = new SkinComponentWriter(SkinPackage.SkinName, BasePath, Files, Package);
            if (SkinPackage.SkinType == "Skin")
            {
                skinFileWriter = new SkinComponentWriter(SkinPackage.SkinName, BasePath, Files, Package);
            }
            else
            {
                skinFileWriter = new ContainerComponentWriter(SkinPackage.SkinName, BasePath, Files, Package);
            }
            skinFileWriter.WriteManifest(writer);
        }
    }
}
