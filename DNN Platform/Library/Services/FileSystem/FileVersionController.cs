// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem.EventArgs;
    using Microsoft.Extensions.DependencyInjection;

    public class FileVersionController(IFolderMappingController folderMappingController, DataProvider dataProvider, IEventManager eventManager, IUserController userController, IPortalController portalController) : ComponentBase<IFileVersionController, FileVersionController>, IFileVersionController
    {
        private readonly IFolderMappingController folderMappingController = folderMappingController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFolderMappingController>();
        private readonly DataProvider dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
        private readonly IEventManager eventManager = eventManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventManager>();
        private readonly IUserController userController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();

        /// <summary>Initializes a new instance of the <see cref="FileVersionController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with DataProvider. Scheduled removal in v12.0.0.")]
        public FileVersionController()
        : this(null, null, null, null, null)
        {
        }

        /// <inheritdoc />
        public string AddFileVersion(IFileInfo file, int userId, bool published, bool removeOldestVersions, Stream content = null)
        {
            Requires.NotNull("file", file);

            byte[] fileContent = null;

            if (content != null)
            {
                var buffer = new byte[16 * 1024];
                using var ms = new MemoryStream();
                int read;
                while ((read = content.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                fileContent = ms.ToArray();
            }

            var newVersion = this.dataProvider
                                             .AddFileVersion(
                                                 file.FileId,
                                                 file.UniqueId,
                                                 file.VersionGuid,
                                                 file.FileName,
                                                 file.Extension,
                                                 file.Size,
                                                 file.Width,
                                                 file.Height,
                                                 file.ContentType,
                                                 file.Folder,
                                                 file.FolderId,
                                                 userId,
                                                 file.SHA1Hash,
                                                 file.LastModificationTime,
                                                 file.Title,
                                                 file.EnablePublishPeriod,
                                                 file.StartDate,
                                                 file.EndDate,
                                                 file.ContentItemID,
                                                 published,
                                                 fileContent);

            DataCache.RemoveCache("GetFileById" + file.FileId);

            if (removeOldestVersions)
            {
                this.RemoveOldestVersions(file);
            }

            if (published)
            {
                this.RenameFile(file, GetVersionedFilename(file, file.PublishedVersion));
                return file.FileName;
            }

            return GetVersionedFilename(file, newVersion);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void SetPublishedVersion(IFileInfo file, int newPublishedVersion)
        {
            this.dataProvider.SetPublishedVersion(file.FileId, newPublishedVersion);
            DataCache.RemoveCache("GetFileById" + file.FileId);

            // Rename the original file to the versioned name
            // Rename the new versioned name to the original file name
            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
            if (folderMapping == null)
            {
                return;
            }

            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);

            folderProvider.RenameFile(file, GetVersionedFilename(file, file.PublishedVersion));
            folderProvider.RenameFile(
                    new FileInfo { FileName = GetVersionedFilename(file, newPublishedVersion), Folder = file.Folder, FolderId = file.FolderId, FolderMappingID = folderMapping.FolderMappingID, PortalId = folderMapping.PortalID },
                    file.FileName);

            // Notify File Changed
            this.OnFileChanged(file, this.userController.GetCurrentUserInfo().UserID);
        }

        /// <inheritdoc />
        public int DeleteFileVersion(IFileInfo file, int version)
        {
            Requires.NotNull("file", file);

            int newVersion;

            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
            if (folderMapping == null)
            {
                return Null.NullInteger;
            }

            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);

            if (file.PublishedVersion == version)
            {
                folderProvider.DeleteFile(new FileInfo { FileId = file.FileId, FileName = file.FileName, Folder = file.Folder, FolderMappingID = folderMapping.FolderMappingID, PortalId = folderMapping.PortalID, FolderId = file.FolderId });
                newVersion = this.dataProvider.DeleteFileVersion(file.FileId, version);

                folderProvider.RenameFile(
                    new FileInfo { FileId = file.FileId, FileName = GetVersionedFilename(file, newVersion), Folder = file.Folder, FolderId = file.FolderId, FolderMappingID = folderMapping.FolderMappingID, PortalId = folderMapping.PortalID },
                    file.FileName);

                // Update the Last Modification Time
                var providerLastModificationTime = folderProvider.GetLastModificationTime(file);
                if (file.LastModificationTime != providerLastModificationTime)
                {
                    this.dataProvider.UpdateFileLastModificationTime(file.FileId, providerLastModificationTime);
                }

                // Notify File Changed
                this.OnFileChanged(file, this.userController.GetCurrentUserInfo().UserID);
            }
            else
            {
                newVersion = this.dataProvider.DeleteFileVersion(file.FileId, version);
                folderProvider.DeleteFile(new FileInfo { FileName = GetVersionedFilename(file, version), Folder = file.Folder, FolderMappingID = folderMapping.FolderMappingID, PortalId = folderMapping.PortalID, FolderId = file.FolderId });
            }

            DataCache.RemoveCache("GetFileById" + file.FileId);

            return newVersion;
        }

        /// <inheritdoc />
        public FileVersionInfo GetFileVersion(IFileInfo file, int version)
        {
            Requires.NotNull("file", file);
            return CBO.FillObject<FileVersionInfo>(this.dataProvider.GetFileVersion(file.FileId, version));
        }

        /// <inheritdoc />
        public void DeleteAllUnpublishedVersions(IFileInfo file, bool resetPublishedVersionNumber)
        {
            Requires.NotNull("file", file);

            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
            if (folderMapping == null)
            {
                return;
            }

            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);

            foreach (var version in this.GetFileVersions(file))
            {
                folderProvider.DeleteFile(new FileInfo { FileName = version.FileName, Folder = file.Folder, FolderMappingID = folderMapping.FolderMappingID, PortalId = folderMapping.PortalID, FolderId = file.FolderId });
                this.dataProvider.DeleteFileVersion(version.FileId, version.Version);
            }

            if (resetPublishedVersionNumber)
            {
                file.PublishedVersion = 1;
                this.dataProvider.ResetFilePublishedVersion(file.FileId);
                DataCache.RemoveCache("GetFileById" + file.FileId);
            }
        }

        /// <inheritdoc />
        public IEnumerable<FileVersionInfo> GetFileVersions(IFileInfo file)
        {
            Requires.NotNull("file", file);
            return CBO.FillCollection<FileVersionInfo>(this.dataProvider.GetFileVersions(file.FileId));
        }

        /// <inheritdoc />
        public bool IsFolderVersioned(int folderId)
        {
            return this.IsFolderVersioned(FolderManager.Instance.GetFolder(folderId));
        }

        /// <inheritdoc />
        public bool IsFolderVersioned(IFolderInfo folder)
        {
            return this.IsFileVersionEnabled(folder.PortalID) && folder.IsVersioned;
        }

        /// <inheritdoc />
        public bool IsFileVersionEnabled(int portalId)
        {
            return PortalController.GetPortalSettingAsBoolean(this.portalController, "FileVersionEnabled", portalId, true);
        }

        /// <inheritdoc />
        public int MaxFileVersions(int portalId)
        {
            return PortalController.GetPortalSettingAsInteger(this.portalController, "MaxFileVersions", portalId, 5);
        }

        /// <inheritdoc />
        public IEnumerable<FileVersionInfo> GetFileVersionsInFolder(int folderId)
        {
            Requires.NotNegative("folderId", folderId);

            return CBO.FillCollection<FileVersionInfo>(this.dataProvider.GetFileVersionsInFolder(folderId));
        }

        /// <inheritdoc />
        public Stream GetVersionContent(IFileInfo file, int version)
        {
            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
            if (folderMapping == null)
            {
                return null;
            }

            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            return GetVersionContent(folderProvider, folder, file, version);
        }

        /// <inheritdoc />
        public void RollbackFileVersion(IFileInfo file, int version, int userId)
        {
            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
            if (folderMapping == null)
            {
                return;
            }

            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            using (var content = GetVersionContent(folderProvider, folder, file, version))
            {
                FileManager.Instance.AddFile(folder, file.FileName, content, true, true, file.ContentType, userId);
            }

            // We need to refresh the file object
            file = FileManager.Instance.GetFile(file.FileId);
            var fileVersion = this.GetFileVersion(file, version);
            file.Extension = fileVersion.Extension;
            file.Size = fileVersion.Size;
            file.SHA1Hash = fileVersion.SHA1Hash;
            file.Width = fileVersion.Width;
            file.Height = fileVersion.Height;
            FileManager.Instance.UpdateFile(file);

            this.RemoveOldestVersions(file);
        }

        internal static string GetVersionedFilename(IFileInfo file, int version)
        {
            return $"{file.FileId}_{version}.v.resources";
        }

        private static Stream GetVersionContent(FolderProvider provider, IFolderInfo folder, IFileInfo file, int version)
        {
            return provider.GetFileStream(folder, file, version);
        }

        private void RenameFile(IFileInfo file, string newFileName)
        {
            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
            if (folderMapping != null)
            {
                var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
                folderProvider.RenameFile(file, newFileName);
            }

            DataCache.RemoveCache("GetFileById" + file.FileId);
        }

        private void OnFileChanged(IFileInfo fileInfo, int userId)
        {
            this.eventManager.OnFileChanged(new FileChangedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
            });
        }

        private void RemoveOldestVersions(IFileInfo file)
        {
            var portalId = FolderManager.Instance.GetFolder(file.FolderId).PortalID;
            var versions = this.GetFileVersions(file);
            var maxVersions = this.MaxFileVersions(portalId) - 1; // The published version is not in the FileVersions collection
            if (versions.Count() > maxVersions)
            {
                foreach (var v in versions.OrderBy(v => v.Version).Take(versions.Count() - maxVersions))
                {
                    this.DeleteFileVersion(file, v.Version);
                }
            }
        }
    }
}
