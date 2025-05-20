﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Host;

using System;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Security;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.UI.Skins;
using DotNetNuke.Web.Client;

using SchedulerMode = DotNetNuke.Abstractions.Application.SchedulerMode;

/// <summary>The default <see cref="IHostSettings"/> implementation.</summary>
/// <param name="hostSettingsService">The host settings service.</param>
public class HostSettings(IHostSettingsService hostSettingsService) : IHostSettings
{
    /// <inheritdoc />
    public TimeSpan AutoAccountUnlockDuration => TimeSpan.FromMinutes(hostSettingsService.GetInteger("AutoAccountUnlockDuration", 10));

    /// <inheritdoc />
    public string AuthenticatedCacheability => hostSettingsService.GetString("AuthenticatedCacheability", "4");

    /// <inheritdoc />
    public string UnauthenticatedCacheability => hostSettingsService.GetString("UnauthenticatedCacheability", "4");

    /// <inheritdoc />
    public bool CdnEnabled => hostSettingsService.GetBoolean("CDNEnabled", false);

    /// <inheritdoc />
    public bool CheckUpgrade => hostSettingsService.GetBoolean("CheckUpgrade", true);

    /// <inheritdoc />
    public string ControlPanel => hostSettingsService.GetString("ControlPanel", Globals.glbDefaultControlPanel);

    /// <inheritdoc />
    public bool DisableEditBar => hostSettingsService.GetBoolean("DisableEditBar", false);

    /// <inheritdoc />
    public bool AllowControlPanelToDetermineVisibility => hostSettingsService.GetBoolean("AllowControlPanelToDetermineVisibility", Globals.glbAllowControlPanelToDetermineVisibility);

    /// <inheritdoc />
    public bool CrmEnableCompositeFiles => hostSettingsService.GetBoolean(ClientResourceSettings.EnableCompositeFilesKey, false);

    /// <inheritdoc />
    public bool CrmMinifyCss => hostSettingsService.GetBoolean(ClientResourceSettings.MinifyCssKey);

    /// <inheritdoc />
    public bool CrmMinifyJs => hostSettingsService.GetBoolean(ClientResourceSettings.MinifyJsKey);

    /// <inheritdoc />
    public int CrmVersion => GetCrmVersion(hostSettingsService);

    /// <inheritdoc />
    public string DefaultAdminContainer
    {
        get
        {
            string setting = hostSettingsService.GetString("DefaultAdminContainer");
            if (string.IsNullOrEmpty(setting))
            {
                setting = SkinController.GetDefaultAdminContainer();
            }

            return setting;
        }
    }

    /// <inheritdoc />
    public string DefaultAdminSkin
    {
        get
        {
            string setting = hostSettingsService.GetString("DefaultAdminSkin");
            if (string.IsNullOrEmpty(setting))
            {
                setting = SkinController.GetDefaultAdminSkin();
            }

            return setting;
        }
    }

    /// <inheritdoc />
    public string DefaultDocType
    {
        get
        {
            string doctype = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">";
            string setting = hostSettingsService.GetString("DefaultDocType");
            if (!string.IsNullOrEmpty(setting))
            {
                switch (setting)
                {
                    case "0":
                        doctype = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">";
                        break;
                    case "1":
                        doctype = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";
                        break;
                    case "2":
                        doctype = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">";
                        break;
                    case "3":
                        doctype = "<!DOCTYPE html>";
                        break;
                }
            }

            return doctype;
        }
    }

    /// <inheritdoc />
    public string DefaultPortalContainer
    {
        get
        {
            string setting = hostSettingsService.GetString("DefaultPortalContainer");
            if (string.IsNullOrEmpty(setting))
            {
                setting = SkinController.GetDefaultPortalContainer();
            }

            return setting;
        }
    }

    /// <inheritdoc />
    public string DefaultPortalSkin
    {
        get
        {
            string setting = hostSettingsService.GetString("DefaultPortalSkin");
            if (string.IsNullOrEmpty(setting))
            {
                setting = SkinController.GetDefaultPortalSkin();
            }

            return setting;
        }
    }

    /// <inheritdoc />
    public bool DisplayBetaNotice => hostSettingsService.GetBoolean("DisplayBetaNotice", true);

