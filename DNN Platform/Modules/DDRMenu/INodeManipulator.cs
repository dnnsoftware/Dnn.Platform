// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.DDRMenu;

using System.Collections.Generic;

using DotNetNuke.Entities.Portals;

/// <summary>Allows modules that implement this interface to manipulate the nodes in the menu before they render.</summary>
public interface INodeManipulator
{
    /// <summary>Manipulates the menu nodes before they render.</summary>
    /// <param name="nodes">The list of nodes before they are manipulated.</param>
    /// <param name="portalSettings">The settings of the portal (site) on which the menu is rendered.</param>
    /// <returns>The list of nodes after they have been manipulated.</returns>
    List<MenuNode> ManipulateNodes(List<MenuNode> nodes, PortalSettings portalSettings);
}
