using System.IO;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Assets;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using System.Linq;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Common;
using Dnn.ExportImport.Components.Common;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNetNuke.Data;
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

        public override uint Priority => 5;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (CheckCancelled(exportJob)) return;
            //Skip the export if all the folders have been processed already.
            if (CheckPoint.Stage >= 2)
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
                var assetsFile = string.Format(_assetsFolder, exportJob.ExportDir.TrimEnd('\\').TrimEnd('/'));

                if (CheckPoint.Stage == 0)
                {
                    //Sync db and filesystem before exporting so all required files are found
                    var folderManager = FolderManager.Instance;
                    folderManager.Synchronize(portalId);
                    CheckPoint.Stage++;
                    CheckPoint.Progress = 5;
                    CheckPointStageCallback(this);
                }
                if (CheckPoint.Stage == 1)
                {
                    var sinceDate = exportDto.SinceTime?.DateTime;
                    var tillDate = exportJob.CreatedOnDate;
                    var portal = PortalController.Instance.GetPortal(portalId);

                    var folders =
                        CBO.FillCollection<ExportFolder>(DataProvider.Instance()
                            .GetFolders(portalId, tillDate, sinceDate)).ToList();
                    folders = folders.Skip(skip).ToList();
                    var totalFolders = folders.Any() ? folders.Count : 0;

                    var progressStep = 95.0 / totalFolders;

                    foreach (var folder in folders)
                    {
                        var isUserFolder = false;
                        var permissions =
                            CBO.FillCollection<ExportFolderPermission>(DataProvider.Instance()
                                .GetFolderPermissionsByPath(portalId, folder.FolderPath, tillDate, sinceDate));
                        var files =
                            CBO.FillCollection<ExportFile>(
                                DataProvider.Instance()
                                    .GetFiles(portalId, folder.FolderId, tillDate, sinceDate));
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
                        Repository.CreateItems(permissions, folder.Id);
                        totalFolderPermissionsExported += permissions.Count;
                        Repository.CreateItems(files, folder.Id);
                        totalFilesExported += files.Count;
                        var folderOffset = portal.HomeDirectoryMapPath.Length +
                                           (portal.HomeDirectoryMapPath.EndsWith("\\") ? 0 : 1);

                        CompressionUtil.AddFilesToArchive(
                            files.Select(
                                file => portal.HomeDirectoryMapPath + folder.FolderPath + GetActualFileName(file)),
                            assetsFile, folderOffset, isUserFolder ? "TempUsers" : null);

                        CheckPoint.Progress += progressStep;
                        currentIndex++;
                        if (CheckPointStageCallback(this)) return;
                    }
                    CheckPoint.Stage++;
                    CheckPoint.StageData = null;
                    currentIndex = 0;
                    if (CheckPointStageCallback(this)) return;
                    //TODO: Check if we need this step or not.
                    //var folderMappings =
                    //    CBO.FillCollection<ExportFolderMapping>(DataProvider.Instance()
                    //        .GetFolderMappings(portalId,tillDate, sinceDate)).ToList();
                    //Repository.CreateItems(folderMappings, null);
                }
            }
            finally
            {
                CheckPoint.StageData = currentIndex > 0 ? JsonConvert.SerializeObject(new { skip = currentIndex }) : null;
                CheckPointStageCallback(this);
            }
            Result.AddSummary("Exported Folders", totalFolderExported.ToString());
            Result.AddSummary("Exported Folder Permissions", totalFolderPermissionsExported.ToString());
            Result.AddSummary("Exported Files", totalFilesExported.ToString());
        }

        public override void ImportData(ExportImportJob importJob, ExportDto exporteDto)
        {
            if (CheckCancelled(importJob)) return;
            //Stage 1: Portals files unzipped. 
            //Stage 2: All folders and files imported.
            //Stage 3: Synchronization completed.
            //Skip the export if all the folders have been processed already.
            if (CheckPoint.Stage >= 3)
                return;

            var totalFolderImported = 0;
            var totalFolderPermissionsImported = 0;
            var totalFilesImported = 0;
            var skip = GetCurrentSkip();
            var currentIndex = skip;
            var portalId = importJob.PortalId;
            var portal = PortalController.Instance.GetPortal(portalId);
            var assetsFile = string.Format(_assetsFolder, importJob.ExportDir.TrimEnd('\\').TrimEnd('/'));
            var userFolderPath = string.Format(UsersAssetsTempFolder, portal.HomeDirectoryMapPath.TrimEnd('\\'));
            if (CheckPoint.Stage == 0)
            {
                CompressionUtil.UnZipArchive(assetsFile, portal.HomeDirectoryMapPath, exporteDto.CollisionResolution== CollisionResolution.Overwrite);
                //Stage 1: Once unzipping of portal files is completed.
                CheckPoint.Stage++;
                CheckPoint.StageData = null;
                CheckPoint.Progress = 10;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckPoint.Stage == 1)
            {
                try
                {
                    //Stage 2 starts
                    var localFolders =
                        CBO.FillCollection<ExportFolder>(DataProvider.Instance().GetFolders(portalId, DateTime.UtcNow.AddYears(1), null)).ToList();
                    var sourceFolders = Repository.GetAllItems<ExportFolder>(x => x.CreatedOnDate, true, skip).ToList();

                    var totalFolders = sourceFolders.Any() ? sourceFolders.Count : 0;
                    var progressStep = 95.0 / totalFolders;

                    foreach (var sourceFolder in sourceFolders)
                    {
                        using (var db = DataContext.Instance())
                        {
                            /** PROCESS FOLDERS **/
                            //Create new or update existing folder
                            if (ProcessFolder(importJob, exporteDto, db, sourceFolder, localFolders))
                            {
                                totalFolderImported++;
                                Repository.UpdateItem(sourceFolder);
                                //SyncUserFolder(importJob, userFolderPath, exportFolder);

                                var sourceFolderPermissions =
                                    Repository.GetRelatedItems<ExportFolderPermission>(sourceFolder.Id).ToList();
                                //Replace folderId for each permission with new one.
                                sourceFolderPermissions.ForEach(x =>
                                {
                                    x.FolderId = Convert.ToInt32(sourceFolder.LocalId);
                                    x.FolderPath = sourceFolder.FolderPath;
                                });

                                /** PROCESS FOLDER PERMISSIONS **/
                                //File local files in the system related to the folder path.
                                var localPermissions =
                                    CBO.FillCollection<ExportFolderPermission>(DataProvider.Instance()
                                        .GetFolderPermissionsByPath(portalId, sourceFolder.FolderPath, DateTime.UtcNow.AddYears(1), null));

                                foreach (var folderPermission in sourceFolderPermissions)
                                {
                                    ProcessFolderPermission(importJob, exporteDto, db, folderPermission,
                                        localPermissions);
                                    Repository.UpdateItem(folderPermission);
                                }
                                totalFolderPermissionsImported += sourceFolderPermissions.Count;

                                /** PROCESS FILES **/
                                var sourceFiles =
                                    Repository.GetRelatedItems<ExportFile>(sourceFolder.Id).ToList();
                                //Replace folderId for each file with new one.
                                sourceFiles.ForEach(x =>
                                {
                                    x.FolderId = Convert.ToInt32(sourceFolder.LocalId);
                                    x.Folder = sourceFolder.FolderPath;
                                });

                                //File local files in the system related to the folder
                                var localFiles =
                                    CBO.FillCollection<ExportFile>(DataProvider.Instance()
                                        .GetFiles(portalId, sourceFolder.FolderId, DateTime.UtcNow.AddYears(1), null));

                                foreach (var file in sourceFiles)
                                {
                                    ProcessFiles(importJob, exporteDto, db, file, localFiles);
                                    Repository.UpdateItem(file);
                                }
                                totalFilesImported += sourceFiles.Count;
                            }
                        }
                        currentIndex++;
                        CheckPoint.Progress += progressStep;
                        if (CheckPointStageCallback(this)) return;
                    }
                    currentIndex = 0;
                    CheckPoint.Stage++;
                    CheckPoint.StageData = null;
                    CheckPoint.Progress = 95;
                    if (CheckPointStageCallback(this)) return;
                }
                finally
                {
                    CheckPoint.StageData = currentIndex > 0
                        ? JsonConvert.SerializeObject(new { skip = currentIndex })
                        : null;
                    CheckPointStageCallback(this);

                    if (Directory.Exists(userFolderPath) && currentIndex == 0)
                        Directory.Delete(userFolderPath, true);
                }
                Result.AddSummary("Imported Folders", totalFolderImported.ToString());
                Result.AddSummary("Imported Folder Permissions", totalFolderPermissionsImported.ToString());
                Result.AddSummary("Imported Files", totalFilesImported.ToString());
            }

            if (CheckPoint.Stage == 2)
            {
                var folderManager = FolderManager.Instance;
                folderManager.Synchronize(portalId);
                CheckPoint.Stage++;
                CheckPoint.StageData = null;
                CheckPoint.Progress = 100;
                CheckPointStageCallback(this);
            }
        }

        private bool ProcessFolder(ExportImportJob importJob, ExportDto exporteDto, IDataContext db,
            ExportFolder folder, IEnumerable<ExportFolder> localFolders)
        {
            var portalId = importJob.PortalId;

            if (folder == null) return false;
            //TODO: First check if the folder is a user folder. If yes, then replace the user ids with the new one in the system.

            var existingFolder =
                localFolders.FirstOrDefault(
                    x =>
                        x.FolderPath == folder.FolderPath ||
                        (string.IsNullOrEmpty(x.FolderPath) && string.IsNullOrEmpty(folder.FolderPath)));
            var isUpdate = false;
            var repExportFolder = db.GetRepository<ExportFolder>();
            var modifiedBy = Util.GetUserIdOrName(importJob, folder.LastModifiedByUserId, folder.LastModifiedByUserName);
            if (existingFolder != null)
            {
                switch (exporteDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore:
                        //TODO: Log that user was ignored.
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                }
            }
            folder.PortalId = portalId;
            if (isUpdate)
            {
                existingFolder.LastModifiedByUserId = modifiedBy;
                existingFolder.LastModifiedOnDate = DateTime.UtcNow;
                existingFolder.LastUpdated = folder.LastUpdated;
                existingFolder.IsProtected = folder.IsProtected;
                existingFolder.IsCached = folder.IsCached;
                existingFolder.IsVersioned = folder.IsVersioned;
                Util.FixDateTime(existingFolder);
                repExportFolder.Update(existingFolder);

                folder.FolderId = existingFolder.FolderId;
                if (folder.UserId != null && folder.UserId > 0 && !string.IsNullOrEmpty(folder.Username))
                {
                    SyncUserFolder(importJob.PortalId, folder);
                }
                //TODO: Is there any real need to update existing folder?
            }
            else
            {
                var previousParent = folder.ParentId;
                folder.FolderId = 0;
                folder.FolderMappingId =
                    FolderMappingController.Instance.GetFolderMapping(portalId, folder.FolderMappingName)
                        .FolderMappingID;
                var createdBy = Util.GetUserIdOrName(importJob, folder.CreatedByUserId, folder.CreatedByUserName);
                if (folder.ParentId != null && folder.ParentId > 0)
                {
                    //Find the previously created parent folder id.
                    folder.ParentId = Repository.GetItem<ExportFolder>(Convert.ToInt32(folder.ParentId))?.LocalId;
                }
                folder.CreatedByUserId = createdBy;
                folder.CreatedOnDate = DateTime.UtcNow;
                folder.LastModifiedByUserId = modifiedBy;
                folder.LastModifiedOnDate = DateTime.UtcNow;
                //ignore folders which start with Users but are not user folders.
                if (!folder.FolderPath.StartsWith(DefaultUsersFoldersPath))
                {
                    repExportFolder.Insert(folder);
                }
                //Case when the folder is a user folder.
                else if (folder.UserId != null && folder.UserId > 0 && !string.IsNullOrEmpty(folder.Username))
                {
                    var userInfo = UserController.GetUserByName(portalId, folder.Username);
                    if (userInfo == null)
                    {
                        folder.LocalId = 0;
                        return false;
                    }
                    var previousFolder = folder;
                    var newFolder = FolderManager.Instance.GetUserFolder(userInfo);
                    ExportFolder.MapFromFolderInfo(newFolder, folder); //Map ExportFolder from new folder. 
                    folder.Id = previousFolder.Id;
                    folder.UserId = previousFolder.UserId; //This is still the old user id.
                    folder.Username = previousFolder.Username; //This is still the old username.
                    folder.LastModifiedByUserName = previousFolder.LastModifiedByUserName;
                    folder.CreatedByUserName = previousFolder.CreatedByUserName;
                    folder.LocalId = folder.FolderId;
                    SyncUserFolder(importJob.PortalId, folder);
                    return true;
                }
                else
                {
                    folder.LocalId = 0;
                    return false;
                }
                //Keep the parent Id.
                folder.ParentId = previousParent;
            }
            folder.LocalId = folder.FolderId;
            return true;
        }

        private void ProcessFolderPermission(ExportImportJob importJob, ExportDto exporteDto, IDataContext db,
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
            var repExportFolderPermission = db.GetRepository<ExportFolderPermission>();
            var modifiedBy = Util.GetUserIdOrName(importJob, folderPermission.LastModifiedByUserId,
                folderPermission.LastModifiedByUserName);
            if (existingFolderPermission != null)
            {
                switch (exporteDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore:
                        //TODO: Log that user was ignored.
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                }
            }
            if (isUpdate)
            {
                existingFolderPermission.LastModifiedByUserId = modifiedBy;
                existingFolderPermission.LastModifiedOnDate = DateTime.UtcNow;
                existingFolderPermission.FolderId = folderPermission.FolderId;
                existingFolderPermission.AllowAccess = folderPermission.AllowAccess;
                repExportFolderPermission.Update(existingFolderPermission);
                folderPermission.FolderPermissionId = existingFolderPermission.FolderPermissionId;

                //TODO: Is there any real need to update existing folder permission?
            }
            else
            {
                var permissionId = DataProvider.Instance()
                    .GetPermissionId(folderPermission.PermissionCode, folderPermission.PermissionKey,
                        folderPermission.PermissionName);
                if (permissionId != null)
                {
                    folderPermission.FolderPermissionId = 0;
                    folderPermission.LastModifiedByUserId = modifiedBy;
                    folderPermission.LastModifiedOnDate = DateTime.UtcNow;
                    folderPermission.PermissionId = Convert.ToInt32(permissionId);
                    if (folderPermission.UserId != null && folderPermission.UserId > 0)
                    {
                        folderPermission.UserId =
                            UserController.GetUserByName(portalId, folderPermission.Username)?.UserID;
                    }
                    if (folderPermission.RoleId != null && folderPermission.RoleId >= 0)
                    {
                        folderPermission.RoleId =
                            RoleController.Instance.GetRoleByName(portalId, folderPermission.RoleName)?.RoleID;
                    }
                    var createdBy = Util.GetUserIdOrName(importJob, folderPermission.CreatedByUserId,
                        folderPermission.CreatedByUserName);

                    folderPermission.CreatedByUserId = createdBy;
                    folderPermission.CreatedOnDate = DateTime.UtcNow;
                    repExportFolderPermission.Insert(folderPermission);
                }
            }
            folderPermission.LocalId = folderPermission.FolderPermissionId;
        }

        private void ProcessFiles(ExportImportJob importJob, ExportDto exporteDto, IDataContext db,
            ExportFile file, IEnumerable<ExportFile> localFiles)
        {
            var portalId = importJob.PortalId;

            if (file == null) return;
            var existingFile = localFiles.FirstOrDefault(x => x.FileName == file.FileName);
            var isUpdate = false;
            var repExportFile = db.GetRepository<ExportFile>();
            var modifiedBy = Util.GetUserIdOrName(importJob, file.LastModifiedByUserId, file.LastModifiedByUserName);
            if (existingFile != null)
            {
                switch (exporteDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore:
                        //TODO: Log that user was ignored.
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                }
            }
            file.PortalId = portalId;
            if (isUpdate)
            {
                existingFile.LastModifiedOnDate = DateTime.UtcNow;
                existingFile.LastModifiedByUserId = modifiedBy;
                file.FileId = existingFile.FileId;
                ExportFile.MapFile(existingFile, file);
                repExportFile.Update(existingFile);
                if ((file.Content != null && existingFile.Content == null) ||
                    (existingFile.Content != null && file.Content == null) ||
                    (file.Content != null && existingFile.Content != null &&
                     file.Content.SequenceEqual(existingFile.Content)))
                {
                    DotNetNuke.Data.DataProvider.Instance().UpdateFileContent(file.FileId, file.Content);
                }
                //TODO: Is there any real need to update existing file?

                //TODO: Unzip and replace the file here. 
            }
            else
            {
                file.FileId = 0;
                var createdBy = Util.GetUserIdOrName(importJob, file.CreatedByUserId, file.CreatedByUserName);
                file.CreatedByUserId = createdBy;
                file.CreatedOnDate = DateTime.UtcNow;
                file.LastModifiedByUserId = modifiedBy;
                file.LastModifiedOnDate = DateTime.UtcNow;
                repExportFile.Insert(file);
                if (file.Content != null)
                    DotNetNuke.Data.DataProvider.Instance().UpdateFileContent(file.FileId, file.Content);

                //TODO: Unzip the file here. 
            }
            file.LocalId = file.FileId;
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