// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemoval
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;
    using DotNetNuke.Services.Upgrade;
    using DotNetNuke.Web;

    /// <summary>An <see cref="IUpgradeable"/> implementation.</summary>
    public class UpgradeController : IUpgradeable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpgradeController));

        private readonly IServiceProvider serviceProvider = GetServiceProvider();

        /// <inheritdoc/>
        public string UpgradeModule(string version)
        {
            // ensure we have Host level page with this module on it
            Logger.Info("Adding Telerik Removal host menu item.");
            ModuleDefinitionInfo mDef = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Telerik Removal");

            // Add tab to Admin Menu
            if (mDef != null)
            {
                var hostPage = Upgrade.AddHostPage(
                    "Telerik Removal",
                    "Remove Telerik from this installation",
                    "~/Icons/Sigma/SecurityRoles_16x16_Standard.png",
                    "~/Icons/Sigma/SecurityRoles_32x32_Standard.png",
                    true);

                // Add module to page
                var moduleId = Upgrade.AddModuleToPage(hostPage, mDef.ModuleDefID, "Telerik Removal", "~/Icons/Sigma/SecurityRoles_32x32_Standard.png", true);
                Logger.Info("Added Telerik Removal host menu item.");
            }

            var option = this.GetHostSetting(DotNetNuke.Maintenance.Constants.TelerikUninstallOptionSettingKey);

            // we have read the value, and now we need to delete the setting
            // to prevent this process from getting triggered if the user
            // manually installs the extension.
            // we can't use IApplicationStatusInfo.Status to detect whether
            // this is an upgrade or a manual installation because this
            // method is executed during Application startup, after the
            // upgrade process has already finished.
            this.DeleteHostSetting(DotNetNuke.Maintenance.Constants.TelerikUninstallOptionSettingKey);

            if (DotNetNuke.Maintenance.Constants.TelerikUninstallYesValue.Equals(option, StringComparison.OrdinalIgnoreCase))
            {
                var uninstaller = this.GetService<ITelerikUninstaller>();
                uninstaller.Execute();
                return string.Join(
                    Environment.NewLine,
                    uninstaller.Progress.Select((step, i) =>
                    {
                        var status = "🔲";
                        if (step.Success.HasValue)
                        {
                            status = step.Success.Value ? "✅" : "🚨";
                        }

                        return $"{i}. {step.StepName} {status} - {step.Notes}";
                    }));
            }

            return "Success";
        }

        private static IServiceProvider GetServiceProvider()
        {
            return HttpContextSource.Current?.GetScope()?.ServiceProvider ??
                DependencyInjectionInitialize.BuildServiceProvider();
        }

        private string GetHostSetting(string key)
        {
            return this.GetService<IHostSettingsService>().GetString(key);
        }

        private void DeleteHostSetting(string key, bool isSecure = false)
        {
            this.GetService<IHostSettingsService>().Update(new ConfigurationSetting
            {
                IsSecure = isSecure,
                Key = key,
                Value = null,
            });
        }

        private T GetService<T>()
            where T : class
        {
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
