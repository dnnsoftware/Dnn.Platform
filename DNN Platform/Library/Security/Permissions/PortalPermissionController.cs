// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Log.EventLog;

/// <summary>PortalPermissionController provides the Business Layer for Portal Permissions.</summary>
public class PortalPermissionController
{
    private static readonly PermissionProvider Provider = PermissionProvider.Instance();

    /// <summary>Returns a flag indicating whether the current user can add top level pages on the current portal.</summary>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanAddTopLevel()
    {
        return CanAddTopLevel(PortalController.Instance.GetCurrentSettings().PortalId);
    }

    /// <summary>Returns a flag indicating whether the current user can add top level pages on a portal.</summary>
    /// <param name="portalId">The portal id.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanAddTopLevel(int portalId)
    {
        return Provider.CanAddTopLevel(portalId);
    }

    /// <summary>Returns a flag indicating whether the current user is a page admin for the current portal.</summary>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanAdminPages()
    {
        return CanAdminPages(PortalController.Instance.GetCurrentSettings().PortalId);
    }

    /// <summary>Returns a flag indicating whether the current user is a page admin for a portal.</summary>
    /// <param name="portalId">The portal id.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanAdminPages(int portalId)
    {
        return Provider.IsPageAdmin(portalId);
    }

    /// <summary>DeletePortalPermissionsByUser deletes a user's Portal Permissions in the Database.</summary>
    /// <param name="user">The user.</param>
    public static void DeletePortalPermissionsByUser(UserInfo user)
    {
        Provider.DeletePortalPermissionsByUser(user);
        EventLogController.Instance.AddLog(user, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.PORTALPERMISSION_DELETED);
        DataCache.ClearPortalPermissionsCache(user.PortalID);
    }

    /// <summary>GetPortalPermissions gets a PortalPermissionCollection.</summary>
    /// <param name="portalId">The ID of the portal.</param>
    /// <returns>A <see cref="PortalPermissionCollection"/> with the portal permissions, or an empty <see cref="PortalPermissionCollection"/> if the portal wasn't found.</returns>
    public static PortalPermissionCollection GetPortalPermissions(int portalId)
    {
        return Provider.GetPortalPermissions(portalId);
    }

    /// <summary>HasPortalPermission checks whether the current user has a specific Portal Permission.</summary>
    /// <remarks>If you pass in a comma delimited list of permissions (eg "ADD,DELETE"), this will return true if the user has any one of the permissions.</remarks>
    /// <param name="permissionKey">The Permission to check.</param>
    /// <returns><see langword="true"/> if the current user has the requested permission, otherwise <see langword="false"/>.</returns>
    public static bool HasPortalPermission(string permissionKey)
    {
        return HasPortalPermission(PortalController.Instance.GetPortal(PortalController.Instance.GetCurrentSettings().PortalId).PortalPermissions, permissionKey);
    }

    /// <summary>HasPortalPermission checks whether the current user has a specific Portal Permission.</summary>
    /// <remarks>If you pass in a comma delimited list of permissions (eg "ADD,DELETE"), this will return true if the user has any one of the permissions.</remarks>
    /// <param name="portalPermissions">The Permissions for the Portal.</param>
    /// <param name="permissionKey">The Permission(s) to check.</param>
    /// <returns><see langword="true"/> if the current user has the requested permission, otherwise <see langword="false"/>.</returns>
    public static bool HasPortalPermission(PortalPermissionCollection portalPermissions, string permissionKey)
    {
        return Provider.HasPortalPermission(portalPermissions, permissionKey);
    }

    /// <summary>SavePortalPermissions saves a Portal's permissions.</summary>
    /// <param name="portal">The Portal to update.</param>
    public static void SavePortalPermissions(PortalInfo portal)
    {
        Provider.SavePortalPermissions(portal);
        EventLogController.Instance.AddLog(portal, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.PORTALPERMISSION_UPDATED);
        DataCache.ClearPortalPermissionsCache(portal.PortalID);
    }

    private static void ClearPermissionCache(int portalId)
    {
        var objPortal = PortalController.Instance.GetPortal(portalId);
        DataCache.ClearPortalPermissionsCache(objPortal.PortalID);
    }
}
