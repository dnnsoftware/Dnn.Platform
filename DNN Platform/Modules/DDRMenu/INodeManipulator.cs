// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;

using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Web.DDRMenu
{
    public interface INodeManipulator
    {
        List<MenuNode> ManipulateNodes(List<MenuNode> nodes, PortalSettings portalSettings);
    }
}
