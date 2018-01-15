#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Portals;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFormSkinsItem : DnnFormItemBase
    {
        //private DropDownList _containerCombo;
        private DnnComboBox _containerCombo;
        private object _containerValue;
        //private DropDownList _skinCombo;
        private DnnComboBox _skinCombo;
        private object _skinValue;

        public string ContainerDataField { get; set; }

        public bool IncludePortalSkins { get; set; }

        public int PortalId { get; set; }

        public string SkinDataField { get; set; }

        private void ContainerIndexChanged(object sender, EventArgs e)
        {
            UpdateDataSource(_containerValue, _containerCombo.SelectedValue, ContainerDataField);
        }

        private void SkinIndexChanged(object sender, EventArgs e)
        {
            UpdateDataSource(_skinValue, _skinCombo.SelectedValue, SkinDataField);
        }

        private Dictionary<string, string> GetSkins(string skinRoot)
        {
            // load host skins
            var skins = SkinController.GetSkins(null, skinRoot, SkinScope.Host).ToDictionary(skin => skin.Key, skin => skin.Value);

            if (IncludePortalSkins)
            {
                // load portal skins
                var portal = PortalController.Instance.GetPortal(PortalId);

                foreach (var skin in SkinController.GetSkins(portal, skinRoot, SkinScope.Site))
                {
                    skins.Add(skin.Key, skin.Value);
                }
            }
            return skins;
        }

        protected override WebControl CreateControlInternal(Control container)
        {
            var panel = new Panel();

            container.Controls.Add(panel);

            var skinLabel = new Label { Text = LocalizeString("Skin") };
            skinLabel.CssClass += "dnnFormSkinLabel";
            panel.Controls.Add(skinLabel);

            //_skinCombo = new DropDownList { ID = ID + "_SkinComboBox" };
            _skinCombo = new DnnComboBox { ID = ID + "_SkinComboBox" };
            _skinCombo.CssClass += "dnnFormSkinInput"; 
            _skinCombo.SelectedIndexChanged += SkinIndexChanged;
            panel.Controls.Add(_skinCombo);

            DnnFormComboBoxItem.BindListInternal(_skinCombo, _skinValue, GetSkins(SkinController.RootSkin), "Key", "Value");

            var containerLabel = new Label { Text = LocalizeString("Container") };
            containerLabel.CssClass += "dnnFormSkinLabel";
            panel.Controls.Add(containerLabel);

            //_containerCombo = new DropDownList { ID = ID + "_ContainerComboBox" };
            _containerCombo = new DnnComboBox { ID = ID + "_ContainerComboBox" };
            _containerCombo.CssClass += "dnnFormSkinInput";
            _containerCombo.SelectedIndexChanged += ContainerIndexChanged;
            panel.Controls.Add(_containerCombo);

            DnnFormComboBoxItem.BindListInternal(_containerCombo, _containerValue, GetSkins(SkinController.RootContainer), "Key", "Value");

            return panel;
        }

        protected override void DataBindInternal()
        {
            DataBindInternal(SkinDataField, ref _skinValue);

            DataBindInternal(ContainerDataField, ref _containerValue);

            Value = new Pair {First = _skinValue, Second = _containerValue};
        }

        protected override void LoadControlState(object state)
        {
            base.LoadControlState(state);
            var pair = Value as Pair;
            if (pair != null)
            {
                _skinValue = pair.First;
                _containerValue = pair.Second;
            }
        }
    }
}