// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.Components;

using System;
using System.Collections.Generic;

using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Upgrade;

/// <summary>Module business controller to implement Dnn module interfaces.</summary>
public class CoreMessagingBusinessController : IUpgradeable
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CoreMessagingBusinessController));

    /// <summary>Runs upgrade logic upon module upgrade.</summary>
    /// <param name="version">The version we are upgrading to.</param>
    /// <returns>"Success" or "Failed".</returns>
    public string UpgradeModule(string version)
    {
        try
        {
            switch (version)
            {
                case "06.02.00":
                    var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Message Center");
                    if (moduleDefinition != null)
                    {
                        var portals = PortalController.Instance.GetPortals();
                        foreach (IPortalInfo portal in portals)
                        {
                            if (portal.UserTabId > Null.NullInteger)
                            {
                                // Find TabInfo
                                var tab = TabController.Instance.GetTab(portal.UserTabId, portal.PortalId, true);
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
