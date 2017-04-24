#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using System.Linq;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Common;
using Dnn.ExportImport.Components.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Dnn.ExportImport.Dto.Assets;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using Newtonsoft.Json;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    public class AssetsExportService : BasePortableService
    {
        private const string DefaultUsersFoldersPath = "Users";

        private static readonly Regex UserFolderEx = new Regex(@"users/\d+/\d+/(\d+)/",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly string _assetsFolder =
            $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{{0}}\\{Constants.ExportZipFiles}";


        private const string UsersAssetsTempFolder = "{0}\\TempUsers\\";

        public override string Category => Constants.Category_Assets;

        public override string ParentCategory => null;

        public override uint Priority => 50;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (CheckCancelled(exportJob)) return;
            //Skip the export if all the folders have been processed already.
            if (CheckPoint.Stage >= 1)
                return;

            //Create Zip File to hold files
            var skip = GetCurrentSkip();
            var currentIndex = skip;
            var totalFolderExported = 0;
            var totalFolderPermissionsExported = 0;
            var totalFilesExported = 0;
            var portalId = exportJob.PortalId;
            try
            {
                var assetsFile = string.Format(_assetsFolder, exportJob.Directory.TrimEnd('\\').TrimEnd('/'));

                if (CheckPoint.Stage == 0)
                {
                    var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
                    var toDate = exportDto.ToDateUtc.ToLocalTime();
                    var portal = PortalController.Instance.GetPortal(portalId);

                    var folders =
                        CBO.FillCollection<ExportFolder>(DataProvider.Instance()
                            .GetFolders(portalId, toDate, fromDate)).ToList();
                    var totalFolders = folders.Any() ? folders.Count : 0;
                    folders = folders.Skip(skip).ToList();


                    //Update the total items count in the check points. This should be updated only once.
                    CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? totalFolders : CheckPoint.TotalItems;
                    CheckPoint.ProcessedItems = skip;
                    CheckPoint.Progress = CheckPoint.TotalItems > 0 ? skip * 100.0 / CheckPoint.TotalItems : 0;
                    if (CheckPointStageCallback(this)) return;
                    using (var zipArchive = CompressionUtil.OpenCreate(assetsFile))
                    {
                        foreach (var folder in folders)
                        {
                            if (CheckCancelled(exportJob)) break;
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
                                //If parent id exists then change the parent folder id to parent id.
                                folder.ParentId =
                                    Repository.GetItem<ExportFolder>(
                                        x => x.FolderId == Convert.ToInt32(folder.ParentId))?.Id;
                            }

                            Repository.CreateItem(folder, null);
                            totalFolderExported++;
                            //Include permissions only if IncludePermissions=true
                            if (exportDto.IncludePermissions)
                            {
                                var permissions =
                                    CBO.FillCollection<ExportFolderPermission>(DataProvider.Instance()
                                        .GetFolderPermissionsByPath(portalId, folder.FolderPath, toDate, fromDate));
                                Repository.CreateItems(permissions, folder.Id);
                                totalFolderPermissionsExported += permissions.Count;
                            }
                            Repository.CreateItems(files, folder.Id);
                            totalFilesExported += files.Count;
                            var folderOffset = portal.HomeDirectoryMapPath.Length +
                                               (portal.HomeDirectoryMapPath.EndsWith("\\") ? 0 : 1);

                            if (folder.StorageLocation != (int)FolderController.StorageLocationTypes.DatabaseSecure)
                            {

                                CompressionUtil.AddFilesToArchive(zipArchive, files.Select(file => portal.HomeDirectoryMapPath + folder.FolderPath + GetActualFileName(file)),
                                    folderOffset, isUserFolder ? "TempUsers" : null);

                            }
                            CheckPoint.ProcessedItems++;
                            CheckPoint.Progress = CheckPoint.ProcessedItems * 100.0 / totalFolders;
                            CheckPoint.StageData = null;
                            currentIndex++;
                            //After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                            if (currentIndex % 10 == 0 && CheckPointStageCallback(this)) return;
                            Repository.RebuildIndex<ExportFolder>(x => x.Id, true);
                            Repository.RebuildIndex<ExportFolder>(x => x.UserId);
                            Repository.RebuildIndex<ExportFile>(x => x.ReferenceId);
                        }
                    }
                    CheckPoint.Completed = true;
                    CheckPoint.Stage++;
                    currentIndex = 0;
                    CheckPoint.Progress = 100;
                }
            }
            finally
            {
                CheckPoint.StageData = currentIndex > 0 ? JsonConvert.SerializeObject(new { skip = currentIndex }) : null;
                CheckPointStageCallback(this);
                Result.AddSummary("Exported Folders", totalFolderExported.ToString());
                Result.AddSummary("Exported Folder Permissions", totalFolderPermissionsExported.ToString());
                Result.AddSummary("Exported Files", totalFilesExported.ToString());
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (CheckCancelled(importJob)) return;
            //Stage 1: Portals files unzipped. 
            //Stage 2: All folders and files imported.
            //Stage 3: Synchronization completed.
            //Skip the export if all the folders have been processed already.
            if (CheckPoint.Stage >= 2)
                return;

            var totalFolderImported = 0;
            var totalFolderPermissionsImported = 0;
            var totalFilesImported = 0;
            var skip = GetCurrentSkip();
            var currentIndex = skip;
            var portalId = importJob.PortalId;
            var portal = PortalController.Instance.GetPortal(portalId);
            var assetsFile = string.Format(_assetsFolder, importJob.Directory.TrimEnd('\\').TrimEnd('/'));
            var userFolderPath = string.Format(UsersAssetsTempFolder, portal.HomeDirectoryMapPath.TrimEnd('\\'));
            if (CheckPoint.Stage == 0)
            {
                if (!File.Exists(assetsFile))
                {
                    Result.AddLogEntry("AssetsFileNotFound", "Assets file not found. Skipping assets import",
                        ReportLevel.Warn);
                }
                else
                {
                    CompressionUtil.UnZipArchive(assetsFile, portal.HomeDirectoryMapPath,
                        importDto.CollisionResolution == CollisionResolution.Overwrite);
                    //Stage 1: Once unzipping of portal files is completed.
                    CheckPoint.Stage++;
                    CheckPoint.StageData = null;
                    CheckPoint.Progress = 10;
                    if (CheckPointStageCallback(this)) return;
                }
            }

            if (CheckPoint.Stage == 1)
            {
                try
                {
                    //Stage 2 starts
                    var sourceFolders = Repository.GetAllItems<ExportFolder>(x => x.CreatedOnDate, true, skip).ToList();

                    var totalFolders = sourceFolders.Any() ? sourceFolders.Count : 0;
                    //Update the total items count in the check points. This should be updated only once.
                    CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? totalFolders : CheckPoint.TotalItems;
                    if (CheckPointStageCallback(this)) return;

                    foreach (var sourceFolder in sourceFolders)
                    {
                        if (CheckCancelled(importJob)) break;
                        // PROCESS FOLDERS
                        //Create new or update existing folder
                        if (ProcessFolder(importJob, importDto, sourceFolder))
                        {
                            totalFolderImported++;

                            //Include permissions only if permissions were exported in package.
                            if (importDto.ExportDto.IncludePermissions)
                            {
                                // PROCESS FOLDER PERMISSIONS
                                var sourceFolderPermissions =
                                    Repository.GetRelatedItems<ExportFolderPermission>(sourceFolder.Id).ToList();
                                //Replace folderId for each permission with new one.
                                sourceFolderPermissions.ForEach(x =>
                                {
                                    x.FolderId = Convert.ToInt32(sourceFolder.FolderId);
                                    x.FolderPath = sourceFolder.FolderPath;
                                });

                                // PROCESS FOLDER PERMISSIONS
                                //File local files in the system related to the folder path.
                                var localPermissions =
                                    CBO.FillCollection<ExportFolderPermission>(DataProvider.Instance()
                                        .GetFolderPermissionsByPath(portalId, sourceFolder.FolderPath,
                                            DateUtils.GetDatabaseUtcTime().AddYears(1), null));

                                foreach (var folderPermission in sourceFolderPermissions)
                                {
                                    ProcessFolderPermission(importJob, importDto, folderPermission,
                                        localPermissions);
                                }
                                totalFolderPermissionsImported += sourceFolderPermissions.Count;
                            }

                            // PROCESS FILES
                            var sourceFiles =
                                Repository.GetRelatedItems<ExportFile>(sourceFolder.Id).ToList();
                            //Replace folderId for each file with new one.
                            sourceFiles.ForEach(x =>
                            {
                                x.FolderId = Convert.ToInt32(sourceFolder.FolderId);
                                x.Folder = sourceFolder.FolderPath;
                            });

                            //File local files in the system related to the folder
                            var localFiles =
                                CBO.FillCollection<ExportFile>(DataProvider.Instance()
                                    .GetFiles(portalId, sourceFolder.FolderId,
                                        DateUtils.GetDatabaseUtcTime().AddYears(1), null));

                            foreach (var file in sourceFiles)
                            {
                                ProcessFiles(importJob, importDto, file, localFiles);
                            }
                            totalFilesImported += sourceFiles.Count;
                        }

                        currentIndex++;
                        CheckPoint.ProcessedItems++;
                        CheckPoint.Progress = 10 + CheckPoint.ProcessedItems * 90.0 / totalFolders;
                        //After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                        if (currentIndex % 10 == 0 && CheckPointStageCallback(this)) return;
                    }
                    currentIndex = 0;
                    CheckPoint.Completed = true;
                    CheckPoint.Stage++;
                    CheckPoint.Progress = 100;
                }
                finally
                {
                    CheckPoint.StageData = currentIndex > 0
                        ? JsonConvert.SerializeObject(new { skip = currentIndex })
                        : null;
                    CheckPointStageCallback(this);

                    Result.AddSummary("Imported Folders", totalFolderImported.ToString());
                    Result.AddSummary("Imported Folder Permissions", totalFolderPermissionsImported.ToString());
                    Result.AddSummary("Imported Files", totalFilesImported.ToString());

                    if (Directory.Exists(userFolderPath) && currentIndex == 0)
                        Directory.Delete(userFolderPath, true);
                }
            }
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<ExportFolder>();
        }

        private bool ProcessFolder(ExportImportJob importJob, ImportDto importDto, ExportFolder folder)
        {
            var portalId = importJob.PortalId;
            if (folder == null) return false;

            var existingFolder = CBO.FillObject<ExportFolder>(DotNetNuke.Data.DataProvider.Instance().GetFolder(portalId, folder.FolderPath ?? ""));
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
                        //TODO: Log that user was ignored.
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                }
            }
            folder.FolderPath = string.IsNullOrEmpty(folder.FolderPath) ? "" : folder.FolderPath;
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, folder.FolderMappingName);
            if (folderMapping == null) return false;

            if (isUpdate)
            {
                Util.FixDateTime(existingFolder);
                DotNetNuke.Data.DataProvider.Instance()
                    .UpdateFolder(importJob.PortalId, folder.VersionGuid, existingFolder.FolderId, folder.FolderPath,
                        folder.StorageLocation, folder.MappedPath, folder.IsProtected, folder.IsCached,
                        DateUtils.GetDatabaseLocalTime(), modifiedBy, folderMapping.FolderMappingID, folder.IsVersioned,
                        folder.WorkflowId ?? Null.NullInteger, existingFolder.ParentId ?? Null.NullInteger);

                folder.FolderId = existingFolder.FolderId;

                if (folder.UserId != null && folder.UserId > 0 && !string.IsNullOrEmpty(folder.Username))
                {
                    SyncUserFolder(importJob.PortalId, folder);
                }
            }
            else
            {
                folder.FolderMappingId = folderMapping.FolderMappingID;
                var createdBy = Util.GetUserIdByName(importJob, folder.CreatedByUserId, folder.CreatedByUserName);
                if (folder.ParentId != null && folder.ParentId > 0)
                {
                    //Find the previously created parent folder id.
                    folder.ParentId = CBO.FillObject<ExportFolder>(DotNetNuke.Data.DataProvider.Instance().GetFolder(portalId, folder.ParentFolderPath ?? ""))?.FolderId;
                }
                //ignore folders which start with Users but are not user folders.
                if (!folder.FolderPath.StartsWith(DefaultUsersFoldersPath))
                {
                    folder.FolderId = DotNetNuke.Data.DataProvider.Instance()
                        .AddFolder(importJob.PortalId, Guid.NewGuid(), folder.VersionGuid, folder.FolderPath,
                            folder.MappedPath, folder.StorageLocation, folder.IsProtected, folder.IsCached,
                            DateUtils.GetDatabaseLocalTime(),
                            createdBy, folderMapping.FolderMappingID, folder.IsVersioned, folder.WorkflowId ?? Null.NullInteger,
                            folder.ParentId ?? Null.NullInteger);
                }
                //Case when the folder is a user folder.
                else if (folder.UserId != null && folder.UserId > 0 && !string.IsNullOrEmpty(folder.Username))
                {
                    var userInfo = UserController.GetUserByName(portalId, folder.Username);
                    if (userInfo == null)
                    {
                        folder.FolderId = 0;
                        return false;
                    }
                    var newFolder = FolderManager.Instance.GetUserFolder(userInfo);
                    folder.FolderId = newFolder.FolderID;
                    folder.FolderPath = newFolder.FolderPath;
                    SyncUserFolder(importJob.PortalId, folder);
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

            if (folderPermission == null) return;

            var existingFolderPermission = localPermissions.FirstOrDefault(
                x =>
                    (x.FolderPath == folderPermission.FolderPath ||
                     (string.IsNullOrEmpty(x.FolderPath) && string.IsNullOrEmpty(folderPermission.FolderPath))) &&
                    x.PermissionCode == folderPermission.PermissionCode &&
                    x.PermissionKey == folderPermission.PermissionKey
                    && x.PermissionName == folderPermission.PermissionName &&
                    (x.RoleName == folderPermission.RoleName ||
                     (string.IsNullOrEmpty(x.RoleName) && string.IsNullOrEmpty(folderPermission.RoleName)))
                    &&
                    (x.Username == folderPermission.Username ||
                     (string.IsNullOrEmpty(x.Username) && string.IsNullOrEmpty(folderPermission.Username))));

            var isUpdate = false;
            if (existingFolderPermission != null)
            {
                switch (importDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore:
                        //TODO: Log that user was ignored.
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
                        folderPermission.UserId =
                            UserController.GetUserByName(portalId, folderPermission.Username)?.UserID;
                        if (folderPermission.UserId == null)
                            return;
                    }
                    if (folderPermission.RoleId != null && folderPermission.RoleId >= 0 && !string.IsNullOrEmpty(folderPermission.RoleName))
                    {
                        folderPermission.RoleId =
                            RoleController.Instance.GetRoleByName(portalId, folderPermission.RoleName)?.RoleID;
                        if (folderPermission.RoleId == null)
                            return;
                    }
                    var createdBy = Util.GetUserIdByName(importJob, folderPermission.CreatedByUserId,
                        folderPermission.CreatedByUserName);

                    folderPermission.FolderPermissionId = DotNetNuke.Data.DataProvider.Instance()
                        .AddFolderPermission(folderPermission.FolderId, folderPermission.PermissionId,
                            folderPermission.RoleId ?? Convert.ToInt32(Globals.glbRoleNothing), folderPermission.AllowAccess,
                            folderPermission.UserId ?? Null.NullInteger, createdBy);
                }
            }
            folderPermission.LocalId = folderPermission.FolderPermissionId;
        }

        private void ProcessFiles(ExportImportJob importJob, ImportDto importDto, ExportFile file, IEnumerable<ExportFile> localFiles)
        {
            var portalId = importJob.PortalId;

            if (file == null) return;
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
                        //TODO: Log that user was ignored.
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
                        //file.ContentItemId ?? Null.NullInteger);--If we keep it we will see FK_PK relationship errors.
                        Null.NullInteger);


                if (file.Content != null)
                    DotNetNuke.Data.DataProvider.Instance().UpdateFileContent(file.FileId, file.Content);
            }
        }

        private void SyncUserFolder(int portalId, ExportFolder folder)
        {
            var portal = PortalController.Instance.GetPortal(portalId);
            var tempUsersFolderPath =
                $"{string.Format(UsersAssetsTempFolder, portal.HomeDirectoryMapPath.TrimEnd('\\'))}{folder.FolderPath}";
            var newUsersFolderPath = $"{portal.HomeDirectoryMapPath}{folder.FolderPath}";
            if (!Directory.Exists(tempUsersFolderPath))
                return;
            if (!Directory.Exists(newUsersFolderPath))
                Directory.CreateDirectory(newUsersFolderPath);
            var files = Directory.GetFiles(tempUsersFolderPath, "*.*", SearchOption.AllDirectories);
            var dirInfo = new DirectoryInfo(newUsersFolderPath);
            foreach (
                var mFile in
                    files.Select(file => new System.IO.FileInfo(file)))
            {
                if (File.Exists(dirInfo + "\\" + mFile.Name))
                    File.Delete(dirInfo + "\\" + mFile.Name);
                mFile.MoveTo(dirInfo + "\\" + mFile.Name);
            }
        }

        private static bool IsUserFolder(string folderPath, out int? userId)
        {
            userId = null;
            var match = UserFolderEx.Match(folderPath);
            if (match.Success)
                userId = int.Parse(match.Groups[1].Value);
            return match.Success;
        }

        private string GetActualFileName(ExportFile objFile)
        {
            return (objFile.StorageLocation == (int)FolderController.StorageLocationTypes.SecureFileSystem)
                ? objFile.FileName + Globals.glbProtectedExtension
                : objFile.FileName;
        }

        private int GetCurrentSkip()
        {
            if (!string.IsNullOrEmpty(CheckPoint.StageData))
            {
                dynamic stageData = JsonConvert.DeserializeObject(CheckPoint.StageData);
                return Convert.ToInt32(stageData.skip) ?? 0;
            }
            return 0;
        }
    }
}