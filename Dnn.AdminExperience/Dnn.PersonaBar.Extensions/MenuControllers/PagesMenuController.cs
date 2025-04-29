// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.MenuControllers;

using System.Collections.Generic;

using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using Dnn.PersonaBar.Pages.Components.Security;
using DotNetNuke.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;

/// <summary>Controls the pages menu.</summary>
public class PagesMenuController : IMenuItemController
{
    private readonly ISecurityService securityService;

    /// <summary>Initializes a new instance of the <see cref="PagesMenuController"/> class.</summary>
    public PagesMenuController()
    {
        this.securityService = SecurityService.Instance;
    }

    /// <inheritdoc/>
    public void UpdateParameters(MenuItem menuItem)
    {
    }

    /// <inheritdoc/>
    public bool Visible(MenuItem menuItem)
    {
        return this.securityService.IsVisible(menuItem);
    }

    /// <inheritdoc/>
    public IDictionary<string, object> GetSettings(MenuItem menuItem)
    {
        var settings = new Dictionary<string, object>
        {
            { "canSeePagesList", this.securityService.CanViewPageList(menuItem.MenuId) },
            { "canAddPages", this.securityService.CanAddPage(PortalSettings.Current?.ActiveTab?.TabID ?? 0) },
            { "portalName", PortalSettings.Current.PortalName },
            { "currentPagePermissions", this.securityService.GetCurrentPagePermissions() },
            { "currentPageName", PortalSettings.Current?.ActiveTab?.TabName },
            { "productSKU", DotNetNukeContext.Current.Application.SKU },
            { "isAdmin", this.securityService.IsPageAdminUser() },
            { "currentParentHasChildren", PortalSettings.Current?.ActiveTab?.HasChildren },
            { "isAdminHostSystemPage", this.securityService.IsAdminHostSystemPage() },
        };

        return settings;
    }
}
