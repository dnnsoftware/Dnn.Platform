using System.Collections.Generic;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;

namespace Dnn.PersonaBar.Security.MenuControllers
{
    public class SecurityMenuController : IMenuItemController
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
            return settings;
        }
    }
}