// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components;

using System;
using System.Collections.Generic;
using System.IO;

using Dnn.Modules.ResourceManager.Components.Common;
using Dnn.Modules.ResourceManager.Exceptions;
using Dnn.Modules.ResourceManager.Helpers;
using Dnn.Modules.ResourceManager.Services.Dto;

using DotNetNuke.Entities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Assets;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.FileSystem.EventArgs;
using DotNetNuke.Services.Localization;

/// <summary>Provides services to manage items.</summary>
public class ItemsManager : ServiceLocator<IItemsManager, ItemsManager>, IItemsManager
{
    private const int MaxDescriptionLength = 500;
    private readonly IRoleController roleController;
    private readonly IFileManager fileManager;
    private readonly IAssetManager assetManager;
    private readonly IPermissionsManager permissionsManager;

    /// <summary>Initializes a new instance of the <see cref="ItemsManager"/> class.</summary>
    public ItemsManager()
    {
        this.roleController = RoleController.Instance;
        this.fileManager = FileManager.Instance;
        this.assetManager = AssetManager.Instance;
        this.permissionsManager = PermissionsManager.Instance;
    }

    /// <inheritdoc />
    public ContentPage GetFolderContent(int folderId, int startIndex, int numItems, string sorting, int moduleMode)
    {
        var noPermissionMessage = Localization.GetExceptionMessage(
            "UserHasNoPermissionToBrowseFolder",
            Constants.UserHasNoPermissionToBrowseFolderDefaultMessage);

        if (!this.permissionsManager.HasFolderContentPermission(folderId, moduleMode))
        {
            throw new FolderPermissionNotMetException(noPermissionMessage);
        }

        try
        {
            return AssetManager.Instance.GetFolderContent(folderId, startIndex, numItems, sorting + " ASC");
        }
        catch (AssetManagerException)
        {
            throw new FolderPermissionNotMetException(noPermissionMessage);
        }
    }

    /// <inheritdoc />
    public Stream GetFileContent(int fileId, out string fileName, out string contentType)
    {
        var file = this.fileManager.GetFile(fileId, true);

        if (!this.permissionsManager.HasGetFileContentPermission(file.FolderId))
        {
            throw new FolderPermissionNotMetException(LocalizationHelper.GetString(Constants.UserHasNoPermissionToDownloadKey));
        }

        var content = this.fileManager.GetFileContent(file);
        fileName = file.FileName;
        contentType = file.ContentType;

        EventManager.Instance.OnFileDownloaded(new FileDownloadedEventArgs
        {
            FileInfo = file,
            UserId = UserController.Instance.GetCurrentUserInfo().UserID,
        });
        return content;
    }

    /// <inheritdoc />
    public IFolderInfo CreateNewFolder(string folderName, int parentFolderId, int folderMappingId, string mappedName, int moduleMode)
    {
        if (!this.permissionsManager.HasAddFoldersPermission(moduleMode, parentFolderId))
        {
            throw new FolderPermissionNotMetException(LocalizationHelper.GetString(Constants.UserHasNoPermissionToAddFoldersKey));
        }

        return AssetManager.Instance.CreateFolder(folderName, parentFolderId, folderMappingId, mappedName);
    }

    /// <inheritdoc />
    public void SaveFileDetails(IFileInfo file, FileDetailsRequest fileDetails)
    {
        var propertyChanged = false;

        this.assetManager.RenameFile(file.FileId, fileDetails.FileName);
        if (file.Title != fileDetails.Title)
        {
            file.Title = fileDetails.Title;
            propertyChanged = true;
        }

        if (file.Description != fileDetails.Description)
        {
            file.Description = fileDetails.Description;
            if (!string.IsNullOrEmpty(file.Description) && file.Description.Length > MaxDescriptionLength)
            {
                file.Description = file.Description.Substring(0, MaxDescriptionLength);
            }

            propertyChanged = true;
        }

        if (propertyChanged)
        {
            FileManager.Instance.UpdateFile(file);
        }
    }

    /// <inheritdoc />
    public void SaveFolderDetails(IFolderInfo folder, FolderDetailsRequest folderDetails)
    {
        if (!string.IsNullOrWhiteSpace(folderDetails.FolderName))
        {
            this.assetManager.RenameFolder(folderDetails.FolderId, folderDetails.FolderName);
        }

        folder.FolderPermissions.Clear();
        folder.FolderPermissions.AddRange(folderDetails.Permissions.RolePermissions.AsFolderPermissions(folderDetails.FolderId));
        folder.FolderPermissions.AddRange(folderDetails.Permissions.UserPermissions.AsFolderPermissions(folderDetails.FolderId));
        FolderManager.Instance.UpdateFolder(folder);
    }

