#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Text;
using System.Web;
using DotNetNuke.Common;

namespace DotNetNuke.ExtensionPoints
{
    public class ToolBarMenuButtonRenderer : IExtensionControlRenderer
    {
        public string GetOutput(IExtensionPoint extensionPoint)
        {
            var extension = (IToolBarMenuButtonExtensionPoint)extensionPoint;

            var cssClass = extension.CssClass;
            var action = extension.Action;
            if (!extension.Enabled)
            {
                cssClass += " disabled";
                action = "void(0);";
            }
            var icon = extension.Icon;
            if (icon.StartsWith("~/"))
            {
                icon = Globals.ResolveUrl(icon);
            }

            var str = new StringBuilder();
            str.AppendFormat("<div id='{0}_wrapper' class='{1}_wrapper'>", extension.ButtonId, extension.MenuCssClass);
            str.AppendFormat(
                        "<button id='{0}' class='{1} {2}' onclick='{3}; return false;' title='{4}'>",
                        extension.ButtonId, cssClass, extension.MenuCssClass, action, extension.Text);
            str.AppendFormat(
                "<span id='{0}_text' style='{1} background-image: url(\"{2}\");'>{3}</span>",
                extension.ButtonId,
                !extension.ShowText ? "text-indent: -10000000px;" : "",
                extension.ShowIcon ? icon : "",
                extension.Text);
            str.AppendLine("</button>");
            str.AppendFormat("<div class='{0}_menu dnnClear'>", extension.MenuCssClass);
            str.AppendLine("<div class='handle'></div>");
            str.AppendLine("<ul>");
            foreach (var item in extension.Items)
            {
                str.AppendLine(GetItemOutput(item));
            }            

            str.AppendLine("</ul>");
            str.AppendLine("</div>");
            str.AppendLine("</div>");
            
            return str.ToString();
        }

        private string GetItemOutput(IMenuButtonItemExtensionPoint item)
        {
            if (string.IsNullOrEmpty(item.Type))
            {
                return string.Format("<li class='{0}' id='{1}' ><a href='#' onclick='{2}; return false;'><span {3}>{4}</span></a></li>", item.CssClass, item.ItemId, item.Action, item.Attributes, item.Text);
            }
            
            return string.Format("<li><input type='{0}' name='{1}' id='{2}' value='{3}' {4} onclick='{5}; return false;'/>{6}</li>", item.Type, item.ItemId, item.ItemId, item.Text, item.Attributes, item.Action, item.Text);
        }
    }
}
