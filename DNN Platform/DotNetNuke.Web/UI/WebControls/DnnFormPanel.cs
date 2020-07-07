// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class DnnFormPanel : WebControl
    {
        public bool Expanded { get; set; }

        public string Text { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
            writer.RenderBeginTag(HtmlTextWriterTag.H2);

            if (this.Expanded)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnSectionExpanded");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(this.Text);
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);

            this.RenderChildren(writer);

            writer.RenderEndTag();
        }
    }
}
