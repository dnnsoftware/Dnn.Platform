using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.PersonaBar.Model;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Installer;

namespace Dnn.PersonaBar.Extensions.Components
{
    public class ExtensionMenuController : IMenuItemController
    {
        public void UpdateParameters(MenuItem menuItem)
        {
        }

        public bool Visible(MenuItem menuItem)
        {
            return true;
        }

        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            var settings = new Dictionary<string, object>();
            settings.Add("isHost", UserController.Instance.GetCurrentUserInfo().IsSuperUser);
            settings.Add("installUrl", Globals.NavigateURL(PortalSettings.Current.ActiveTab.TabID, PortalSettings.Current, "Install", "popUp=true"));
            return settings;
        }
    }
}