using System.Collections.Generic;
using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.WebControls;
using effority.Ealo.Specialized;
using EaloTabInfo = effority.Ealo.Specialized.TabInfo;
using TabInfo = DotNetNuke.Entities.Tabs.TabInfo;

namespace DotNetNuke.Web.DDRMenu.Localisation
{
    using DotNetNuke.Entities.Portals;

    public class Ealo : ILocalisation
	{
		private bool haveChecked;
		private bool found;

		public bool HaveApi()
		{
			if (!haveChecked)
			{
				found = (DesktopModuleController.GetDesktopModuleByModuleName("effority.Ealo.Tabs", PortalSettings.Current.PortalId) != null);
				haveChecked = true;
			}

			return found;
		}

		public TabInfo LocaliseTab(TabInfo tab, int portalId)
		{
			return EaloWorker.LocaliseTab(tab, portalId);
		}

		public DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes)
		{
			return null;
		}

		// Separate class only instantiated if Ealo is available.
		static class EaloWorker
		{
			private static readonly Dictionary<string, Dictionary<int, EaloTabInfo>> ealoTabLookup =
				new Dictionary<string, Dictionary<int, EaloTabInfo>>();

			public static TabInfo LocaliseTab(TabInfo tab, int portalId)
			{
				var culture = DNNAbstract.GetCurrentCulture();
				Dictionary<int, EaloTabInfo> ealoTabs;
				if (!ealoTabLookup.TryGetValue(culture, out ealoTabs))
				{
					ealoTabs = Tabs.GetAllTabsAsDictionary(culture, true);
					lock (ealoTabLookup)
					{
						if (!ealoTabLookup.ContainsKey(culture))
						{
							ealoTabLookup.Add(culture, ealoTabs);
						}
					}
				}

				EaloTabInfo ealoTab;
				if (ealoTabs.TryGetValue(tab.TabID, out ealoTab))
				{
					if (ealoTab.EaloTabName != null)
					{
						tab.TabName = ealoTab.EaloTabName.StringTextOrFallBack;
					}
					if (ealoTab.EaloTitle != null)
					{
						tab.Title = ealoTab.EaloTitle.StringTextOrFallBack;
					}
				}
				return tab;
			}
		}
	}
}