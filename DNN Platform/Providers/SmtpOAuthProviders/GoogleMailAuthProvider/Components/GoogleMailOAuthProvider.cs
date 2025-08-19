// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.GoogleMailAuthProvider.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Common.Extensions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail.OAuth;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;

using Microsoft.Extensions.DependencyInjection;

/// <inheritdoc/>
public class GoogleMailOAuthProvider : ISmtpOAuthProvider
{
    private readonly IHostSettingsService hostSettingsService;
    private readonly IPortalAliasService portalAliasService;
    private readonly IHostSettings hostSettings;
    private readonly IPortalController portalController;

    /// <summary>Initializes a new instance of the <see cref="GoogleMailOAuthProvider"/> class.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public GoogleMailOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService)
        : this(hostSettingsService, portalAliasService, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GoogleMailOAuthProvider"/> class.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal contoller.</param>
    public GoogleMailOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService, IHostSettings hostSettings, IPortalController portalController)
    {
        this.hostSettingsService = hostSettingsService;
        this.portalAliasService = portalAliasService;
        this.hostSettings = hostSettings ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ?? new HostSettings(hostSettingsService);
        this.portalController = portalController ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
    }

    /// <inheritdoc />
    public string Name => Constants.Name;

    /// <inheritdoc />
    public string LocalizedName => Localization.GetSafeJSString(this.Name, Constants.LocalResourcesFile);

    /// <inheritdoc />
    public bool IsAuthorized(int portalId)
    {
        return Task.Run(() => this.IsAuthorizedAsync(portalId)).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<bool> IsAuthorizedAsync(int portalId, CancellationToken cancellationToken = default)
    {
        var accountEmail = this.GetSetting(portalId, Constants.AccountEmailSettingName);
        if (string.IsNullOrWhiteSpace(accountEmail))
        {
            return false;
        }

        var credential = await new GoogleCredentialDataStore(portalId, this.hostSettingsService, this.hostSettings, this.portalController).GetAsync<TokenResponse>(accountEmail);

        return !string.IsNullOrWhiteSpace(credential?.AccessToken);
    }

    /// <inheritdoc />
    public string GetAuthorizeUrl(int portalId)
    {
        var accountEmail = this.GetSetting(portalId, Constants.AccountEmailSettingName);
        var clientId = this.GetSetting(portalId, Constants.ClientIdSettingName);
        var clientSecret = this.GetSetting(portalId, Constants.ClientSecretSettingName);

        if (string.IsNullOrWhiteSpace(accountEmail)
            || string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(clientSecret))
        {
            return string.Empty;
        }

        var portalSettings = new PortalSettings(portalId == Null.NullInteger ? this.hostSettings.HostPortalId : portalId);
        var portalAlias = this.portalAliasService.GetPortalAliasesByPortalId(portalId == Null.NullInteger ? this.hostSettings.HostPortalId : portalId)
            .OrderByDescending(a => a.IsPrimary)
            .First();
        var sslEnabled = portalSettings.SSLEnabled && portalSettings.SSLSetup == DotNetNuke.Abstractions.Security.SiteSslSetup.On;

        var siteUrl = $"{(sslEnabled ? "https" : "http")}://{portalAlias.HttpAlias}";

        return string.Format(Constants.CallbackUrl, siteUrl, portalId);
    }

    /// <inheritdoc />
    public IList<SmtpOAuthSetting> GetSettings(int portalId)
    {
        return portalId > Null.NullInteger ? GetSettingsFromPortal(portalId) : this.GetSettingsFromHost();
    }

    /// <inheritdoc />
    public bool UpdateSettings(int portalId, IDictionary<string, string> settings, out IList<string> errorMessages)
    {
        errorMessages = new List<string>();
        bool changed;

        if (portalId == Null.NullInteger)
        {
            changed = this.UpdateHostSettings(settings);
        }
        else
        {
            changed = this.UpdatePortalSettings(portalId, settings);
        }

        if (changed)
        {
            var settingName = string.Format(Constants.DataStoreSettingName, portalId);
            this.DeleteSetting(portalId, settingName);
        }

        return changed;
    }

    /// <inheritdoc />
    public void Authorize(int portalId, IOAuth2SmtpClient smtpClient)
    {
        var settings = this.GetSettings(portalId);
        var accountEmail = settings.FirstOrDefault(i => i.Name == Constants.AccountEmailSettingName)?.Value ?? string.Empty;
        var codeFlow = this.CreateAuthorizationCodeFlow(portalId);
        if (codeFlow == null)
        {
            return;
        }

        var response = new GoogleCredentialDataStore(portalId, this.hostSettingsService, this.hostSettings, this.portalController).GetAsync<TokenResponse>(accountEmail).Result;
        var credential = new UserCredential(codeFlow, accountEmail, response);
        if (credential.Token.IsStale)
        {
            _ = credential.RefreshTokenAsync(CancellationToken.None).Result;
        }

        smtpClient.Authenticate(credential.UserId, credential.Token.AccessToken);
    }

    /// <inheritdoc />
    public async Task AuthorizeAsync(int portalId, IOAuth2SmtpClient smtpClient, CancellationToken cancellationToken)
    {
        var settings = this.GetSettings(portalId);
        var accountEmail = settings.FirstOrDefault(i => i.Name == Constants.AccountEmailSettingName)?.Value ?? string.Empty;
        var codeFlow = this.CreateAuthorizationCodeFlow(portalId);
        if (codeFlow == null)
        {
            return;
        }

        var response = await new GoogleCredentialDataStore(portalId, this.hostSettingsService, this.hostSettings, this.portalController).GetAsync<TokenResponse>(accountEmail);
        var credential = new UserCredential(codeFlow, accountEmail, response);
        if (credential.Token.IsStale)
        {
            _ = await credential.RefreshTokenAsync(cancellationToken);
        }

        await smtpClient.AuthenticateAsync(credential.UserId, credential.Token.AccessToken, cancellationToken);
    }

    /// <summary>Create authorization code flow.</summary>
    /// <param name="smtpOAuthController">The SMTP OAuth controller.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The authorization code flow.</returns>
    internal static IAuthorizationCodeFlow CreateAuthorizationCodeFlow(ISmtpOAuthController smtpOAuthController, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController, int portalId)
    {
        return CreateAuthorizationCodeFlow(smtpOAuthController.GetOAuthProvider(Constants.Name), hostSettingsService, hostSettings, portalController, portalId);
    }

    private static IAuthorizationCodeFlow CreateAuthorizationCodeFlow(ISmtpOAuthProvider authProvider, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController, int portalId)
    {
        var settings = authProvider.GetSettings(portalId);
        var accountEmail = settings.FirstOrDefault(i => i.Name == Constants.AccountEmailSettingName)?.Value ?? string.Empty;
        var clientId = settings.FirstOrDefault(i => i.Name == Constants.ClientIdSettingName)?.Value ?? string.Empty;
        var clientSecret = settings.FirstOrDefault(i => i.Name == Constants.ClientSecretSettingName)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(accountEmail) ||
            string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret))
        {
            return null;
        }

