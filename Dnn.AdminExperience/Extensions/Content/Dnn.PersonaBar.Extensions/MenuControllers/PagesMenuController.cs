// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using Dnn.PersonaBar.Library.Permissions;
using Dnn.PersonaBar.Pages.Components.Security;
using DotNetNuke.Application;
using DotNetNuke.Entities.Portals;

namespace Dnn.PersonaBar.Pages.MenuControllers
{
    public class PagesMenuController : IMenuItemController
    {
        private readonly ISecurityService _securityService;
        public PagesMenuController()
        {
            _securityService = SecurityService.Instance;
        }

        public void UpdateParameters(MenuItem menuItem)
        {

        }

        public bool Visible(MenuItem menuItem)
        {
            return _securityService.IsVisible(menuItem);
        }

        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            var settings = new Dictionary<string, object>
            {
                {"canSeePagesList", _securityService.CanViewPageList(menuItem.MenuId)},
                {"portalName", PortalSettings.Current.PortalName},
                {"currentPagePermissions", _securityService.GetCurrentPagePermissions()},
                {"currentPageName", PortalSettings.Current?.ActiveTab?.TabName},
                {"productSKU", DotNetNukeContext.Current.Application.SKU},
                {"isAdmin", _securityService.IsPageAdminUser()},
                {"currentParentHasChildren", PortalSettings.Current?.ActiveTab?.HasChildren},
                {"isAdminHostSystemPage", _securityService.IsAdminHostSystemPage() }
            };

            return settings;
        }
    }
}
