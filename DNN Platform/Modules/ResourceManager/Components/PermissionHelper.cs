// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager.Components;

using System.Collections.Generic;
using System.Linq;

using Dnn.Modules.ResourceManager.Services.Dto;

using DotNetNuke.Abstractions.Security.Permissions;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;

/// <summary>Helper methods for permissions.</summary>
public static class PermissionHelper
{
    /// <summary>Adds user permissions to the <see cref="Permissions"/> dto.</summary>
    /// <param name="dto"><see cref="Permissions"/> data transfer object to extend.</param>
    /// <param name="permissionInfo">Permission to add.</param>
    public static void AddUserPermission(this Permissions dto, IPermissionInfo permissionInfo)
    {
        var userPermission = dto.UserPermissions.FirstOrDefault(p => p.UserId == permissionInfo.UserId);
        if (userPermission == null)
        {
            userPermission = new UserPermission
            {
                UserId = permissionInfo.UserId,
                DisplayName = permissionInfo.DisplayName,
            };
            dto.UserPermissions.Add(userPermission);
        }

        if (userPermission.Permissions.All(p => p.PermissionId != permissionInfo.PermissionId))
        {
            userPermission.Permissions.Add(new Permission
            {
                PermissionId = permissionInfo.PermissionId,
                PermissionName = permissionInfo.PermissionName,
                AllowAccess = permissionInfo.AllowAccess,
            });
        }
    }

    /// <summary>Adds role permissions to the <see cref="Permissions"/> dto.</summary>
    /// <param name="dto"><see cref="Permissions"/> dto to extend.</param>
    /// <param name="permissionInfo">Permission to add.</param>
    public static void AddRolePermission(this Permissions dto, IPermissionInfo permissionInfo)
    {
        var rolePermission = dto.RolePermissions.FirstOrDefault(p => p.RoleId == permissionInfo.RoleId);
        if (rolePermission == null)
        {
            rolePermission = new RolePermission
            {
                RoleId = permissionInfo.RoleId,
                RoleName = permissionInfo.RoleName,
            };
            dto.RolePermissions.Add(rolePermission);
        }

        if (rolePermission.Permissions.All(p => p.PermissionId != permissionInfo.PermissionId))
        {
            rolePermission.Permissions.Add(new Permission
            {
                PermissionId = permissionInfo.PermissionId,
                PermissionName = permissionInfo.PermissionName,
                AllowAccess = permissionInfo.AllowAccess,
            });
        }
    }

    /// <summary>Ensures the <see cref="Permissions"/> dto has the default roles.</summary>
    /// <param name="dto"><see cref="Permissions"/> dto to extend.</param>
    public static void EnsureDefaultRoles(this Permissions dto)
    {
        // Administrators Role always has implicit permissions, then it should be always in
        dto.EnsureRole(RoleController.Instance.GetRoleById(PortalSettings.Current.PortalId, PortalSettings.Current.AdministratorRoleId), true, true);

        // Show also default roles
        dto.EnsureRole(RoleController.Instance.GetRoleById(PortalSettings.Current.PortalId, PortalSettings.Current.RegisteredRoleId), false, true);
        dto.EnsureRole(new RoleInfo { RoleID = int.Parse(Globals.glbRoleAllUsers), RoleName = Globals.glbRoleAllUsersName }, false, true);
    }

    /// <summary>Ensures the <see cref="Permissions"/> dto has the given role.</summary>
    /// <param name="dto"><see cref="Permissions"/> data transfer object to extend.</param>
    /// <param name="role">The role to ensure is include, <see cref="RoleInfo"/>.</param>
    public static void EnsureRole(this Permissions dto, RoleInfo role)
    {
        dto.EnsureRole(role, false);
    }

    /// <summary>Ensures the <see cref="Permissions"/> dto has the given role.</summary>
    /// <param name="dto">The <see cref="Permissions"/> dto to extend.</param>
    /// <param name="role">The <see cref="RoleInfo"/> role ensure is included.</param>
    /// <param name="locked">A value indicating whether that role is locked.</param>
    public static void EnsureRole(this Permissions dto, RoleInfo role, bool locked)
    {
        dto.EnsureRole(role, locked, false);
    }

    /// <summary>Ensures the <see cref="Permissions"/> dto has a specific role.</summary>
    /// <param name="dto">The <see cref="Permissions"/> dto to extend.</param>
    /// <param name="role">The <see cref="RoleInfo"/> role to ensure is included.</param>
    /// <param name="locked">A value indicating whether the role is locked.</param>
    /// <param name="isDefault">A value indicating whether the role is a default role.</param>
    public static void EnsureRole(this Permissions dto, RoleInfo role, bool locked, bool isDefault)
    {
        if (dto.RolePermissions.All(r => r.RoleId != role.RoleID))
        {
            dto.RolePermissions.Add(new RolePermission
            {
                RoleId = role.RoleID,
                RoleName = role.RoleName,
                Locked = locked,
                IsDefault = isDefault,
            });
        }
    }

    /// <summary>Check if the permission is for full control.</summary>
    /// <param name="permissionInfo">The <see cref="PermissionInfo"/> to check.</param>
    /// <returns>A value indicating whether this permission is for full control.</returns>
    public static bool IsFullControl(IPermissionDefinitionInfo permissionInfo)
    {
        return (permissionInfo.PermissionKey == "EDIT") && PermissionProvider.Instance().SupportsFullControl();
    }

