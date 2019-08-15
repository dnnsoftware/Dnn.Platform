using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI.WebControls;
using EaloTabInfo = effority.Ealo.Specialized.TabInfo;

namespace DotNetNuke.Web.DDRMenu.Localisation
{
	public class Localiser
	{
		private readonly int portalId;
		private static bool apiChecked;
		private static ILocalisation _LocalisationApi;
		private static ILocalisation LocalisationApi
		{
			get
			{
				if (!apiChecked)
				{
                    foreach (var api in new ILocalisation[] { new Generic() })
                    {
                        if (api.HaveApi())
                        {
                            _LocalisationApi = api;
                            break;
                        }
                    }
					apiChecked = true;
				}
				return _LocalisationApi;
			}
		}

		public static DNNNodeCollection LocaliseDNNNodeCollection(DNNNodeCollection nodes)
		{
			return (LocalisationApi == null) ? nodes : (LocalisationApi.LocaliseNodes(nodes) ?? nodes);
		}

		public Localiser(int portalId)
		{
			this.portalId = portalId;
		}

		public void LocaliseNode(MenuNode node)
		{
            var tab = (node.TabId > 0) ? TabController.Instance.GetTab(node.TabId, Null.NullInteger, false) : null;
			if (tab != null)
			{
				var localised = LocaliseTab(tab);
				tab = localised ?? tab;

				if (localised != null)
				{
					node.TabId = tab.TabID;
					node.Text = tab.TabName;
					node.Enabled = !tab.DisableLink;
					if (!tab.IsVisible)
					{
						node.TabId = -1;
					}
				}

				node.Title = tab.Title;
				node.Description = tab.Description;
				node.Keywords = tab.KeyWords;
			}
			else
			{
				node.TabId = -1;
			}

			node.Children.ForEach(LocaliseNode);
		}

		private TabInfo LocaliseTab(TabInfo tab)
		{
			return (LocalisationApi == null) ? null : LocalisationApi.LocaliseTab(tab, portalId);
		}
	}
}