    /// <inheritdoc />
    public void DeleteFile(int fileId, int moduleMode, int groupId)
    {
        var file = FileManager.Instance.GetFile(fileId);
        if (file == null)
        {
            return;
        }

        if (moduleMode == (int)Constants.ModuleModes.Group && this.IsGroupIcon(file))
        {
            throw new FolderPermissionNotMetException(LocalizationHelper.GetString(Constants.GroupIconCantBeDeletedKey));
        }

        if (!this.permissionsManager.HasDeletePermission(moduleMode, file.FolderId))
        {
            throw new FolderPermissionNotMetException(LocalizationHelper.GetString("UserHasNoPermissionToDeleteFile.Error"));
        }

        AssetManager.Instance.DeleteFile(fileId);
    }

    /// <inheritdoc />
    public void DeleteFolder(int folderId, bool unlinkAllowedStatus, int moduleMode)
    {
        var folder = FolderManager.Instance.GetFolder(folderId);
        if (folder == null)
        {
            return;
        }

        if (!this.permissionsManager.HasDeletePermission(moduleMode, folderId))
        {
            throw new FolderPermissionNotMetException(LocalizationHelper.GetString("UserHasNoPermissionToDeleteFolder.Error"));
        }

        var nonDeletedSubfolders = new List<IFolderInfo>();
        AssetManager.Instance.DeleteFolder(folderId, unlinkAllowedStatus, nonDeletedSubfolders);
    }

    /// <inheritdoc/>
    public void MoveFile(int sourceFileId, int destinationFolderId, int moduleMode, int groupId)
    {
        var file = FileManager.Instance.GetFile(sourceFileId);
        var destinationFolder = FolderManager.Instance.GetFolder(destinationFolderId);

        if (file == null || destinationFolder == null)
        {
            return;
        }

        if (!this.permissionsManager.HasDeletePermission(moduleMode, file.FolderId))
        {
            throw new FolderPermissionNotMetException(LocalizationHelper.GetString("UserHasNoPermissionToDeleteFolder.Error"));
        }

        if (!this.permissionsManager.HasAddFilesPermission(moduleMode, destinationFolderId))
        {
            throw new FolderPermissionNotMetException(LocalizationHelper.GetString("UserHasNoPermissionToManageFolder.Error"));
        }

        FileManager.Instance.MoveFile(file, destinationFolder);
    }

    /// <inheritdoc/>
    public void MoveFolder(int sourceFolderId, int destinationFolderId, int moduleMode, int groupId)
    {
        var sourceFolder = FolderManager.Instance.GetFolder(sourceFolderId);
        var sourceFolderParent = FolderManager.Instance.GetFolder(sourceFolder.ParentID);
        var destinationFolder = FolderManager.Instance.GetFolder(destinationFolderId);

        if (sourceFolder == null || destinationFolder == null || sourceFolderParent == null)
        {
            return;
        }

        if (
            !this.permissionsManager.HasDeletePermission(moduleMode, sourceFolderId) ||
            !this.permissionsManager.HasDeletePermission(moduleMode, sourceFolderParent.FolderID))
        {
            throw new FolderPermissionNotMetException(LocalizationHelper.GetString("UserHasNoPermissionToDeleteFolder.Error"));
        }

        if (
            !this.permissionsManager.HasAddFilesPermission(moduleMode, destinationFolderId) ||
            !this.permissionsManager.HasAddFoldersPermission(moduleMode, destinationFolderId))
        {
            throw new FolderPermissionNotMetException(LocalizationHelper.GetString("UserHasNoPermissionToManageFolder.Error"));
        }

        FolderManager.Instance.MoveFolder(sourceFolder, destinationFolder);
    }

    /// <inheritdoc />
    protected override Func<IItemsManager> GetFactory()
    {
        return () => new ItemsManager();
    }

    private bool IsGroupIcon(IFileInfo file)
    {
        var groupId = Utils.GetFolderGroupId(file.FolderId);
        if (groupId < 0)
        {
            return false;
        }

        var portalId = PortalSettings.Current.PortalId;
        var role = this.roleController.GetRoleById(portalId, groupId);
        return role?.IconFile?.Substring(7) == file.FileId.ToString();
    }
}
