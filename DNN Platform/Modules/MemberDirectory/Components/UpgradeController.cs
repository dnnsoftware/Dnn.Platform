#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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