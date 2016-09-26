using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dnn.EditBar.Library;
using Dnn.EditBar.Library.Items;
using Dnn.EditBar.UI.Helpers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs.TabVersions;
using DotNetNuke.Security.Permissions;

namespace Dnn.EditBar.UI.Items
{
    public class PreviewMenu : BaseMenuItem
    {
        public override string Name { get; } = "Preview";

        public override string Text { get; } = "Preview";

        public override string Parent { get;} = Constants.LeftMenu;

        public override int Order { get; } = 15;

        public override string Loader { get; } = "Preview";

        public override bool CustomLayout { get; } = true;

        public override IDictionary<string, object> Settings
        {
            get
            {
                var settings = new Dictionary<string, object>();

                var portalSettings = PortalSettings.Current;
                if (portalSettings != null)
                {
                    settings.Add("tabVersionQueryStringParameter", TabVersionSettings.Instance.GetTabVersionQueryStringParameter(portalSettings.PortalId));
                }

                return settings;
            }
        }
    }
}
