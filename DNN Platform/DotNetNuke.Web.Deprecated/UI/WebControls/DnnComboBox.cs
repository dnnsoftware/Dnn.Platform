// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;

    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using Telerik.Web.UI;

    public class DnnComboBox : RadComboBox
    {
        public void AddItem(string text, string value)
        {
            this.Items.Add(new DnnComboBoxItem(text, value));
        }

        public void InsertItem(int index, string text, string value)
        {
            this.Items.Insert(index, new DnnComboBoxItem(text, value));
        }

        public void DataBind(string initialValue)
        {
            this.DataBind(initialValue, false);
        }

        public void DataBind(string initial, bool findByText)
        {
            this.DataBind();

            this.Select(initial, findByText);
        }

        public void Select(string initial, bool findByText)
        {
            if (findByText)
            {
                if (this.FindItemByText(initial, true) != null)
                {
                    this.FindItemByText(initial, true).Selected = true;
                }
            }
            else
            {
                if (this.FindItemByValue(initial, true) != null)
                {
                    this.FindItemByValue(initial, true).Selected = true;
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.EnableEmbeddedBaseStylesheet = false;
            this.OnClientLoad = "$.dnnComboBoxLoaded";
            this.OnClientFocus = "$.dnnComboBoxHack";
            this.OnClientDropDownOpened = "$.dnnComboBoxScroll";
            this.OnClientItemsRequested = "$.dnnComboBoxItemRequested";
            this.MaxHeight = 240;
            this.ZIndex = 100010;
            this.Localization.ItemsCheckedString = Utilities.GetLocalizedString("ItemsCheckedString");
            this.Localization.CheckAllString = Utilities.GetLocalizedString("CheckAllString");
            this.Localization.AllItemsCheckedString = Utilities.GetLocalizedString("AllItemsCheckedString");
            this.Localization.NoMatches = Utilities.GetLocalizedString("NoMatches");
            this.Localization.ShowMoreFormatString = Utilities.GetLocalizedString("ShowMoreFormatString");
        }

        protected override void OnPreRender(EventArgs e)
        {
            Utilities.ApplySkin(this);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            base.OnPreRender(e);
        }
    }
}
