// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.Components
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Upgrade;

    public class CoreMessagingBusinessController : IUpgradeable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CoreMessagingBusinessController));

        public string UpgradeModule(string Version)
        {
            try
            {
                switch (Version)
                {
                    case "06.02.00":
                        var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Message Center");
                        if (moduleDefinition != null)
                        {
                            var portals = PortalController.Instance.GetPortals();
                            foreach (PortalInfo portal in portals)
                            {
                                if (portal.UserTabId > Null.NullInteger)
                                {
                                    // Find TabInfo
                                    var tab = TabController.Instance.GetTab(portal.UserTabId, portal.PortalID, true);
                                    if (tab != null)
                                    {
                                        foreach (var module in ModuleController.Instance.GetTabModules(portal.UserTabId).Values)
                                        {
                                            if (module.DesktopModule.FriendlyName == "Messaging")
                                            {
                                                // Delete the Module from the Modules list
                                                ModuleController.Instance.DeleteTabModule(module.TabID, module.ModuleID, false);

                                                // Add new module to the page
                                                Upgrade.AddModuleToPage(tab, moduleDefinition.ModuleDefID, "Message Center", string.Empty, true);

                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        break;
                }

                return "Success";
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

                return "Failed";
            }
        }
    }
}
