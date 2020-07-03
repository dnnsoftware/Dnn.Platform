// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.FileSystem

// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;

    public class DatabaseFolderProvider : SecureFolderProvider
    {
        /// <summary>
        /// Clears the content of the file in the database.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        public static void ClearFileContent(int fileId)
        {
            DataProvider.Instance().ClearFileContent(fileId);
            DataProvider.Instance().UpdateFileVersion(fileId, Guid.NewGuid());
        }

        /// <summary>
        /// Updates the content of the file in the database.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="content">The new content.</param>
        public static void UpdateFileContent(int fileId, Stream content)
        {
            if (content != null)
            {
                byte[] fileContent;
                var buffer = new byte[16 * 1024];
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = content.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }

                    fileContent = ms.ToArray();
                }

                UpdateFileContent(fileId, fileContent);
            }
            else
            {
                ClearFileContent(fileId);
            }

            DataProvider.Instance().UpdateFileVersion(fileId, Guid.NewGuid());
        }

        /// <summary>
        /// Updates the content of the file in the database.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="content">The new content.</param>
        public static void UpdateFileContent(int fileId, byte[] content)
        {
            if (content != null)
            {
                DataProvider.Instance().UpdateFileContent(fileId, content);
                DataProvider.Instance().UpdateFileVersion(fileId, Guid.NewGuid());
            }
            else
            {
                ClearFileContent(fileId);
            }
        }

        public override void CopyFile(string folderPath, string fileName, string newFolderPath, FolderMappingInfo folderMapping)
        {
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.PropertyNotNull("newFolderPath", newFolderPath);
            Requires.NotNull("folderMapping", folderMapping);

            if (folderPath == newFolderPath)
            {
                return;
            }

            var sourceFolder = FolderManager.Instance.GetFolder(folderMapping.PortalID, folderPath);
            var destinationFolder = FolderManager.Instance.GetFolder(folderMapping.PortalID, newFolderPath);

            Requires.NotNull("sourceFolder", sourceFolder);
            Requires.NotNull("destinationFolder", destinationFolder);

            using (var fileContent = this.GetFileStream(sourceFolder, fileName))
            {
                if (!fileContent.CanSeek)
                {
                    using (var seekableStream = FileManager.Instance.GetSeekableStream(fileContent))
                    {
                        this.AddFile(destinationFolder, fileName, seekableStream);
                    }
                }
                else
                {
                    this.AddFile(destinationFolder, fileName, fileContent);
                }
            }
        }

        public override void AddFile(IFolderInfo folder, string fileName, Stream content)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            this.UpdateFile(folder, fileName, content);
        }

        public override void DeleteFile(IFileInfo file)
        {
            Requires.NotNull("file", file);

            ClearFileContent(file.FileId);
        }

        public override bool FileExists(IFolderInfo folder, string fileName)
        {
            Requires.NotNull("folder", folder);
            Requires.PropertyNotNull("fileName", fileName);

            return FileManager.Instance.GetFile(folder, fileName, true) != null;
        }

        public override bool FolderExists(string folderPath, FolderMappingInfo folderMapping)
        {
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            return FolderManager.Instance.GetFolder(folderMapping.PortalID, folderPath) != null;
        }

        public override FileAttributes? GetFileAttributes(IFileInfo file)
        {
            return null;
        }

        public override string[] GetFiles(IFolderInfo folder)
        {
            Requires.NotNull("folder", folder);

            return FolderManager.Instance.GetFiles(folder).Select(file => file.FileName).ToArray();
        }

        public override long GetFileSize(IFileInfo file)
        {
            Requires.NotNull("file", file);

            return file.Size;
        }

        public override Stream GetFileStream(IFileInfo file)
        {
            Requires.NotNull("file", file);

            return this.GetFileStreamInternal(DataProvider.Instance().GetFileContent(file.FileId));
        }

        public override Stream GetFileStream(IFolderInfo folder, string fileName)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            var file = FileManager.Instance.GetFile(folder, fileName, true);

            return file != null ? this.GetFileStreamInternal(DataProvider.Instance().GetFileContent(file.FileId)) : null;
        }

        public override Stream GetFileStream(IFolderInfo folder, IFileInfo file, int version)
        {
            Requires.NotNull("file", file);

            return file != null ? this.GetFileStreamInternal(DataProvider.Instance().GetFileVersionContent(file.FileId, version)) : null;
        }

        public override string GetFolderProviderIconPath()
        {
            return IconControllerWrapper.Instance.IconURL("FolderDatabase", "32x32");
        }

        public override DateTime GetLastModificationTime(IFileInfo file)
        {
            return file.LastModificationTime;
        }

        public override IEnumerable<string> GetSubFolders(string folderPath, FolderMappingInfo folderMapping)
        {
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            var folderManager = FolderManager.Instance;

            var folder = folderManager.GetFolder(folderMapping.PortalID, folderPath);

            return folderManager.GetFolders(folder).Select(subfolder => subfolder.FolderPath);
        }

        public override bool IsInSync(IFileInfo file)
        {
            return true;
        }

        public override void MoveFile(IFileInfo file, IFolderInfo destinationFolder)
        {
        }

        public override void RenameFile(IFileInfo file, string newFileName)
        {
        }

        public override void RenameFolder(IFolderInfo folder, string newFolderName)
        {
        }

        public override void SetFileAttributes(IFileInfo file, FileAttributes fileAttributes)
        {
        }

        public override bool SupportsFileAttributes()
        {
            return false;
        }

        public override void UpdateFile(IFileInfo file, Stream content)
        {
            Requires.NotNull("file", file);

            this.UpdateFileInternal(file.FileId, content);
        }

        public override void UpdateFile(IFolderInfo folder, string fileName, Stream content)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            var file = FileManager.Instance.GetFile(folder, fileName, true);

            if (file == null)
            {
                return;
            }

            this.UpdateFileInternal(file.FileId, content);
        }

        private Stream GetFileStreamInternal(IDataReader dr)
        {
            byte[] bytes = null;
            try
            {
                if (dr.Read())
                {
                    bytes = (byte[])dr["Content"];
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return bytes != null ? new MemoryStream(bytes) : null;
        }

        private void UpdateFileInternal(int fileId, Stream content)
        {
            byte[] fileContent = null;

            if (content != null)
            {
                var restorePosition = content.CanSeek;
                long originalPosition = Null.NullInteger;

                if (restorePosition)
                {
                    originalPosition = content.Position;
                    content.Position = 0;
                }

                var buffer = new byte[16 * 1024];

                using (var ms = new MemoryStream())
                {
                    int read;

                    while ((read = content.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }

                    fileContent = ms.ToArray();
                }

                if (restorePosition)
                {
                    content.Position = originalPosition;
                }
            }

            UpdateFileContent(fileId, fileContent);
        }
    }
}
