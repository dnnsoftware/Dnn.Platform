// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Extensions.MenuControllers
{
    public class ExtensionMenuController : IMenuItemController
    {
        protected INavigationManager NavigationManager { get; }
        public ExtensionMenuController()
        {
            NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public void UpdateParameters(MenuItem menuItem)
        {
        }

        public bool Visible(MenuItem menuItem)
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            return user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
        }

        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            var settings = new Dictionary<string, object>();
            settings.Add("portalId", PortalSettings.Current.PortalId);
            settings.Add("installUrl", NavigationManager.NavigateURL(PortalSettings.Current.ActiveTab.TabID, PortalSettings.Current, "Install", "popUp=true"));
            return settings;
        }
    }
}
