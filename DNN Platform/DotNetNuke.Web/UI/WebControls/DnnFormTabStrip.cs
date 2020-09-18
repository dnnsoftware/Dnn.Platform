// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class DnnFormTabStrip : ListControl
    {
        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);

            foreach (ListItem item in this.Items)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Li);

                writer.AddAttribute(HtmlTextWriterAttribute.Href, item.Value);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(item.Text);

                writer.RenderEndTag();

                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }
    }
}
