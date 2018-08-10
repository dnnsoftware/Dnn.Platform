using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dnn.EditBar.Library;
using Dnn.EditBar.Library.Items;
using Dnn.EditBar.UI.Helpers;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;

namespace Dnn.EditBar.UI.Items
{
    [Serializable]
    public class PageSettingsMenu : BaseMenuItem
    {
        public override string Name { get; } = "PageSettings";

        public override string Text { get; } = "PageSettings";

        public override string Parent { get; } = Constants.LeftMenu;

        public override string Loader { get; } = "PageSettings";

        public override int Order { get; } = 15;

        public override bool Visible()
        {
            return PortalSettings.Current?.UserMode == PortalSettings.Mode.Edit
                && Host.ControlPanel.EndsWith("PersonaBarContainer.ascx", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
