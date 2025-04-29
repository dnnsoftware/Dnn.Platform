// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions;

using System;
using System.Collections.Generic;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;

public partial class FolderPermissionController : ServiceLocator<IFolderPermissionController, FolderPermissionController>, IFolderPermissionController
{
    private static readonly PermissionProvider Provider = PermissionProvider.Instance();

    /// <summary>Returns a list with all roles with implicit permissions on Folders.</summary>
    /// <param name="portalId">The Portal Id where the Roles are.</param>
    /// <returns>A List with the implicit roles.</returns>
    public static IEnumerable<RoleInfo> ImplicitRoles(int portalId)
    {
        return Provider.ImplicitRolesForPages(portalId);
    }

    /// <summary>Returns a flag indicating whether the current user can add a folder or file.</summary>
    /// <param name="folder">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanAddFolder(FolderInfo folder)
    {
        return Provider.CanAddFolder(folder);
    }

    /// <summary>Returns a flag indicating whether the current user can addmister a folder.</summary>
    /// <param name="folder">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanAdminFolder(FolderInfo folder)
    {
        return Provider.CanAdminFolder(folder);
    }

    /// <summary>Returns a flag indicating whether the current user can browse the folder.</summary>
    /// <param name="folder">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanBrowseFolder(FolderInfo folder)
    {
        return Provider.CanBrowseFolder(folder);
    }

    /// <summary>Returns a flag indicating whether the current user can copy a folder or file.</summary>
    /// <param name="folder">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanCopyFolder(FolderInfo folder)
    {
        return Provider.CanCopyFolder(folder);
    }

    /// <summary>Returns a flag indicating whether the current user can delete a folder or file.</summary>
    /// <param name="folder">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanDeleteFolder(FolderInfo folder)
    {
        return Provider.CanDeleteFolder(folder);
    }

    /// <summary>Returns a flag indicating whether the current user can manage a folder's settings.</summary>
    /// <param name="folder">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanManageFolder(FolderInfo folder)
    {
        return Provider.CanManageFolder(folder);
    }

    /// <summary>Returns a flag indicating whether the current user can view a folder or file.</summary>
    /// <param name="folder">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanViewFolder(FolderInfo folder)
    {
        return Provider.CanViewFolder(folder);
    }

    public static void DeleteFolderPermissionsByUser(UserInfo objUser)
    {
        Provider.DeleteFolderPermissionsByUser(objUser);
        ClearPermissionCache(objUser.PortalID);
    }

    public static FolderPermissionCollection GetFolderPermissionsCollectionByFolder(int portalID, string folder)
    {
        return Provider.GetFolderPermissionsCollectionByFolder(portalID, folder);
    }

    public static bool HasFolderPermission(int portalId, string folderPath, string permissionKey)
    {
        return HasFolderPermission(GetFolderPermissionsCollectionByFolder(portalId, folderPath), permissionKey);
    }

    public static bool HasFolderPermission(FolderPermissionCollection objFolderPermissions, string permissionKey)
    {
        bool hasPermission = Provider.HasFolderPermission(objFolderPermissions, "WRITE");
        if (!hasPermission)
        {
            if (permissionKey.Contains(","))
            {
                foreach (string permission in permissionKey.Split(','))
                {
                    if (Provider.HasFolderPermission(objFolderPermissions, permission))
                    {
                        hasPermission = true;
                        break;
                    }
                }
            }
            else
            {
                hasPermission = Provider.HasFolderPermission(objFolderPermissions, permissionKey);
            }
        }

        return hasPermission;
    }

    /// <summary>Copies the permissions to subfolders.</summary>
    /// <param name="folder">The parent folder.</param>
    /// <param name="newPermissions">The new permissions.</param>
    public static void CopyPermissionsToSubfolders(IFolderInfo folder, FolderPermissionCollection newPermissions)
    {
        bool clearCache = CopyPermissionsToSubfoldersRecursive(folder, newPermissions);
        if (clearCache)
        {
            DataCache.ClearFolderCache(folder.PortalID);
        }
    }

    /// <summary>SaveFolderPermissions updates a Folder's permissions.</summary>
    /// <param name="folder">The Folder to update.</param>
    public static void SaveFolderPermissions(FolderInfo folder)
    {
        SaveFolderPermissions((IFolderInfo)folder);
    }

    /// <summary>SaveFolderPermissions updates a Folder's permissions.</summary>
    /// <param name="folder">The Folder to update.</param>
    public static void SaveFolderPermissions(IFolderInfo folder)
    {
        Provider.SaveFolderPermissions(folder);
        ClearPermissionCache(folder.PortalID);
    }

    /// <summary>Returns a flag indicating whether the current user can add a folder or file.</summary>
    /// <param name="folder">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    bool IFolderPermissionController.CanAddFolder(IFolderInfo folder)
    {
        return Provider.CanAddFolder((FolderInfo)folder);
    }

    /// <summary>Returns a flag indicating whether the current user can addmister a folder.</summary>
    /// <param name="folder">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    bool IFolderPermissionController.CanAdminFolder(IFolderInfo folder)
    {
        return Provider.CanAdminFolder((FolderInfo)folder);
    }

    /// <summary>Returns a flag indicating whether the current user can view a folder or file.</summary>
    /// <param name="folder">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    bool IFolderPermissionController.CanViewFolder(IFolderInfo folder)
    {
        return Provider.CanViewFolder((FolderInfo)folder);
    }

    /// <inheritdoc/>
    protected override Func<IFolderPermissionController> GetFactory()
    {
        return () => new FolderPermissionController();
    }

    private static void ClearPermissionCache(int portalID)
    {
        DataCache.ClearFolderPermissionsCache(portalID);
        DataCache.ClearCache(string.Format("Folders|{0}|", portalID));
        DataCache.ClearFolderCache(portalID);
    }

    private static bool CopyPermissionsToSubfoldersRecursive(IFolderInfo folder, FolderPermissionCollection newPermissions)
    {
        bool clearCache = Null.NullBoolean;
        IEnumerable<IFolderInfo> childFolders = FolderManager.Instance.GetFolders(folder);
        foreach (var f in childFolders)
        {
            if (CanAdminFolder((FolderInfo)f))
            {
                f.FolderPermissions.Clear();
                f.FolderPermissions.AddRange(newPermissions);
                SaveFolderPermissions(f);
                clearCache = true;
            }

            clearCache = CopyPermissionsToSubfoldersRecursive(f, newPermissions) || clearCache;
        }

        return clearCache;
    }
}
