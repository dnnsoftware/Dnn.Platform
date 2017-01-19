#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.UI;
using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Modules.DigitalAssets.Components.WebControls
{
    public class DnnComboBox : RadComboBox
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            base.EnableEmbeddedBaseStylesheet = false;
            OnClientLoad = "$.dnnComboBoxLoaded";
            OnClientFocus = "$.dnnComboBoxHack";
            OnClientDropDownOpened = "$.dnnComboBoxScroll";
            OnClientItemsRequested = "$.dnnComboBoxItemRequested";
            MaxHeight = 240;
            ZIndex = 100010;
            Localization.ItemsCheckedString = Utilities.GetLocalizedString("ItemsCheckedString");
            Localization.CheckAllString = Utilities.GetLocalizedString("CheckAllString");
            Localization.AllItemsCheckedString = Utilities.GetLocalizedString("AllItemsCheckedString");
            Localization.NoMatches = Utilities.GetLocalizedString("NoMatches");
            Localization.ShowMoreFormatString = Utilities.GetLocalizedString("ShowMoreFormatString");
        }

        protected override void OnPreRender(EventArgs e)
        {
            Utilities.ApplySkin(this);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            base.OnPreRender(e);
        }

        public void AddItem(string text, string value)
        {
            Items.Add(new DnnComboBoxItem(text, value));
        }

        public void InsertItem(int index, string text, string value)
        {
            Items.Insert(index, new DnnComboBoxItem(text, value));
        }

        public void DataBind(string initialValue)
        {
            DataBind(initialValue, false);
        }

        public void DataBind(string initial, bool findByText)
        {
            DataBind();

            Select(initial, findByText);
        }

        public void Select(string initial, bool findByText)
        {
            if (findByText)
            {
                if (FindItemByText(initial, true) != null)
                {
					FindItemByText(initial, true).Selected = true;
                }
            }
            else
            {
				if (FindItemByValue(initial, true) != null)
                {
					FindItemByValue(initial, true).Selected = true;
                }
            } 
        }
    }
}