using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;

namespace Dnn.EditBar.UI.Helpers
{
    public static class PagesHelper
    {
        public static bool IsPrivatePage()
        {
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null)
            {
                return false;
            }

            var viewRoles = TabPermissionController.GetTabPermissions(portalSettings.ActiveTab.TabID, portalSettings.PortalId).ToString("VIEW");
            var roles = viewRoles.Split(';');
            var numRoles = roles.Count(r => !r.StartsWith("!") && !String.IsNullOrEmpty(r));

            //True if roles allowed to see the image are exactly the implicit roles
            return numRoles == TabPermissionController.ImplicitRoles(portalSettings.PortalId).Count();
        }
    }
}
