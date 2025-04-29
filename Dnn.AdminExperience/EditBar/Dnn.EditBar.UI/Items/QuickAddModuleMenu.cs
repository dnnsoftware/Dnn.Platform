// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items;

using System;
using System.Linq;
using System.Net;

using Dnn.EditBar.Library;
using Dnn.EditBar.Library.Items;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Web.Components.Controllers;

[Serializable]
public class QuickAddModuleMenu : BaseMenuItem
{
    private const string LocalResourceFile = "~/DesktopModules/Admin/Dnn.EditBar/App_LocalResources/QuickAddModule.resx";

    /// <inheritdoc/>
    public override string Name { get; } = "QuickAddModule";

    /// <inheritdoc/>
    public override string Text
    {
        get
        {
            return DotNetNuke.Services.Localization.Localization.GetString("QuickAddModule", LocalResourceFile, System.Threading.Thread.CurrentThread.CurrentCulture.Name);
        }
    }

    /// <inheritdoc/>
    public override string CssClass
    {
        get
        {
            return string.Empty;
        }
    }

    /// <inheritdoc/>
    public override bool CustomLayout => true;

    /// <inheritdoc/>
    public override string Template
    {
        get
        {
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null)
            {
                return string.Empty;
            }

            var moduleList = ControlBarController.Instance.GetCategoryDesktopModules(portalSettings.PortalId, "Common", string.Empty).Select(m =>
            {
                var selected = string.Equals(m.Value.DesktopModule.ModuleName, "DNN_HTML", StringComparison.InvariantCultureIgnoreCase) ? " selected" : string.Empty;
                return $"<option value=\"{m.Value.DesktopModuleID}\"{selected}>{WebUtility.HtmlEncode(m.Value.FriendlyName)}</option>";
            });
            var panes = string.Empty;
            foreach (string paneName in portalSettings.ActiveTab.Panes)
            {
                var selected = string.Equals(paneName, "ContentPane", StringComparison.InvariantCultureIgnoreCase) ? " selected" : string.Empty;
                panes += $"<option value=\"{WebUtility.HtmlEncode(paneName)}\"{selected}>{WebUtility.HtmlEncode(paneName)}</options>";
            }

            var toolTip = DotNetNuke.Services.Localization.Localization.GetString("QuickAddModule.Tooltip", LocalResourceFile);
            var moduleSelect = $"<select id=\"menu-QuickAddModule-module\">{string.Join(string.Empty, moduleList)}</select>";
            var paneSelect = $"<select id=\"menu-QuickAddModule-pane\">{panes}</select>";
            var addButton = $"<select id=\"menu-QuickAddModule-btn\"><option value=\"TOP\" class=\"menu-QuickAddModule-opt\">TOP</option><option value=\"BOTTOM\" class=\"menu-QuickAddModule-opt\">BOTTOM</option><option value=\"ADD\" class=\"menu-QuickAddModule-opt\" selected style=\"display:none\"></option></select>";
            return $"<div>{moduleSelect}{paneSelect}{addButton}</div><div class=\"submenuEditBar\">{WebUtility.HtmlEncode(toolTip)}</div>";
        }
    }

    /// <inheritdoc/>
    public override string Parent { get; } = Constants.LeftMenu;

    /// <inheritdoc/>
    public override string Loader { get; } = "QuickAddModule";

    /// <inheritdoc/>
    public override int Order { get; } = 4;

    /// <inheritdoc/>
    public override bool Visible()
    {
        var portalSettings = PortalSettings.Current;
        if (portalSettings == null)
        {
            return false;
        }

        if (!portalSettings.ShowQuickModuleAddMenu)
        {
            return false;
        }

        return Personalization.GetUserMode() == PortalSettings.Mode.Edit
               && ControlBarController.Instance.GetCategoryDesktopModules(portalSettings.PortalId, "All", string.Empty).Any();
    }
}