    /// <summary>Checks if the permission is for view.</summary>
    /// <param name="permissionInfo">The <see cref="PermissionInfo"/> to check.</param>
    /// <returns>A value indicating whether the permission is for view.</returns>
    public static bool IsViewPermission(IPermissionDefinitionInfo permissionInfo)
    {
        return permissionInfo.PermissionKey == "VIEW";
    }

    /// <summary>Gets roles for the portal.</summary>
    /// <param name="portalId">The id of the portal.</param>
    /// <returns>An objected containing the roles.</returns>
    public static object GetRoles(int portalId)
    {
        var data = new { Groups = new List<object>(), Roles = new List<object>() };

        // Retrieves role groups info
        data.Groups.Add(new { GroupId = -2, Name = "AllRoles" });
        data.Groups.Add(new { GroupId = -1, Name = "GlobalRoles", Selected = true });

        foreach (RoleGroupInfo group in RoleController.GetRoleGroups(portalId))
        {
            data.Groups.Add(new { GroupId = group.RoleGroupID, Name = group.RoleGroupName });
        }

        // Retrieves roles info
        data.Roles.Add(new { RoleID = int.Parse(Globals.glbRoleUnauthUser), GroupId = -1, RoleName = Globals.glbRoleUnauthUserName });
        data.Roles.Add(new { RoleID = int.Parse(Globals.glbRoleAllUsers), GroupId = -1, RoleName = Globals.glbRoleAllUsersName });
        foreach (RoleInfo role in RoleController.Instance.GetRoles(portalId).OrderBy(r => r.RoleName))
        {
            data.Roles.Add(new { GroupId = role.RoleGroupID, RoleId = role.RoleID, Name = role.RoleName });
        }

        return data;
    }

    /// <summary>Converts roles permissions into permission info collection.</summary>
    /// <param name="permissions">The list of <see cref="RolePermission"/> to convert.</param>
    /// <param name="folderId">The folder id.</param>
    /// <returns>An ArrayList of <see cref="FolderPermissionInfo"/>.</returns>
    public static IEnumerable<FolderPermissionInfo> AsFolderPermissions(this IEnumerable<RolePermission> permissions, int folderId)
    {
        return permissions.SelectMany(
            p => p.Permissions,
            (p, permission) =>
            {
                var info = new FolderPermissionInfo
                {
                    AllowAccess = permission.AllowAccess,
                };
                IFolderPermissionInfo iInfo = info;
                iInfo.FolderId = folderId;
                iInfo.PermissionId = permission.PermissionId;
                iInfo.RoleId = p.RoleId;
                iInfo.UserId = Null.NullInteger;
                return info;
            });
    }

    /// <summary>Converts a list of <see cref="UserPermission"/> into a collection of <see cref="FolderPermissionInfo"/>.</summary>
    /// <param name="permissions">The list of <see cref="UserPermission"/> to extend.</param>
    /// <param name="folderId">The id of the folder.</param>
    /// <returns>An ArrayList of <see cref="FolderPermissionInfo"/>.</returns>
    public static IEnumerable<FolderPermissionInfo> AsFolderPermissions(this IEnumerable<UserPermission> permissions, int folderId)
    {
        return permissions.SelectMany(
            p => p.Permissions,
            (p, permission) =>
            {
                var info = new FolderPermissionInfo
                {
                    AllowAccess = permission.AllowAccess,
                };
                IFolderPermissionInfo iInfo = info;
                iInfo.FolderId = folderId;
                iInfo.PermissionId = permission.PermissionId;
                iInfo.RoleId = int.Parse(Globals.glbRoleNothing);
                iInfo.UserId = p.UserId;
                return info;
            });
    }

    /// <summary>Get the permissions for a folder.</summary>
    /// <param name="permissionDefinitionService">The permission service.</param>
    /// <param name="collection">The collection of <see cref="FolderPermissionInfo"/>.</param>
    /// <returns>A <see cref="Permissions"/> dto.</returns>
    public static Permissions GetFolderPermissions(
        this IPermissionDefinitionService permissionDefinitionService,
        FolderPermissionCollection collection)
    {
        var permissions = new Permissions();

        // Load the definitions
        foreach (var definition in permissionDefinitionService.GetDefinitionsByFolder())
        {
            var definitionDto = new Permission
            {
                PermissionId = definition.PermissionId,
                PermissionName = definition.PermissionName,
                FullControl = IsFullControl(definition),
                View = IsViewPermission(definition),
            };

            permissions.PermissionDefinitions.Add(definitionDto);
        }

        // Load the permissions
        permissions.EnsureDefaultRoles();

        foreach (var role in PermissionProvider.Instance().ImplicitRolesForFolders(PortalSettings.Current.PortalId))
        {
            permissions.EnsureRole(role, true, true);
        }

        foreach (IFolderPermissionInfo permission in collection)
        {
            if (permission.UserId != Null.NullInteger)
            {
                permissions.AddUserPermission(permission);
            }
            else
            {
                permissions.AddRolePermission(permission);
            }
        }

        // Sort the permissions
        permissions.RolePermissions = permissions.RolePermissions
            .OrderByDescending(p => p.Locked)
            .ThenByDescending(p => p.IsDefault)
            .ThenBy(p => p.RoleName)
            .ToList();

        permissions.UserPermissions = permissions.UserPermissions
            .OrderBy(p => p.DisplayName)
            .ToList();

        return permissions;
    }
}
