// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.UI.Skins;
    using Telerik.Web.UI;

    [ToolboxData("<{0}:DnnSkinComboBox runat='server'></{0}:DnnSkinComboBox>")]
    public class DnnSkinComboBox : DnnComboBox
    {
        public DnnSkinComboBox()
        {
            this.PortalId = Null.NullInteger;
        }

        public int PortalId { get; set; }

        public string RootPath { get; set; }

        public SkinScope Scope { get; set; }

        public bool IncludeNoneSpecificItem { get; set; }

        public string NoneSpecificText { get; set; }

        private PortalInfo Portal
        {
            get { return this.PortalId == Null.NullInteger ? null : PortalController.Instance.GetPortal(this.PortalId); }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.DataTextField = "Key";
            this.DataValueField = "Value";

            if (!this.Page.IsPostBack && !string.IsNullOrEmpty(this.RootPath))
            {
                this.DataSource = SkinController.GetSkins(this.Portal, this.RootPath, this.Scope)
                                           .ToDictionary(skin => skin.Key, skin => skin.Value);
                this.DataBind(this.SelectedValue);

                if (this.IncludeNoneSpecificItem)
                {
                    this.InsertItem(0, this.NoneSpecificText, string.Empty);
                }
            }

            this.AttachEvents();
        }

        protected override void PerformDataBinding(IEnumerable dataSource)
        {
            // do not select item during data binding, item will select later
            var selectedValue = this.SelectedValue;
            this.SelectedValue = string.Empty;

            base.PerformDataBinding(dataSource);

            this.SelectedValue = selectedValue;
        }

        private void AttachEvents()
        {
            if (!UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                return;
            }

            this.Attributes.Add("PortalPath", this.Portal != null ? this.Portal.HomeDirectory : string.Empty);
            this.Attributes.Add("HostPath", Globals.HostPath);

            this.OnClientSelectedIndexChanged = "selectedIndexChangedMethod";
            var indexChangedMethod = @"function selectedIndexChangedMethod(sender, eventArgs){
    var value = eventArgs.get_item().get_value();
    value = value.replace('[L]', sender.get_attributes().getAttribute('PortalPath'));
    value = value.replace('[G]', sender.get_attributes().getAttribute('HostPath'));
    sender.get_inputDomElement().title = value;
}";
            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "OnClientSelectedIndexChanged", indexChangedMethod, true);

            foreach (RadComboBoxItem item in this.Items)
            {
                if (string.IsNullOrEmpty(item.Value))
                {
                    continue;
                }

                var tooltip = item.Value.Replace("[G]", Globals.HostPath);
                if (this.Portal != null)
                {
                    tooltip = tooltip.Replace("[L]", this.Portal.HomeDirectory);
                }

                item.ToolTip = tooltip;
                if (item.Value.Equals(this.SelectedValue))
                {
                    this.ToolTip = tooltip;
                }
            }
        }
    }
}
