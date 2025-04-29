// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components;

using System;

using Dnn.Modules.ResourceManager.Components.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;

/// <summary>Provides permissions checks.</summary>
public class PermissionsManager : ServiceLocator<IPermissionsManager, PermissionsManager>, IPermissionsManager
{
    private readonly IFolderManager folderManager;
    private readonly IRoleController roleController;
    private readonly IUserController userController;

    /// <summary>Initializes a new instance of the <see cref="PermissionsManager"/> class.</summary>
    public PermissionsManager()
    {
        this.folderManager = FolderManager.Instance;
        this.roleController = RoleController.Instance;
        this.userController = UserController.Instance;
    }

    /// <inheritdoc/>
    public bool HasFolderContentPermission(int folderId, int moduleMode)
    {
        return this.HasGroupFolderPublicOrMemberPermission(folderId);
    }

    /// <inheritdoc/>
    public bool HasGetFileContentPermission(int folderId)
    {
        if (!this.HasGroupFolderPublicOrMemberPermission(folderId))
        {
            return false;
        }

        var folder = this.folderManager.GetFolder(folderId);
        return HasPermission(folder, "READ");
    }

    /// <inheritdoc/>
    public bool HasAddFilesPermission(int moduleMode, int folderId)
    {
        if (!this.HasGroupFolderMemberPermission(folderId))
        {
            return false;
        }

        if (moduleMode == (int)Constants.ModuleModes.User && !this.IsInUserFolder(folderId))
        {
            return false;
        }

        var folder = this.folderManager.GetFolder(folderId);
        return folder != null && HasPermission(folder, "ADD");
    }

    /// <inheritdoc/>
    public bool HasAddFoldersPermission(int moduleMode, int folderId)
    {
        if (!this.HasGroupFolderOwnerPermission(folderId))
        {
            return false;
        }

        if (moduleMode == (int)Constants.ModuleModes.User)
        {
            return false;
        }

        var folder = this.folderManager.GetFolder(folderId);
        return folder != null && HasPermission(folder, "ADD");
    }

    /// <inheritdoc/>
    public bool HasDeletePermission(int moduleMode, int folderId)
    {
        if (!this.HasGroupFolderOwnerPermission(folderId))
        {
            return false;
        }

        if (moduleMode == (int)Constants.ModuleModes.User && !this.IsInUserFolder(folderId))
        {
            return false;
        }

        var folder = this.folderManager.GetFolder(folderId);
        return FolderPermissionController.CanDeleteFolder((FolderInfo)folder);
    }

    /// <inheritdoc/>
    public bool HasManagePermission(int moduleMode, int folderId)
    {
        if (!this.HasGroupFolderOwnerPermission(folderId))
        {
            return false;
        }

        if (moduleMode == (int)Constants.ModuleModes.User && !this.IsInUserFolder(folderId))
        {
            return false;
        }

        var folder = this.folderManager.GetFolder(folderId);
        return FolderPermissionController.CanManageFolder((FolderInfo)folder);
    }

    /// <summary>Checks if the current user has permission on a group folder.</summary>
    /// <param name="folderId">The id of the folder.</param>
    /// <returns>A value indicating whether the user has permission on the group folder.</returns>
    public bool HasGroupFolderMemberPermission(int folderId)
    {
        var groupId = Utils.GetFolderGroupId(folderId);
        if (groupId < 0)
        {
            return true;
        }

        return this.UserIsGroupMember(groupId);
    }

    /// <inheritdoc/>
    protected override Func<IPermissionsManager> GetFactory()
    {
        return () => new PermissionsManager();
    }

    /// <summary>Check if a user has a specific permission key on a folder.</summary>
    /// <param name="folder">The id of the folder to check.</param>
    /// <param name="permissionKey">The permission key.</param>
    /// <returns>A value indicating whether the user has the permission key for the folder.</returns>
    private static bool HasPermission(IFolderInfo folder, string permissionKey)
    {
        var hasPermission = PortalSettings.Current.UserInfo.IsSuperUser;

        if (!hasPermission && folder != null)
        {
            hasPermission = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
        }

        return hasPermission;
    }

    private bool HasGroupFolderPublicOrMemberPermission(int folderId)
    {
        var groupId = Utils.GetFolderGroupId(folderId);
        if (groupId < 0)
        {
            return true;
        }

        var portalId = PortalSettings.Current.PortalId;
        var folderGroup = this.roleController.GetRoleById(portalId, groupId);

        return folderGroup.IsPublic || this.UserIsGroupMember(groupId);
    }

    private bool HasGroupFolderOwnerPermission(int folderId)
    {
        var groupId = Utils.GetFolderGroupId(folderId);
        if (groupId < 0)
        {
            return true;
        }

        return this.UserIsGroupOwner(groupId);
    }

    private bool UserIsGroupMember(int groupId)
    {
        return this.GetUserRoleInfo(groupId) != null;
    }

    private bool UserIsGroupOwner(int groupId)
    {
        var userRole = this.GetUserRoleInfo(groupId);
        return userRole != null && userRole.IsOwner;
    }

    private bool IsInUserFolder(int folderId)
    {
        var user = this.userController.GetCurrentUserInfo();
        var userFolder = this.folderManager.GetUserFolder(user);
        return this.IsUserFolder(folderId, userFolder);
    }

    private bool IsUserFolder(int folderId, IFolderInfo userFolder)
    {
        if (userFolder.FolderID == folderId)
        {
            return true;
        }

        if (userFolder.ParentID != Null.NullInteger)
        {
            var parent = this.folderManager.GetFolder(userFolder.ParentID);
            return this.IsUserFolder(folderId, parent);
        }

        return false;
    }

    private UserRoleInfo GetUserRoleInfo(int groupId)
    {
        var userId = this.userController.GetCurrentUserInfo().UserID;
        var portalId = PortalSettings.Current.PortalId;
        return this.roleController.GetUserRole(portalId, userId, groupId);
    }
}
