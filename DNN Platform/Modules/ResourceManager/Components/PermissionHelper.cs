// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager.Components
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.Modules.ResourceManager.Services.Dto;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;

    /// <summary>
    /// Helper methods for permissions.
    /// </summary>
    public static class PermissionHelper
    {
        /// <summary>
        /// Adds user permissions to the <see cref="Permissions"/> dto.
        /// </summary>
        /// <param name="dto"><see cref="Permissions"/> data transfer object to extend.</param>
        /// <param name="permissionInfo">Permission to add.</param>
        public static void AddUserPermission(this Permissions dto, PermissionInfoBase permissionInfo)
        {
            var userPermission = dto.UserPermissions.FirstOrDefault(p => p.UserId == permissionInfo.UserID);
            if (userPermission == null)
            {
                userPermission = new UserPermission
                {
                    UserId = permissionInfo.UserID,
                    DisplayName = permissionInfo.DisplayName,
                };
                dto.UserPermissions.Add(userPermission);
            }

            if (userPermission.Permissions.All(p => p.PermissionId != permissionInfo.PermissionID))
            {
                userPermission.Permissions.Add(new Permission
                {
                    PermissionId = permissionInfo.PermissionID,
                    PermissionName = permissionInfo.PermissionName,
                    AllowAccess = permissionInfo.AllowAccess,
                });
            }
        }

        /// <summary>
        /// Adds role permissions to the <see cref="Permissions"/> dto.
        /// </summary>
        /// <param name="dto"><see cref="Permissions"/> dto to extend.</param>
        /// <param name="permissionInfo">Permission to add.</param>
        public static void AddRolePermission(this Permissions dto, PermissionInfoBase permissionInfo)
        {
            var rolePermission = dto.RolePermissions.FirstOrDefault(p => p.RoleId == permissionInfo.RoleID);
            if (rolePermission == null)
            {
                rolePermission = new RolePermission
                {
                    RoleId = permissionInfo.RoleID,
                    RoleName = permissionInfo.RoleName,
                };
                dto.RolePermissions.Add(rolePermission);
            }

            if (rolePermission.Permissions.All(p => p.PermissionId != permissionInfo.PermissionID))
            {
                rolePermission.Permissions.Add(new Permission
                {
                    PermissionId = permissionInfo.PermissionID,
                    PermissionName = permissionInfo.PermissionName,
                    AllowAccess = permissionInfo.AllowAccess,
                });
            }
        }

        /// <summary>
        /// Ensures the <see cref="Permissions"/> dto has the default roles.
        /// </summary>
        /// <param name="dto"><see cref="Permissions"/> dto to extend.</param>
        public static void EnsureDefaultRoles(this Permissions dto)
        {
            // Administrators Role always has implicit permissions, then it should be always in
            dto.EnsureRole(RoleController.Instance.GetRoleById(PortalSettings.Current.PortalId, PortalSettings.Current.AdministratorRoleId), true, true);

            // Show also default roles
            dto.EnsureRole(RoleController.Instance.GetRoleById(PortalSettings.Current.PortalId, PortalSettings.Current.RegisteredRoleId), false, true);
            dto.EnsureRole(new RoleInfo { RoleID = int.Parse(Globals.glbRoleAllUsers), RoleName = Globals.glbRoleAllUsersName }, false, true);
        }

        /// <summary>
        /// Ensures the <see cref="Permissions"/> dto has the given role.
        /// </summary>
        /// <param name="dto"><see cref="Permissions"/> data transfer object to extend.</param>
        /// <param name="role">The role to ensure is include, <see cref="RoleInfo"/>.</param>
        public static void EnsureRole(this Permissions dto, RoleInfo role)
        {
            dto.EnsureRole(role, false);
        }

        /// <summary>
        /// Ensures the <see cref="Permissions"/> dto has the given role.
        /// </summary>
        /// <param name="dto">The <see cref="Permissions"/> dto to extend.</param>
        /// <param name="role">The <see cref="RoleInfo"/> role ensure is included.</param>
        /// <param name="locked">A value indicating whether that role is locked.</param>
        public static void EnsureRole(this Permissions dto, RoleInfo role, bool locked)
        {
            dto.EnsureRole(role, locked, false);
        }

        /// <summary>
        /// Ensures the <see cref="Permissions"/> dto has a specific role.
        /// </summary>
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

        /// <summary>
        /// Check if the permission is for full control.
        /// </summary>
        /// <param name="permissionInfo">The <see cref="PermissionInfo"/> to check.</param>
        /// <returns>A value indicating whether this permission is for full control.</returns>
        public static bool IsFullControl(PermissionInfo permissionInfo)
        {
            return (permissionInfo.PermissionKey == "EDIT") && PermissionProvider.Instance().SupportsFullControl();
        }

        /// <summary>
        /// Checks if the permission is for view.
        /// </summary>
        /// <param name="permissionInfo">The <see cref="PermissionInfo"/> to check.</param>
        /// <returns>A value indicating whether the permission is for view.</returns>
        public static bool IsViewPermission(PermissionInfo permissionInfo)
        {
            return permissionInfo.PermissionKey == "VIEW";
        }

        /// <summary>
        /// Gets roles for the portal.
        /// </summary>
        /// <param name="portalId">The id of the portal.</param>
        /// <returns>An objected containing the roles.</returns>
        public static object GetRoles(int portalId)
        {
            var data = new { Groups = new List<object>(), Roles = new List<object>() };

            // retreive role groups info
            data.Groups.Add(new { GroupId = -2, Name = "AllRoles" });
            data.Groups.Add(new { GroupId = -1, Name = "GlobalRoles", Selected = true });

            foreach (RoleGroupInfo group in RoleController.GetRoleGroups(portalId))
            {
                data.Groups.Add(new { GroupId = group.RoleGroupID, Name = group.RoleGroupName });
            }

            // retreive roles info
            data.Roles.Add(new { RoleID = int.Parse(Globals.glbRoleUnauthUser), GroupId = -1, RoleName = Globals.glbRoleUnauthUserName });
            data.Roles.Add(new { RoleID = int.Parse(Globals.glbRoleAllUsers), GroupId = -1, RoleName = Globals.glbRoleAllUsersName });
            foreach (RoleInfo role in RoleController.Instance.GetRoles(portalId).OrderBy(r => r.RoleName))
            {
                data.Roles.Add(new { GroupId = role.RoleGroupID, RoleId = role.RoleID, Name = role.RoleName });
            }

            return data;
        }

        /// <summary>
        /// Converts roles permissions into permission info collection.
        /// </summary>
        /// <param name="permissions">The list of <see cref="RolePermission"/> to convert.</param>
        /// <param name="folderId">The folder id.</param>
        /// <returns>An ArrayList of <see cref="FolderPermissionInfo"/>.</returns>
        public static ArrayList ToPermissionInfos(this IList<RolePermission> permissions, int folderId)
        {
            var newPermissions = new ArrayList();
            foreach (var permission in permissions)
            {
                foreach (var p in permission.Permissions)
                {
                    newPermissions.Add(new FolderPermissionInfo()
                    {
                        AllowAccess = p.AllowAccess,
                        FolderID = folderId,
                        PermissionID = p.PermissionId,
                        RoleID = permission.RoleId,
                        UserID = Null.NullInteger,
                    });
                }
            }

            return newPermissions;
        }

        /// <summary>
        /// Converts a list of <see cref="UserPermission"/> into a collection of <see cref="FolderPermissionInfo"/>.
        /// </summary>
        /// <param name="permissions">The list of <see cref="UserPermission"/> to extend.</param>
        /// <param name="folderId">The id of the folder.</param>
        /// <returns>An ArrayList of <see cref="FolderPermissionInfo"/>.</returns>
        public static ArrayList ToPermissionInfos(this IList<UserPermission> permissions, int folderId)
        {
            var newPermissions = new ArrayList();
            foreach (var permission in permissions)
            {
                foreach (var p in permission.Permissions)
                {
                    newPermissions.Add(new FolderPermissionInfo()
                    {
                        AllowAccess = p.AllowAccess,
                        FolderID = folderId,
                        PermissionID = p.PermissionId,
                        RoleID = Null.NullInteger,
                        UserID = permission.UserId,
                    });
                }
            }

            return newPermissions;
        }
    }
}
