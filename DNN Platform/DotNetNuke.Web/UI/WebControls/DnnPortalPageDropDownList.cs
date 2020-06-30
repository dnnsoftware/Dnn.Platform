// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
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

    [ToolboxData("<{0}:DnnPortalPageDropDownList runat='server'></{0}:DnnPortalPageDropDownList>")]
    public class DnnPortalPageDropDownList : DnnDropDownList
    {
        private readonly Lazy<int> _portalId = new Lazy<int>(() => PortalSettings.Current.ActiveTab.IsSuperTab ? -1 : PortalSettings.Current.PortalId);

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
                return (pageId == Null.NullInteger) ? null : TabController.Instance.GetTab(pageId, this._portalId.Value, false);
            }

            set
            {
                this.SelectedItem = (value != null) ? new ListItem() { Text = value.IndentedTabName, Value = value.TabID.ToString(CultureInfo.InvariantCulture) } : null;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.SelectItemDefaultText = Localization.GetString("DropDownList.SelectWebPageDefaultText", Localization.SharedResourceFile);
            this.Services.GetTreeMethod = "ItemListService/GetPagesInPortalGroup";
            this.Services.GetNodeDescendantsMethod = "ItemListService/GetPageDescendantsInPortalGroup";
            this.Services.SearchTreeMethod = "ItemListService/SearchPagesInPortalGroup";
            this.Services.GetTreeWithNodeMethod = "ItemListService/GetTreePathForPageInPortalGroup";
            this.Services.SortTreeMethod = "ItemListService/SortPagesInPortalGroup";
            this.Services.ServiceRoot = "InternalServices";
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.AddCssClass("page");
            base.OnPreRender(e);
        }
    }
}
