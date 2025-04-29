// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Controls;

using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

/// <summary>The Form will reset action to raw url instead of rewrite url.</summary>
public class Form : HtmlForm
{
    /// <inheritdoc/>
    protected override void RenderAttributes(HtmlTextWriter writer)
    {
        var stringWriter = new StringWriter();
        var htmlWriter = new HtmlTextWriter(stringWriter);
        base.RenderAttributes(htmlWriter);
        string html = stringWriter.ToString();

        // Locate and replace action attribute
        int startPoint = html.IndexOf("action=\"");
        if (startPoint >= 0)
        {
            int endPoint = html.IndexOf("\"", startPoint + 8) + 1;
            html = html.Remove(startPoint, endPoint - startPoint);
            html = html.Insert(startPoint, "action=\"" + HttpUtility.HtmlEncode(HttpContext.Current.Request.RawUrl) + "\"");
        }

        if (this.ID != null)
        {
            // Locate and replace id attribute
            startPoint = html.IndexOf("id=\"");
            if (startPoint >= 0)
            {
                int endPoint = html.IndexOf("\"", startPoint + 4) + 1;
                html = html.Remove(startPoint, endPoint - startPoint);
                html = html.Insert(startPoint, "id=\"" + this.ClientID + "\"");
            }
        }

        writer.Write(html);
    }
}
