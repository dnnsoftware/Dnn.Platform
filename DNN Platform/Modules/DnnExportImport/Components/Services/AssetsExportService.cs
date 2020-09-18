// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Dto.Assets;
    using Dnn.ExportImport.Dto.Workflow;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;
    using Newtonsoft.Json;

    using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

    public class AssetsExportService : BasePortableService
    {
        private const string DefaultUsersFoldersPath = "Users";

        private const string UsersAssetsTempFolder = "{0}\\TempUsers\\";

        private static readonly Regex UserFolderEx = new Regex(
            @"users/\d+/\d+/(\d+)/",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly string _assetsFolder =
            $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{{0}}\\{Constants.ExportZipFiles}";

        public override string Category => Constants.Category_Assets;

        public override string ParentCategory => null;

        public override uint Priority => 50;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (this.CheckCancelled(exportJob))
            {
                return;
            }

            // Skip the export if all the folders have been processed already.
            if (this.CheckPoint.Stage >= 1)
            {
                return;
            }

            // Create Zip File to hold files
            var skip = this.GetCurrentSkip();
            var currentIndex = skip;
            var totalFolderExported = 0;
            var totalFolderPermissionsExported = 0;
            var totalFilesExported = 0;
            var portalId = exportJob.PortalId;
            try
            {
                var assetsFile = string.Format(this._assetsFolder, exportJob.Directory.TrimEnd('\\').TrimEnd('/'));

                if (this.CheckPoint.Stage == 0)
                {
                    var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
                    var toDate = exportDto.ToDateUtc.ToLocalTime();
                    var portal = PortalController.Instance.GetPortal(portalId);

                    var folders =
                        CBO.FillCollection<ExportFolder>(DataProvider.Instance()
                            .GetFolders(portalId, toDate, fromDate)).ToList();
                    var totalFolders = folders.Any() ? folders.Count : 0;
                    folders = folders.Skip(skip).ToList();

                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalFolders : this.CheckPoint.TotalItems;
                    this.CheckPoint.ProcessedItems = skip;
                    this.CheckPoint.Progress = this.CheckPoint.TotalItems > 0 ? skip * 100.0 / this.CheckPoint.TotalItems : 0;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }

                    using (var zipArchive = CompressionUtil.OpenCreate(assetsFile))
                    {
                        foreach (var folder in folders)
                        {
                            if (this.CheckCancelled(exportJob))
                            {
                                break;
                            }

                            var isUserFolder = false;

                            var files =
                                CBO.FillCollection<ExportFile>(
                                    DataProvider.Instance()
                                        .GetFiles(portalId, folder.FolderId, toDate, fromDate)).Where(x => x.Extension != Constants.TemplatesExtension).ToList();
                            int? userId;
                            if (IsUserFolder(folder.FolderPath, out userId))
                            {
                                isUserFolder = true;
                                folder.UserId = userId;
                                folder.Username =
                                    UserController.GetUserById(portalId, Convert.ToInt32(userId))?.Username;
                            }

                            if (folder.ParentId != null && folder.ParentId > 0)
                            {
                                // If parent id exists then change the parent folder id to parent id.
                                folder.ParentId =
                                    this.Repository.GetItem<ExportFolder>(
                                        x => x.FolderId == Convert.ToInt32(folder.ParentId))?.Id;
                            }

                            this.Repository.CreateItem(folder, null);
                            totalFolderExported++;

                            // Include permissions only if IncludePermissions=true
                            if (exportDto.IncludePermissions)
                            {
                                var permissions =
                                    CBO.FillCollection<ExportFolderPermission>(DataProvider.Instance()
                                        .GetFolderPermissionsByPath(portalId, folder.FolderPath, toDate, fromDate));
                                this.Repository.CreateItems(permissions, folder.Id);
                                totalFolderPermissionsExported += permissions.Count;
                            }

                            this.Repository.CreateItems(files, folder.Id);
                            totalFilesExported += files.Count;
                            var folderOffset = portal.HomeDirectoryMapPath.Length +
                                               (portal.HomeDirectoryMapPath.EndsWith("\\") ? 0 : 1);

                            if (folder.StorageLocation != (int)FolderController.StorageLocationTypes.DatabaseSecure)
                            {
                                CompressionUtil.AddFilesToArchive(zipArchive, files.Select(file => portal.HomeDirectoryMapPath + folder.FolderPath + this.GetActualFileName(file)),
                                    folderOffset, isUserFolder ? "TempUsers" : null);
                            }

                            this.CheckPoint.ProcessedItems++;
                            this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / totalFolders;
                            this.CheckPoint.StageData = null;
                            currentIndex++;

                            // After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                            if (currentIndex % 10 == 0 && this.CheckPointStageCallback(this))
                            {
                                return;
                            }

                            this.Repository.RebuildIndex<ExportFolder>(x => x.Id, true);
                            this.Repository.RebuildIndex<ExportFolder>(x => x.UserId);
                            this.Repository.RebuildIndex<ExportFile>(x => x.ReferenceId);
                        }
                    }

                    this.CheckPoint.Completed = true;
                    this.CheckPoint.Stage++;
                    currentIndex = 0;
                    this.CheckPoint.Progress = 100;
                }
            }
            finally
            {
                this.CheckPoint.StageData = currentIndex > 0 ? JsonConvert.SerializeObject(new { skip = currentIndex }) : null;
                this.CheckPointStageCallback(this);
                this.Result.AddSummary("Exported Folders", totalFolderExported.ToString());
                this.Result.AddSummary("Exported Folder Permissions", totalFolderPermissionsExported.ToString());
                this.Result.AddSummary("Exported Files", totalFilesExported.ToString());
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (this.CheckCancelled(importJob))
            {
                return;
            }

            // Stage 1: Portals files unzipped.
            // Stage 2: All folders and files imported.
            // Stage 3: Synchronization completed.
            // Skip the export if all the folders have been processed already.
            if (this.CheckPoint.Stage >= 2 || this.CheckPoint.Completed)
            {
                return;
            }

            var totalFolderImported = 0;
            var totalFolderPermissionsImported = 0;
            var totalFilesImported = 0;
            var skip = this.GetCurrentSkip();
            var currentIndex = skip;
            var portalId = importJob.PortalId;
            var portal = PortalController.Instance.GetPortal(portalId);
            var assetsFile = string.Format(this._assetsFolder, importJob.Directory.TrimEnd('\\').TrimEnd('/'));
            var userFolderPath = string.Format(UsersAssetsTempFolder, portal.HomeDirectoryMapPath.TrimEnd('\\'));
            if (this.CheckPoint.Stage == 0)
            {
                if (!File.Exists(assetsFile))
                {
                    this.Result.AddLogEntry("AssetsFileNotFound", "Assets file not found. Skipping assets import",
                        ReportLevel.Warn);
                    this.CheckPoint.Completed = true;
                    this.CheckPointStageCallback(this);
                }
                else
                {
                    CompressionUtil.UnZipArchive(assetsFile, portal.HomeDirectoryMapPath,
                        importDto.CollisionResolution == CollisionResolution.Overwrite);

                    // Stage 1: Once unzipping of portal files is completed.
                    this.CheckPoint.Stage++;
                    this.CheckPoint.StageData = null;
                    this.CheckPoint.Progress = 10;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }
                }
            }

            if (this.CheckPoint.Stage == 1)
            {
                try
                {
                    // Stage 2 starts
                    var sourceFolders = this.Repository.GetAllItems<ExportFolder>(x => x.CreatedOnDate, true, skip).ToList();

                    var totalFolders = sourceFolders.Any() ? sourceFolders.Count : 0;

                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalFolders : this.CheckPoint.TotalItems;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }

                    foreach (var sourceFolder in sourceFolders)
                    {
                        if (this.CheckCancelled(importJob))
                        {
                            break;
                        }

                        // PROCESS FOLDERS
                        // Create new or update existing folder
                        if (this.ProcessFolder(importJob, importDto, sourceFolder))
                        {
                            totalFolderImported++;

                            // Include permissions only if permissions were exported in package.
                            if (importDto.ExportDto.IncludePermissions)
                            {
                                // PROCESS FOLDER PERMISSIONS
                                var sourceFolderPermissions =
                                    this.Repository.GetRelatedItems<ExportFolderPermission>(sourceFolder.Id).ToList();

                                // Replace folderId for each permission with new one.
                                sourceFolderPermissions.ForEach(x =>
                                {
                                    x.FolderId = Convert.ToInt32(sourceFolder.FolderId);
                                    x.FolderPath = sourceFolder.FolderPath;
                                });

                                // PROCESS FOLDER PERMISSIONS
                                // File local files in the system related to the folder path.
                                var localPermissions =
                                    CBO.FillCollection<ExportFolderPermission>(DataProvider.Instance()
                                        .GetFolderPermissionsByPath(portalId, sourceFolder.FolderPath,
                                            DateUtils.GetDatabaseUtcTime().AddYears(1), null));

                                foreach (var folderPermission in sourceFolderPermissions)
                                {
                                    this.ProcessFolderPermission(importJob, importDto, folderPermission,
                                        localPermissions);
                                }

                                totalFolderPermissionsImported += sourceFolderPermissions.Count;
                            }

                            // PROCESS FILES
                            var sourceFiles =
                                this.Repository.GetRelatedItems<ExportFile>(sourceFolder.Id).ToList();

                            // Replace folderId for each file with new one.
                            sourceFiles.ForEach(x =>
                            {
                                x.FolderId = Convert.ToInt32(sourceFolder.FolderId);
                                x.Folder = sourceFolder.FolderPath;
                            });

                            // File local files in the system related to the folder
                            var localFiles =
                                CBO.FillCollection<ExportFile>(DataProvider.Instance()
                                    .GetFiles(portalId, sourceFolder.FolderId,
                                        DateUtils.GetDatabaseUtcTime().AddYears(1), null));

                            foreach (var file in sourceFiles)
                            {
                                this.ProcessFiles(importJob, importDto, file, localFiles);
                            }

                            totalFilesImported += sourceFiles.Count;
                        }

                        currentIndex++;
                        this.CheckPoint.ProcessedItems++;
                        this.CheckPoint.Progress = 10 + (this.CheckPoint.ProcessedItems * 90.0 / totalFolders);

                        // After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                        if (currentIndex % 10 == 0 && this.CheckPointStageCallback(this))
                        {
                            return;
                        }
                    }

                    currentIndex = 0;
                    this.CheckPoint.Completed = true;
                    this.CheckPoint.Stage++;
                    this.CheckPoint.Progress = 100;
                }
                finally
                {
                    this.CheckPoint.StageData = currentIndex > 0
                        ? JsonConvert.SerializeObject(new { skip = currentIndex })
                        : null;
                    this.CheckPointStageCallback(this);

                    this.Result.AddSummary("Imported Folders", totalFolderImported.ToString());
                    this.Result.AddSummary("Imported Folder Permissions", totalFolderPermissionsImported.ToString());
                    this.Result.AddSummary("Imported Files", totalFilesImported.ToString());

                    if (Directory.Exists(userFolderPath) && currentIndex == 0)
                    {
                        Directory.Delete(userFolderPath, true);
                    }
                }
            }
        }

        public override int GetImportTotal()
        {
            return this.Repository.GetCount<ExportFolder>();
        }

        private static bool IsUserFolder(string folderPath, out int? userId)
        {
            userId = null;
            var match = UserFolderEx.Match(folderPath);
            if (match.Success)
            {
                userId = int.Parse(match.Groups[1].Value);
            }

            return match.Success;
        }

        private bool ProcessFolder(ExportImportJob importJob, ImportDto importDto, ExportFolder folder)
        {
            var portalId = importJob.PortalId;
            if (folder == null)
            {
                return false;
            }

            var existingFolder = CBO.FillObject<ExportFolder>(DotNetNuke.Data.DataProvider.Instance().GetFolder(portalId, folder.FolderPath ?? string.Empty));
            var isUpdate = false;
            var modifiedBy = Util.GetUserIdByName(importJob, folder.LastModifiedByUserId, folder.LastModifiedByUserName);
            if (existingFolder != null)
            {
                switch (importDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                }
            }

            folder.FolderPath = string.IsNullOrEmpty(folder.FolderPath) ? string.Empty : folder.FolderPath;
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, folder.FolderMappingName);
            if (folderMapping == null)
            {
                return false;
            }

            var workFlowId = this.GetLocalWorkFlowId(folder.WorkflowId);
            if (isUpdate)
            {
                Util.FixDateTime(existingFolder);
                DotNetNuke.Data.DataProvider.Instance()
                    .UpdateFolder(importJob.PortalId, folder.VersionGuid, existingFolder.FolderId, folder.FolderPath,
                        folder.StorageLocation, folder.MappedPath, folder.IsProtected, folder.IsCached,
                        DateUtils.GetDatabaseLocalTime(), modifiedBy, folderMapping.FolderMappingID, folder.IsVersioned,
                        workFlowId, existingFolder.ParentId ?? Null.NullInteger);

                folder.FolderId = existingFolder.FolderId;

                if (folder.UserId != null && folder.UserId > 0 && !string.IsNullOrEmpty(folder.Username))
                {
                    this.SyncUserFolder(importJob.PortalId, folder);
                }
            }
            else
            {
                folder.FolderMappingId = folderMapping.FolderMappingID;
                var createdBy = Util.GetUserIdByName(importJob, folder.CreatedByUserId, folder.CreatedByUserName);
                if (folder.ParentId != null && folder.ParentId > 0)
                {
                    // Find the previously created parent folder id.
                    folder.ParentId = CBO.FillObject<ExportFolder>(DotNetNuke.Data.DataProvider.Instance().GetFolder(portalId, folder.ParentFolderPath ?? string.Empty))?.FolderId;
                }

                // ignore folders which start with Users but are not user folders.
                if (!folder.FolderPath.StartsWith(DefaultUsersFoldersPath))
                {
                    folder.FolderId = DotNetNuke.Data.DataProvider.Instance()
                        .AddFolder(importJob.PortalId, Guid.NewGuid(), folder.VersionGuid, folder.FolderPath,
                            folder.MappedPath, folder.StorageLocation, folder.IsProtected, folder.IsCached,
                            DateUtils.GetDatabaseLocalTime(),
                            createdBy, folderMapping.FolderMappingID, folder.IsVersioned, workFlowId,
                            folder.ParentId ?? Null.NullInteger);
                }

                // Case when the folder is a user folder.
                else if (folder.UserId != null && folder.UserId > 0 && !string.IsNullOrEmpty(folder.Username))
                {
                    var userInfo = UserController.GetUserByName(portalId, folder.Username);
                    if (userInfo == null)
                    {
                        folder.FolderId = 0;
                        return false;
                    }

                    userInfo.IsSuperUser = false;
                    var newFolder = FolderManager.Instance.GetUserFolder(userInfo);
                    folder.FolderId = newFolder.FolderID;
                    folder.FolderPath = newFolder.FolderPath;
                    this.SyncUserFolder(importJob.PortalId, folder);
                    return true;
                }
                else
                {
                    folder.FolderId = 0;
                    return false;
                }
            }

            return true;
        }

        private void ProcessFolderPermission(ExportImportJob importJob, ImportDto importDto,
            ExportFolderPermission folderPermission, IEnumerable<ExportFolderPermission> localPermissions)
        {
            var portalId = importJob.PortalId;
            var noRole = Convert.ToInt32(Globals.glbRoleNothing);
            if (folderPermission == null)
            {
                return;
            }

            var roleId = Util.GetRoleIdByName(portalId, folderPermission.RoleId ?? noRole, folderPermission.RoleName);
            var userId = UserController.GetUserByName(portalId, folderPermission.Username)?.UserID;

            var existingFolderPermission = localPermissions.FirstOrDefault(
                x =>
                    (x.FolderPath == folderPermission.FolderPath ||
                     (string.IsNullOrEmpty(x.FolderPath) && string.IsNullOrEmpty(folderPermission.FolderPath))) &&
                    x.PermissionCode == folderPermission.PermissionCode && x.PermissionKey == folderPermission.PermissionKey
                    && x.PermissionName.Equals(folderPermission.PermissionName, StringComparison.InvariantCultureIgnoreCase) &&
                    x.RoleId == roleId && x.UserId == userId);

            var isUpdate = false;
            if (existingFolderPermission != null)
            {
                switch (importDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore:
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                }
            }

            if (isUpdate)
            {
                var modifiedBy = Util.GetUserIdByName(importJob, folderPermission.LastModifiedByUserId,
                    folderPermission.LastModifiedByUserName);

                DotNetNuke.Data.DataProvider.Instance()
                    .UpdateFolderPermission(existingFolderPermission.FolderPermissionId, folderPermission.FolderId,
                        existingFolderPermission.PermissionId, existingFolderPermission.RoleId ?? Convert.ToInt32(Globals.glbRoleNothing),
                        folderPermission.AllowAccess, existingFolderPermission.UserId ?? Null.NullInteger, modifiedBy);

                folderPermission.FolderPermissionId = existingFolderPermission.FolderPermissionId;
            }
            else
            {
                var permissionId = DataProvider.Instance()
                    .GetPermissionId(folderPermission.PermissionCode, folderPermission.PermissionKey,
                        folderPermission.PermissionName);

                if (permissionId != null)
                {
                    folderPermission.PermissionId = Convert.ToInt32(permissionId);
                    if (folderPermission.UserId != null && folderPermission.UserId > 0 && !string.IsNullOrEmpty(folderPermission.Username))
                    {
                        folderPermission.UserId = userId;
                        if (folderPermission.UserId == null)
                        {
                            return;
                        }
                    }

                    if (folderPermission.RoleId != null && folderPermission.RoleId > noRole && !string.IsNullOrEmpty(folderPermission.RoleName))
                    {
                        folderPermission.RoleId = roleId;
                        if (folderPermission.RoleId == null)
                        {
                            return;
                        }
                    }

                    var createdBy = Util.GetUserIdByName(importJob, folderPermission.CreatedByUserId,
                        folderPermission.CreatedByUserName);

                    folderPermission.FolderPermissionId = DotNetNuke.Data.DataProvider.Instance()
                        .AddFolderPermission(folderPermission.FolderId, folderPermission.PermissionId,
                            folderPermission.RoleId ?? noRole, folderPermission.AllowAccess,
                            folderPermission.UserId ?? Null.NullInteger, createdBy);
                }
            }

            folderPermission.LocalId = folderPermission.FolderPermissionId;
        }

        private void ProcessFiles(ExportImportJob importJob, ImportDto importDto, ExportFile file, IEnumerable<ExportFile> localFiles)
        {
            if (file == null)
            {
                return;
            }

            var existingFile = localFiles.FirstOrDefault(x => x.FileName == file.FileName);
            var isUpdate = false;
            if (existingFile != null)
            {
                switch (importDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore:
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                }
            }

            if (isUpdate)
            {
                var modifiedBy = Util.GetUserIdByName(importJob, file.LastModifiedByUserId, file.LastModifiedByUserName);
                file.FileId = existingFile.FileId;
                DotNetNuke.Data.DataProvider.Instance()
                    .UpdateFile(existingFile.FileId, file.VersionGuid, file.FileName, file.Extension, file.Size,
                        file.Width ?? Null.NullInteger, file.Height ?? Null.NullInteger, file.ContentType, file.FolderId,
                        modifiedBy, file.Sha1Hash, DateUtils.GetDatabaseLocalTime(), file.Title, file.Description,
                        file.StartDate, file.EndDate ?? Null.NullDate, file.EnablePublishPeriod,
                        existingFile.ContentItemId ?? Null.NullInteger);

                if ((file.Content != null && existingFile.Content == null) ||
                    (existingFile.Content != null && file.Content == null) ||
                    (file.Content != null && existingFile.Content != null &&
                     file.Content.SequenceEqual(existingFile.Content)))
                {
                    DotNetNuke.Data.DataProvider.Instance().UpdateFileContent(file.FileId, file.Content);
                }
            }
            else
            {
                var createdBy = Util.GetUserIdByName(importJob, file.CreatedByUserId, file.CreatedByUserName);
                file.FileId = DotNetNuke.Data.DataProvider.Instance()
                    .AddFile(importJob.PortalId, Guid.NewGuid(), file.VersionGuid, file.FileName, file.Extension,
                        file.Size,
                        file.Width ?? Null.NullInteger, file.Height ?? Null.NullInteger, file.ContentType, file.Folder,
                        file.FolderId,
                        createdBy, file.Sha1Hash, DateUtils.GetDatabaseLocalTime(), file.Title, file.Description,
                        file.StartDate, file.EndDate ?? Null.NullDate, file.EnablePublishPeriod,

                        // file.ContentItemId ?? Null.NullInteger);--If we keep it we will see FK_PK relationship errors.
                        Null.NullInteger);

                if (file.Content != null)
                {
                    DotNetNuke.Data.DataProvider.Instance().UpdateFileContent(file.FileId, file.Content);
                }
            }
        }

        private void SyncUserFolder(int portalId, ExportFolder folder)
        {
            var portal = PortalController.Instance.GetPortal(portalId);
            var tempUsersFolderPath =
                $"{string.Format(UsersAssetsTempFolder, portal.HomeDirectoryMapPath.TrimEnd('\\'))}{folder.FolderPath}";
            var newUsersFolderPath = $"{portal.HomeDirectoryMapPath}{folder.FolderPath}";
            if (!Directory.Exists(tempUsersFolderPath))
            {
                return;
            }

            if (!Directory.Exists(newUsersFolderPath))
            {
                Directory.CreateDirectory(newUsersFolderPath);
            }

            var files = Directory.GetFiles(tempUsersFolderPath, "*.*", SearchOption.AllDirectories);
            var dirInfo = new DirectoryInfo(newUsersFolderPath);
            foreach (
                var mFile in
                    files.Select(file => new System.IO.FileInfo(file)))
            {
                if (File.Exists(dirInfo + "\\" + mFile.Name))
                {
                    File.Delete(dirInfo + "\\" + mFile.Name);
                }

                mFile.MoveTo(dirInfo + "\\" + mFile.Name);
            }
        }

        private string GetActualFileName(ExportFile objFile)
        {
            return (objFile.StorageLocation == (int)FolderController.StorageLocationTypes.SecureFileSystem)
                ? objFile.FileName + Globals.glbProtectedExtension
                : objFile.FileName;
        }

        private int GetCurrentSkip()
        {
            if (!string.IsNullOrEmpty(this.CheckPoint.StageData))
            {
                dynamic stageData = JsonConvert.DeserializeObject(this.CheckPoint.StageData);
                return Convert.ToInt32(stageData.skip) ?? 0;
            }

            return 0;
        }

        private int GetLocalWorkFlowId(int? exportedWorkFlowId)
        {
            if (exportedWorkFlowId != null && exportedWorkFlowId > 1) // 1 is direct publish
            {
                var state = this.Repository.GetItem<ExportWorkflow>(item => item.WorkflowID == exportedWorkFlowId);
                return state?.LocalId ?? -1;
            }

            return -1;
        }
    }
}
