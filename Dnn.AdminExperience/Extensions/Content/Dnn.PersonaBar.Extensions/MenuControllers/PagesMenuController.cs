#region Copyright
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
