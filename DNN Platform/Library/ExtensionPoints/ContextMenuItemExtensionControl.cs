﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;

namespace DotNetNuke.ExtensionPoints
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:ContextMenuItemExtensionControl runat=server></{0}:ContextMenuItemExtensionControl>")]
    public class ContextMenuItemExtensionControl : DefaultExtensionControl
    {
        private string content = "";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var extensionPointManager = new ExtensionPointManager();

            StringBuilder str = new StringBuilder();

            foreach (var extension in extensionPointManager.GetContextMenuItemExtensionPoints(Module, Group))
            {
                var icon = extension.Icon;
                if (icon.StartsWith("~/"))
                {
                    icon = Globals.ResolveUrl(icon);
                }

                str.Append(@"<li id=""" + extension.CtxMenuItemId + @""" class=""" + extension.CssClass + @""">
    <a id=""" + extension.CtxMenuItemId + @"_link"" href=""#"" onclick=""" + extension.Action + @""" >
        <img id=""" + extension.CtxMenuItemId + @"_icon"" alt=""" + extension.AltText + @""" src=""" + icon + @""" title=""" + extension.AltText + @""">
        <span id=""" + extension.CtxMenuItemId + @"_text"">"+extension.Text+@"</span>
    </a>
</li>");
            }

            content = str.ToString();
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write(content);
        }
    }
}
