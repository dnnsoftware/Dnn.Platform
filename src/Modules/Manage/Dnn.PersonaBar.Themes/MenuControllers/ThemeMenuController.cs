using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.PersonaBar.Model;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Themes.MenuControllers
{
    public class ThemeMenuController : IMenuItemController
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
            return new Dictionary<string, object>
            {
                {"previewUrl", Globals.NavigateURL()},
                {"isHost", UserController.Instance.GetCurrentUserInfo().IsSuperUser}
            };
        }
    }
}