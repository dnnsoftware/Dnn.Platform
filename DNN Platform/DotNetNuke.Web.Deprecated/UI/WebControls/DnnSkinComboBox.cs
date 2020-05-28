// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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

namespace DotNetNuke.Web.UI.WebControls
{
    [ToolboxData("<{0}:DnnSkinComboBox runat='server'></{0}:DnnSkinComboBox>")]
    public class DnnSkinComboBox : DnnComboBox
    {
        #region Public Properties

        public int PortalId { get; set; }

        public string RootPath { get; set; }

        public SkinScope Scope { get; set; }

        public bool IncludeNoneSpecificItem { get; set; }

        public string NoneSpecificText { get; set; }

        #endregion

        #region Private Properties

        private PortalInfo Portal
        {
            get { return PortalId == Null.NullInteger ? null : PortalController.Instance.GetPortal(PortalId); }
        }

        #endregion

        #region Constructors

        public DnnSkinComboBox()
        {
            PortalId = Null.NullInteger;
        }

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DataTextField = "Key";
            DataValueField = "Value";

            if (!Page.IsPostBack && !string.IsNullOrEmpty(RootPath))
            {
                DataSource = SkinController.GetSkins(Portal, RootPath, Scope)
                                           .ToDictionary(skin => skin.Key, skin => skin.Value);
                DataBind(SelectedValue);

                if (IncludeNoneSpecificItem)
                {
                    InsertItem(0, NoneSpecificText, string.Empty);
                }
            }

            AttachEvents();
        }

        protected override void PerformDataBinding(IEnumerable dataSource)
        {
            //do not select item during data binding, item will select later
            var selectedValue = SelectedValue;
            SelectedValue = string.Empty;

            base.PerformDataBinding(dataSource);

            SelectedValue = selectedValue;
        }

        #endregion

        #region Private Methods

        private void AttachEvents()
        {
            if (!UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                return;
            }

            Attributes.Add("PortalPath", Portal != null ? Portal.HomeDirectory : string.Empty);
            Attributes.Add("HostPath", Globals.HostPath);

            OnClientSelectedIndexChanged = "selectedIndexChangedMethod";
            var indexChangedMethod = @"function selectedIndexChangedMethod(sender, eventArgs){
    var value = eventArgs.get_item().get_value();
    value = value.replace('[L]', sender.get_attributes().getAttribute('PortalPath'));
    value = value.replace('[G]', sender.get_attributes().getAttribute('HostPath'));
    sender.get_inputDomElement().title = value;
}";
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "OnClientSelectedIndexChanged", indexChangedMethod, true);

            foreach (RadComboBoxItem item in Items)
            {
                if (string.IsNullOrEmpty(item.Value))
                {
                    continue;
                }

                var tooltip = item.Value.Replace("[G]", Globals.HostPath);
                if (Portal != null)
                {
                    tooltip = tooltip.Replace("[L]", Portal.HomeDirectory);
                }

                item.ToolTip = tooltip;
                if (item.Value.Equals(SelectedValue))
                {
                    ToolTip = tooltip;
                }
            }
        }

        #endregion
    }
}
