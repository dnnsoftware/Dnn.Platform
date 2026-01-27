// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.MemberDirectory.Components
{
    using System;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Log.EventLog;

    public class UpgradeController : IUpgradeable
    {
        /// <inheritdoc />
        public string UpgradeModule(string version)
        {
            try
            {
                switch (version)
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

        private static void UpdateDisplaySearchSettings()
        {
            foreach (IPortalInfo portal in PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in ModuleController.Instance.GetModulesByDefinition(portal.PortalId, "Member Directory"))
                {
                    foreach (ModuleInfo tabModule in ModuleController.Instance.GetAllTabsModulesByModuleID(module.ModuleID))
                    {
                        if (tabModule.TabModuleSettings.ContainsKey("DisplaySearch") && bool.TryParse(tabModule.TabModuleSettings["DisplaySearch"].ToString(), out var oldValue))
                        {
                            ModuleController.Instance.UpdateTabModuleSetting(tabModule.TabModuleID, "DisplaySearch", oldValue ? "Both" : "None");
                        }
                    }
                }
            }
        }
    }
}
