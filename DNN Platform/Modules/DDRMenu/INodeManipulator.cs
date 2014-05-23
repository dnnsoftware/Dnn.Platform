using System.Collections.Generic;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Web.DDRMenu
{
	public interface INodeManipulator
	{
		List<MenuNode> ManipulateNodes(List<MenuNode> nodes, PortalSettings portalSettings);
	}
}