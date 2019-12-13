#region Usings

using System;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
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
