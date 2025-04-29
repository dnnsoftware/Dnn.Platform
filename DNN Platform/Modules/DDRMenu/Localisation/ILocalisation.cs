// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.Localisation;

using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI.WebControls;

/// <summary>The ILocalisation interface can be implemented by menu localization providers in order to localise tabs (pages) or Nodes.</summary>
public interface ILocalisation
{
    /// <summary>Checks if a module has a DDR localization api implemented.</summary>
    /// <returns>A value indicating whether the module has the supported localization api.</returns>
    bool HaveApi();

    /// <summary>Localizes a tab (page) information only (not content).</summary>
    /// <param name="tab">The tab (page) to localize.</param>
    /// <param name="portalId">The id of the portal where that tab exists.</param>
    /// <returns>A new localized <see cref="TabInfo"/>.</returns>
    TabInfo LocaliseTab(TabInfo tab, int portalId);

    /// <summary>Localizes menu nodes.</summary>
    /// <param name="nodes">The original node collection.</param>
    /// <returns>The localized node collection.</returns>
    DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes);
}
