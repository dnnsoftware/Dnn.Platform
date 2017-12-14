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
