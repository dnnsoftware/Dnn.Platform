using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.UI.WebControls.Extensions;

namespace DotNetNuke.Web.UI.WebControls
{
    [ToolboxData("<{0}:DnnPortalPageDropDownList runat='server'></{0}:DnnPortalPageDropDownList>")]
    public class DnnPortalPageDropDownList : DnnDropDownList
    {
        private readonly Lazy<int> _portalId = new Lazy<int>(() => PortalSettings.Current.ActiveTab.IsSuperTab ? -1 : PortalSettings.Current.PortalId);

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            SelectItemDefaultText = Localization.GetString("DropDownList.SelectWebPageDefaultText", Localization.SharedResourceFile);
            Services.GetTreeMethod = "ItemListService/GetPagesInPortalGroup";
            Services.GetNodeDescendantsMethod = "ItemListService/GetPageDescendantsInPortalGroup";
            Services.SearchTreeMethod = "ItemListService/SearchPagesInPortalGroup";
            Services.GetTreeWithNodeMethod = "ItemListService/GetTreePathForPageInPortalGroup";
            Services.SortTreeMethod = "ItemListService/SortPagesInPortalGroup";
            Services.ServiceRoot = "InternalServices";
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.AddCssClass("page");
            base.OnPreRender(e);
        }

        /// <summary>
        /// Gets the selected Page in the control, or selects the Page in the control.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TabInfo SelectedPage
        {
            get
            {
                var pageId = SelectedItemValueAsInt;
                return (pageId == Null.NullInteger) ? null : TabController.Instance.GetTab(pageId, _portalId.Value, false);
            }
            set
            {
                SelectedItem = (value != null) ? new ListItem() { Text = value.IndentedTabName, Value = value.TabID.ToString(CultureInfo.InvariantCulture) } : null;
            }
        }

    }
}
