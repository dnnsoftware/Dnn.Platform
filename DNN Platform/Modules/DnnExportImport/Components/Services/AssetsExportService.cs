using System.IO;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Assets;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using System.Linq;
using DotNetNuke.Services.FileSystem;
using ICSharpCode.SharpZipLib.Zip;
using DotNetNuke.Common;
using Dnn.ExportImport.Components.Common;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    public class AssetsExportService : Potable2Base
    {
        private static readonly Regex UserFolderEx = new Regex(@"users/\d+/\d+/(\d+)/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly string AssetsFolder;

        static AssetsExportService()
        {
            AssetsFolder = $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{{0}}{"_Assets.zip"}";
        }

        private int _progressPercentage;

        public override string Category => "ASSETS";
        public override string ParentCategory => null;
        public override uint Priority => 5;

        public int ProgressPercentage
        {
            get { return _progressPercentage; }
            private set
            {
                if (value < 0) value = 0;
                else if (value > 100) value = 100;
                _progressPercentage = value;
            }
        }

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            var portalId = exportJob.PortalId;
            var sinceDate = exportDto.ExportTime?.UtcDateTime;
            var portal = PortalController.Instance.GetPortal(portalId);
            var totalFolderExported = 0;
            var totalFolderPermissionsExported = 0;
            var totalFilesExported = 0;
            ProgressPercentage = 0;

            //Sync db and filesystem before exporting so all required files are found
            var folderManager = FolderManager.Instance;
            folderManager.Synchronize(portalId);
            ProgressPercentage = 5;
            var filename = string.Format(AssetsFolder, exportJob.ExportFile);
            //Create Zip File to hold files
            var assetsStream = new ZipOutputStream(File.Create(filename));
            assetsStream.SetLevel(6);
            try
            {
                var folders =
                    CBO.FillCollection<ExportFolder>(DataProvider.Instance()
                        .GetFolders(portalId, sinceDate)).ToList();
                var totalFolders = folders.Any() ? folders.Count : 0;
                var progressStep = totalFolders/95;

                foreach (var folder in folders)
                {
                    var permissions =
                        CBO.FillCollection<ExportFolderPermission>(DataProvider.Instance()
                            .GetFolderPermissionsByPath(portalId, folder.FolderPath, sinceDate));
                    var files =
                        CBO.FillCollection<ExportFile>(
                            DataProvider.Instance()
                                .GetFiles(portalId, folder.FolderId, sinceDate));
                    int? userId;
                    if (IsUserFolder(folder.FolderPath, out userId))
                    {
                        folder.UserId = userId;
                        folder.Username = UserController.GetUserById(portalId, Convert.ToInt32(userId))?.Username;
                    }
                    if (folder.ParentId != null && folder.ParentId > 0)
                    {
                        //If parent id exists then change the parent folder id to parent id.
                        folder.ParentId =
                            Repository.GetItem<ExportFolder>(x => x.FolderId == Convert.ToInt32(folder.ParentId))?.Id;
                    }

                    Repository.CreateItem(folder, null);
                    totalFolderExported++;
                    Repository.CreateItems(permissions, folder.Id);
                    totalFolderPermissionsExported += permissions.Count;
                    Repository.CreateItems(files, folder.Id);
                    totalFilesExported += files.Count;
                    foreach (var file in files)
                    {
                        var filePath = portal.HomeDirectoryMapPath + folder.FolderPath + GetActualFileName(file);
                        if (File.Exists(filePath))
                        {
                            FileSystemUtils.AddToZip(ref assetsStream, filePath, GetActualFileName(file),
                                folder.FolderPath);
                        }
                    }
                }
                ProgressPercentage += progressStep;
                Result.AddSummary("Exported Folders", totalFolderExported.ToString());
                Result.AddSummary("Exported Folder Permissions", totalFolderPermissionsExported.ToString());
                Result.AddSummary("Exported Files", totalFilesExported.ToString());

                var folderMappings =
                    CBO.FillCollection<ExportFolderMapping>(DataProvider.Instance()
                        .GetFolderMappings(portalId, sinceDate)).ToList();
                Repository.CreateItems(folderMappings, null);

                //Finish and Close Zip file
                assetsStream.Finish();
                assetsStream.Close();
            }
            finally
            {
                if (assetsStream != null && !assetsStream.IsFinished)
                {
                    assetsStream.Finish();
                    assetsStream.Close();
                    assetsStream.Dispose();
                }
            }
        }

        public override void ImportData(ExportImportJob importJob, ExportDto exporteDto)
        {
            var portalId = importJob.PortalId;
            //Sync db and filesystem before exporting so all required files are found
            var folderManager = FolderManager.Instance;
            folderManager.Synchronize(portalId);

            var localFolders = CBO.FillCollection<ExportFolder>(DataProvider.Instance().GetFolders(portalId, null)).ToList();
            var exportedFolders = Repository.GetAllItems<ExportFolder>(x=>x.FolderPath).ToList();

            foreach (var exportFolder in exportedFolders)
            {
                using (var db = DataContext.Instance())
                {
                    /** PROCESS FOLDERS **/
                    //Create new or update existing folder
                    ProcessFolder(importJob, exporteDto, db, exportFolder, localFolders);
                    Repository.UpdateItem(exportFolder);

                    var exportFolderPermissions =
                        Repository.GetRelatedItems<ExportFolderPermission>(exportFolder.Id).ToList();
                    //Replace folderId for each permission with new one.
                    exportFolderPermissions.ForEach(x => { x.FolderId = Convert.ToInt32(exportFolder.LocalId); });

                    /** PROCESS FOLDER PERMISSIONS **/
                    //File local files in the system related to the folder path.
                    var localPermissions =
                        CBO.FillCollection<ExportFolderPermission>(DataProvider.Instance()
                            .GetFolderPermissionsByPath(portalId, exportFolder.FolderPath, null));

                    foreach (var folderPermission in exportFolderPermissions)
                    {
                        ProcessFolderPermission(importJob, exporteDto, db, folderPermission, localPermissions);
                        Repository.UpdateItem(folderPermission);
                    }

                    /** PROCESS FILES **/
                    var exportFiles =
                        Repository.GetRelatedItems<ExportFile>(exportFolder.Id).ToList();
                    //Replace folderId for each file with new one.
                    exportFiles.ForEach(x => { x.FolderId = Convert.ToInt32(exportFolder.LocalId); });

                    //File local files in the system related to the folder
                    var localFiles =
                        CBO.FillCollection<ExportFile>(DataProvider.Instance()
                                .GetFiles(portalId, exportFolder.FolderId, null));

                    foreach (var file in exportFiles)
                    {
                        ProcessFiles(importJob, exporteDto, db, file, localFiles);
                        Repository.UpdateItem(file);
                    }
                }
            }
            folderManager.Synchronize(portalId);
        }

        private void ProcessFolder(ExportImportJob importJob, ExportDto exporteDto, IDataContext db,
            ExportFolder folder,IEnumerable<ExportFolder> localFolders)
        {
            var portalId = importJob.PortalId;

            if (folder == null) return;
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
                    case CollisionResolution.Ignore: //Just ignore the record
                    //TODO: Log that user was ignored.
                    case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                        //TODO: Log that users was ignored as duplicate not possible for users.
                        return;
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
                repExportFolder.Update(existingFolder);

                folder.FolderId = existingFolder.FolderId;
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
                repExportFolder.Insert(folder);
                //Keep the parent Id.
                folder.ParentId = previousParent;
            }
            folder.LocalId = folder.FolderId;
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
                    case CollisionResolution.Ignore: //Just ignore the record
                    //TODO: Log that user was ignored.
                    case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                        //TODO: Log that users was ignored as duplicate not possible for users.
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
                    if (folderPermission.RoleId != null && folderPermission.RoleId>=0)
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
                    case CollisionResolution.Ignore: //Just ignore the record
                    //TODO: Log that user was ignored.
                    case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                        //TODO: Log that users was ignored as duplicate not possible for users.
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                }
            }
            file.PortalId = portalId;
            if (isUpdate)
            {
                file.LastModifiedOnDate = DateTime.UtcNow;
                file.LastModifiedByUserId = modifiedBy;
                file.FileId = existingFile.FileId;
                repExportFile.Update(file);
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
            return (objFile.StorageLocation == (int) FolderController.StorageLocationTypes.SecureFileSystem)
                ? objFile.FileName + Globals.glbProtectedExtension
                : objFile.FileName;
        }
    }
}