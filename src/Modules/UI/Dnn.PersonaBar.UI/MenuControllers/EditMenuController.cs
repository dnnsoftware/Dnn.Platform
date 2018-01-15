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

using System.Collections.Generic;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Web.Api.Internal;

namespace Dnn.PersonaBar.UI.MenuControllers
{
    [DnnPageEditor]
    public class EditMenuController : IMenuItemController
    {
        public void UpdateParameters(MenuItem menuItem)
        {
        }

        public bool Visible(MenuItem menuItem)
        {
            return IsPageAdmin() || IsModuleAdmin();
        }

        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            return null;
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
            return TabPermissionController.CanAddContentToPage() 
                    || TabPermissionController.CanAddPage() 
                    || TabPermissionController.CanAdminPage() 
                    || TabPermissionController.CanCopyPage() 
                    || TabPermissionController.CanDeletePage() 
                    || TabPermissionController.CanExportPage() 
                    || TabPermissionController.CanImportPage() 
                    || TabPermissionController.CanManagePage();
        }
    }
}
