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
    [Serializable]
    public class ExitEditModeMenu : BaseMenuItem
    {
        public override string Name { get; } = "ExitEditMode";

        public override string Text => "ExitEditMode";

        public override string CssClass => string.Empty;

        public override string Template { get; } = "";

        public override string Parent { get;} = Constants.RightMenu;

        public override string Loader { get; } = "ExitEditMode";

        public override int Order { get; } = 100;

        public override bool Visible()
        {
            return PortalSettings.Current?.UserMode == PortalSettings.Mode.Edit;
        }
    }
}
