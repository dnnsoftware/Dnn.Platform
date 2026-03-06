// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Helper
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Dnn.PersonaBar.Library.Dto;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Helpers methods for permissions.</summary>
    public static partial class PermissionHelper
    {
        public static void AddUserPermission(this Dto.Permissions dto, PermissionInfoBase permissionInfo)
            => dto.AddUserPermission((IPermissionInfo)permissionInfo);

        public static void AddUserPermission(this Dto.Permissions dto, IPermissionInfo permissionInfo)
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

        public static void AddRolePermission(this Dto.Permissions dto, PermissionInfoBase permissionInfo)
            => dto.AddRolePermission((IPermissionInfo)permissionInfo);

        public static void AddRolePermission(this Dto.Permissions dto, IPermissionInfo permissionInfo)
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

        public static void EnsureDefaultRoles(this Dto.Permissions dto)
        {
            // Administrators Role always has implicit permissions, then it should be always in
            dto.EnsureRole(RoleController.Instance.GetRoleById(PortalSettings.Current.PortalId, PortalSettings.Current.AdministratorRoleId), true, true);

            // Show also default roles
            dto.EnsureRole(RoleController.Instance.GetRoleById(PortalSettings.Current.PortalId, PortalSettings.Current.RegisteredRoleId), false, true);
            dto.EnsureRole(new RoleInfo { RoleID = int.Parse(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture), RoleName = Globals.glbRoleAllUsersName, }, false, true);
        }

        public static void EnsureRole(this Dto.Permissions dto, RoleInfo role)
        {
            dto.EnsureRole(role, false);
        }

        public static void EnsureRole(this Dto.Permissions dto, RoleInfo role, bool locked)
        {
            dto.EnsureRole(role, locked, false);
        }

        public static void EnsureRole(this Dto.Permissions dto, RoleInfo role, bool locked, bool isDefault)
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

        /// <summary>Gets a value indicating whether the <paramref name="permissionInfo"/> gives full control.</summary>
        /// <param name="permissionInfo">The permission definition.</param>
        /// <returns><see langword="true"/> if it's the full control permission, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        public static partial bool IsFullControl(PermissionInfo permissionInfo)
            => IsFullControl((IPermissionDefinitionInfo)permissionInfo);

        /// <summary>Gets a value indicating whether the <paramref name="permissionDefinition"/> gives full control.</summary>
        /// <param name="permissionDefinition">The permission definition.</param>
        /// <returns><see langword="true"/> if it's the full control permission, otherwise <see langword="false"/>.</returns>
        public static bool IsFullControl(IPermissionDefinitionInfo permissionDefinition)
        {
            return permissionDefinition.PermissionKey == "EDIT" && PermissionProvider.Instance().SupportsFullControl();
        }

        /// <summary>Gets a value indicating whether the <paramref name="permissionInfo"/> is the view permission.</summary>
        /// <param name="permissionInfo">The permission definition.</param>
        /// <returns><see langword="true"/> if it's the view permission, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Use IsViewPermission")]
        public static partial bool IsViewPermisison(PermissionInfo permissionInfo)
            => IsViewPermission(permissionInfo);

        /// <summary>Gets a value indicating whether the <paramref name="permissionDefinition"/> is the view permission.</summary>
        /// <param name="permissionDefinition">The permission definition.</param>
        /// <returns><see langword="true"/> if it's the view permission, otherwise <see langword="false"/>.</returns>
        public static bool IsViewPermission(IPermissionDefinitionInfo permissionDefinition)
        {
            return permissionDefinition.PermissionKey == "VIEW";
        }

        /// <summary>Gets the roles for the portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>An object with <c>Groups</c> and <c>Roles</c> fields.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking RoleProvider")]
        public static partial object GetRoles(int portalId)
            => GetRoles(Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>(), portalId);

        /// <summary>Gets the roles for the portal.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>An object with <c>Groups</c> and <c>Roles</c> fields.</returns>
        public static object GetRoles(RoleProvider roleProvider, int portalId)
        {
            var data = new { Groups = new List<object>(), Roles = new List<object>() };

            // retrieve role groups info
            data.Groups.Add(new { GroupId = -2, Name = "AllRoles" });
            data.Groups.Add(new { GroupId = -1, Name = "GlobalRoles", Selected = true });

            foreach (RoleGroupInfo group in RoleController.GetRoleGroups(roleProvider, portalId))
            {
                data.Groups.Add(new { GroupId = group.RoleGroupID, Name = group.RoleGroupName });
            }

            // retrieve roles info
            data.Roles.Add(new { RoleID = int.Parse(Globals.glbRoleUnauthUser, CultureInfo.InvariantCulture), GroupId = -1, RoleName = Globals.glbRoleUnauthUserName });
            data.Roles.Add(new { RoleID = int.Parse(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture), GroupId = -1, RoleName = Globals.glbRoleAllUsersName });
            foreach (RoleInfo role in RoleController.Instance.GetRoles(portalId).OrderBy(r => r.RoleName))
            {
                data.Roles.Add(new { GroupId = role.RoleGroupID, RoleId = role.RoleID, Name = role.RoleName });
            }

            return data;
        }
    }
}
