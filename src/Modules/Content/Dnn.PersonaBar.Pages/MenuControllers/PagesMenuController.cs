#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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

using System.Collections.Generic;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using Newtonsoft.Json.Linq;

namespace Dnn.PersonaBar.Pages.MenuControllers
{
    public class PagesMenuController : IMenuItemController
    {
        public void UpdateParameters(MenuItem menuItem)
        {
            
        }

        public bool Visible(MenuItem menuItem)
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            var isSuperUser = user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
            if (isSuperUser)
            {
                return true;
            }

            return IsPageAdmin() || IsModuleAdmin();
        }

        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            var isSuperUser = user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
            var settings = new Dictionary<string, object>
            {
                {"isSuperUser", isSuperUser},
                {"portalName", PortalSettings.Current.PortalName},
                {"currentPagePermissions", GetCurrentPagePermissions()}
            };

            return settings;
        }

        private JObject GetCurrentPagePermissions()
        {
            var permissions = new JObject
            {
                {"addContentToPage", TabPermissionController.CanAddContentToPage()},
                {"addPage", TabPermissionController.CanAddPage()},
                {"adminPage", TabPermissionController.CanAdminPage()},
                {"copyPage", TabPermissionController.CanCopyPage()},
                {"deletePage", TabPermissionController.CanDeletePage()},
                {"exportPage", TabPermissionController.CanExportPage()},
                {"importPage", TabPermissionController.CanImportPage()},
                {"managePage", TabPermissionController.CanManagePage()}
            };

            return permissions;
        }

        private bool IsModuleAdmin()
        {
            bool moduleAdmin = Null.NullBoolean;
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            foreach (ModuleInfo module in TabController.CurrentPage.Modules)
            {
                if (!module.IsDeleted)
                {
                    bool hasEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString, module);
                    if (hasEditPermissions)
                    {
                        moduleAdmin = true;
                        break;
                    }
                }
            }
            return portalSettings.ControlPanelSecurity == PortalSettings.ControlPanelPermission.ModuleEditor && moduleAdmin;
        }

        private bool IsPageAdmin()
        {
            return //TabPermissionController.CanAddContentToPage() ||
                    TabPermissionController.CanAddPage()
                    || TabPermissionController.CanAdminPage()
                    || TabPermissionController.CanCopyPage()
                    || TabPermissionController.CanDeletePage()
                    || TabPermissionController.CanExportPage()
                    || TabPermissionController.CanImportPage()
                    || TabPermissionController.CanManagePage();
        }
    }
}
