// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Web.DDRMenu
{
	public interface INodeManipulator
	{
		List<MenuNode> ManipulateNodes(List<MenuNode> nodes, PortalSettings portalSettings);
	}
}
