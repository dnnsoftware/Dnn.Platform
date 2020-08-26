﻿// Licensed to the .NET Foundation under one or more agreements.
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

    public static class PermissionHelper
    {
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

        public static void EnsureDefaultRoles(this Permissions dto)
        {
            // Administrators Role always has implicit permissions, then it should be always in
            dto.EnsureRole(RoleController.Instance.GetRoleById(PortalSettings.Current.PortalId, PortalSettings.Current.AdministratorRoleId), true, true);

            // Show also default roles
            dto.EnsureRole(RoleController.Instance.GetRoleById(PortalSettings.Current.PortalId, PortalSettings.Current.RegisteredRoleId), false, true);
            dto.EnsureRole(new RoleInfo { RoleID = int.Parse(Globals.glbRoleAllUsers), RoleName = Globals.glbRoleAllUsersName }, false, true);
        }

        public static void EnsureRole(this Permissions dto, RoleInfo role)
        {
            dto.EnsureRole(role, false);
        }

        public static void EnsureRole(this Permissions dto, RoleInfo role, bool locked)
        {
            dto.EnsureRole(role, locked, false);
        }

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

        public static bool IsFullControl(PermissionInfo permissionInfo)
        {
            return (permissionInfo.PermissionKey == "EDIT") && PermissionProvider.Instance().SupportsFullControl();
        }

        public static bool IsViewPermission(PermissionInfo permissionInfo)
        {
            return permissionInfo.PermissionKey == "VIEW";
        }

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
                        UserID = Null.NullInteger
                    });
                }
            }
            return newPermissions;
        }

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
                        UserID = permission.UserId
                    });
                }
            }
            return newPermissions;
        }
    }
}
