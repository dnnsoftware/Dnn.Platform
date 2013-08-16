#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using DotNetNuke.Services.Localization;
using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnComboBoxItem : RadComboBoxItem
    {
        public DnnComboBoxItem()
        {
            base.Load += new System.EventHandler(DnnComboBoxItem_Load);
        }       

        public DnnComboBoxItem(string text) : base(text)
        {
            base.Load += new System.EventHandler(DnnComboBoxItem_Load);
        }

        public DnnComboBoxItem(string text, string value) : base(text, value)
        {
            base.Load += new System.EventHandler(DnnComboBoxItem_Load);
        }

        public string ResourceKey { get; set; }


        // try to load text from resource file if resourceKey exists
        void DnnComboBoxItem_Load(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(ResourceKey))
            {
                string resourceFile = Utilities.GetLocalResourceFile(this);
                if (!string.IsNullOrEmpty(resourceFile))
                    this.Text = Localization.GetString(ResourceKey, resourceFile);
            }
        }

        //protected override void Render(System.Web.UI.HtmlTextWriter writer)
        //{
        //    if (!string.IsNullOrEmpty(ResourceKey))
        //    {
        //        string resourceFile = Utilities.GetLocalResourceFile(this);
        //        if (!string.IsNullOrEmpty(resourceFile))                
        //            this.Text = Localization.GetString(ResourceKey, resourceFile);
                
        //    }

        //    base.Render(writer);
        //}
    }
}