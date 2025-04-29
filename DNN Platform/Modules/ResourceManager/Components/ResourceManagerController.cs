// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager.Components;

using System;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Upgrade;

/// <summary>Provides upgrade support for module.</summary>
public class ResourceManagerController : IUpgradeable
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ResourceManagerController));

    /// <inheritdoc/>
    public string UpgradeModule(string version)
    {
        try
        {
            switch (version)
            {
                case "00.00.01":
                    Logger.Info("Adding Global Assets host menu item.");
                    ModuleDefinitionInfo mDef = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("ResourceManager");

                    // Add tab to Admin Menu
                    if (mDef != null)
                    {
                        var hostPage = Upgrade.AddHostPage(
                            "Global Assets",
                            "Manage Global assets",
                            "~/Icons/Sigma/Files_16X16_Standard.png",
                            "~/Icons/Sigma/Files_32X32_Standard.png",
                            true);

                        // Add module to page
                        var moduleId = Upgrade.AddModuleToPage(hostPage, mDef.ModuleDefID, "Global Assets Management", "~/Icons/Sigma/Files_32X32_Standard.png", true);
                        ModuleController.Instance.UpdateModuleSetting(moduleId, Constants.HomeFolderSettingName, "1");
                        ModuleController.Instance.UpdateModuleSetting(moduleId, Constants.ModeSettingName, "0");
                        Logger.Info("Added Global Assets host menu item.");
                    }

                    // Remove Previous Host File Manager pages
                    Logger.Info("Removing old pages.");
                    Upgrade.RemoveHostPage("File Manager");
                    Upgrade.RemoveHostPage("File Management");

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
