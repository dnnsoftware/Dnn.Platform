// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFormTabStrip : ListControl 
    {

        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);

            foreach (ListItem item in Items)
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
