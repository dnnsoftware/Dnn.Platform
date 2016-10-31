using System;
using System.Linq;
using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Installer.Packages;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public class AuthSystemPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AuthSystemPackageEditor));

        #region IPackageEditor Implementation

        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
            var detail = new AuthSystemPackageDetailDto(portalId, package)
            {
                AuthenticationType = authSystem.AuthenticationType,
            };

            var hasCustomSettings = !string.IsNullOrEmpty(authSystem.SettingsControlSrc);
            if (hasCustomSettings)
            {
                detail.SettingUrl = GetSettingUrl(portalId, package.PackageID);
            }

            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
            if (isHostUser)
            {
                detail.ReadOnly |= authSystem.AuthenticationType == "DNN";
                detail.LoginControlSource = authSystem.LoginControlSrc;
                detail.LogoffControlSource = authSystem.LogoffControlSrc;
                detail.SettingsControlSource = authSystem.SettingsControlSrc;
                detail.Enabled = authSystem.IsEnabled;
            }

            return detail;
        }

        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

                if (isHostUser)
                {
                    string value;
                    var authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(packageSettings.PackageId);

                    if (packageSettings.EditorActions.TryGetValue("loginControlSource", out value)
                        && !string.IsNullOrEmpty(value))
                    {
                        authSystem.LoginControlSrc = value;
                    }
                    if (packageSettings.EditorActions.TryGetValue("logoffControlSource", out value)
                        && !string.IsNullOrEmpty(value))
                    {
                        authSystem.LogoffControlSrc = value;
                    }
                    if (packageSettings.EditorActions.TryGetValue("settingsControlSource", out value)
                        && !string.IsNullOrEmpty(value))
                    {
                        authSystem.SettingsControlSrc = value;
                    }
                    if (packageSettings.EditorActions.TryGetValue("enabled", out value)
                        && !string.IsNullOrEmpty(value))
                    {
                        authSystem.IsEnabled = bool.Parse(value);
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

        private static string GetSettingUrl(int portalId, int authSystemPackageId)
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