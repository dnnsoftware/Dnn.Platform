// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ExtensionPoints;

using System.Web.UI;
using System.Web.UI.WebControls;

public class PanelTabExtensionControl : WebControl
{
    public string PanelId { get; set; }

    /// <inheritdoc/>
    public override void RenderBeginTag(HtmlTextWriter writer)
    {
        writer.Write(string.Empty);
    }

    /// <inheritdoc/>
    public override void RenderEndTag(HtmlTextWriter writer)
    {
        writer.Write(string.Empty);
    }

    /// <inheritdoc/>
    protected override void RenderContents(HtmlTextWriter op)
    {
        op.Write("<div class=\"ehccContent dnnClear\" id=\"" + this.PanelId + "\">");
        base.RenderContents(op);
        op.Write("</div>");
    }
}
