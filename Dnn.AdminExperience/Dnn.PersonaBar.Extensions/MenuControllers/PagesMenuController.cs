// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.MenuControllers
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Permissions;
    using Dnn.PersonaBar.Pages.Components.Security;
    using DotNetNuke.Application;
    using DotNetNuke.Entities.Portals;

    public class PagesMenuController : IMenuItemController
    {
        private readonly ISecurityService _securityService;

        public PagesMenuController()
        {
            this._securityService = SecurityService.Instance;
        }

        public void UpdateParameters(MenuItem menuItem)
        {

        }

        public bool Visible(MenuItem menuItem)
        {
            return this._securityService.IsVisible(menuItem);
        }

        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            var settings = new Dictionary<string, object>
            {
                {"canSeePagesList", this._securityService.CanViewPageList(menuItem.MenuId)},
                {"portalName", PortalSettings.Current.PortalName},
                {"currentPagePermissions", this._securityService.GetCurrentPagePermissions()},
                {"currentPageName", PortalSettings.Current?.ActiveTab?.TabName},
                {"productSKU", DotNetNukeContext.Current.Application.SKU},
                {"isAdmin", this._securityService.IsPageAdminUser()},
                {"currentParentHasChildren", PortalSettings.Current?.ActiveTab?.HasChildren},
                {"isAdminHostSystemPage", this._securityService.IsAdminHostSystemPage() }
            };

            return settings;
        }
    }
}
