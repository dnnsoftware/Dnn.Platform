#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.
#endregion

using System;
using System.Collections.Generic;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Modules.SubscriptionsMgmt.Components.Common;
using DotNetNuke.Services.Upgrade;

namespace DotNetNuke.Modules.SubscriptionsMgmt.Components.Controllers
{
	public class BusinessController : IUpgradeable
	{
		#region IUpgradeable

		/// <summary>Implements the IUpgradeable Interface</summary>
		public string UpgradeModule(string version)
		{
			var message = string.Empty;

			switch (version)
			{
				case "01.00.00":
					//Integration.Mechanics.Instance.AddScoringDefinitions();
					//message += "Added scoring definitions for the Subscription Management module. " + Environment.NewLine;

					//SocialLibrary.Components.Common.Utilities.CategorizeSocialModule(DesktopModuleController.GetDesktopModuleByFriendlyName(Constants.DesktopModuleFriendlyName));
					// message += "Added Subscription Management module to Social module category. " + Environment.NewLine;

					break;
				case "07.02.00":
					AddModuleToMessageTab();
					break;
			}

			return message;
		}

		#endregion

		private void AddModuleToMessageTab()
		{
			var portalController = new PortalController();
			var moduleDef = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Subscription Management");

			if (moduleDef != null)
			{
				foreach (PortalInfo portal in portalController.GetPortals())
				{
					//add the subscription manage module into message tab.
					var messageTab = FindMessageTab(portal.PortalID);
					if (messageTab != null)
					{
						Upgrade.AddModuleToPage(messageTab, moduleDef.ModuleDefID, "Suscription Management", string.Empty, true, false, "centerPane");
					}
				}
			}
		}

		private static TabInfo FindMessageTab(int portalId)
		{
			var tabController = new TabController();
			var moduleController = new ModuleController();
			var portalSettings = new PortalSettings(portalId);

			var profileTab = tabController.GetTab(portalSettings.UserTabId, portalSettings.PortalId, false);
			if (profileTab != null)
			{
				var childTabs = tabController.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
				foreach (TabInfo tab in childTabs)
				{
					foreach (KeyValuePair<int, ModuleInfo> kvp in moduleController.GetTabModules(tab.TabID))
					{
						var module = kvp.Value;
						if (module.DesktopModule.FriendlyName == "Message Center" && !module.IsDeleted)
						{
							return tab;
						}
					}
				}
			}

			return null;
		}

	}
}
