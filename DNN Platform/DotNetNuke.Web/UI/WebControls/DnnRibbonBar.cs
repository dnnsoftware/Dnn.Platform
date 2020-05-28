// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
