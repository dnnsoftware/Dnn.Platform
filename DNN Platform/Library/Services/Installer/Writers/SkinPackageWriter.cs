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
                if (file.Extension.ToLowerInvariant() != ".dnn")
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
