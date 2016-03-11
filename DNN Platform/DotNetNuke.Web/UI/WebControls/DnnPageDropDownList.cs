using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.UI.WebControls.Extensions;

namespace DotNetNuke.Web.UI.WebControls
{
    [ToolboxData("<{0}:DnnPageDropDownList runat='server'></{0}:DnnPageDropDownList>")]
    public class DnnPageDropDownList : DnnDropDownList
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Roles = new List<int>();

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
            Services.Parameters.Add("includeActive", IncludeActiveTab.ToString().ToLowerInvariant());
            Services.Parameters.Add("disabledNotSelectable", DisabledNotSelectable.ToString().ToLowerInvariant());
            Services.Parameters.Add("includeHostPages", (IncludeHostPages && UserController.Instance.GetCurrentUserInfo().IsSuperUser).ToString().ToLowerInvariant());
            Services.Parameters.Add("roles", string.Join(";", Roles.ToArray()));

            base.OnPreRender(e);

            //add the selected folder's level path so that it can expand to the selected node in client side.
            var selectedPage = SelectedPage;
            if (selectedPage != null && selectedPage.ParentId > Null.NullInteger)
            {
                var tabLevel = string.Empty;
                var parentTab = TabController.Instance.GetTab(selectedPage.ParentId, PortalId, false);
                while (parentTab != null)
                {
                    tabLevel = string.Format("{0},{1}", parentTab.TabID, tabLevel);
                    parentTab = TabController.Instance.GetTab(parentTab.ParentId, PortalId, false);
                }

                ExpandPath = tabLevel.TrimEnd(',');
            }
        }

        /// <summary>
        /// Whether disabled pages are not selectable
        /// Please note: IncludeDisabledTabs needs also be set to true to include disabled pages
        /// </summary>
        public bool DisabledNotSelectable { get; set; }

        /// <summary>
        /// Whether include active page.
        /// </summary>
        public bool IncludeActiveTab { get; set; }
        
        /// <summary>
		/// Whether include pages which are disabled.
		/// </summary>
		public bool IncludeDisabledTabs { get; set; }

		/// <summary>
		/// Whether include pages which tab type is not normal.
		/// </summary>
		public bool IncludeAllTabTypes { get; set; }

        /// <summary>
        /// Whether include Host Pages
        /// </summary>
        public bool IncludeHostPages { get; set; }

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
                return (pageId == Null.NullInteger) ? null : TabController.Instance.GetTab(pageId, PortalId, false);
            }
            set
            {
                SelectedItem = (value != null) ? new ListItem() { Text = value.IndentedTabName, Value = value.TabID.ToString(CultureInfo.InvariantCulture) } : null;
            }
        }

        /// <summary>
        /// Specific to only show tabs which have view permission on these roles.
        /// </summary>
        public IList<int> Roles { get; set; } 

    }
}