    /// <inheritdoc />
    public bool EnableBannedList => hostSettingsService.GetBoolean("EnableBannedList", true);

    /// <inheritdoc />
    public bool EnableBrowserLanguage => hostSettingsService.GetBoolean("EnableBrowserLanguage", true);

    /// <inheritdoc />
    public bool EnableContentLocalization => hostSettingsService.GetBoolean("EnableContentLocalization", false);

    /// <inheritdoc />
    public bool DebugMode => hostSettingsService.GetBoolean("DebugMode", false);

    /// <inheritdoc />
    public bool EnableCustomModuleCssClass => hostSettingsService.GetBoolean("EnableCustomModuleCssClass", true);

    /// <inheritdoc />
    public bool UpgradeForceSsl => hostSettingsService.GetBoolean("UpgradeForceSSL", false);

    /// <inheritdoc />
    public string SslDomain => hostSettingsService.GetString("SSLDomain");

    /// <inheritdoc />
    public bool EnableFileAutoSync => hostSettingsService.GetBoolean("EnableFileAutoSync", false);

    /// <inheritdoc />
    public bool EnableIPChecking => hostSettingsService.GetBoolean("EnableIPChecking", false);

    /// <inheritdoc />
    public bool EnableModuleOnLineHelp => hostSettingsService.GetBoolean("EnableModuleOnLineHelp", false);

    /// <inheritdoc />
    public bool EnableRequestFilters => hostSettingsService.GetBoolean("EnableRequestFilters", false);

    /// <inheritdoc />
    public bool EnableStrengthMeter => hostSettingsService.GetBoolean("EnableStrengthMeter", false);

    /// <inheritdoc />
    public bool EnablePasswordHistory => hostSettingsService.GetBoolean("EnablePasswordHistory", true);

    /// <inheritdoc />
    public bool EnableUrlLanguage => hostSettingsService.GetBoolean("EnableUrlLanguage", true);

    /// <inheritdoc />
    public bool EventLogBuffer => hostSettingsService.GetBoolean("EventLogBuffer", false);

    /// <inheritdoc />
    public IFileExtensionAllowList AllowedExtensionAllowList => new FileExtensionWhitelist(hostSettingsService.GetString("FileExtensions"));

    /// <inheritdoc />
    public IFileExtensionAllowList DefaultEndUserExtensionAllowList => new FileExtensionWhitelist(hostSettingsService.GetString("DefaultEndUserExtensionWhitelist"));

    /// <inheritdoc />
    public string Guid => GetHostGuid(hostSettingsService);

    /// <inheritdoc />
    public string HelpUrl => hostSettingsService.GetString("HelpURL");

    /// <inheritdoc />
    public string HostEmail => hostSettingsService.GetString("HostEmail");

    /// <inheritdoc />
    public int HostPortalId => hostSettingsService.GetInteger("HostPortalId");

    /// <inheritdoc />
    public double HostSpace => hostSettingsService.GetDouble("HostSpace", 0);

    /// <inheritdoc />
    public string HostTitle => hostSettingsService.GetString("HostTitle");

    /// <inheritdoc />
    public string HostUrl => hostSettingsService.GetString("HostURL");

    /// <inheritdoc />
    public int HttpCompressionAlgorithm => hostSettingsService.GetInteger("HttpCompression");

    /// <inheritdoc />
    public int MessageSchedulerBatchSize => hostSettingsService.GetInteger("MessageSchedulerBatchSize", 50);

    /// <inheritdoc />
    public TimeSpan MembershipResetLinkValidity => TimeSpan.FromMinutes(hostSettingsService.GetInteger("MembershipResetLinkValidity", 60));

    /// <inheritdoc />
    public TimeSpan AdminMembershipResetLinkValidity => TimeSpan.FromMinutes(hostSettingsService.GetInteger("AdminMembershipResetLinkValidity", 1440));

    /// <inheritdoc />
    public int MembershipNumberPasswords => hostSettingsService.GetInteger("MembershipNumberPasswords", 5);

    /// <inheritdoc />
    public int MembershipDaysBeforePasswordReuse => hostSettingsService.GetInteger("MembershipDaysBeforePasswordReuse", 0);

