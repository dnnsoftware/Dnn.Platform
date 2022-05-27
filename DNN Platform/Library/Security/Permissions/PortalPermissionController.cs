// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

namespace DotNetNuke.Security.Permissions
{
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Log.EventLog;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : PortalPermissionController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// PortalPermissionController provides the Business Layer for Portal Permissions.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class PortalPermissionController
    {
        private static readonly PermissionProvider _provider = PermissionProvider.Instance();

        /// <summary>
        /// Returns a list with all roles with implicit permissions on Portals.
        /// </summary>
        /// <param name="portalId">The Portal Id where the Roles are.</param>
        /// <returns>A List with the implicit roles.</returns>
        public static IEnumerable<RoleInfo> ImplicitRoles(int portalId)
        {
            return _provider.ImplicitRolesForPages(portalId);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can add top level pages.
        /// </summary>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanAddTopLevel()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a flag indicating whether the current user is a page admin.
        /// </summary>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanAdminPages()
        {
            throw new NotImplementedException();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeletePortalPermissionsByUser deletes a user's Portal Permissions in the Database.
        /// </summary>
        /// <param name="user">The user.</param>
        /// -----------------------------------------------------------------------------
        public static void DeletePortalPermissionsByUser(UserInfo user)
        {
            _provider.DeletePortalPermissionsByUser(user);
            EventLogController.Instance.AddLog(user, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.PORTALPERMISSION_DELETED);
            DataCache.ClearPortalPermissionsCache(user.PortalID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetPortalPermissions gets a PortalPermissionCollection.
        /// </summary>
        /// <param name="portalId">The ID of the portal.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static PortalPermissionCollection GetPortalPermissions(int portalId)
        {
            return _provider.GetPortalPermissions(portalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HasPortalPermission checks whether the current user has a specific Portal Permission.
        /// </summary>
        /// <remarks>If you pass in a comma delimited list of permissions (eg "ADD,DELETE", this will return
        /// true if the user has any one of the permissions.</remarks>
        /// <param name="permissionKey">The Permission to check.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static bool HasPortalPermission(string permissionKey)
        {
            return HasPortalPermission(PortalController.Instance.GetPortal(PortalController.Instance.GetCurrentSettings().PortalId).PortalPermissions, permissionKey);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HasPortalPermission checks whether the current user has a specific Portal Permission.
        /// </summary>
        /// <remarks>If you pass in a comma delimited list of permissions (eg "ADD,DELETE", this will return
        /// true if the user has any one of the permissions.</remarks>
        /// <param name="portalPermissions">The Permissions for the Portal.</param>
        /// <param name="permissionKey">The Permission(s) to check.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static bool HasPortalPermission(PortalPermissionCollection portalPermissions, string permissionKey)
        {
            return _provider.HasPortalPermission(portalPermissions, permissionKey);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SavePortalPermissions saves a Portal's permissions.
        /// </summary>
        /// <param name="portal">The Portal to update.</param>
        /// -----------------------------------------------------------------------------
        public static void SavePortalPermissions(PortalInfo portal)
        {
            _provider.SavePortalPermissions(portal);
            EventLogController.Instance.AddLog(portal, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.PORTALPERMISSION_UPDATED);
            DataCache.ClearPortalPermissionsCache(portal.PortalID);
        }

        private static void ClearPermissionCache(int portalId)
        {
            var objPortal = PortalController.Instance.GetPortal(portalId);
            DataCache.ClearPortalPermissionsCache(objPortal.PortalID);
        }
    }
}
