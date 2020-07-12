// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;

    [DefaultProperty("Text")]
    [ToolboxData("<{0}:ContextMenuItemExtensionControl runat=server></{0}:ContextMenuItemExtensionControl>")]
    public class ContextMenuItemExtensionControl : DefaultExtensionControl
    {
        private string content = string.Empty;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var extensionPointManager = new ExtensionPointManager();

            StringBuilder str = new StringBuilder();

            foreach (var extension in extensionPointManager.GetContextMenuItemExtensionPoints(this.Module, this.Group))
            {
                var icon = extension.Icon;
                if (icon.StartsWith("~/"))
                {
                    icon = Globals.ResolveUrl(icon);
                }

                str.Append(@"<li id=""" + extension.CtxMenuItemId + @""" class=""" + extension.CssClass + @""">
    <a id=""" + extension.CtxMenuItemId + @"_link"" href=""#"" onclick=""" + extension.Action + @""" >
        <img id=""" + extension.CtxMenuItemId + @"_icon"" alt=""" + extension.AltText + @""" src=""" + icon + @""" title=""" + extension.AltText + @""">
        <span id=""" + extension.CtxMenuItemId + @"_text"">" + extension.Text + @"</span>
    </a>
</li>");
            }

            this.content = str.ToString();
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write(this.content);
        }
    }
}
