// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.MemberDirectory.Components
{
    using System;
    using System.IO;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Upgrade;

    public class UpgradeController : IUpgradeable
    {
        public string UpgradeModule(string Version)
        {
            try
            {
                switch (Version)
                {
                    case "07.00.06":
                        this.UpdateDisplaySearchSettings();
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
