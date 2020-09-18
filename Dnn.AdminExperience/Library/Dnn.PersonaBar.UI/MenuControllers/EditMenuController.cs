// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.MenuControllers
{
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

    [DnnPageEditor]
    public class EditMenuController : IMenuItemController
    {
        public void UpdateParameters(MenuItem menuItem)
        {
        }

        public bool Visible(MenuItem menuItem)
        {
            return this.IsPageAdmin() || this.IsModuleAdmin();
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
