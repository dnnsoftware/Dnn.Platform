// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.Globalization;
    using System.Text;

    using DotNetNuke.Common;

    public class ToolBarMenuButtonRenderer : IExtensionControlRenderer
    {
        /// <inheritdoc/>
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
            if (icon.StartsWith("~/", StringComparison.Ordinal))
            {
                icon = Globals.ResolveUrl(icon);
            }

            var str = new StringBuilder();
            str.AppendFormat(CultureInfo.InvariantCulture, "<div id='{0}_wrapper' class='{1}_wrapper'>", extension.ButtonId, extension.MenuCssClass);
            str.AppendFormat(
                CultureInfo.InvariantCulture,
                "<button id='{0}' class='{1} {2}' onclick='{3}; return false;' title='{4}'>",
                extension.ButtonId,
                cssClass,
                extension.MenuCssClass,
                action,
                extension.Text);
            str.AppendFormat(
                CultureInfo.InvariantCulture,
                "<span id='{0}_text' style='{1} background-image: url(\"{2}\");'>{3}</span>",
                extension.ButtonId,
                !extension.ShowText ? "text-indent: -10000000px;" : string.Empty,
                extension.ShowIcon ? icon : string.Empty,
                extension.Text);
            str.AppendLine("</button>");
            str.AppendFormat(CultureInfo.InvariantCulture, "<div class='{0}_menu dnnClear'>", extension.MenuCssClass);
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

        private static string GetItemOutput(IMenuButtonItemExtensionPoint item)
        {
            if (string.IsNullOrEmpty(item.Type))
            {
                return $"<li class='{item.CssClass}' id='{item.ItemId}' ><a href='#' onclick='{item.Action}; return false;'><span {item.Attributes}>{item.Text}</span></a></li>";
            }

            return $"<li><input type='{item.Type}' name='{item.ItemId}' id='{item.ItemId}' value='{item.Text}' {item.Attributes} onclick='{item.Action}; return false;'/>{item.Text}</li>";
        }
    }
}
