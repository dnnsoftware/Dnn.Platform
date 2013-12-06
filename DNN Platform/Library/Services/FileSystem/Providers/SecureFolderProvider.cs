#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Services.FileSystem.Internal;

// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.FileSystem
// ReSharper restore CheckNamespace
{
    public class SecureFolderProvider : StandardFolderProvider
    {
        #region Public Properties

        /// <summary>
        /// Gets the file extension to use for protected files.
        /// </summary>
        public string ProtectedExtension
        {
            get
            {
                return Globals.glbProtectedExtension;
            }
        }

        /// <summary>
        /// Gets a value indicating if the provider ensures the files/folders it manages are secure from outside access.
        /// </summary>
        public override bool IsStorageSecure
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Abstract Methods

        public override string[] GetFiles(IFolderInfo folder)
        {
            Requires.NotNull("folder", folder);

            var fileNames = DirectoryWrapper.Instance.GetFiles(folder.PhysicalPath);

            for (var i = 0; i < fileNames.Length; i++)
            {
                var fileName = Path.GetFileName(fileNames[i]);
				if(!fileName.EndsWith(ProtectedExtension))
				{
					FileWrapper.Instance.Move(fileNames[i], fileNames[i] + ProtectedExtension);
				}
				else
				{
					fileName = fileName.Substring(0, fileName.LastIndexOf(ProtectedExtension));
				}

                fileNames[i] = fileName;
            }

            return fileNames;
        }

        public override string GetFileUrl(IFileInfo file)
        {
            return FileLinkClickController.Instance.GetFileLinkClick(file);
        }

        public override string GetFolderProviderIconPath()
        {
            return IconControllerWrapper.Instance.IconURL("FolderSecure", "32x32");
        }

        #endregion

        #region Protected Methods
        protected override string GetActualPath(FolderMappingInfo folderMapping, string folderPath, string fileName)
        {
            return base.GetActualPath(folderMapping, folderPath, fileName) + ProtectedExtension;
        }

        protected override string GetActualPath(IFileInfo file)
        {
            return base.GetActualPath(file) + ProtectedExtension;
        }

        protected override string GetActualPath(IFolderInfo folder, string fileName)
        {
            return base.GetActualPath(folder, fileName) + ProtectedExtension;
        }

        #endregion
    }
}