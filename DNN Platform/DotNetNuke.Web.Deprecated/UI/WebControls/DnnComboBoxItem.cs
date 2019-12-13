// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using DotNetNuke.Services.Localization;
using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnComboBoxItem : RadComboBoxItem
    {
        public DnnComboBoxItem()
        {
        }       

        public DnnComboBoxItem(string text) : base(text)
        {
        }

        public DnnComboBoxItem(string text, string value) : base(text, value)
        {
        }

        public string ResourceKey { 
            get
            {
                if (ViewState["ResourceKey"] != null)
                {
                    return ViewState["ResourceKey"].ToString();
                }

                return string.Empty;
            }
            set
            {
                ViewState["ResourceKey"] = value;
            }
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            if (!string.IsNullOrEmpty(ResourceKey))
            {
                string resourceFile = Utilities.GetLocalResourceFile(this);
                if (!string.IsNullOrEmpty(resourceFile))
                {
                    Text = Localization.GetString(ResourceKey, resourceFile);
                }
            }
        }
    }
}
