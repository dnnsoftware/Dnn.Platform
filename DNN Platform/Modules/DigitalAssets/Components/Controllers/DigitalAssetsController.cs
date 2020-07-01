// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Composition;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;
    using DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint;
    using DotNetNuke.Modules.DigitalAssets.Services.Models;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Assets;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.EventArgs;
    using DotNetNuke.Services.Upgrade;
    using DotNetNuke.Web.UI;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    [Export(typeof(IDigitalAssetsController))]
    [ExportMetadata("Edition", "CE")]
    public class DigitalAssetsController : IDigitalAssetsController, IUpgradeable
    {
        protected static readonly DigitalAssetsSettingsRepository SettingsRepository = new DigitalAssetsSettingsRepository();
        private static readonly Hashtable MappedPathsSupported = new Hashtable();

        private static bool IsHostMenu
        {
            get
            {
                return Globals.IsHostTab(PortalSettings.Current.ActiveTab.TabID);
            }
        }

        public IEnumerable<FolderMappingInfo> GetDefaultFolderProviderValues(int moduleId)
        {
            var portalId = this.GetCurrentPortalId(moduleId);

            return new List<FolderMappingInfo>
                {
                    FolderMappingController.Instance.GetFolderMapping(portalId, "Standard"),
                    FolderMappingController.Instance.GetFolderMapping(portalId, "Secure"),
                    FolderMappingController.Instance.GetFolderMapping(portalId, "Database"),
                };
        }

        public int? GetDefaultFolderTypeId(int moduleId)
        {
            if (PortalSettings.Current.UserInfo.IsSuperUser && SettingsRepository.GetMode(moduleId) == DigitalAssestsMode.User)
            {
                return null;
            }

            var folderTypeId = SettingsRepository.GetDefaultFolderTypeId(moduleId);
            if (!folderTypeId.HasValue)
            {
                folderTypeId = FolderMappingController.Instance.GetDefaultFolderMapping(this.GetCurrentPortalId(moduleId)).FolderMappingID;
            }

            return folderTypeId;
        }

        public int GetCurrentPortalId(int moduleId)
        {
            if (PortalSettings.Current.UserInfo.IsSuperUser)
            {
                if (SettingsRepository.GetMode(moduleId) == DigitalAssestsMode.User)
                {
                    return Null.NullInteger;
                }
            }

            return IsHostMenu ? Null.NullInteger : PortalSettings.Current.PortalId;
        }

        public IEnumerable<FolderMappingViewModel> GetFolderMappings(int moduleId)
        {
            var portalId = this.GetCurrentPortalId(moduleId);
            return FolderMappingController.Instance.GetFolderMappings(portalId).Select(this.GetFolderMappingViewModel);
        }

        public IEnumerable<FolderViewModel> GetFolders(int moduleId, int folderId)
        {
            var subfolderFilter = SettingsRepository.GetSubfolderFilter(moduleId);
            if (subfolderFilter != SubfolderFilter.IncludeSubfoldersFolderStructure)
            {
                return new List<FolderViewModel>();
            }

            var folder = this.GetFolderInfo(folderId);

            if (!FolderPermissionController.CanBrowseFolder((FolderInfo)folder))
            {
                // The user cannot access the content
                return new List<FolderViewModel>();
            }

            return AssetManager.Instance.GetFolders(folder, "FolderName", true).Select(this.GetFolderViewModel);
        }

        public PageViewModel GetFolderContent(int moduleId, int folderId, int startIndex, int numItems, string sortExpression)
        {
            var subfolderFilter = SettingsRepository.GetSubfolderFilter(moduleId);
            var page = AssetManager.Instance.GetFolderContent(folderId, startIndex, numItems, sortExpression, subfolderFilter);
            return new PageViewModel
            {
                Folder = this.GetFolderViewModel(page.Folder),
                Items = page.Items.Select(this.GetItemViewModel).ToList(),
                TotalCount = page.TotalCount,
            };
        }

        public void SyncFolderContent(int folderId, bool recursive)
        {
            var folder = this.GetFolderInfo(folderId);

            if (!FolderPermissionController.CanBrowseFolder((FolderInfo)folder))
            {
                // The user cannot access the content
                return;
            }

            FolderManager.Instance.Synchronize(folder.PortalID, folder.FolderPath, recursive, true);
        }

        public PageViewModel SearchFolderContent(int moduleId, int folderId, string pattern, int startIndex, int numItems, string sortExpression)
        {
            var subfolderFilter = SettingsRepository.GetSubfolderFilter(moduleId);
            var page = AssetManager.Instance.SearchFolderContent(folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter);

            return new PageViewModel
            {
                Folder = this.GetFolderViewModel(page.Folder),
                Items = page.Items.Select(this.GetItemViewModel).ToList(),
                TotalCount = page.TotalCount,
            };
        }

        public FolderViewModel GetFolder(int folderID)
        {
            return this.GetFolderViewModel(this.GetFolderInfo(folderID));
        }

        public FolderViewModel GetRootFolder(int moduleId)
        {
            if (SettingsRepository.GetMode(moduleId) != DigitalAssestsMode.Normal)
            {
                return null;
            }

            var rootFolderId = SettingsRepository.GetRootFolderId(moduleId);
            if (rootFolderId.HasValue && SettingsRepository.GetFilterCondition(moduleId) == FilterCondition.FilterByFolder)
            {
                return this.GetFolder(rootFolderId.Value);
            }

            var portalId = this.GetCurrentPortalId(moduleId);
            return this.GetFolderViewModel(FolderManager.Instance.GetFolder(portalId, string.Empty));
        }

        public FolderViewModel GetGroupFolder(int groupId, PortalSettings portalSettings)
        {
            var role = RoleController.Instance.GetRoleById(portalSettings.PortalId, groupId);
            if (role == null || role.SecurityMode == SecurityMode.SecurityRole)
            {
                return null;
            }

            if (!role.IsPublic && RoleController.Instance.GetUserRole(portalSettings.PortalId, portalSettings.UserId, role.RoleID) == null)
            {
                return null;
            }

            var groupFolder = this.EnsureGroupFolder(groupId, portalSettings);
            var folderViewModel = this.GetFolderViewModel(groupFolder);
            folderViewModel.FolderName = role.RoleName;
            return folderViewModel;
        }

        public FolderViewModel GetUserFolder(UserInfo userInfo)
        {
            var folder = this.GetFolderViewModel(FolderManager.Instance.GetUserFolder(userInfo));
            folder.FolderName = LocalizationHelper.GetString("MyFolder");
            return folder;
        }

        public FolderViewModel CreateFolder(string folderName, int folderParentID, int folderMappingID, string mappedPath)
        {
            return this.GetFolderViewModel(AssetManager.Instance.CreateFolder(folderName, folderParentID, folderMappingID, mappedPath));
        }

        public ItemViewModel GetFile(int fileID)
        {
            return this.GetItemViewModel(FileManager.Instance.GetFile(fileID, true));
        }

        public void UnlinkFolder(int folderID)
        {
            var folder = FolderManager.Instance.GetFolder(folderID);

            // Check if user has appropiate permissions
            if (!this.HasPermission(folder, "DELETE"))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("UserHasNoPermissionToUnlinkFolder.Error"));
            }

            FolderManager.Instance.UnlinkFolder(folder);
        }

        public int GetMappedSubFoldersCount(IEnumerable<ItemBaseViewModel> items, int portalID)
        {
            var totalSubfoldersCount = 0;
            if (items.All(i => !i.IsFolder))
            {
                return totalSubfoldersCount;
            }

            var allFolders = FolderManager.Instance.GetFolders(portalID);
            foreach (var item in items.Where(i => i.IsFolder && this.HasPermission(FolderManager.Instance.GetFolder(i.ItemID), "VIEW")))
            {
                var folder = FolderManager.Instance.GetFolder(item.ItemID);
                var allSubFolders = allFolders.Where(f => f.FolderPath.StartsWith(folder.FolderPath));
                totalSubfoldersCount = totalSubfoldersCount + allSubFolders.Count(f => this.GetUnlinkAllowedStatus(f) == "onlyUnlink");
            }

            return totalSubfoldersCount;
        }

        public IEnumerable<ItemPathViewModel> DeleteItems(IEnumerable<DeleteItem> items)
        {
            var nonDeletedItems = new List<IFolderInfo>();

            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    var onlyUnlink = item.UnlinkAllowedStatus == "onlyUnlink";
                    AssetManager.Instance.DeleteFolder(item.ItemId, onlyUnlink, nonDeletedItems);
                }
                else
                {
                    AssetManager.Instance.DeleteFile(item.ItemId);
                }
            }

            return nonDeletedItems.Select(this.GetItemPathViewModel);
        }

        public ItemViewModel RenameFile(int fileID, string newFileName)
        {
            return this.GetItemViewModel(AssetManager.Instance.RenameFile(fileID, newFileName));
        }

        public FolderViewModel RenameFolder(int folderID, string newFolderName)
        {
            return this.GetFolderViewModel(AssetManager.Instance.RenameFolder(folderID, newFolderName));
        }

        public Stream GetFileContent(int fileId, out string fileName, out string contentType)
        {
            var file = FileManager.Instance.GetFile(fileId, true);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            if (!this.HasPermission(folder, "READ"))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("UserHasNoPermissionToDownload.Error"));
            }

            var content = FileManager.Instance.GetFileContent(file);
            fileName = file.FileName;
            contentType = file.ContentType;

            EventManager.Instance.OnFileDownloaded(new FileDownloadedEventArgs()
            {
                FileInfo = file,
                UserId = UserController.Instance.GetCurrentUserInfo().UserID,
            });
            return content;
        }

        public CopyMoveItemViewModel CopyFile(int fileId, int destinationFolderId, bool overwrite)
        {
            var file = FileManager.Instance.GetFile(fileId, true);
            var folder = FolderManager.Instance.GetFolder(destinationFolderId);
            var sourceFolder = FolderManager.Instance.GetFolder(file.FolderId);

            if (!this.HasPermission(sourceFolder, "COPY"))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("UserHasNoPermissionToCopyFolder.Error"));
            }

            if (file.FolderId == destinationFolderId)
            {
                var destFileName = Path.GetFileNameWithoutExtension(file.FileName) + "-Copy" + Path.GetExtension(file.FileName);
                var i = 1;
                while (FileManager.Instance.FileExists(folder, destFileName, true))
                {
                    destFileName = Path.GetFileNameWithoutExtension(file.FileName) + "-Copy(" + i + ")" + Path.GetExtension(file.FileName);
                    i++;
                }

                var renamedFile = FileManager.Instance.AddFile(folder, destFileName, FileManager.Instance.GetFileContent(file));
                return new CopyMoveItemViewModel { ItemName = renamedFile.FileName, AlreadyExists = false };
            }

            if (!overwrite && FileManager.Instance.FileExists(folder, file.FileName, true))
            {
                return new CopyMoveItemViewModel { ItemName = file.FileName, AlreadyExists = true };
            }

            var copy = FileManager.Instance.CopyFile(file, folder);
            return new CopyMoveItemViewModel { ItemName = copy.FileName, AlreadyExists = false };
        }

        public CopyMoveItemViewModel MoveFile(int fileId, int destinationFolderId, bool overwrite)
        {
            var file = FileManager.Instance.GetFile(fileId, true);
            var folder = FolderManager.Instance.GetFolder(destinationFolderId);
            var sourceFolder = FolderManager.Instance.GetFolder(file.FolderId);
            if (!this.HasPermission(sourceFolder, "COPY"))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("UserHasNoPermissionToMoveFolder.Error"));
            }

            if (file.FolderId == destinationFolderId)
            {
                // User must not move files in the same folder
                throw new DotNetNukeException(LocalizationHelper.GetString("DestinationFolderCannotMatchSourceFolder.Error"));
            }

            if (!overwrite && FileManager.Instance.FileExists(folder, file.FileName, true))
            {
                return new CopyMoveItemViewModel { ItemName = file.FileName, AlreadyExists = true };
            }

            var copy = FileManager.Instance.MoveFile(file, folder);
            return new CopyMoveItemViewModel { ItemName = copy.FileName, AlreadyExists = false };
        }

        public CopyMoveItemViewModel MoveFolder(int folderId, int destinationFolderId, bool overwrite)
        {
            var folder = this.GetFolderInfo(folderId);
            if (!this.HasPermission(folder, "COPY"))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("UserHasNoPermissionToMoveFolder.Error"));
            }

            var destinationFolder = FolderManager.Instance.GetFolder(destinationFolderId);

            FolderManager.Instance.MoveFolder(folder, destinationFolder);
            return new CopyMoveItemViewModel { ItemName = folder.FolderName, AlreadyExists = false };
        }

        public string GetUrl(int fileId)
        {
            var file = FileManager.Instance.GetFile(fileId, true);
            if (file != null)
            {
                var folder = FolderManager.Instance.GetFolder(file.FolderId);
                if (!this.HasPermission(folder, "READ"))
                {
                    throw new DotNetNukeException(LocalizationHelper.GetString("UserHasNoPermissionToDownload.Error"));
                }
            }

            return FileManager.Instance.GetUrl(file);
        }

        public string GetInvalidChars()
        {
            var invalidChars = new string(Path.GetInvalidFileNameChars());

            foreach (var ch in Path.GetInvalidPathChars())
            {
                if (invalidChars.IndexOf(ch) == -1) // The ch does not exists
                {
                    invalidChars += ch;
                }
            }

            return invalidChars;
        }

        public string GetInvalidCharsErrorText()
        {
            return string.Format(LocalizationHelper.GetString("FolderFileNameHasInvalidcharacters.Error"), "\\:/*?\"<>|");
        }

        public virtual PreviewInfoViewModel GetFolderPreviewInfo(IFolderInfo folder)
        {
            return new PreviewInfoViewModel
            {
                Title = LocalizationHelper.GetString("PreviewPanelTitle.Text"),
                ItemId = folder.FolderID,
                IsFolder = true,
                PreviewImageUrl = this.GetFolderIconUrl(folder.PortalID, folder.FolderMappingID),
                Fields = this.GetFolderPreviewFields(folder),
            };
        }

        public virtual PreviewInfoViewModel GetFilePreviewInfo(IFileInfo file, ItemViewModel item)
        {
            var result = new PreviewInfoViewModel
            {
                Title = LocalizationHelper.GetString("PreviewPanelTitle.Text"),
                ItemId = file.FileId,
                IsFolder = false,
                PreviewImageUrl = item.IconUrl,
                Fields = this.GetFilePreviewFields(file),
            };
            return result;
        }

        public ZipExtractViewModel UnzipFile(int fileId, bool overwrite)
        {
            var file = FileManager.Instance.GetFile(fileId, true);
            var destinationFolder = FolderManager.Instance.GetFolder(file.FolderId);
            var invalidFiles = new List<string>();
            var filesCount = FileManager.Instance.UnzipFile(file, destinationFolder, invalidFiles);
            return new ZipExtractViewModel() { Ok = true, InvalidFiles = invalidFiles, TotalCount = filesCount };
        }

        public virtual int GetInitialTab(NameValueCollection requestParams, NameValueCollection damState)
        {
            return 0; // Always
        }

        public bool HasPermission(IFolderInfo folder, string permissionKey)
        {
            var hasPermision = PortalSettings.Current.UserInfo.IsSuperUser;

            if (!hasPermision && folder != null)
            {
                hasPermision = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

            return hasPermision;
        }

        public string UpgradeModule(string version)
        {
            try
            {
                switch (version)
                {
                    case "07.01.00":
                        ModuleDefinitionInfo mDef = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Digital Asset Management");

                        // Add tab to Admin Menu
                        if (mDef != null)
                        {
                            var hostPage = Upgrade.AddHostPage(
                                "File Management",
                                "Manage assets.",
                                "~/Icons/Sigma/Files_16X16_Standard.png",
                                "~/Icons/Sigma/Files_32X32_Standard.png",
                                true);

                            // Add module to page
                            Upgrade.AddModuleToPage(hostPage, mDef.ModuleDefID, "File Management", "~/Icons/Sigma/Files_32X32_Standard.png", true);

                            Upgrade.AddAdminPages(
                                "File Management",
                                "Manage assets within the portal",
                                "~/Icons/Sigma/Files_16X16_Standard.png",
                                "~/Icons/Sigma/Files_32X32_Standard.png",
                                true,
                                mDef.ModuleDefID,
                                "File Management",
                                "~/Icons/Sigma/Files_16X16_Standard.png",
                                true);
                        }

                        // Remove Host File Manager page
                        Upgrade.RemoveHostPage("File Manager");

                        // Remove Admin File Manager Pages
                        Upgrade.RemoveAdminPages("//Admin//FileManager");

                        break;
                }

                return "Success";
            }
            catch (Exception)
            {
                return "Failed";
            }
        }

        protected ItemPathViewModel GetItemPathViewModel(IFolderInfo folder)
        {
            return new ItemPathViewModel
            {
                IsFolder = true,
                ItemID = folder.FolderID,
                DisplayPath = folder.DisplayPath,
                IconUrl = this.GetFolderIconUrl(folder.PortalID, folder.FolderMappingID),
            };
        }

        protected ItemPathViewModel GetItemPathViewModel(IFileInfo file)
        {
            return new ItemPathViewModel
            {
                IsFolder = false,
                ItemID = file.FileId,
                DisplayPath = file.RelativePath,
                IconUrl = GetFileIconUrl(file.Extension),
            };
        }

        protected virtual FolderViewModel GetFolderViewModel(IFolderInfo folder)
        {
            var folderName = string.IsNullOrEmpty(folder.FolderName)
                ? LocalizationHelper.GetString("RootFolder.Text")
                : folder.FolderName;

            var folderViewModel = new FolderViewModel
            {
                FolderID = folder.FolderID,
                FolderMappingID = folder.FolderMappingID,
                FolderName = folderName,
                FolderPath = folder.FolderPath,
                PortalID = folder.PortalID,
                LastModifiedOnDate = folder.LastModifiedOnDate.ToString("g"),
                IconUrl = this.GetFolderIconUrl(folder.PortalID, folder.FolderMappingID),
                Permissions = this.GetPermissionViewModelCollection(folder),
                HasChildren = folder.HasChildren,
            };
            folderViewModel.Attributes.Add(new KeyValuePair<string, object>("UnlinkAllowedStatus", this.GetUnlinkAllowedStatus(folder)));
            return folderViewModel;
        }

        protected virtual ItemViewModel GetItemViewModel(IFolderInfo folder)
        {
            var parentFolderId = Null.NullInteger;
            var parentFolderPath = string.Empty;

            var parentFolder = FolderManager.Instance.GetFolder(folder.ParentID);
            if (parentFolder != null)
            {
                parentFolderId = parentFolder.FolderID;
                parentFolderPath = parentFolder.FolderPath;
            }

            return new ItemViewModel
            {
                IsFolder = true,
                ItemID = folder.FolderID,
                ItemName = folder.FolderName,
                LastModifiedOnDate = folder.LastModifiedOnDate.ToString("g"),
                PortalID = folder.PortalID,
                IconUrl = this.GetFolderIconUrl(folder.PortalID, folder.FolderMappingID),
                Permissions = this.GetPermissionViewModelCollection(folder),
                ParentFolderID = parentFolderId,
                ParentFolder = parentFolderPath,
                FolderMappingID = folder.FolderMappingID,
                UnlinkAllowedStatus = this.GetUnlinkAllowedStatus(folder),
            };
        }

        protected virtual ItemViewModel GetItemViewModel(IFileInfo file)
        {
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            return new ItemViewModel
            {
                IsFolder = false,
                ItemID = file.FileId,
                ItemName = file.FileName,
                LastModifiedOnDate = file.LastModifiedOnDate.ToString("g"),
                PortalID = file.PortalId,
                IconUrl = GetFileIconUrl(file.Extension),
                Permissions = this.GetPermissionViewModelCollection(folder),
                ParentFolderID = folder.FolderID,
                ParentFolder = folder.FolderPath,
                Size = string.Format(new FileSizeFormatProvider(), "{0:fs}", file.Size),
                UnlinkAllowedStatus = "false",
            };
        }

        protected string GetFolderIconUrl(int portalId, int folderMappingID)
        {
            var imageUrl = IconController.IconURL("ExtClosedFolder", "32x32", "Standard");

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, folderMappingID);
            if (folderMapping != null && File.Exists(HttpContext.Current.Server.MapPath(folderMapping.ImageUrl)))
            {
                imageUrl = folderMapping.ImageUrl;
            }

            return imageUrl;
        }

        private static string GetFileIconUrl(string extension)
        {
            if (!string.IsNullOrEmpty(extension) && File.Exists(HttpContext.Current.Server.MapPath(IconController.IconURL("Ext" + extension, "32x32", "Standard"))))
            {
                return IconController.IconURL("Ext" + extension, "32x32", "Standard");
            }

            return IconController.IconURL("ExtFile", "32x32", "Standard");
        }

        private IFolderInfo GetFolderInfo(int folderId)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);
            if (folder == null)
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("FolderDoesNotExists.Error"));
            }

            return folder;
        }

        private IEnumerable<PermissionViewModel> GetPermissionViewModelCollection(IFolderInfo folder)
        {
            // TODO Split permission between CE and PE packages
            string[] permissionKeys = { "ADD", "BROWSE", "COPY", "READ", "WRITE", "DELETE", "MANAGE", "VIEW", "FULLCONTROL" };

            return permissionKeys.Select(permissionKey => new PermissionViewModel { Key = permissionKey, Value = this.HasPermission(folder, permissionKey) }).ToList();
        }

        private FolderMappingViewModel GetFolderMappingViewModel(FolderMappingInfo folderMapping)
        {
            return new FolderMappingViewModel
            {
                Id = folderMapping.FolderMappingID,
                FolderTypeName = folderMapping.FolderProviderType,
                Name = folderMapping.MappingName,
            };
        }

        private Field GetTotalFilesField(IFolderInfo folder)
        {
            var field = new Field(DefaultMetadataNames.TotalFiles);
            field.DisplayName = LocalizationHelper.GetString("Field" + field.Name + ".DisplayName");
            var totalFiles = Convert.ToInt32(FolderManager.Instance.GetFiles(folder, true, false).Count());
            field.Type = totalFiles.GetType();
            field.Value = totalFiles;
            field.StringValue = field.Value.ToString();
            return field;
        }

        private Field GetFolderSizeField(IFolderInfo folder)
        {
            var field = new Field(DefaultMetadataNames.Size);
            var size = FolderManager.Instance.GetFiles(folder, true, false).Sum(f => (long)f.Size);
            field.DisplayName = LocalizationHelper.GetString("Field" + field.Name + ".DisplayName");
            field.Type = size.GetType();
            field.Value = size;
            field.StringValue = string.Format(new FileSizeFormatProvider(), "{0:fs}", size);

            return field;
        }

        private Field GetFileKindField(IFileInfo file)
        {
            var field = new Field(DefaultMetadataNames.Type);
            field.DisplayName = LocalizationHelper.GetString("Field" + field.Name + ".DisplayName");
            field.Type = file.Extension.GetType();
            field.Value = file.Extension;
            field.StringValue = field.Value.ToString();

            return field;
        }

        private Field GetFileSizeField(IFileInfo file)
        {
            var field = new Field(DefaultMetadataNames.Size);
            field.DisplayName = LocalizationHelper.GetString("Field" + field.Name + ".DisplayName");
            field.Type = file.Size.GetType();
            field.Value = file.Size;
            field.StringValue = string.Format(new FileSizeFormatProvider(), "{0:fs}", file.Size);

            return field;
        }

        private List<Field> GetFolderPreviewFields(IFolderInfo folder)
        {
            var fields = new List<Field>
                             {
                                 this.GetFolderSizeField(folder),
                                 this.GetTotalFilesField(folder),
                             };
            fields.AddRange(this.GetAuditFields((FolderInfo)folder, folder.PortalID));
            return fields;
        }

        private List<Field> GetFilePreviewFields(IFileInfo file)
        {
            var fields = new List<Field>
                             {
                                 this.GetFileKindField(file),
                                 this.GetFileSizeField(file),
                             };
            fields.AddRange(this.GetAuditFields((FileInfo)file, file.PortalId));
            return fields;
        }

        private IEnumerable<Field> GetAuditFields(BaseEntityInfo item, int portalId)
        {
            var createdByUser = item.CreatedByUser(portalId);
            var lastModifiedByUser = item.LastModifiedByUser(portalId);
            return new List<Field>
                {
                    new Field(DefaultMetadataNames.Created)
                    {
                        DisplayName = LocalizationHelper.GetString("Field" + DefaultMetadataNames.Created + ".DisplayName"),
                        Type = typeof(DateTime),
                        Value = item.CreatedOnDate,
                        StringValue = item.CreatedOnDate.ToString(CultureInfo.CurrentCulture),
                    },
                    new Field(DefaultMetadataNames.CreatedBy)
                    {
                        DisplayName = LocalizationHelper.GetString("Field" + DefaultMetadataNames.CreatedBy + ".DisplayName"),
                        Type = typeof(int),
                        Value = item.CreatedByUserID,
                        StringValue = createdByUser != null ? createdByUser.DisplayName : string.Empty,
                    },
                    new Field(DefaultMetadataNames.Modified)
                    {
                        DisplayName = LocalizationHelper.GetString("Field" + DefaultMetadataNames.Modified + ".DisplayName"),
                        Type = typeof(DateTime),
                        Value = item.LastModifiedOnDate,
                        StringValue = item.LastModifiedOnDate.ToString(CultureInfo.CurrentCulture),
                    },
                    new Field(DefaultMetadataNames.ModifiedBy)
                    {
                        DisplayName = LocalizationHelper.GetString("Field" + DefaultMetadataNames.ModifiedBy + ".DisplayName"),
                        Type = typeof(int),
                        Value = item.LastModifiedByUserID,
                        StringValue = lastModifiedByUser != null ? lastModifiedByUser.DisplayName : string.Empty
                    },
                };
        }

        private bool AreMappedPathsSupported(int folderMappingId)
        {
            if (MappedPathsSupported.ContainsKey(folderMappingId))
            {
                return (bool)MappedPathsSupported[folderMappingId];
            }

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folderMappingId);
            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
            var result = folderProvider.SupportsMappedPaths;
            MappedPathsSupported[folderMappingId] = result;
            return result;
        }

        private string GetUnlinkAllowedStatus(IFolderInfo folder)
        {
            if (this.AreMappedPathsSupported(folder.FolderMappingID) && folder.ParentID > 0 && this.GetFolder(folder.ParentID).FolderMappingID != folder.FolderMappingID)
            {
                return "onlyUnlink";
            }

            if (this.AreMappedPathsSupported(folder.FolderMappingID))
            {
                return "true";
            }

            return "false";
        }

        private IFolderInfo EnsureGroupFolder(int groupId, PortalSettings portalSettings)
        {
            const int AllUsersRoleId = -1;
            var groupFolderPath = "Groups/" + groupId;

            if (!FolderManager.Instance.FolderExists(portalSettings.PortalId, groupFolderPath))
            {
                var pc = new PermissionController();
                var browsePermission = pc.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "BROWSE").Cast<PermissionInfo>().FirstOrDefault();
                var readPermission = pc.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "READ").Cast<PermissionInfo>().FirstOrDefault();
                var writePermission = pc.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "WRITE").Cast<PermissionInfo>().FirstOrDefault();

                if (!FolderManager.Instance.FolderExists(portalSettings.PortalId, "Groups"))
                {
                    var folder = FolderManager.Instance.AddFolder(portalSettings.PortalId, "Groups");

                    folder.FolderPermissions.Remove(browsePermission.PermissionID, AllUsersRoleId, Null.NullInteger);
                    folder.FolderPermissions.Remove(readPermission.PermissionID, AllUsersRoleId, Null.NullInteger);
                    folder.IsProtected = true;
                    FolderManager.Instance.UpdateFolder(folder);
                }

                var groupFolder = FolderManager.Instance.AddFolder(portalSettings.PortalId, groupFolderPath);

                groupFolder.FolderPermissions.Add(new FolderPermissionInfo(browsePermission) { FolderPath = groupFolder.FolderPath, RoleID = groupId, AllowAccess = true });
                groupFolder.FolderPermissions.Add(new FolderPermissionInfo(readPermission) { FolderPath = groupFolder.FolderPath, RoleID = groupId, AllowAccess = true });
                groupFolder.FolderPermissions.Add(new FolderPermissionInfo(writePermission) { FolderPath = groupFolder.FolderPath, RoleID = groupId, AllowAccess = true });

                groupFolder.IsProtected = true;
                FolderManager.Instance.UpdateFolder(groupFolder);
                return groupFolder;
            }

            return FolderManager.Instance.GetFolder(portalSettings.PortalId, groupFolderPath);
        }

        private ItemViewModel GetItemViewModel(object item)
        {
            var folder = item as IFolderInfo;
            if (folder != null)
            {
                return this.GetItemViewModel(folder);
            }

            var file = item as IFileInfo;
            return this.GetItemViewModel(file);
        }
    }
}
