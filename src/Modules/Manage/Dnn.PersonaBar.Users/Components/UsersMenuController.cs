using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components
{
    public class UsersMenuController : IMenuItemController
    {
        private PortalSettings PortalSettings => PortalController.Instance.GetCurrentPortalSettings();

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
            settings.Add("isAdmin", UserController.Instance.GetCurrentUserInfo().Roles.Contains(PortalSettings.AdministratorRoleName));
            settings.Add("userId", UserController.Instance.GetCurrentUserInfo().UserID);
            return settings;
        }
    }
}