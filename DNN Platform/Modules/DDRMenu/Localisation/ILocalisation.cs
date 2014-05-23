using System.Collections.Generic;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI.WebControls;

namespace DotNetNuke.Web.DDRMenu.Localisation
{
	public interface ILocalisation
	{
		bool HaveApi();
		TabInfo LocaliseTab(TabInfo tab, int portalId);
		DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes);
	}
}