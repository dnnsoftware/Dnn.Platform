// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ExtensionPoints;

using System.Web.UI;
using System.Web.UI.WebControls;

public class PanelEditPagePanelExtensionControl : WebControl
{
    public string PanelId { get; set; }

    public string Text { get; set; }

    /// <inheritdoc/>
    protected override void RenderContents(HtmlTextWriter op)
    {
        op.Write(@"<div class=""" + this.CssClass + @""">
	<h2 id=""" + this.PanelId + @""" class=""dnnFormSectionHead"">
<a href="" class=""dnnLabelExpanded"">" + this.Text + @"</a>
</h2>
	<fieldset>");
        base.RenderContents(op);
        op.Write("</fieldset></div>");
    }
}
