// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.MenuControllers
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Web.Api.Internal;

    /// <summary>An <see cref="IMenuItemController"/> for the edit menu.</summary>
    [DnnPageEditor]
    public class EditMenuController : IMenuItemController
    {
        /// <inheritdoc/>
        public void UpdateParameters(MenuItem menuItem)
        {
        }

        /// <inheritdoc/>
        public bool Visible(MenuItem menuItem)
        {
            return IsPageAdmin() || IsModuleAdmin();
        }

        /// <inheritdoc/>
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            return null;
        }

        private static bool IsModuleAdmin()
        {
            var moduleAdmin = TabController.CurrentPage.Modules.Cast<ModuleInfo>()
                .Where(module => !module.IsDeleted)
                .Any(module => ModulePermissionController.HasModuleAccess(
                    SecurityAccessLevel.Edit,
                    Null.NullString,
                    module));

            return PortalSettings.Current.ControlPanelSecurity == PortalSettings.ControlPanelPermission.ModuleEditor && moduleAdmin;
        }

        private static bool IsPageAdmin()
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
