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
    public class EditPageMenu : BaseMenuItem
    {
        public override string Name { get; } = "EditPage";

        public override string Text
        {
            get
            {
                return InViewMode ? "Edit" : "Close";
            }
        }

        public override string CssClass
        {
            get
            {
                return InViewMode ? "main-button" : string.Empty;
            }
        }

        public override string Template { get; } = "";

        public override string Parent { get;} = Constants.RightMenu;

        public override string Loader { get; } = "EditPage";

        public override int Order { get; } = 100;

        public override IDictionary<string, object> Settings
        {
            get
            {
                var settings = new Dictionary<string, object>();

                var portalSettings = PortalSettings.Current;
                settings.Add("userMode", portalSettings?.UserMode.ToString());

                return settings;
            }
        }

        public bool InViewMode
        {
            get
            {
                var portalSettings = PortalSettings.Current;
                return portalSettings?.UserMode == PortalSettings.Mode.View;
            }
        }
    }
}
