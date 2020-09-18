// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.Text;
    using System.Web;

    using DotNetNuke.Common;

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
                !extension.ShowText ? "text-indent: -10000000px;" : string.Empty,
                extension.ShowIcon ? icon : string.Empty,
                extension.Text);
            str.AppendLine("</button>");
            str.AppendFormat("<div class='{0}_menu dnnClear'>", extension.MenuCssClass);
            str.AppendLine("<div class='handle'></div>");
            str.AppendLine("<ul>");
            foreach (var item in extension.Items)
            {
                str.AppendLine(this.GetItemOutput(item));
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
