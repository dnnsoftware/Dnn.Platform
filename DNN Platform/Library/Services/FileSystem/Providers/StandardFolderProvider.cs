// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.FileSystem

// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem.Internal;

    public class StandardFolderProvider : FolderProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(StandardFolderProvider));

        private static readonly char[] InvalidFileUrlChars = new char[] { '%', ';', '?', ':', '@', '&', '=', '+', '$', ',' };

        /// <summary>
        /// Gets a value indicating whether gets a value indicating if the provider requires network connectivity to do its tasks.
        /// </summary>
        public override bool RequiresNetworkConnectivity
        {
            get
            {
                return false;
            }
        }

        public override bool SupportsMoveFile
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsMoveFolder
        {
            get { return true; }
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

            var filePath = this.GetActualPath(folderMapping, folderPath, fileName);
            var newFilePath = this.GetActualPath(folderMapping, newFolderPath, fileName);

            if (FileWrapper.Instance.Exists(filePath))
            {
                FileWrapper.Instance.Copy(filePath, newFilePath, true);
            }
        }

        public override void AddFile(IFolderInfo folder, string fileName, Stream content)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("content", content);

            this.UpdateFile(folder, fileName, content);
        }

        public override void AddFolder(string folderPath, FolderMappingInfo folderMapping)
        {
        }

        public override void DeleteFile(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var path = this.GetActualPath(file);

            if (FileWrapper.Instance.Exists(path))
            {
                FileWrapper.Instance.SetAttributes(path, FileAttributes.Normal);
                FileWrapper.Instance.Delete(path);
            }
        }

        public override void DeleteFolder(IFolderInfo folder)
        {
        }

        public override bool FileExists(IFolderInfo folder, string fileName)
        {
            Requires.NotNull("folder", folder);
            Requires.PropertyNotNull("fileName", fileName);

            return FileWrapper.Instance.Exists(this.GetActualPath(folder, fileName));
        }

        public override bool FolderExists(string folderPath, FolderMappingInfo folderMapping)
        {
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            return DirectoryWrapper.Instance.Exists(this.GetActualPath(folderMapping, folderPath));
        }

        public override FileAttributes? GetFileAttributes(IFileInfo file)
        {
            Requires.NotNull("file", file);

            FileAttributes? fileAttributes = null;

            try
            {
                fileAttributes = FileWrapper.Instance.GetAttributes(this.GetActualPath(file));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return fileAttributes;
        }

        public override string[] GetFiles(IFolderInfo folder)
        {
            Requires.NotNull("folder", folder);

            var fileNames = DirectoryWrapper.Instance.GetFiles(this.GetActualPath(folder));

            for (var i = 0; i < fileNames.Length; i++)
            {
                fileNames[i] = Path.GetFileName(fileNames[i]);
            }

            return fileNames;
        }

        public override long GetFileSize(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var physicalFile = new System.IO.FileInfo(this.GetActualPath(file));

            return physicalFile.Length;
        }

        public override Stream GetFileStream(IFileInfo file)
        {
            Requires.NotNull("file", file);

            return this.GetFileStreamInternal(this.GetActualPath(file));
        }

        public override Stream GetFileStream(IFolderInfo folder, string fileName)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);
            return this.GetFileStreamInternal(this.GetActualPath(folder, fileName));
        }

        public override string GetFileUrl(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var portalSettings = file.PortalId == PortalSettings.Current?.PortalId ?
                PortalSettings.Current :
                this.GetPortalSettings(file.PortalId);

            var rootFolder = file.PortalId == Null.NullInteger ? Globals.HostPath : portalSettings.HomeDirectory;

            var fullPath = rootFolder + file.Folder + file.FileName;

            // check if a filename has a character that is not valid for urls
            if (fullPath.IndexOfAny(InvalidFileUrlChars) >= 0)
            {
                return Globals.LinkClick(
                    $"fileid={file.FileId}",
                    Null.NullInteger,
                    Null.NullInteger,
                    true,
                    false,
                    portalSettings.PortalId,
                    portalSettings.EnableUrlLanguage,
                    portalSettings.GUID.ToString());
            }

            // Does site management want the cachebuster parameter?
            if (portalSettings.AddCachebusterToResourceUris)
            {
                var cachebusterToken = UrlUtils.EncryptParameter(
                    file.LastModificationTime.GetHashCode().ToString(),
                    portalSettings.GUID.ToString());

                return TestableGlobals.Instance.ResolveUrl(fullPath + "?ver=" + cachebusterToken);
            }

            return TestableGlobals.Instance.ResolveUrl(fullPath);
        }

        public override string GetFolderProviderIconPath()
        {
            return IconControllerWrapper.Instance.IconURL("FolderStandard", "32x32");
        }

        public override DateTime GetLastModificationTime(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var lastModificationTime = Null.NullDate;

            try
            {
                lastModificationTime = FileWrapper.Instance.GetLastWriteTime(this.GetActualPath(file));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return lastModificationTime;
        }

        public override IEnumerable<string> GetSubFolders(string folderPath, FolderMappingInfo folderMapping)
        {
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            return DirectoryWrapper.Instance.GetDirectories(this.GetActualPath(folderMapping, folderPath))
                .Select(directory => this.GetRelativePath(folderMapping, directory));
        }

        public override bool IsInSync(IFileInfo file)
        {
            Requires.NotNull("file", file);

            return Convert.ToInt32((file.LastModificationTime - this.GetLastModificationTime(file)).TotalSeconds) == 0;
        }

        public override void MoveFile(IFileInfo file, IFolderInfo destinationFolder)
        {
            Requires.NotNull("file", file);
            Requires.NotNull("destinationFolder", destinationFolder);

            if (file.FolderId != destinationFolder.FolderID)
            {
                string oldName = this.GetActualPath(file);
                string newName = this.GetActualPath(destinationFolder, file.FileName);
                FileWrapper.Instance.Move(oldName, newName);
            }
        }

        public override void MoveFolder(string folderPath, string newFolderPath, FolderMappingInfo folderMapping)
        {
            // The folder has already been moved in filesystem
        }

        public override void RenameFile(IFileInfo file, string newFileName)
        {
            Requires.NotNull("file", file);
            Requires.NotNullOrEmpty("newFileName", newFileName);

            if (file.FileName != newFileName)
            {
                IFolderInfo folder = FolderManager.Instance.GetFolder(file.FolderId);
                string oldName = this.GetActualPath(file);
                string newName = this.GetActualPath(folder, newFileName);
                FileWrapper.Instance.Move(oldName, newName);
            }
        }

        public override void RenameFolder(IFolderInfo folder, string newFolderName)
        {
            // The folder has already been moved in filesystem
        }

        public override void SetFileAttributes(IFileInfo file, FileAttributes fileAttributes)
        {
            Requires.NotNull("file", file);

            FileWrapper.Instance.SetAttributes(this.GetActualPath(file), fileAttributes);
        }

        public override bool SupportsFileAttributes()
        {
            return true;
        }

        public override void UpdateFile(IFileInfo file, Stream content)
        {
            Requires.NotNull("file", file);
            Requires.NotNull("content", content);

            this.UpdateFile(FolderManager.Instance.GetFolder(file.FolderId), file.FileName, content);
        }

        public override void UpdateFile(IFolderInfo folder, string fileName, Stream content)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("content", content);

            var arrData = new byte[2048];
            var actualPath = this.GetActualPath(folder, fileName);

            if (FileWrapper.Instance.Exists(actualPath))
            {
                FileWrapper.Instance.SetAttributes(actualPath, FileAttributes.Normal);
                FileWrapper.Instance.Delete(actualPath);
            }

            using (var outStream = FileWrapper.Instance.Create(actualPath))
            {
                var originalPosition = content.Position;
                content.Position = 0;

                try
                {
                    var intLength = content.Read(arrData, 0, arrData.Length);

                    while (intLength > 0)
                    {
                        outStream.Write(arrData, 0, intLength);
                        intLength = content.Read(arrData, 0, arrData.Length);
                    }
                }
                finally
                {
                    content.Position = originalPosition;
                }
            }
        }

        internal virtual string GetHash(IFileInfo file)
        {
            var fileManager = new FileManager();
            return fileManager.GetHash(file);
        }

        internal virtual PortalSettings GetPortalSettings(int portalId)
        {
            return new PortalSettings(portalId);
        }

        /// <summary>
        /// Get actual path to a file.
        /// </summary>
        /// <param name="folderMapping">Folder Mapping of the folder.</param>
        /// <param name="folderPath">Folder Path where the file is contained.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>A windows supported path to the file.</returns>
        protected virtual string GetActualPath(FolderMappingInfo folderMapping, string folderPath, string fileName)
        {
            var actualFolderPath = this.GetActualPath(folderMapping, folderPath);
            return Path.Combine(actualFolderPath, fileName);
        }

        /// <summary>
        /// Get actual path to an IFileInfo.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>A windows supported path to the file.</returns>
        protected virtual string GetActualPath(IFileInfo file)
        {
            return file.PhysicalPath;
        }

        /// <summary>
        /// Get actual path to a file in specified folder.
        /// </summary>
        /// <param name="folder">The folder that contains the file.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>A windows supported path to the file.</returns>
        protected virtual string GetActualPath(IFolderInfo folder, string fileName)
        {
            return Path.Combine(folder.PhysicalPath, fileName);
        }

        /// <summary>
        /// Get actual path to a folder in the specified folder mapping.
        /// </summary>
        /// <param name="folderMapping">The folder mapping.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>A windows supported path to the folder.</returns>
        protected virtual string GetActualPath(FolderMappingInfo folderMapping, string folderPath)
        {
            return PathUtils.Instance.GetPhysicalPath(folderMapping.PortalID, folderPath);
        }

        /// <summary>
        /// Get actual path to a folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns>A windows supported path to the folder.</returns>
        protected virtual string GetActualPath(IFolderInfo folder)
        {
            return folder.PhysicalPath;
        }

        protected Stream GetFileStreamInternal(string filePath)
        {
            Stream stream = null;

            try
            {
                stream = FileWrapper.Instance.OpenRead(filePath);
            }
            catch (IOException iex)
            {
                Logger.Warn(iex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return stream;
        }

        /// <summary>
        /// Get the path relative to the root of the FolderMapping.
        /// </summary>
        /// <param name="folderMapping">Path is relative to this.</param>
        /// <param name="path">The path.</param>
        /// <returns>A relative path.</returns>
        protected virtual string GetRelativePath(FolderMappingInfo folderMapping, string path)
        {
            return PathUtils.Instance.GetRelativePath(folderMapping.PortalID, path);
        }
    }
}
