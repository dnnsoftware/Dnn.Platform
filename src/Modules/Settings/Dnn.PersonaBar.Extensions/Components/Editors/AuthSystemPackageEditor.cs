using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
using Dnn.PersonaBar.Library.Helper;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.UI;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public class AuthSystemPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AuthSystemPackageEditor));

        #region IPackageEditor Implementation

        public PackageDetailDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
            var detail = new CommonPackageDetailDto(portalId, package);

            var hasCustomSettings = !string.IsNullOrEmpty(authSystem.SettingsControlSrc);
            if (hasCustomSettings)
            {
                detail.Settings.Add("settingUrl", GetSettingUrl(portalId, package.PackageID));
            }

            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
            if (isHostUser)
            {
                detail.Settings.Add("readonly", authSystem.AuthenticationType == "DNN");
                detail.Settings.Add("loginControlSource", authSystem.LoginControlSrc);
                detail.Settings.Add("logoffControlSource", authSystem.LogoffControlSrc);
                detail.Settings.Add("settingsControlSource", authSystem.SettingsControlSrc);
                detail.Settings.Add("enabled", authSystem.IsEnabled);
            }

            return detail;
        }

        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(packageSettings.PackageId);
                var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

                if (isHostUser)
                {
                    if(packageSettings.EditorActions.ContainsKey("loginControlSource")
                        && !string.IsNullOrEmpty(packageSettings.EditorActions["loginControlSource"]))
                    {
                        authSystem.LoginControlSrc = packageSettings.EditorActions["loginControlSource"];
                    }
                    if (packageSettings.EditorActions.ContainsKey("logoffControlSource")
                        && !string.IsNullOrEmpty(packageSettings.EditorActions["logoffControlSource"]))
                    {
                        authSystem.LogoffControlSrc = packageSettings.EditorActions["logoffControlSource"];
                    }
                    if (packageSettings.EditorActions.ContainsKey("settingsControlSource")
                        && !string.IsNullOrEmpty(packageSettings.EditorActions["settingsControlSource"]))
                    {
                        authSystem.SettingsControlSrc = packageSettings.EditorActions["settingsControlSource"];
                    }
                    if (packageSettings.EditorActions.ContainsKey("enabled")
                        && !string.IsNullOrEmpty(packageSettings.EditorActions["enabled"]))
                    {
                        authSystem.IsEnabled = bool.Parse(packageSettings.EditorActions["loginControlSource"]);
                    }

                    AuthenticationController.UpdateAuthentication(authSystem);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                errorMessage = ex.Message;
                return false;
            }
        }

        #endregion

        #region Private Methods

        private string GetSettingUrl(int portalId, int authSystemPackageId)
        {
            var module = ModuleController.Instance.GetModulesByDefinition(portalId, "Extensions")
                .Cast<ModuleInfo>().FirstOrDefault();
            if (module == null)
            {
                return string.Empty;
            }

            var tabId = TabController.Instance.GetTabsByModuleID(module.ModuleID).Keys.FirstOrDefault();
            if (tabId <= 0)
            {
                return string.Empty;
            }
            //ctl/Edit/mid/345/packageid/52
            return Globals.NavigateURL(tabId, PortalSettings.Current, "Edit", 
                                            "mid=" + module.ModuleID, 
                                            "packageid=" + authSystemPackageId,
                                            "popUp=true",
                                            "mode=settings");
        }

        #endregion
    }
}