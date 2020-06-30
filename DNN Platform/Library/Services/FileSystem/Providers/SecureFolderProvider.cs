// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.FileSystem

// ReSharper restore CheckNamespace
{
    using System;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Services.FileSystem.Internal;

    public class SecureFolderProvider : StandardFolderProvider
    {
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
        /// Gets a value indicating whether gets a value indicating if the provider ensures the files/folders it manages are secure from outside access.
        /// </summary>
        public override bool IsStorageSecure
        {
            get
            {
                return true;
            }
        }

        public override string[] GetFiles(IFolderInfo folder)
        {
            Requires.NotNull("folder", folder);

            var fileNames = DirectoryWrapper.Instance.GetFiles(folder.PhysicalPath);

            for (var i = 0; i < fileNames.Length; i++)
            {
                var fileName = Path.GetFileName(fileNames[i]);
                if (!fileName.EndsWith(this.ProtectedExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    var destFileName = fileNames[i] + this.ProtectedExtension;
                    if (FileWrapper.Instance.Exists(destFileName))
                    {
                        FileWrapper.Instance.Delete(destFileName);
                    }

                    FileWrapper.Instance.Move(fileNames[i], destFileName);
                }
                else
                {
                    fileName = fileName.Substring(0, fileName.LastIndexOf(this.ProtectedExtension, StringComparison.InvariantCultureIgnoreCase));
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

        protected override string GetActualPath(FolderMappingInfo folderMapping, string folderPath, string fileName)
        {
            return base.GetActualPath(folderMapping, folderPath, fileName) + this.ProtectedExtension;
        }

        protected override string GetActualPath(IFileInfo file)
        {
            return base.GetActualPath(file) + this.ProtectedExtension;
        }

        protected override string GetActualPath(IFolderInfo folder, string fileName)
        {
            return base.GetActualPath(folder, fileName) + this.ProtectedExtension;
        }
    }
}
