// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.IO;
using System.Web;
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;

using System;

using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Upgrade;

namespace DotNetNuke.Modules.MemberDirectory.Components
{

	public class UpgradeController : IUpgradeable
	{
		public string UpgradeModule(string Version)
		{
			try
			{
				switch (Version)
				{
					case "07.00.06":
						UpdateDisplaySearchSettings();
						break;
				}
			}
			catch (Exception ex)
			{
				ExceptionLogController xlc = new ExceptionLogController();
				xlc.AddLog(ex);

				return "Failed";
			}

			return "Success";
		}

		private void UpdateDisplaySearchSettings()
		{
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in ModuleController.Instance.GetModulesByDefinition(portal.PortalID, "Member Directory"))
	            {
					foreach (ModuleInfo tabModule in ModuleController.Instance.GetAllTabsModulesByModuleID(module.ModuleID))
					{
					    bool oldValue;
                        if (tabModule.TabModuleSettings.ContainsKey("DisplaySearch") && bool.TryParse(tabModule.TabModuleSettings["DisplaySearch"].ToString(), out oldValue))
			            {
                            ModuleController.Instance.UpdateTabModuleSetting(tabModule.TabModuleID, "DisplaySearch", oldValue ? "Both" : "None");
			            }
		            }
	            }
            }
		}
	}
}
