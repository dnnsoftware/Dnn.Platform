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

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    public sealed class DnnComboBoxItem : CheckBox
    {
        public DnnComboBoxItem()
        {
        }

        public DnnComboBoxItem(string text)
        {
            Text = text;
        }

        public DnnComboBoxItem(string text, string value) : this(text)
        {
            Value = value;
        }

        public string ResourceKey
        {
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

        [Bindable(true, BindingDirection.TwoWay)]
        [DefaultValue(false)]
        [Themeable(false)]
        public string Value
        {
            get
            {
                var obj = ViewState["Value"];
                return obj != null ? (string)obj : "";
            }
            set
            {
                this.ViewState["Value"] = (object)value;
            }
        }
    }
}