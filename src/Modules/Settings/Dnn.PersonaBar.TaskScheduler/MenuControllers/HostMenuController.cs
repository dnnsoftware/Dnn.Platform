using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.TaskScheduler.MenuControllers
{
    public class HostMenuController : IMenuItemController
    {
        public void UpdateParameters(MenuItem menuItem)
        {
            
        }

        public bool Visible(MenuItem menuItem)
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            return user.IsSuperUser;
        }

        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            return null;
        }
    }
}
