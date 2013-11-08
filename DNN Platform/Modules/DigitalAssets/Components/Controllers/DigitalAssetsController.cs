#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;
using DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Upgrade;
using DotNetNuke.Web.UI;

using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers
{
    [Export(typeof(IDigitalAssetsController))]
    [ExportMetadata("Edition", "CE")]
    public class DigitalAssetsController : IDigitalAssetsController, IUpgradeable
    {
        #region Static Private Methods
        private static bool IsHostMenu
        {
            get
            {
                return Globals.IsHostTab(PortalSettings.Current.ActiveTab.TabID);
            }
        }

        protected static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string propertyName, bool asc)
        {
            var methodName = asc ? "OrderBy" : "OrderByDescending";
            var arg = Expression.Parameter(typeof(T), "x");

            // Use reflection to mirror LINQ
            var property = typeof(T).GetProperty(propertyName);

            // If property is undefined returns the original source
            if (property == null) return (IOrderedQueryable<T>)source;

            Expression expr = Expression.Property(arg, property);

            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), property.PropertyType);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            var result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), property.PropertyType)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }

        private static string GetFileIconUrl(string extension)
        {
            if (!string.IsNullOrEmpty(extension) && File.Exists(HttpContext.Current.Server.MapPath(IconController.IconURL("Ext" + extension, "32x32", "Standard"))))
            {
                return IconController.IconURL("Ext" + extension, "32x32", "Standard");
            }

            return IconController.IconURL("ExtFile", "32x32", "Standard");
        }

        private static string CleanDotsAtTheEndOfTheName(string name)
        {
            return name.Trim().TrimEnd(new[] { '.', ' ' });
        }

        #endregion

        #region Private Methods
        private IFolderInfo GetFolderInfo(int folderId)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);
            if (folder == null)
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("FolderDoesNotExists.Error"));
            }
            return folder;
        }

        private IEnumerable<IFolderInfo> GetFolders(IFolderInfo parentFolder, string orderingField, bool asc)
        {
            Requires.NotNull("parentFolder", parentFolder);

            var folders = FolderManager.Instance.GetFolders(parentFolder).Where(f => HasPermission(f, "BROWSE") || HasPermission(f, "READ"));

            // Set default sorting values
            var field = string.IsNullOrEmpty(orderingField) ? "FolderName" : orderingField;

            return ApplyOrder(folders.AsQueryable(), field, asc);
        } 

        private IEnumerable<IFileInfo> GetFiles(IFolderInfo folder, string orderingField, bool asc)
        {
            Requires.NotNull("folder", folder);

            if (Host.EnableFileAutoSync)
            {
                FolderManager.Instance.Synchronize(folder.PortalID, folder.FolderPath, false, true);
            }

            // Set default sorting values
            var field = string.IsNullOrEmpty(orderingField) ? "FileName" : orderingField;

            return ApplyOrder(FolderManager.Instance.GetFiles(folder, false, true).AsQueryable(), field, asc);            
        }

        /// <summary>
        /// This method deletes a folder and his content (sub folder and files) in a recursive way.
        /// </summary>
        /// <param name="folder">Folder to delete</param>
        /// <param name="notDeletedItems">The not deleted items list. The subfiles / subfolders for which the user has no permissions to delete</param>
        /// <retur>True if the Folder has been deleted, otherwise returns false</retur>
        private bool DeleteFolder(IFolderInfo folder, ICollection<ItemPathViewModel> notDeletedItems)
        {
            var notDeletedSubfolders = new List<IFolderInfo>();
            FolderManager.Instance.DeleteFolder(folder, notDeletedSubfolders);
            if (!notDeletedSubfolders.Any())
            {
                return false;
            }
            
            foreach (var notDeletedSubfolder in notDeletedSubfolders)
            {
                notDeletedItems.Add(GetItemPathViewModel(notDeletedSubfolder));
            }
            return true;
        }

        private IEnumerable<PermissionViewModel> GetPermissionViewModelCollection(IFolderInfo folder)
        {
            // TODO Split permission between CE and PE packages
            string[] permissionKeys = { "ADD", "BROWSE", "COPY", "READ", "WRITE", "DELETE", "MANAGE", "VIEW", "FULLCONTROL" };

            return permissionKeys.Select(permissionKey => new PermissionViewModel { Key = permissionKey, Value = HasPermission(folder, permissionKey) }).ToList();
        }

        private string GetNewFolderPath(string newFolderName, IFolderInfo folder)
        {
            if (folder.FolderName.ToLowerInvariant() == newFolderName.ToLowerInvariant())
            {
                return folder.FolderPath;
            }

            var oldFolderPath = folder.FolderPath;
            if (oldFolderPath.Length > 0)
            {
                oldFolderPath = oldFolderPath.Substring(0, oldFolderPath.LastIndexOf(folder.FolderName, StringComparison.Ordinal));
            }

            return PathUtils.Instance.FormatFolderPath(oldFolderPath + newFolderName);
        }

        private FolderMappingViewModel GetFolderMappingViewModel(FolderMappingInfo folderMapping)
        {
            return new FolderMappingViewModel
            {
                Id = folderMapping.FolderMappingID,
                FolderTypeName = folderMapping.FolderProviderType,
                Name = folderMapping.MappingName
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

        private bool IsInvalidName(string itemName)
        {
            var invalidFilenameChars = new Regex("[" + Regex.Escape(GetInvalidChars()) + "]");

            return invalidFilenameChars.IsMatch(itemName);
        }

        private bool IsReservedName(string name)
        {
            var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "CLOCK$" };
            return reservedNames.Contains(Path.GetFileNameWithoutExtension(name.ToUpperInvariant()));
        }

        private List<Field> GetFolderPreviewFields(IFolderInfo folder)
        {
            var fields = new List<Field>
                             {                                 
                                 GetFolderSizeField(folder), 
                                 GetTotalFilesField(folder)
                             };
            fields.AddRange(GetAuditFields((FolderInfo)folder));
            return fields;
        }

        private List<Field> GetFilePreviewFields(IFileInfo file)
        {
            var fields = new List<Field>
                             {
                                 GetFileKindField(file),
                                 GetFileSizeField(file),
                             };
            fields.AddRange(GetAuditFields((FileInfo)file));
            return fields;
        }

        private IEnumerable<Field> GetAuditFields(BaseEntityInfo item)
        {
            var createdByUser = item.CreatedByUser(CurrentPortalId);
            var lastModifiedByUser = item.LastModifiedByUser(CurrentPortalId);
            return new List<Field>
                {                    
                    new Field(DefaultMetadataNames.Created)
                    {
                        DisplayName = LocalizationHelper.GetString("Field" + DefaultMetadataNames.Created + ".DisplayName"), 
                        Type = typeof(DateTime), 
                        Value = item.CreatedOnDate,
                        StringValue = item.CreatedOnDate.ToString(CultureInfo.CurrentCulture)
                    },
                new Field(DefaultMetadataNames.CreatedBy)
                    {
                        DisplayName = LocalizationHelper.GetString("Field" + DefaultMetadataNames.CreatedBy + ".DisplayName"), 
                        Type = typeof(int), 
                        Value = item.CreatedByUserID,
                        StringValue = createdByUser != null ? createdByUser.DisplayName : ""
                    },
                new Field(DefaultMetadataNames.Modified)
                    {
                        DisplayName = LocalizationHelper.GetString("Field" + DefaultMetadataNames.Modified + ".DisplayName"), 
                        Type = typeof(DateTime), 
                        Value = item.LastModifiedOnDate,
                        StringValue = item.LastModifiedOnDate.ToString(CultureInfo.CurrentCulture)
                    },
                new Field(DefaultMetadataNames.ModifiedBy)
                    {
                        DisplayName = LocalizationHelper.GetString("Field" + DefaultMetadataNames.ModifiedBy + ".DisplayName"), 
                        Type = typeof(int), 
                        Value = item.LastModifiedByUserID,
                        StringValue = lastModifiedByUser != null ? lastModifiedByUser.DisplayName : ""
                    }
                };
        }

        private string ReplaceFolderName(string path, string folderName, string newFolderName)
        {
            string newPath = PathUtils.Instance.RemoveTrailingSlash(path);
            if (string.IsNullOrEmpty(newPath))
            {
                return path;
            }
            var nameIndex = newPath.LastIndexOf(folderName, StringComparison.Ordinal);
            if (nameIndex == -1)
            {
                return path;
            }

            var result = newPath.Substring(0, nameIndex) + newPath.Substring(nameIndex).Replace(folderName, newFolderName);
            return result;
        }
        #endregion

        #region Public Properties
        public int CurrentPortalId
        {
            get
            {
                return IsHostMenu ? Null.NullInteger : PortalSettings.Current.PortalId;
            }
        }
        #endregion

        #region Public Methods
        public IEnumerable<FolderMappingViewModel> GetFolderMappings()
        {
            return FolderMappingController.Instance.GetFolderMappings(CurrentPortalId).Select(GetFolderMappingViewModel);
        }

        public IEnumerable<FolderViewModel> GetFolders(int folderId)
        {
            var folder = GetFolderInfo(folderId);

            if (!(HasPermission(folder, "BROWSE") || HasPermission(folder, "READ")))
            {
                //The user cannot access the content
                return new List<FolderViewModel>();
            }
            return GetFolders(folder, "FolderName", true).Select(GetFolderViewModel);
        }

        public PageViewModel GetFolderContent(int folderId, int startIndex, int numItems, string sortExpression)
        {
            var folder = GetFolderInfo(folderId);

            if (!(HasPermission(folder, "BROWSE") || HasPermission(folder, "READ")))
            {
                //The user cannot access the content               
                return new PageViewModel
                {
                    Folder = GetFolderViewModel(folder),
                    Items = new List<ItemViewModel>(),
                    TotalCount = 0
                };
            }

            var sortProperties = SortProperties.Parse(sortExpression);

            var folders = GetFolders(folder, sortProperties.Column == "ItemName" ? "FolderName" : sortProperties.Column, sortProperties.Ascending).ToList();
            var files = GetFiles(folder, sortProperties.Column == "ItemName" ? "FileName" : sortProperties.Column, sortProperties.Ascending).ToList();

            IEnumerable<ItemViewModel> content;
            if (startIndex + numItems <= folders.Count())
            {
                content = folders.Skip(startIndex).Take(numItems).Select(GetItemViewModel);
            } 
            else if (startIndex >= folders.Count())
            {
                content = files.Skip(startIndex - folders.Count).Take(numItems).Select(GetItemViewModel);
            }
            else
            {
                var numFiles = numItems - (folders.Count - startIndex);
                content = folders.Skip(startIndex).Select(GetItemViewModel).Union(files.Take(numFiles).Select(GetItemViewModel));
            }

            return new PageViewModel
                {
                    Folder = GetFolderViewModel(folder),
                    Items = content.ToList(),
                    TotalCount = folders.Count() + files.Count()
                };
        }

        public void SyncFolderContent(int folderId, bool recursive)
        {
            var folder = GetFolderInfo(folderId);

            if (!(HasPermission(folder, "BROWSE") || HasPermission(folder, "READ")))
            {
                //The user cannot access the content               
                return;
            }

            FolderManager.Instance.Synchronize(folder.PortalID, folder.FolderPath, recursive, true);
        }

        public PageViewModel SearchFolderContent(int folderId, string pattern, int startIndex, int numItems, string sortExpression)
        {
            var folder = GetFolderInfo(folderId);

            var results = FolderManager.Instance.SearchFiles(folder, pattern, true).Select(GetItemViewModel);

            var sortProperties = SortProperties.Parse(sortExpression);
            results = ApplyOrder(results.AsQueryable(), sortProperties.Column, sortProperties.Ascending);

            return new PageViewModel
                {
                    Folder = GetFolderViewModel(folder),
                    Items = results.Skip(startIndex).Take(numItems).ToList(),
                    TotalCount = results.Count()
                };
        }

        public FolderViewModel GetFolder(int folderID)
        {
            return GetFolderViewModel(GetFolderInfo(folderID));
        }

        public FolderViewModel GetRootFolder()
        {
            return GetFolderViewModel(FolderManager.Instance.GetFolder(CurrentPortalId, ""));
        }

        public FolderViewModel GetGroupFolder(int groupId, PortalSettings portalSettings)
        {
            var roleController = new RoleController();
            var role = roleController.GetRole(groupId, this.CurrentPortalId);
            if (role == null || role.SecurityMode != SecurityMode.SocialGroup)
            {
                return null;
            }

            if (!role.IsPublic && roleController.GetUserRole(portalSettings.PortalId, portalSettings.UserId, role.RoleID) == null)
            {
                return null;
            }

            var groupFolder = EnsureGroupFolder(groupId, portalSettings);
            var folderViewModel = this.GetFolderViewModel(groupFolder);
            folderViewModel.FolderName = role.RoleName;
            return folderViewModel;
        }

        private IFolderInfo EnsureGroupFolder(int groupId, PortalSettings portalSettings)
        {
            const int AllUsersRoleId = -1;
            var groupFolderPath = "Groups/" + groupId;

            if (!FolderManager.Instance.FolderExists(this.CurrentPortalId, groupFolderPath))
            {
                var pc = new PermissionController();
                var browsePermission = pc.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "BROWSE").Cast<PermissionInfo>().FirstOrDefault();
                var readPermission = pc.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "READ").Cast<PermissionInfo>().FirstOrDefault(); 
                var writePermission = pc.GetPermissionByCodeAndKey("SYSTEM_FOLDER", "WRITE").Cast<PermissionInfo>().FirstOrDefault(); 

                if (!FolderManager.Instance.FolderExists(this.CurrentPortalId, "Groups"))
                {
                    var folder = FolderManager.Instance.AddFolder(this.CurrentPortalId, "Groups");

                    folder.FolderPermissions.Remove(browsePermission.PermissionID, AllUsersRoleId, Null.NullInteger);
                    folder.FolderPermissions.Remove(readPermission.PermissionID, AllUsersRoleId, Null.NullInteger);
                    folder.IsProtected = true;
                    FolderManager.Instance.UpdateFolder(folder);
                }

                var groupFolder = FolderManager.Instance.AddFolder(this.CurrentPortalId, groupFolderPath);

                groupFolder.FolderPermissions.Add(new FolderPermissionInfo(browsePermission) { FolderPath = groupFolder.FolderPath, RoleID = groupId, AllowAccess = true });
                groupFolder.FolderPermissions.Add(new FolderPermissionInfo(readPermission) { FolderPath = groupFolder.FolderPath, RoleID = groupId, AllowAccess = true });
                groupFolder.FolderPermissions.Add(new FolderPermissionInfo(writePermission) { FolderPath = groupFolder.FolderPath, RoleID = groupId, AllowAccess = true });

                groupFolder.IsProtected = true;
                FolderManager.Instance.UpdateFolder(groupFolder);
                return groupFolder;
            }
            
            return FolderManager.Instance.GetFolder(this.CurrentPortalId, groupFolderPath);        
        }

        public FolderViewModel CreateFolder(string folderName, int folderParentID, int folderMappingID, string mappedPath)
        {
            Requires.NotNullOrEmpty("folderName", folderName);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(CurrentPortalId, folderMappingID);

            var filterFolderName = CleanDotsAtTheEndOfTheName(folderName);

            if (IsInvalidName(filterFolderName))
            {
                throw new DotNetNukeException(GetInvalidCharsErrorText());
            }

            // Check if the new name is a reserved name
            if (IsReservedName(filterFolderName))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("FolderFileNameIsReserved.Error"));
            }

            var parentFolder = GetFolderInfo(folderParentID);

            if (!HasPermission(parentFolder, "ADD"))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("UserHasNoPermissionToAdd.Error"));
            }

            var folderPath = PathUtils.Instance.FormatFolderPath(
                PathUtils.Instance.FormatFolderPath(
                PathUtils.Instance.StripFolderPath(parentFolder.FolderPath).Replace("\\", "/")) + filterFolderName);

            mappedPath = PathUtils.Instance.FormatFolderPath(mappedPath);

            if (!Regex.IsMatch(mappedPath, @"^(?!\s*[\\/]).*$"))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("InvalidMappedPath.Error"));
            }

            try
            {
                var folder = FolderManager.Instance.AddFolder(folderMapping, folderPath, mappedPath);
                return GetFolderViewModel(folder);
            }
            catch (FolderAlreadyExistsException)
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("FolderAlreadyExists.Error"));
            }
        }

        public ItemViewModel GetFile(int fileID)
        {
            return GetItemViewModel(FileManager.Instance.GetFile(fileID, true));
        }

        public IEnumerable<ItemPathViewModel> DeleteItems(IEnumerable<ItemBaseViewModel> items)
        {
            var notDeletedItems = new List<ItemPathViewModel>();

            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    var folder = FolderManager.Instance.GetFolder(item.ItemID);
                    if (folder == null) continue;

                    if (!HasPermission(folder, "DELETE"))
                    {
                        notDeletedItems.Add(GetItemPathViewModel(folder));
                    }
                    else
                    {
                        DeleteFolder(folder, notDeletedItems);
                    }
                }
                else
                {
                    var fileInfo = FileManager.Instance.GetFile(item.ItemID, true);
                    if (fileInfo == null) continue;

                    var folder = FolderManager.Instance.GetFolder(fileInfo.FolderId);

                    if (!HasPermission(folder, "DELETE"))
                    {
                        notDeletedItems.Add(GetItemPathViewModel(fileInfo));
                    }
                    else
                    {
                        FileManager.Instance.DeleteFile(fileInfo);
                    }
                }
            }

            return notDeletedItems;
        }

        public ItemViewModel RenameFile(int fileID, string newFileName)
        {
            Requires.NotNullOrEmpty("newFileName", newFileName);

            var filteredName = CleanDotsAtTheEndOfTheName(newFileName);

            if (string.IsNullOrEmpty(filteredName))
            {
                throw new DotNetNukeException(string.Format(LocalizationHelper.GetString("FolderFileNameHasInvalidcharacters.Error"), newFileName));
            }

            // Chech if the new name has invalid chars
            if (IsInvalidName(filteredName))
            {
                throw new DotNetNukeException(GetInvalidCharsErrorText());
            }

            // Check if the new name is a reserved name
            if (IsReservedName(filteredName))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("FolderFileNameIsReserved.Error"));
            }

            var file = FileManager.Instance.GetFile(fileID, true);

            // Check if the name has not changed
            if (file.FileName == newFileName)
            {
                return GetItemViewModel(file);
            }

            // Check if user has appropiate permissions
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            if (!HasPermission(folder, "MANAGE"))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("UserHasNoPermissionToEditFile.Error"));
            }

            var renamedFile = FileManager.Instance.RenameFile(file, newFileName);

            return GetItemViewModel(renamedFile);
        }

        public FolderViewModel RenameFolder(int folderID, string newFolderName)
        {
            Requires.NotNullOrEmpty("newFolderName", newFolderName);

            newFolderName = CleanDotsAtTheEndOfTheName(newFolderName);

            // Check if the new name has invalid chars
            if (IsInvalidName(newFolderName))
            {
                throw new DotNetNukeException(GetInvalidCharsErrorText());
            }

            // Check if the name is reserved
            if (IsReservedName(newFolderName))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("FolderFileNameIsReserved.Error"));
            }

            var folder = GetFolderInfo(folderID);

            // Check if user has appropiate permissions
            if (!HasPermission(folder, "MANAGE"))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("UserHasNoPermissionToEditFolder.Error"));
            }

            // check if the name has not changed
            if (folder.FolderName == newFolderName)
            {
                return GetFolderViewModel(folder);
            }
            if (folder.FolderName.ToLowerInvariant() == newFolderName.ToLowerInvariant())
            {
                folder.FolderPath = ReplaceFolderName(folder.FolderPath, folder.FolderName, newFolderName);
                return GetFolderViewModel(FolderManager.Instance.UpdateFolder(folder));
            }

            var newFolderPath = GetNewFolderPath(newFolderName, folder);
            // Check if the new folder already exists
            if (FolderManager.Instance.FolderExists(CurrentPortalId, newFolderPath))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("FolderAlreadyExists.Error"));
            }

            FolderManager.Instance.RenameFolder(folder, newFolderName);
            return GetFolderViewModel(folder);
        }

        public Stream GetFileContent(int fileId, out string fileName, out string contentType)
        {
            var file = FileManager.Instance.GetFile(fileId, true);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            if (!HasPermission(folder, "READ"))
            {
                throw new DotNetNukeException(LocalizationHelper.GetString("UserHasNoPermissionToDownload.Error"));
            }

            var content = FileManager.Instance.GetFileContent(file);
            fileName = file.FileName;
            contentType = file.ContentType;
            return content;
        }

        public CopyMoveItemViewModel CopyFile(int fileId, int destinationFolderId, bool overwrite)
        {
            var file = FileManager.Instance.GetFile(fileId, true);
            var folder = FolderManager.Instance.GetFolder(destinationFolderId);
            var sourceFolder = FolderManager.Instance.GetFolder(file.FolderId);

            if (!HasPermission(sourceFolder, "COPY"))
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
            if (!HasPermission(sourceFolder, "COPY"))
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
            var folder = GetFolderInfo(folderId);
            if (!HasPermission(folder, "COPY"))
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
                PreviewImageUrl = GetFolderIconUrl(folder.PortalID, folder.FolderMappingID),
                Fields = GetFolderPreviewFields(folder)
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
                Fields = GetFilePreviewFields(file)
            };
            return result;
        }
        
        public ZipExtractViewModel UnzipFile(int fileId, bool overwrite)
        {
            var file = FileManager.Instance.GetFile(fileId, true);
            FileManager.Instance.UnzipFile(file);
            return null;
        }
        #endregion

        #region Protected Methods
        protected bool HasPermission(IFolderInfo folder, string permissionKey)
        {
            var hasPermision = PortalSettings.Current.UserInfo.IsSuperUser;

            if (!hasPermision && folder != null)
            {
                hasPermision = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

            return hasPermision;
        }

        protected ItemPathViewModel GetItemPathViewModel(IFolderInfo folder)
        {
            return new ItemPathViewModel
            {
                IsFolder = true,
                ItemID = folder.FolderID,
                DisplayPath = folder.DisplayPath,
                IconUrl = GetFolderIconUrl(folder.PortalID, folder.FolderMappingID)
            };
        }

        protected ItemPathViewModel GetItemPathViewModel(IFileInfo file)
        {
            return new ItemPathViewModel
            {
                IsFolder = false,
                ItemID = file.FileId,
                DisplayPath = file.RelativePath,
                IconUrl = GetFileIconUrl(file.Extension)
            };
        }
        
        protected virtual FolderViewModel GetFolderViewModel(IFolderInfo folder)
        {
            var folderName = string.IsNullOrEmpty(folder.FolderName)
                ? LocalizationHelper.GetString("RootFolder.Text")
                : folder.FolderName;

            return new FolderViewModel
            {
                FolderID = folder.FolderID,
                FolderMappingID = folder.FolderMappingID,
                FolderName = folderName,
                FolderPath = folder.FolderPath,
                PortalID = folder.PortalID,
                LastModifiedOnDate = folder.LastModifiedOnDate.ToString("g"),
                IconUrl = GetFolderIconUrl(folder.PortalID, folder.FolderMappingID),
                Permissions = GetPermissionViewModelCollection(folder),
                HasChildren = folder.HasChildren
            };
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
                IconUrl = GetFolderIconUrl(folder.PortalID, folder.FolderMappingID),
                Permissions = GetPermissionViewModelCollection(folder),
                ParentFolderID = parentFolderId,
                ParentFolder = parentFolderPath,
                FolderMappingID = folder.FolderMappingID
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
                Permissions = GetPermissionViewModelCollection(folder),
                ParentFolderID = folder.FolderID,
                ParentFolder = folder.FolderPath,
                Size = string.Format(new FileSizeFormatProvider(), "{0:fs}", file.Size)
            };
        }

        protected string GetFolderIconUrl(int portalId, int folderMappingID)
        {
            var imageUrl = FolderMappingController.Instance.GetFolderMapping(portalId, folderMappingID).ImageUrl;

            if (File.Exists(HttpContext.Current.Server.MapPath(imageUrl)))
            {
                return imageUrl;
            }

            return IconController.IconURL("ExtClosedFolder", "32x32", "Standard");
        }
        #endregion
        
        public string UpgradeModule(string version)
        {
            try
            {
                switch (version)
                {
                    case "07.01.00":
                        ModuleDefinitionInfo mDef = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Digital Asset Management");

                        //Add tab to Admin Menu
                        if (mDef != null)
                        {
                            var hostPage = Upgrade.AddHostPage("File Management",
                                                            "Manage assets.",
                                                            "~/Icons/Sigma/Files_16X16_Standard.png",
                                                            "~/Icons/Sigma/Files_32X32_Standard.png",
                                                            true);

                            //Add module to page
                            Upgrade.AddModuleToPage(hostPage, mDef.ModuleDefID, "File Management", "~/Icons/Sigma/Files_32X32_Standard.png", true);

                            Upgrade.AddAdminPages("File Management",
                                                 "Manage assets within the portal",
                                                 "~/Icons/Sigma/Files_16X16_Standard.png",
                                                 "~/Icons/Sigma/Files_32X32_Standard.png",
                                                 true,
                                                 mDef.ModuleDefID,
                                                 "File Management",
                                                 "~/Icons/Sigma/Files_16X16_Standard.png",
                                                 true);
                        }

                        //Remove Host File Manager page
                        Upgrade.RemoveHostPage("File Manager");

                        //Remove Admin File Manager Pages
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
    }
}
