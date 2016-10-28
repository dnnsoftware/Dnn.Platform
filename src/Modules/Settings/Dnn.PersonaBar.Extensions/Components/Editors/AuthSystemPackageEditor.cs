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
            }

            return detail;
        }

        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }

        #endregion

        #region Private Methods

        private string GetSettingUrl(int portalId, int authSystemPackageId)
        {
            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "Extensions");
            if (package == null)
            {
                return String.Empty;
            }
            var tabId = TabController.Instance.GetTabsByPackageID(portalId, package.PackageID, false).Keys.FirstOrDefault();
            if (tabId <= 0)
            {
                return String.Empty;
            }

            return Globals.NavigateURL(tabId);
        }

        #endregion
    }
}