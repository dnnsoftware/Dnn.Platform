﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
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
                if (!fileName.EndsWith(ProtectedExtension, StringComparison.InvariantCultureIgnoreCase))
				{
                    var destFileName = fileNames[i] + ProtectedExtension;
                    if (FileWrapper.Instance.Exists(destFileName))
                        FileWrapper.Instance.Delete(destFileName);
					FileWrapper.Instance.Move(fileNames[i], destFileName);
				}
				else
				{
                    fileName = fileName.Substring(0, fileName.LastIndexOf(ProtectedExtension, StringComparison.InvariantCultureIgnoreCase));
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
