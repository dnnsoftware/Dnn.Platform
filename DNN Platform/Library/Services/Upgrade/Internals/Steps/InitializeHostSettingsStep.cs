// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    using System;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Upgrade.Internals;
    using DotNetNuke.Services.Upgrade.Internals.Steps;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// InstallSuperUserStep - Step that installs SuperUser Account.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class InitializeHostSettingsStep : BaseInstallationStep
    {
        /// <summary>
        /// Main method to execute the step.
        /// </summary>
        public override void Execute()
        {
            this.Percentage = 0;
            this.Status = StepStatus.Running;

            this.Details = Localization.GetString("InitHostSetting", this.LocalInstallResourceFile);
            var installConfig = InstallController.Instance.GetInstallConfig();

            // if any super user (even deleted) is found - exit
            var superUsers = UserController.GetUsers(true, true, Null.NullInteger);
            if (superUsers != null && superUsers.Count > 0)
            {
                this.Details = "...";
                this.Status = StepStatus.Done;
                return;
            }

            // Need to clear the cache to pick up new HostSettings from the SQLDataProvider script
            DataCache.RemoveCache(DataCache.HostSettingsCacheKey);

            string domainName = Globals.GetDomainName(HttpContext.Current.Request);
            foreach (var setting in installConfig.Settings)
            {
                var settingName = setting.Name;
                var settingValue = setting.Value;

                switch (settingName)
                {
                    case "HostURL":
                        if (string.IsNullOrEmpty(settingValue))
                        {
                            settingValue = domainName;
                        }

                        break;
                    case "HostEmail":
                        if (string.IsNullOrEmpty(settingValue))
                        {
                            settingValue = installConfig.SuperUser.Email;
                        }

                        break;
                }

                HostController.Instance.Update(settingName, settingValue, setting.IsSecure);
            }

            // Synchronise Host Folder
            FolderManager.Instance.Synchronize(Null.NullInteger, string.Empty, true, true);

            this.Status = StepStatus.Done;
        }
    }
}
