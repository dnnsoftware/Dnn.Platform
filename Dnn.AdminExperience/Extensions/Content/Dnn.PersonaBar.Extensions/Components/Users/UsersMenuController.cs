using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;

namespace Dnn.PersonaBar.Users.Components
{
    public class UsersMenuController : IMenuItemController
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
            settings.Add("userId", UserController.Instance.GetCurrentUserInfo().UserID);
            settings.Add("requiresQuestionAndAnswer", MembershipProviderConfig.RequiresQuestionAndAnswer);
            settings.Add("dataConsentActive", PortalSettings.Current.DataConsentActive);
            return settings;
        }
    }
}