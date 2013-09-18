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
    [ToolboxData("<{0}:DnnPageDropDownList runat='server'></{0}:DnnPageDropDownList>")]
    public class DnnPageDropDownList : DnnDropDownList
    {

        private readonly Lazy<TabController> _controller = new Lazy<TabController>(() => new TabController());

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            SelectItemDefaultText = Localization.GetString("DropDownList.SelectWebPageDefaultText", Localization.SharedResourceFile);
            Services.GetTreeMethod = "ItemListService/GetPages";
            Services.GetNodeDescendantsMethod = "ItemListService/GetPageDescendants";
            Services.SearchTreeMethod = "ItemListService/SearchPages";
            Services.GetTreeWithNodeMethod = "ItemListService/GetTreePathForPage";
            Services.ServiceRoot = "InternalServices";
            Services.SortTreeMethod = "ItemListService/SortPages";
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.AddCssClass("page");
            if (InternalPortalId.HasValue)
            {
                Services.Parameters.Add("PortalId", InternalPortalId.Value.ToString(CultureInfo.InvariantCulture));
			}

			Services.Parameters.Add("includeDisabled", IncludeDisabledTabs.ToString().ToLowerInvariant());
			Services.Parameters.Add("includeAllTypes", IncludeAllTabTypes.ToString().ToLowerInvariant());

            base.OnPreRender(e);
        }

		/// <summary>
		/// Whether include pages which are disabled.
		/// </summary>
		public bool IncludeDisabledTabs { get; set; }

		/// <summary>
		/// Whether include pages which tab type is not normal.
		/// </summary>
		public bool IncludeAllTabTypes { get; set; }

        public int PortalId
        {
            get
            {
                if (InternalPortalId.HasValue)
                {
                    return InternalPortalId.Value;
                }
                return PortalSettings.Current.ActiveTab.IsSuperTab ? -1 : PortalSettings.Current.PortalId;
            }
            set
            {
                InternalPortalId = value;
            }
        }

        private int? InternalPortalId
        {
            get
            {
                return ViewState.GetValue<int?>("PortalId", null);
            }
            set
            {
                ViewState.SetValue<int?>("PortalId", value, null);
            }
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
                return (pageId == Null.NullInteger) ? null : _controller.Value.GetTab(pageId, PortalId, false);
            }
            set
            {
                SelectedItem = (value != null) ? new ListItem() { Text = value.IndentedTabName, Value = value.TabID.ToString(CultureInfo.InvariantCulture) } : null;
            }
        }

    }
}
