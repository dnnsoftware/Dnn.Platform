// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemoval
{
    using System;

    using Dnn.Modules.TelerikRemovalLibrary;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Web;

    /// <summary>
    /// An <see cref="IUpgradeable"/> implementation.
    /// </summary>
    public class UpgradeController : IUpgradeable
    {
        /// <summary>
        /// Key of the Telerik unintall option in the host settings.
        /// </summary>
        private static readonly string TelerikUninstallOptionSettingKey = "telerikUninstallOption";

        private readonly IServiceProvider serviceProvider = GetServiceProvider();

        /// <inheritdoc/>
        public string UpgradeModule(string version)
        {
            var option = this.GetHostSetting(TelerikUninstallOptionSettingKey);

            // we have read the value, and now we need to delete the setting
            // to prevent this process from getting triggered if the user
            // manually installs the extension.
            // we can't use IApplicationStatusInfo.Status to detect whether
            // this is an upgrade or a manual installation because this
            // method is executed during Application startup, after the
            // upgrade process has already finished.
            this.DeleteHostSetting(TelerikUninstallOptionSettingKey);

            if ("Y".Equals(option, StringComparison.OrdinalIgnoreCase))
            {
                this.GetService<ITelerikUninstaller>().Execute();
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
