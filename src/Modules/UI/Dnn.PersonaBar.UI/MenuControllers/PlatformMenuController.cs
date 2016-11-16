using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Application;

namespace Dnn.PersonaBar.UI.MenuControllers
{
    public class PlatformMenuController : IMenuItemController
    {
        public void UpdateParameters(MenuItem menuItem)
        {
        }

        public bool Visible(MenuItem menuItem)
        {
            return DotNetNukeContext.Current.Application.SKU == "DNN";
        }

        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            return null;
        }
    }
}
