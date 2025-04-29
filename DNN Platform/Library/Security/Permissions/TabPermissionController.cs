// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions;

using System.Collections.Generic;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Log.EventLog;

/// <summary>TabPermissionController provides the Business Layer for Tab Permissions.</summary>
public class TabPermissionController
{
    private static readonly PermissionProvider Provider = PermissionProvider.Instance();

    /// <summary>Returns a list with all roles with implicit permissions on Tabs.</summary>
    /// <param name="portalId">The Portal Id where the Roles are.</param>
    /// <returns>A List with the implicit roles.</returns>
    public static IEnumerable<RoleInfo> ImplicitRoles(int portalId)
    {
        return Provider.ImplicitRolesForPages(portalId);
    }

    /// <summary>Returns a flag indicating whether the current user can add content to the current page.</summary>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanAddContentToPage()
    {
        return CanAddContentToPage(TabController.CurrentPage);
    }

    /// <summary>Returns a flag indicating whether the current user can add content to a page.</summary>
    /// <param name="tab">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanAddContentToPage(TabInfo tab)
    {
        return Provider.CanAddContentToPage(tab);
    }

    /// <summary>Returns a flag indicating whether the current user can add a child page to the current page.</summary>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanAddPage()
    {
        return CanAddPage(TabController.CurrentPage);
    }

    /// <summary>Returns a flag indicating whether the current user can add a child page to a page.</summary>
    /// <param name="tab">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanAddPage(TabInfo tab)
    {
        return Provider.CanAddPage(tab);
    }

    /// <summary>Returns a flag indicating whether the current user can administer the current page.</summary>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanAdminPage()
    {
        return CanAdminPage(TabController.CurrentPage);
    }

    /// <summary>Returns a flag indicating whether the current user can administer a page.</summary>
    /// <param name="tab">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanAdminPage(TabInfo tab)
    {
        return Provider.CanAdminPage(tab);
    }

    /// <summary>Returns a flag indicating whether the current user can copy the current page.</summary>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanCopyPage()
    {
        return CanCopyPage(TabController.CurrentPage);
    }

    /// <summary>Returns a flag indicating whether the current user can copy a page.</summary>
    /// <param name="tab">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanCopyPage(TabInfo tab)
    {
        return Provider.CanCopyPage(tab);
    }

    /// <summary>Returns a flag indicating whether the current user can delete the current page.</summary>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanDeletePage()
    {
        return CanDeletePage(TabController.CurrentPage);
    }

    /// <summary>Returns a flag indicating whether the current user can delete a page.</summary>
    /// <param name="tab">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanDeletePage(TabInfo tab)
    {
        return Provider.CanDeletePage(tab);
    }

    /// <summary>Returns a flag indicating whether the current user can export the current page.</summary>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanExportPage()
    {
        return CanExportPage(TabController.CurrentPage);
    }

    /// <summary>Returns a flag indicating whether the current user can export a page.</summary>
    /// <param name="tab">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanExportPage(TabInfo tab)
    {
        return Provider.CanExportPage(tab);
    }

    /// <summary>Returns a flag indicating whether the current user can import the current page.</summary>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanImportPage()
    {
        return CanImportPage(TabController.CurrentPage);
    }

    /// <summary>Returns a flag indicating whether the current user can import a page.</summary>
    /// <param name="tab">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanImportPage(TabInfo tab)
    {
        return Provider.CanImportPage(tab);
    }

    /// <summary>Returns a flag indicating whether the current user can manage the current page's settings.</summary>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanManagePage()
    {
        return CanManagePage(TabController.CurrentPage);
    }

    /// <summary>Returns a flag indicating whether the current user can manage a page's settings.</summary>
    /// <param name="tab">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanManagePage(TabInfo tab)
    {
        return Provider.CanManagePage(tab);
    }

    /// <summary>Returns a flag indicating whether the current user can see the current page in a navigation object.</summary>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanNavigateToPage()
    {
        return CanNavigateToPage(TabController.CurrentPage);
    }

    /// <summary>Returns a flag indicating whether the current user can see a page in a navigation object.</summary>
    /// <param name="tab">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanNavigateToPage(TabInfo tab)
    {
        return Provider.CanNavigateToPage(tab);
    }

    /// <summary>Returns a flag indicating whether the current user can view the current page.</summary>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanViewPage()
    {
        return CanViewPage(TabController.CurrentPage);
    }

    /// <summary>Returns a flag indicating whether the current user can view a page.</summary>
    /// <param name="tab">The page.</param>
    /// <returns>A flag indicating whether the user has permission.</returns>
    public static bool CanViewPage(TabInfo tab)
    {
        return Provider.CanViewPage(tab);
    }

    /// <summary>DeleteTabPermissionsByUser deletes a user's Tab Permissions in the Database.</summary>
    /// <param name="user">The user.</param>
    public static void DeleteTabPermissionsByUser(UserInfo user)
    {
        Provider.DeleteTabPermissionsByUser(user);
        EventLogController.Instance.AddLog(user, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.TABPERMISSION_DELETED);
        DataCache.ClearTabPermissionsCache(user.PortalID);
    }

    /// <summary>GetTabPermissions gets a TabPermissionCollection.</summary>
    /// <param name="tabId">The ID of the tab.</param>
    /// <param name="portalId">The ID of the portal.</param>
    /// <returns>A <see cref="TabPermissionCollection"/> instance with the tab's permissions, or an empty <see cref="TabPermissionCollection"/> if it can't be found.</returns>
    public static TabPermissionCollection GetTabPermissions(int tabId, int portalId)
    {
        return Provider.GetTabPermissions(tabId, portalId);
    }

    /// <summary>HasTabPermission checks whether the current user has a specific Tab Permission.</summary>
    /// <remarks>If you pass in a comma delimited list of permissions (eg "ADD,DELETE"), this will return true if the user has any one of the permissions.</remarks>
    /// <param name="permissionKey">The Permission to check.</param>
    /// <returns><see langword="true"/> if the current user has the given permission, otherwise <see langword="false"/>.</returns>
    public static bool HasTabPermission(string permissionKey)
    {
        return HasTabPermission(PortalController.Instance.GetCurrentPortalSettings().ActiveTab.TabPermissions, permissionKey);
    }

    /// <summary>HasTabPermission checks whether the current user has a specific Tab Permission.</summary>
    /// <remarks>If you pass in a comma delimited list of permissions (eg "ADD,DELETE"), this will return true if the user has any one of the permissions.</remarks>
    /// <param name="tabPermissions">The Permissions for the Tab.</param>
    /// <param name="permissionKey">The Permission(s) to check.</param>
    /// <returns><see langword="true"/> if the current user has the given permission, otherwise <see langword="false"/>.</returns>
    public static bool HasTabPermission(TabPermissionCollection tabPermissions, string permissionKey)
    {
        return Provider.HasTabPermission(tabPermissions, permissionKey);
    }

    /// <summary>SaveTabPermissions saves a Tab's permissions.</summary>
    /// <param name="tab">The Tab to update.</param>
    public static void SaveTabPermissions(TabInfo tab)
    {
        Provider.SaveTabPermissions(tab);
        EventLogController.Instance.AddLog(tab, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.TABPERMISSION_UPDATED);
        DataCache.ClearTabPermissionsCache(tab.PortalID);
    }

    private static void ClearPermissionCache(int tabId)
    {
        var objTab = TabController.Instance.GetTab(tabId, Null.NullInteger, false);
        DataCache.ClearTabPermissionsCache(objTab.PortalID);
    }
}
