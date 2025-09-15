// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Host;

using System;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;

/// <summary>The default <see cref="IMailSettings"/> implementation.</summary>
/// <param name="hostSettingsService">The host settings service.</param>
/// <param name="hostSettings">The host settings.</param>
public class MailSettings(IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController) : IMailSettings
{
    /// <inheritdoc />
    public string GetAuthentication(int portalId) => this.GetSmtpSetting(portalId, "SMTPAuthentication");

    /// <inheritdoc />
    public string GetPassword(int portalId)
    {
        if (this.IsPortalEnabled(portalId))
        {
            return PortalController.GetEncryptedString(hostSettings, portalController, "SMTPPassword", portalId, Config.GetDecryptionkey());
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
    public string GetServer(int portalId) => this.GetSmtpSetting(portalId, "SMTPServer");

    /// <inheritdoc />
    public string GetUsername(int portalId) => this.GetSmtpSetting(portalId, "SMTPUsername");

    /// <inheritdoc />
    public int GetConnectionLimit(int portalId)
    {
        if (this.IsPortalEnabled(portalId))
        {
            return PortalController.GetPortalSettingAsInteger("SMTPConnectionLimit", portalId, 2);
        }

        return hostSettingsService.GetInteger("SMTPConnectionLimit", 2);
    }

    /// <inheritdoc />
    public TimeSpan GetMaxIdleTime(int portalId)
    {
        var idleMilliseconds = this.IsPortalEnabled(portalId)
            ? PortalController.GetPortalSettingAsInteger("SMTPMaxIdleTime", portalId, 100000)
            : hostSettingsService.GetInteger("SMTPMaxIdleTime", 100000);
        return TimeSpan.FromMilliseconds(idleMilliseconds);
    }

    /// <inheritdoc />
    public bool GetSecureConnectionEnabled(int portalId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public string GetAuthProvider(int portalId)
    {
        if (this.IsPortalEnabled(portalId))
        {
            return PortalController.GetPortalSetting("SMTPAuthProvider", portalId, string.Empty);
        }

        return hostSettingsService.GetString("SMTPAuthProvider", string.Empty);
    }

    /// <inheritdoc />
    public bool IsPortalEnabled(int portalId)
    {
        var currentSmtpMode = PortalController.GetPortalSetting("SMTPmode", portalId, Null.NullString);
        return currentSmtpMode.Equals("P", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Gets the SMTP setting, if portal SMTP is configured, it will return items from the portal settings collection.</summary>
    private string GetSmtpSetting(int portalId, string settingName)
    {
        if (this.IsPortalEnabled(portalId))
        {
            return PortalController.GetPortalSetting(settingName, portalId, Null.NullString);
        }

        return hostSettingsService.GetString(settingName);
    }
}
