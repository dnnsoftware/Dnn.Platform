// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.UI.Skins;

    public class DnnFormSkinsItem : DnnFormItemBase
    {
        // private DropDownList _containerCombo;
        private DnnComboBox _containerCombo;
        private object _containerValue;

        // private DropDownList _skinCombo;
        private DnnComboBox _skinCombo;
        private object _skinValue;

        public string ContainerDataField { get; set; }

        public bool IncludePortalSkins { get; set; }

        public int PortalId { get; set; }

        public string SkinDataField { get; set; }

        protected override WebControl CreateControlInternal(Control container)
        {
            var panel = new Panel();

            container.Controls.Add(panel);

            var skinLabel = new Label { Text = this.LocalizeString("Skin") };
            skinLabel.CssClass += "dnnFormSkinLabel";
            panel.Controls.Add(skinLabel);

            // _skinCombo = new DropDownList { ID = ID + "_SkinComboBox" };
            this._skinCombo = new DnnComboBox { ID = this.ID + "_SkinComboBox" };
            this._skinCombo.CssClass += "dnnFormSkinInput";
            this._skinCombo.SelectedIndexChanged += this.SkinIndexChanged;
            panel.Controls.Add(this._skinCombo);

            DnnFormComboBoxItem.BindListInternal(this._skinCombo, this._skinValue, this.GetSkins(SkinController.RootSkin), "Key", "Value");

            var containerLabel = new Label { Text = this.LocalizeString("Container") };
            containerLabel.CssClass += "dnnFormSkinLabel";
            panel.Controls.Add(containerLabel);

            // _containerCombo = new DropDownList { ID = ID + "_ContainerComboBox" };
            this._containerCombo = new DnnComboBox { ID = this.ID + "_ContainerComboBox" };
            this._containerCombo.CssClass += "dnnFormSkinInput";
            this._containerCombo.SelectedIndexChanged += this.ContainerIndexChanged;
            panel.Controls.Add(this._containerCombo);

            DnnFormComboBoxItem.BindListInternal(this._containerCombo, this._containerValue, this.GetSkins(SkinController.RootContainer), "Key", "Value");

            return panel;
        }

        protected override void DataBindInternal()
        {
            this.DataBindInternal(this.SkinDataField, ref this._skinValue);

            this.DataBindInternal(this.ContainerDataField, ref this._containerValue);

            this.Value = new Pair { First = this._skinValue, Second = this._containerValue };
        }

        protected override void LoadControlState(object state)
        {
            base.LoadControlState(state);
            var pair = this.Value as Pair;
            if (pair != null)
            {
                this._skinValue = pair.First;
                this._containerValue = pair.Second;
            }
        }

        private void ContainerIndexChanged(object sender, EventArgs e)
        {
            this.UpdateDataSource(this._containerValue, this._containerCombo.SelectedValue, this.ContainerDataField);
        }

        private void SkinIndexChanged(object sender, EventArgs e)
        {
            this.UpdateDataSource(this._skinValue, this._skinCombo.SelectedValue, this.SkinDataField);
        }

        private Dictionary<string, string> GetSkins(string skinRoot)
        {
            // load host skins
            var skins = SkinController.GetSkins(null, skinRoot, SkinScope.Host).ToDictionary(skin => skin.Key, skin => skin.Value);

            if (this.IncludePortalSkins)
            {
                // load portal skins
                var portal = PortalController.Instance.GetPortal(this.PortalId);

                foreach (var skin in SkinController.GetSkins(portal, skinRoot, SkinScope.Site))
                {
                    skins.Add(skin.Key, skin.Value);
                }
            }

            return skins;
        }
    }
}
