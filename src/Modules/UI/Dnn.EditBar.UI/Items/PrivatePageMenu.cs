using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dnn.EditBar.Library;
using Dnn.EditBar.Library.Items;
using Dnn.EditBar.UI.Helpers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;

namespace Dnn.EditBar.UI.Items
{
    public class PrivatePageMenu : BaseMenuItem
    {
        public override string Name { get; } = "PrivatePage";

        public override string Template { get; } = "<div class=\"private-page\" title=\"" + LocalizationHelper.GetString("PrivatePage") + "\" />";

        public override string Parent { get; } = Constants.RightMenu;

        public override string Loader { get; } = "";

        public override bool Visible()
        {
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null)
            {
                return false;
            }

            var viewRoles = TabPermissionController.GetTabPermissions(portalSettings.ActiveTab.TabID, portalSettings.PortalId).ToString("VIEW");
            var roles = viewRoles.Split(';');
            var numRoles = roles.Count(r => !r.StartsWith("!") && !String.IsNullOrEmpty(r));

            return numRoles == TabPermissionController.ImplicitRoles(portalSettings.PortalId).Count();
        }
    }
}