        var clientSecrets = new ClientSecrets
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
        };

        return new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer
            {
                DataStore = new GoogleCredentialDataStore(portalId, hostSettingsService, hostSettings, portalController),
                Scopes = new[] { "https://mail.google.com/", },
                ClientSecrets = clientSecrets,
                Prompt = "consent",
            });
    }

    private static IList<SmtpOAuthSetting> GetSettingsFromPortal(int portalId)
    {
        var portalSettings = PortalController.Instance.GetPortalSettings(portalId);
        if (portalSettings == null)
        {
            throw new ArgumentException("Invalid portal Id.");
        }

        var accountEmail = portalSettings.GetValueOrDefault(Constants.AccountEmailSettingName, string.Empty);
        var clientId = portalSettings.GetValueOrDefault(Constants.ClientIdSettingName, string.Empty);
        var clientSecret = portalSettings.GetValueOrDefault(Constants.ClientSecretSettingName, string.Empty);
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            clientSecret = PortalSecurity.Instance.Decrypt(Config.GetDecryptionkey(), clientSecret);
        }

        return new List<SmtpOAuthSetting>
        {
            new SmtpOAuthSetting
            {
                Name = Constants.AccountEmailSettingName,
                Value = accountEmail,
                Label = Localization.GetString("AccountEmail", Constants.LocalResourcesFile),
                Help = Localization.GetString("AccountEmail.Help", Constants.LocalResourcesFile),
                IsRequired = true,
            },
            new SmtpOAuthSetting
            {
                Name = Constants.ClientIdSettingName,
                Value = clientId,
                Label = Localization.GetString("ClientId", Constants.LocalResourcesFile),
                Help = Localization.GetString("ClientId.Help", Constants.LocalResourcesFile),
                IsRequired = true,
            },
            new SmtpOAuthSetting
            {
                Name = Constants.ClientSecretSettingName,
                Value = clientSecret,
                Label = Localization.GetString("ClientSecret", Constants.LocalResourcesFile),
                Help = Localization.GetString("ClientSecret.Help", Constants.LocalResourcesFile),
                IsSecure = true,
                IsRequired = true,
            },
        };
    }

    /// <summary>Create authorization code flow.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The authorization code flow.</returns>
    private IAuthorizationCodeFlow CreateAuthorizationCodeFlow(int portalId)
    {
        return CreateAuthorizationCodeFlow(this, this.hostSettingsService, this.hostSettings, this.portalController, portalId);
    }

    private IList<SmtpOAuthSetting> GetSettingsFromHost()
    {
        var hostSettings = this.hostSettingsService.GetSettingsDictionary().ToDictionary(i => i.Key, i => i.Value);

        var accountEmail = hostSettings.GetValueOrDefault(Constants.AccountEmailSettingName, string.Empty);
        var clientId = hostSettings.GetValueOrDefault(Constants.ClientIdSettingName, string.Empty);
        var clientSecret = hostSettings.GetValueOrDefault(Constants.ClientSecretSettingName, string.Empty);
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            clientSecret = PortalSecurity.Instance.Decrypt(Config.GetDecryptionkey(), clientSecret);
        }

        return new List<SmtpOAuthSetting>
        {
            new SmtpOAuthSetting
            {
                Name = Constants.AccountEmailSettingName,
                Value = accountEmail,
                Label = Localization.GetString("AccountEmail", Constants.LocalResourcesFile),
                Help = Localization.GetString("AccountEmail.Help", Constants.LocalResourcesFile),
                IsRequired = true,
            },
            new SmtpOAuthSetting
            {
                Name = Constants.ClientIdSettingName,
                Value = clientId,
                Label = Localization.GetString("ClientId", Constants.LocalResourcesFile),
                Help = Localization.GetString("ClientId.Help", Constants.LocalResourcesFile),
                IsRequired = true,
            },
            new SmtpOAuthSetting
            {
                Name = Constants.ClientSecretSettingName,
                Value = clientSecret,
                Label = Localization.GetString("ClientSecret", Constants.LocalResourcesFile),
                Help = Localization.GetString("ClientSecret.Help", Constants.LocalResourcesFile),
                IsSecure = true,
                IsRequired = true,
            },
        };
    }

    private bool UpdateHostSettings(IDictionary<string, string> settings)
    {
        var accountEmail = this.GetSetting(Null.NullInteger, Constants.AccountEmailSettingName);
        var clientId = this.GetSetting(Null.NullInteger, Constants.ClientIdSettingName);
        var clientSecret = this.GetSetting(Null.NullInteger, Constants.ClientSecretSettingName);

        var changed = false;
        if (settings.ContainsKey(Constants.AccountEmailSettingName) && settings[Constants.AccountEmailSettingName] != accountEmail)
        {
            this.hostSettingsService.Update(Constants.AccountEmailSettingName, settings[Constants.AccountEmailSettingName], false);
            changed = true;
        }

        if (settings.ContainsKey(Constants.ClientIdSettingName) && settings[Constants.ClientIdSettingName] != clientId)
        {
            this.hostSettingsService.Update(Constants.ClientIdSettingName, settings[Constants.ClientIdSettingName], false);
            changed = true;
        }

        if (settings.ContainsKey(Constants.ClientSecretSettingName) && settings[Constants.ClientSecretSettingName] != clientSecret)
        {
            var encryptedSecret = PortalSecurity.Instance.Encrypt(Config.GetDecryptionkey(), settings[Constants.ClientSecretSettingName]);
            this.hostSettingsService.Update(Constants.ClientSecretSettingName, encryptedSecret, false);
            changed = true;
        }

        if (changed)
        {
            DataCache.ClearCache();
        }

        return changed;
    }

    private bool UpdatePortalSettings(int portalId, IDictionary<string, string> settings)
    {
        var accountEmail = this.GetSetting(portalId, Constants.AccountEmailSettingName);
        var clientId = this.GetSetting(portalId, Constants.ClientIdSettingName);
        var clientSecret = this.GetSetting(portalId, Constants.ClientSecretSettingName);

        var changed = false;
        if (settings.ContainsKey(Constants.AccountEmailSettingName) && settings[Constants.AccountEmailSettingName] != accountEmail)
        {
            PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.AccountEmailSettingName, settings[Constants.AccountEmailSettingName], false);
            changed = true;
        }

        if (settings.ContainsKey(Constants.ClientIdSettingName) && settings[Constants.ClientIdSettingName] != clientId)
        {
            PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.ClientIdSettingName, settings[Constants.ClientIdSettingName], false);
            changed = true;
        }

        if (settings.ContainsKey(Constants.ClientSecretSettingName) && settings[Constants.ClientSecretSettingName] != clientSecret)
        {
            var encryptedSecret = PortalSecurity.Instance.Encrypt(Config.GetDecryptionkey(), settings[Constants.ClientSecretSettingName]);
            PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.ClientSecretSettingName, encryptedSecret, false);
            changed = true;
        }

        if (changed)
        {
            DataCache.ClearPortalCache(portalId, false);
        }

        return changed;
    }

    private string GetSetting(int portalId, string settingName)
    {
        var settings = this.GetSettings(portalId);
        var setting = settings.FirstOrDefault(i => i.Name == settingName);

        return setting?.Value ?? string.Empty;
    }

    private void DeleteSetting(int portalId, string settingName)
    {
        if (portalId == Null.NullInteger)
        {
            this.hostSettingsService.Update(settingName, string.Empty, false);
        }
        else
        {
            PortalController.UpdatePortalSetting(this.portalController, portalId, settingName, string.Empty, false);
        }
    }
}
