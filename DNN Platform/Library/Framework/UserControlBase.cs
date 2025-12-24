// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    /// <summary>
    /// The UserControlBase class defines a custom base class inherited by all
    /// user controls within the Portal.
    /// </summary>
    public class UserControlBase : UserControl
    {
        public bool IsHostMenu
            => Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public PortalSettings PortalSettings
            => PortalController.Instance.GetCurrentPortalSettings();

        /// <inheritdoc cref="HtmlUtils.JavaScriptStringEncode(string)"/>
        public static IHtmlString JavaScriptStringEncode(string value)
            => HtmlUtils.JavaScriptStringEncode(value);

        /// <inheritdoc cref="HtmlUtils.JavaScriptStringEncode(string,bool)"/>
        public static IHtmlString JavaScriptStringEncode(string value, bool addDoubleQuotes)
            => HtmlUtils.JavaScriptStringEncode(value, addDoubleQuotes);
    }
}
