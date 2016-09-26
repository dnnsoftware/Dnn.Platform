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

        public override string Text { get; } = "PrivatePage";

        public override string Parent { get; } = Constants.RightMenu;

        public override string Loader { get; } = "PrivatePage";

        public override int Order { get; } = 100;

        public override bool Visible()
        {
            return PagesHelper.IsPrivatePage();
        }
    }
}
