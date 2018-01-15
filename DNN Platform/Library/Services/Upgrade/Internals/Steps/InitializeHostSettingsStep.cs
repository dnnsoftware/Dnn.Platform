#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
