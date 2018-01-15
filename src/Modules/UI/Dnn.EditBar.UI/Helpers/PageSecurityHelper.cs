#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;

namespace Dnn.EditBar.UI.Helpers
{
    /// <summary>
    /// This class offers some common methods to work with pages and their permission
    /// </summary>
    public class PageSecurityHelper
    {
        /// <summary>
        /// Checks if current user is page editor of the passed tab
        /// </summary>
        /// <param name="tabId">Tab ID</param>
        /// <param name="portalSettings">Portal Settings related with the portal which contains the tab</param>
        /// <returns>Returns true if current user has a edit permission on the page or is admin of some module in the page. Otherwise, returns false</returns>
        public static bool IsPageEditor(int tabId, PortalSettings portalSettings)
        {
            var tabPermissions = TabPermissionController.GetTabPermissions(tabId, portalSettings.PortalId);
            return TabPermissionController.HasTabPermission(tabPermissions, "EDIT,CONTENT,MANAGE") 
                || IsModuleAdmin(portalSettings, TabController.Instance.GetTab(tabId, portalSettings.PortalId));
        }
        
        /// <summary>
        /// Checks if current user is page admin of the currentpage
        /// </summary>
        /// <returns>Returns true if current user has any admin permission. Otherwise returns false</returns>
        public static bool IsPageAdmin()
        {
            return TabPermissionController.CanAddContentToPage() || TabPermissionController.CanAddPage() || TabPermissionController.CanAdminPage() || TabPermissionController.CanCopyPage() ||
                TabPermissionController.CanDeletePage() || TabPermissionController.CanExportPage() || TabPermissionController.CanImportPage() || TabPermissionController.CanManagePage();
        }

        /// <summary>
        /// Check if current user is Module admin of any module in the current page
        /// </summary>
        /// <param name="portalSettings">Portal Settings related with the portal which contains the tab</param>
        /// <returns>Returns true if current user has admin permission over some module in the current page. Otherwise, returns false</returns>
        public static bool IsModuleAdmin(PortalSettings portalSettings)
        {
            return IsModuleAdmin(portalSettings, portalSettings.ActiveTab);
        }

        /// <summary>
        /// Check if current user is Module admin of any module in the passed page
        /// </summary>
        /// <param name="portalSettings">Portal Settings related with the portal which contains the tab</param>
        /// <param name="tab">Tab to check</param>
        /// <returns>Returns true if current user has admin permission over some module in the passed page. Otherwise, returns false</returns>
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
