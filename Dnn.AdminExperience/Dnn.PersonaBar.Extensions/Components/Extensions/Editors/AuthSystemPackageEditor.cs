// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    using System;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;
    using DotNetNuke.Services.Installer.Packages;
    using Microsoft.Extensions.DependencyInjection;

    public class AuthSystemPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AuthSystemPackageEditor));
        private static readonly INavigationManager NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();

        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
            var detail = new AuthSystemPackageDetailDto(portalId, package)
            {
                AuthenticationType = authSystem.AuthenticationType,
            };

            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
            if (isHostUser)
            {
                detail.ReadOnly |= authSystem.AuthenticationType == "DNN";
                detail.LoginControlSource = authSystem.LoginControlSrc;
                detail.LogoffControlSource = authSystem.LogoffControlSrc;
                detail.SettingsControlSource = authSystem.SettingsControlSrc;
                detail.Enabled = authSystem.IsEnabled;
            }

            LoadCustomSettings(portalId, package, authSystem, detail);
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
                        bool b;
                        bool.TryParse(value, out b);
                        authSystem.IsEnabled = b;
                    }

                    AuthenticationController.UpdateAuthentication(authSystem);
                    SaveCustomSettings(packageSettings);
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

        private static string GetSettingUrl(int portalId, int authSystemPackageId)
        {
            return NavigationManager.NavigateURL(PortalSettings.Current.ActiveTab.TabID, PortalSettings.Current, "EditExtension",
                "packageid=" + authSystemPackageId,
                "popUp=true",
                "mode=settings");
        }

        private static void LoadCustomSettings(int portalId, PackageInfo package, AuthenticationInfo authSystem, AuthSystemPackageDetailDto detail)
        {
            var hasCustomSettings = !string.IsNullOrEmpty(authSystem.SettingsControlSrc);
            if (hasCustomSettings)
            {
                detail.SettingUrl = GetSettingUrl(portalId, package.PackageID);
            }

            // special case for DNN provided external authentication systems
            switch (detail.AuthenticationType.ToLowerInvariant())
            {
                case "facebook":
                case "google":
                case "live":
                case "twitter":
                    var config = OAuthConfigBase.GetConfig(detail.AuthenticationType, portalId);
                    if (config != null)
                    {
                        detail.AppId = config.APIKey;
                        detail.AppSecret = config.APISecret;
                        detail.AppEnabled = config.Enabled;
                    }
                    break;
            }
        }

        private static void SaveCustomSettings(PackageSettingsDto packageSettings)
        {
            // special case for specific DNN provided external authentication systems
            string authType;
            if (packageSettings.EditorActions.TryGetValue("authenticationType", out authType))
            {
                switch (authType.ToLowerInvariant())
                {
                    case "facebook":
                    case "google":
                    case "live":
                    case "twitter":
                        var dirty = false;
                        string value;
                        var config = OAuthConfigBase.GetConfig(authType, packageSettings.PortalId);

                        if (packageSettings.EditorActions.TryGetValue("appId", out value)
                            && config.APIKey != value)
                        {
                            config.APIKey = value;
                            dirty = true;
                        }

                        if (packageSettings.EditorActions.TryGetValue("appSecret", out value)
                            && config.APISecret != value)
                        {
                            config.APISecret = value;
                            dirty = true;
                        }

                        if (packageSettings.EditorActions.TryGetValue("appEnabled", out value)
                            && config.Enabled.ToString().ToUpperInvariant() != value.ToUpperInvariant())
                        {
                            config.Enabled = "TRUE".Equals(value, StringComparison.OrdinalIgnoreCase);
                            dirty = true;
                        }

                        if (dirty) OAuthConfigBase.UpdateConfig(config);
                        break;
                }
            }
        }
    }
}
