// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
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

    [ToolboxData("<{0}:DnnPageDropDownList runat='server'></{0}:DnnPageDropDownList>")]
    public class DnnPageDropDownList : DnnDropDownList
    {
        /// <summary>
        /// Gets or sets a value indicating whether whether disabled pages are not selectable
        /// Please note: IncludeDisabledTabs needs also be set to true to include disabled pages.
        /// </summary>
        public bool DisabledNotSelectable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether include active page.
        /// </summary>
        public bool IncludeActiveTab { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether include pages which are disabled.
        /// </summary>
        public bool IncludeDisabledTabs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether include pages which tab type is not normal.
        /// </summary>
        public bool IncludeAllTabTypes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether include Host Pages.
        /// </summary>
        public bool IncludeHostPages { get; set; }

        public int PortalId
        {
            get
            {
                if (this.InternalPortalId.HasValue)
                {
                    return this.InternalPortalId.Value;
                }

                return PortalSettings.Current.ActiveTab.IsSuperTab ? -1 : PortalSettings.Current.PortalId;
            }

            set
            {
                this.InternalPortalId = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected Page in the control, or selects the Page in the control.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TabInfo SelectedPage
        {
            get
            {
                var pageId = this.SelectedItemValueAsInt;
                return (pageId == Null.NullInteger) ? null : TabController.Instance.GetTab(pageId, this.PortalId, false);
            }

            set
            {
                this.SelectedItem = (value != null) ? new ListItem() { Text = value.IndentedTabName, Value = value.TabID.ToString(CultureInfo.InvariantCulture) } : null;
            }
        }

        /// <summary>
        /// Gets or sets specific to only show tabs which have view permission on these roles.
        /// </summary>
        public IList<int> Roles { get; set; }

        private int? InternalPortalId
        {
            get
            {
                return this.ViewState.GetValue<int?>("PortalId", null);
            }

            set
            {
                this.ViewState.SetValue<int?>("PortalId", value, null);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.Roles = new List<int>();

            this.SelectItemDefaultText = Localization.GetString("DropDownList.SelectWebPageDefaultText", Localization.SharedResourceFile);
            this.Services.GetTreeMethod = "ItemListService/GetPages";
            this.Services.GetNodeDescendantsMethod = "ItemListService/GetPageDescendants";
            this.Services.SearchTreeMethod = "ItemListService/SearchPages";
            this.Services.GetTreeWithNodeMethod = "ItemListService/GetTreePathForPage";
            this.Services.ServiceRoot = "InternalServices";
            this.Services.SortTreeMethod = "ItemListService/SortPages";
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.AddCssClass("page");
            if (this.InternalPortalId.HasValue)
            {
                this.Services.Parameters.Add("PortalId", this.InternalPortalId.Value.ToString(CultureInfo.InvariantCulture));
            }

            this.Services.Parameters.Add("includeDisabled", this.IncludeDisabledTabs.ToString().ToLowerInvariant());
            this.Services.Parameters.Add("includeAllTypes", this.IncludeAllTabTypes.ToString().ToLowerInvariant());
            this.Services.Parameters.Add("includeActive", this.IncludeActiveTab.ToString().ToLowerInvariant());
            this.Services.Parameters.Add("disabledNotSelectable", this.DisabledNotSelectable.ToString().ToLowerInvariant());
            this.Services.Parameters.Add("includeHostPages", (this.IncludeHostPages && UserController.Instance.GetCurrentUserInfo().IsSuperUser).ToString().ToLowerInvariant());
            this.Services.Parameters.Add("roles", string.Join(";", this.Roles.ToArray()));

            base.OnPreRender(e);

            // add the selected folder's level path so that it can expand to the selected node in client side.
            var selectedPage = this.SelectedPage;
            if (selectedPage != null && selectedPage.ParentId > Null.NullInteger)
            {
                var tabLevel = string.Empty;
                var parentTab = TabController.Instance.GetTab(selectedPage.ParentId, this.PortalId, false);
                while (parentTab != null)
                {
                    tabLevel = string.Format("{0},{1}", parentTab.TabID, tabLevel);
                    parentTab = TabController.Instance.GetTab(parentTab.ParentId, this.PortalId, false);
                }

                this.ExpandPath = tabLevel.TrimEnd(',');
            }
        }
    }
}