    /// <inheritdoc />
    public string MembershipFailedIPException => hostSettingsService.GetString("MembershipFailedIPException", "403");

    /// <inheritdoc />
    public string ModuleCachingMethod => hostSettingsService.GetString("ModuleCaching");

    /// <inheritdoc />
    public string PageCachingMethod => hostSettingsService.GetString("PageCaching");

    /// <inheritdoc />
    public int PageQuota => hostSettingsService.GetInteger("PageQuota", 0);

    /// <inheritdoc />
    public string PageStatePersister
    {
        get
        {
            string setting = hostSettingsService.GetString("PageStatePersister");
            if (string.IsNullOrEmpty(setting))
            {
                setting = "P";
            }

            return setting;
        }
    }

    /// <inheritdoc />
    public TimeSpan PasswordExpiry => TimeSpan.FromDays(hostSettingsService.GetInteger("PasswordExpiry", 0));

    /// <inheritdoc />
    public TimeSpan PasswordExpiryReminder => TimeSpan.FromDays(hostSettingsService.GetInteger("PasswordExpiryReminder", 7));

    /// <inheritdoc />
    public string ProxyPassword => hostSettingsService.GetString("ProxyPassword");

    /// <inheritdoc />
    public int ProxyPort => hostSettingsService.GetInteger("ProxyPort");

    /// <inheritdoc />
    public string ProxyServer => hostSettingsService.GetString("ProxyServer");

    /// <inheritdoc />
    public string ProxyUsername => hostSettingsService.GetString("ProxyUsername");

    /// <inheritdoc />
    public bool RememberCheckbox => hostSettingsService.GetBoolean("RememberCheckbox", true);

    /// <inheritdoc />
    public SchedulerMode SchedulerMode => !Enum.TryParse<SchedulerMode>(hostSettingsService.GetString("SchedulerMode"), ignoreCase: true, out var schedulerMode) ? SchedulerMode.TimerMethod : schedulerMode;

    /// <inheritdoc />
    public TimeSpan SchedulerDelayAtAppStart => TimeSpan.FromSeconds(hostSettingsService.GetInteger("SchedulerdelayAtAppStart", 1));

    /// <inheritdoc />
    public bool SearchIncludeCommon => hostSettingsService.GetBoolean("SearchIncludeCommon", false);

    /// <inheritdoc />
    public bool SearchIncludeNumeric => hostSettingsService.GetBoolean("SearchIncludeNumeric", true);

    /// <inheritdoc />
    public int SearchMaxWordLength => hostSettingsService.GetInteger("MaxSearchWordLength", 50);

    /// <inheritdoc />
    public int SearchMinWordLength => hostSettingsService.GetInteger("MinSearchWordLength", 4);

    /// <inheritdoc />
    public string SearchIncludedTagInfoFilter => hostSettingsService.GetString("SearchIncludedTagInfoFilter", string.Empty);

    /// <inheritdoc />
    public bool ShowCriticalErrors => hostSettingsService.GetBoolean("ShowCriticalErrors", true);

    /// <inheritdoc />
    public bool ThrowCboExceptions => hostSettingsService.GetBoolean("ThrowCBOExceptions", false);

    /// <inheritdoc />
    public bool UseFriendlyUrls => hostSettingsService.GetBoolean("UseFriendlyUrls", false);

    /// <inheritdoc />
    public bool UseCustomErrorMessages => hostSettingsService.GetBoolean("UseCustomErrorMessages", false);

    /// <inheritdoc />
    public int UserQuota => hostSettingsService.GetInteger("UserQuota", 0);

    /// <inheritdoc />
    public TimeSpan WebRequestTimeout => TimeSpan.FromMilliseconds(hostSettingsService.GetInteger("WebRequestTimeout", 10000));

    /// <inheritdoc />
    public bool EnableMsAjaxCdn => hostSettingsService.GetBoolean("EnableMsAjaxCDN", false);

    /// <inheritdoc />
    public TimeSpan AsyncTimeout => TimeSpan.FromMinutes(Math.Max(90, hostSettingsService.GetInteger("AsyncTimeout", 90)));

    /// <inheritdoc />
    public bool IsLocked => hostSettingsService.GetBoolean("IsLocked", false);

