// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Helpers
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;

    /// <summary>
    /// This class offers some common methods to work with pages and their permission.
    /// </summary>
    public class PageSecurityHelper
    {
        /// <summary>
        /// Checks if current user is page editor of the passed tab.
        /// </summary>
        /// <param name="tabId">Tab ID.</param>
        /// <param name="portalSettings">Portal Settings related with the portal which contains the tab.</param>
        /// <returns>Returns true if current user has a edit permission on the page or is admin of some module in the page. Otherwise, returns false.</returns>
        public static bool IsPageEditor(int tabId, PortalSettings portalSettings)
        {
            var tabPermissions = TabPermissionController.GetTabPermissions(tabId, portalSettings.PortalId);
            return TabPermissionController.HasTabPermission(tabPermissions, "EDIT,CONTENT,MANAGE")
                || IsModuleAdmin(portalSettings, TabController.Instance.GetTab(tabId, portalSettings.PortalId));
        }

        /// <summary>
        /// Checks if current user is page admin of the currentpage.
        /// </summary>
        /// <returns>Returns true if current user has any admin permission. Otherwise returns false.</returns>
        public static bool IsPageAdmin()
        {
            return TabPermissionController.CanAddContentToPage() || TabPermissionController.CanAddPage() || TabPermissionController.CanAdminPage() || TabPermissionController.CanCopyPage() ||
                TabPermissionController.CanDeletePage() || TabPermissionController.CanExportPage() || TabPermissionController.CanImportPage() || TabPermissionController.CanManagePage();
        }

        /// <summary>
        /// Check if current user is Module admin of any module in the current page.
        /// </summary>
        /// <param name="portalSettings">Portal Settings related with the portal which contains the tab.</param>
        /// <returns>Returns true if current user has admin permission over some module in the current page. Otherwise, returns false.</returns>
        public static bool IsModuleAdmin(PortalSettings portalSettings)
        {
            return IsModuleAdmin(portalSettings, portalSettings.ActiveTab);
        }

        /// <summary>
        /// Check if current user is Module admin of any module in the passed page.
        /// </summary>
        /// <param name="portalSettings">Portal Settings related with the portal which contains the tab.</param>
        /// <param name="tab">Tab to check.</param>
        /// <returns>Returns true if current user has admin permission over some module in the passed page. Otherwise, returns false.</returns>
        public static bool IsModuleAdmin(PortalSettings portalSettings, TabInfo tab)
        {
            var isModuleAdmin = false;
            foreach (ModuleInfo objModule in tab.Modules)
            {
                if (!objModule.IsDeleted)
                {
                    bool blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString, objModule);
                    if (blnHasModuleEditPermissions)
                    {
                        isModuleAdmin = true;
                        break;
                    }
                }
            }

            return portalSettings.ControlPanelSecurity == PortalSettings.ControlPanelPermission.ModuleEditor && isModuleAdmin;
        }
    }
}
