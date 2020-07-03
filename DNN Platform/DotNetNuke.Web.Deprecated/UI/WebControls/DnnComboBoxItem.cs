// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using DotNetNuke.Services.Localization;
    using Telerik.Web.UI;

    public class DnnComboBoxItem : RadComboBoxItem
    {
        public DnnComboBoxItem()
        {
        }

        public DnnComboBoxItem(string text)
            : base(text)
        {
        }

        public DnnComboBoxItem(string text, string value)
            : base(text, value)
        {
        }

        public string ResourceKey
        {
            get
            {
                if (this.ViewState["ResourceKey"] != null)
                {
                    return this.ViewState["ResourceKey"].ToString();
                }

                return string.Empty;
            }

            set
            {
                this.ViewState["ResourceKey"] = value;
            }
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            if (!string.IsNullOrEmpty(this.ResourceKey))
            {
                string resourceFile = Utilities.GetLocalResourceFile(this);
                if (!string.IsNullOrEmpty(resourceFile))
                {
                    this.Text = Localization.GetString(this.ResourceKey, resourceFile);
                }
            }
        }
    }
}