    /// <inheritdoc />
    public PerformanceSettings PerformanceSetting
    {
        get => !Enum.TryParse<PerformanceSettings>(hostSettingsService.GetString("PerformanceSetting"), out var performanceSetting) ? PerformanceSettings.ModerateCaching : performanceSetting;
        set => hostSettingsService.Update("PerformanceSetting", value.ToString());
    }

    /// <summary>Gets the Client Resource Management version number.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <returns>The version number.</returns>
    public static int GetCrmVersion(IHostSettingsService hostSettingsService)
    {
        return hostSettingsService.GetInteger(ClientResourceSettings.VersionKey, 1);
    }

    /// <summary>Get the host GUID.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <returns>The GUID string.</returns>
    public static string GetHostGuid(IHostSettingsService hostSettingsService)
    {
        return hostSettingsService.GetString("GUID");
    }

    /// <inheritdoc />
    public string SmtpAuthentication(int portalId) => this.GetSmtpSetting(portalId, "SMTPAuthentication");

    /// <inheritdoc />
    public string SmtpPassword(int portalId)
    {
        if (this.SmtpPortalEnabled(portalId))
        {
            return PortalController.GetEncryptedString(this, "SMTPPassword", portalId, Config.GetDecryptionkey());
        }

        string decryptedText;
        try
        {
            decryptedText = hostSettingsService.GetEncryptedString("SMTPPassword", Config.GetDecryptionkey());
        }
        catch (Exception)
        {
            // fixes case where SMTP Password failed to encrypt due to failing upgrade
            var current = hostSettingsService.GetString("SMTPPassword");
            if (!string.IsNullOrEmpty(current))
            {
                hostSettingsService.UpdateEncryptedString("SMTPPassword", current, Config.GetDecryptionkey());
                decryptedText = current;
            }
            else
            {
                decryptedText = string.Empty;
            }
        }

        return decryptedText;
    }

    /// <inheritdoc />
    public string SmtpServer(int portalId) => this.GetSmtpSetting(portalId, "SMTPServer");

    /// <inheritdoc />
    public string SmtpUsername(int portalId) => this.GetSmtpSetting(portalId, "SMTPUsername");

    /// <inheritdoc />
    public int SmtpConnectionLimit(int portalId)
    {
        if (this.SmtpPortalEnabled(portalId))
        {
            return PortalController.GetPortalSettingAsInteger("SMTPConnectionLimit", portalId, 2);
        }

        return hostSettingsService.GetInteger("SMTPConnectionLimit", 2);
    }

    /// <inheritdoc />
    public TimeSpan SmtpMaxIdleTime(int portalId)
    {
        var idleMilliseconds = this.SmtpPortalEnabled(portalId)
            ? PortalController.GetPortalSettingAsInteger("SMTPMaxIdleTime", portalId, 100000)
            : hostSettingsService.GetInteger("SMTPMaxIdleTime", 100000);
        return TimeSpan.FromMilliseconds(idleMilliseconds);
    }

    /// <inheritdoc />
    public bool EnableSmtpSsl(int portalId)
    {
        if (this.SmtpPortalEnabled(portalId))
        {
            return PortalController.GetPortalSettingAsBoolean("SMTPEnableSSL", portalId, false);
        }

        return hostSettingsService.GetBoolean("SMTPEnableSSL", false);
    }

    /// <inheritdoc />
    public string SmtpAuthProvider(int portalId)
    {
        if (this.SmtpPortalEnabled(portalId))
        {
            return PortalController.GetPortalSetting("SMTPAuthProvider", portalId, string.Empty);
        }

        return hostSettingsService.GetString("SMTPAuthProvider", string.Empty);
    }

    /// <inheritdoc />
    public bool SmtpPortalEnabled(int portalId)
    {
        var currentSmtpMode = PortalController.GetPortalSetting("SMTPmode", portalId, Null.NullString);
        return currentSmtpMode.Equals("P", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Gets the SMTP setting, if portal SMTP is configured, it will return items from the portal settings collection.</summary>
    private string GetSmtpSetting(int portalId, string settingName)
    {
        if (this.SmtpPortalEnabled(portalId))
        {
            return PortalController.GetPortalSetting(settingName, portalId, Null.NullString);
        }

        return hostSettingsService.GetString(settingName);
    }
}
