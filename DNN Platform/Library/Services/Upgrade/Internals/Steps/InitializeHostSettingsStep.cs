#region Usings

using System;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Upgrade.Internals;
using DotNetNuke.Services.Upgrade.Internals.Steps;

#endregion

namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// InstallSuperUserStep - Step that installs SuperUser Account
    /// </summary>
    /// -----------------------------------------------------------------------------    
    public class InitializeHostSettingsStep : BaseInstallationStep
    {
        #region Implementation of IInstallationStep

        /// <summary>
        /// Main method to execute the step
        /// </summary>        
        public override void Execute()
        {
            Percentage = 0;
            Status = StepStatus.Running;

            Details = Localization.Localization.GetString("InitHostSetting", LocalInstallResourceFile);
            var installConfig = InstallController.Instance.GetInstallConfig();

            //if any super user (even deleted) is found - exit
            var superUsers = UserController.GetUsers(true, true, Null.NullInteger);
            if (superUsers != null && superUsers.Count > 0)
            {
                Details = "...";
                Status = StepStatus.Done;
                return;
            }

            //Need to clear the cache to pick up new HostSettings from the SQLDataProvider script
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

            //Synchronise Host Folder
            FolderManager.Instance.Synchronize(Null.NullInteger, "", true, true);

            Status = StepStatus.Done;
        }

        #endregion
    }
}
