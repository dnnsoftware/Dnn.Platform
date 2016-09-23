using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dnn.EditBar.Library;
using Dnn.EditBar.Library.Items;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;

namespace Dnn.EditBar.UI.Items
{
    public class EditPageMenu : BaseMenuItem
    {
        public override string Name { get; } = "AddModule";

        public override string Text
        {
            get
            {
                return "Add Module";
            }
        }

        public override string CssClass
        {
            get
            {
                return string.Empty;
            }
        }

        public override string Template { get; } = "";

        public override string Parent { get;} = Constants.LeftMenu;

        public override string Loader { get; } = "AddModule";
    }
}
