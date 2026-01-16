// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Web;

    using DotNetNuke.Common;

    public class ToolBarButtonRenderer : IExtensionControlRenderer
    {
        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public string GetOutput(IExtensionPoint extensionPoint)
        {
            var extension = (IToolBarButtonExtensionPoint)extensionPoint;

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
            str.AppendFormat(
                CultureInfo.InvariantCulture,
                "<button id=\"{0}\" class=\"{1}\" onclick=\"{2}\" title=\"{3}\">",
                extension.ButtonId,
                cssClass,
                HttpUtility.HtmlAttributeEncode($"{action}; return false;"),
                extension.Text);

            str.AppendFormat(
                CultureInfo.InvariantCulture,
                "<span id='{0}_text' style='{1} background-image: url(\"{2}\");'>{3}</span>",
                extension.ButtonId,
                !extension.ShowText ? "text-indent: -10000000px;" : string.Empty,
                extension.ShowIcon ? icon : string.Empty,
                extension.Text);

            str.AppendLine("</button>");

            return str.ToString();
        }
    }
}
