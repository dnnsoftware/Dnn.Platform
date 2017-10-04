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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;


#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    [ParseChildren(true)]
    public class DnnRibbonBar : WebControl
    {
        public DnnRibbonBar() : base("div")
        {
            CssClass = "dnnRibbon";
            Control control = this;
            Utilities.ApplySkin(control, "", "RibbonBar", "RibbonBar");
        }

        [Category("Behavior"), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DnnRibbonBarGroupCollection Groups
        {
            get
            {
                return (DnnRibbonBarGroupCollection) Controls;
            }
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (obj is DnnRibbonBarGroup)
            {
                base.AddParsedSubObject(obj);
            }
            else
            {
                throw new NotSupportedException("DnnRibbonBarGroupCollection must contain controls of type DnnRibbonBarGroup");
            }
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new DnnRibbonBarGroupCollection(this);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (Visible)
            {
                Utilities.ApplySkin(this, "", "RibbonBar", "RibbonBar");
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if ((Groups.Count > 0))
            {
                Groups[0].CssClass = Groups[0].CssClass + " " + Groups[0].CssClass.Trim() + "First";
                Groups[Groups.Count - 1].CssClass = Groups[Groups.Count - 1].CssClass + " " + Groups[Groups.Count - 1].CssClass.Trim() + "Last";
            }

            base.RenderBeginTag(writer);

            writer.AddAttribute("class", "barContent");
            writer.RenderBeginTag("div");

            writer.AddAttribute("cellpadding", "0");
            writer.AddAttribute("cellspacing", "0");
            writer.AddAttribute("border", "0");
            writer.RenderBeginTag("table");
            writer.RenderBeginTag("tr");

            foreach (DnnRibbonBarGroup grp in Groups)
            {
                if ((grp.Visible))
                {
                    writer.RenderBeginTag("td");
                    grp.RenderControl(writer);
                    writer.RenderEndTag();
                }
            }
            //MyBase.RenderChildren(writer)

            writer.RenderEndTag();
            //tr
            writer.RenderEndTag();
            //table
            writer.RenderEndTag();
            //div

            writer.AddAttribute("class", "barBottomLeft");
            writer.RenderBeginTag("div");
            writer.RenderEndTag();

            writer.AddAttribute("class", "barBottomRight");
            writer.RenderBeginTag("div");
            writer.RenderEndTag();

            base.RenderEndTag(writer);
        }
    }
}