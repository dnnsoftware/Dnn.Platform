// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExchangeOnlineAuthProvider.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Abstractions.Security;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Common.Extensions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail.OAuth;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

/// <inheritdoc />
public class ExchangeOnlineOAuthProvider : ISmtpOAuthProvider
{
    private readonly IHostSettingsService hostSettingsService;
    private readonly IPortalAliasService portalAliasService;
    private readonly IPortalController portalController;
    private readonly IHostSettings hostSettings;
    private readonly ICryptographyProvider cryptographyProvider;

    /// <summary>Initializes a new instance of the <see cref="ExchangeOnlineOAuthProvider"/> class.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public ExchangeOnlineOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService)
        : this(hostSettingsService, portalAliasService, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ExchangeOnlineOAuthProvider"/> class.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
    public ExchangeOnlineOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService, IHostSettings hostSettings, IPortalController portalController)
        : this(hostSettingsService, portalAliasService, hostSettings, portalController, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ExchangeOnlineOAuthProvider"/> class.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="cryptographyProvider">The cryptography provider.</param>
    public ExchangeOnlineOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService, IHostSettings hostSettings, IPortalController portalController, ICryptographyProvider cryptographyProvider)
    {
        this.hostSettingsService = hostSettingsService;
        this.portalAliasService = portalAliasService;
        this.hostSettings = hostSettings ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ?? new HostSettings(hostSettingsService);
        this.portalController = portalController ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
        this.cryptographyProvider = cryptographyProvider ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<ICryptographyProvider>();
    }

    /// <inheritdoc />
    public string Name => Constants.Name;

    /// <inheritdoc />
    public string LocalizedName => Localization.GetSafeJSString(this.Name, Constants.LocalResourcesFile);

    /// <inheritdoc />
    public bool IsAuthorized(int portalId)
    {
        var clientApplication = CreateClientApplication(this, this.hostSettingsService, this.hostSettings, this.portalController, portalId);
        if (clientApplication == null)
        {
            return false;
        }

        var accounts = clientApplication.GetAccountsAsync().Result;

        return accounts?.Any() ?? false;
    }

    /// <inheritdoc />
    public async Task<bool> IsAuthorizedAsync(int portalId, CancellationToken cancellationToken = default)
    {
        var clientApplication = CreateClientApplication(this, this.hostSettingsService, this.hostSettings, this.portalController, portalId);
        if (clientApplication == null)
        {
            return false;
        }

        var accounts = await clientApplication.GetAccountsAsync(cancellationToken);
        return accounts?.Any() ?? false;
    }

    /// <inheritdoc />
    public string GetAuthorizeUrl(int portalId)
    {
        var clientId = this.GetSetting(portalId, Constants.ClientIdSettingName);
        var clientSecret = this.GetSetting(portalId, Constants.ClientSecretSettingName);

        if (string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(clientSecret))
        {
            return string.Empty;
        }

        var portalSettings = new PortalSettings(portalId == Null.NullInteger ? this.hostSettings.HostPortalId : portalId);
        var portalAlias = this.portalAliasService.GetPortalAliasesByPortalId(portalId == Null.NullInteger ? this.hostSettings.HostPortalId : portalId)
            .OrderByDescending(a => a.IsPrimary)
            .First();
        var sslEnabled = portalSettings.SSLEnabled && portalSettings.SSLSetup == SiteSslSetup.On;

        var siteUrl = $"{(sslEnabled ? "https" : "http")}://{portalAlias.HttpAlias}";

        return string.Format(Constants.CallbackUrl, siteUrl, portalId);
    }

    /// <inheritdoc />
    public IList<SmtpOAuthSetting> GetSettings(int portalId)
    {
        return portalId > Null.NullInteger ? this.GetSettingsFromPortal(portalId) : this.GetSettingsFromHost();
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
            this.DeleteSetting(portalId, Constants.AuthenticationSettingName);
        }

        return changed;
    }

    /// <inheritdoc />
    public void Authorize(int portalId, IOAuth2SmtpClient smtpClient)
    {
        if (!this.IsAuthorized(portalId))
        {
            return;
        }

        var clientApplication = CreateClientApplication(this, this.hostSettingsService, this.hostSettings, this.portalController, portalId);
        var account = clientApplication.GetAccountsAsync().Result.First();
        var scopes = GetAuthenticationScopes();
        var result = clientApplication.AcquireTokenSilent(scopes, account).ExecuteAsync().Result;
        if (result != null)
        {
            var username = result.Account.Username;
            var accessToken = result.AccessToken;

            smtpClient.Authenticate(username, accessToken);
        }
    }

    /// <inheritdoc />
    public async Task AuthorizeAsync(int portalId, IOAuth2SmtpClient smtpClient, CancellationToken cancellationToken = default)
    {
        if (!await this.IsAuthorizedAsync(portalId, cancellationToken))
        {
            return;
        }

        var clientApplication = CreateClientApplication(this, this.hostSettingsService, this.hostSettings, this.portalController, portalId);
        var accounts = await clientApplication.GetAccountsAsync(cancellationToken);
        var account = accounts.First();
        var scopes = GetAuthenticationScopes();
        var result = await clientApplication.AcquireTokenSilent(scopes, account).ExecuteAsync(cancellationToken);
        if (result != null)
        {
            var username = result.Account.Username;
            var accessToken = result.AccessToken;

            await smtpClient.AuthenticateAsync(username, accessToken, cancellationToken);
        }
    }

    /// <summary>Create the authentication client application.</summary>
    /// <param name="smtpOAuthController">The SMTP OAuth controller.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The client application.</returns>
    internal static ConfidentialClientApplication CreateClientApplication(ISmtpOAuthController smtpOAuthController, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController, int portalId)
    {
        return CreateClientApplication(smtpOAuthController.GetOAuthProvider(Constants.Name), hostSettingsService, hostSettings, portalController, portalId);
    }

    /// <summary>Get the authentication scopes list.</summary>
    /// <returns>The scopes.</returns>
    internal static IList<string> GetAuthenticationScopes() => ["https://outlook.office365.com/.default",];

    private static ConfidentialClientApplication CreateClientApplication(ISmtpOAuthProvider authProvider, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController, int portalId)
    {
        var settings = authProvider.GetSettings(portalId);
        var tenantId = settings.FirstOrDefault(i => i.Name == Constants.TenantIdSettingName)?.Value ?? string.Empty;
        var clientId = settings.FirstOrDefault(i => i.Name == Constants.ClientIdSettingName)?.Value ?? string.Empty;
        var clientSecret = settings.FirstOrDefault(i => i.Name == Constants.ClientSecretSettingName)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret))
        {
            return null;
        }

        var redirectUrl = authProvider.GetAuthorizeUrl(portalId);
        var questionMarkIndex = redirectUrl.IndexOf('?');
        if (questionMarkIndex > -1)
        {
            redirectUrl = redirectUrl.Substring(0, questionMarkIndex);
        }

        var options = new ConfidentialClientApplicationOptions
        {
            TenantId = tenantId,
            ClientId = clientId,
            ClientSecret = clientSecret,
            RedirectUri = redirectUrl,
            Instance = Constants.AzureInstance,
        };
        var clientApplication = (ConfidentialClientApplication)ConfidentialClientApplicationBuilder
            .CreateWithApplicationOptions(options)
            .Build();

        var tokenCacheHelper = new TokenCacheHelper(portalId, hostSettingsService, hostSettings, portalController);
        tokenCacheHelper.EnableSerialization(clientApplication.UserTokenCache);

        return clientApplication;
    }

    private IList<SmtpOAuthSetting> GetSettingsFromPortal(int portalId)
    {
        var portalSettings = PortalController.Instance.GetPortalSettings(portalId);
        if (portalSettings == null)
        {
            throw new ArgumentException("Invalid portal Id.");
        }

        var tenantId = portalSettings.GetValueOrDefault(Constants.TenantIdSettingName, string.Empty);
        var clientId = portalSettings.GetValueOrDefault(Constants.ClientIdSettingName, string.Empty);
        var clientSecret = portalSettings.GetValueOrDefault(Constants.ClientSecretSettingName, string.Empty);
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            var algorithmName = portalSettings.GetValueOrDefault(Constants.ClientSecretEncryptionAlgorithm, string.Empty);
            if (!string.IsNullOrWhiteSpace(algorithmName))
            {
                var initializationVector = portalSettings.GetValueOrDefault(Constants.ClientSecretInitializationVector, string.Empty);
                clientSecret = this.cryptographyProvider.DecryptString(clientSecret, Config.GetDecryptionkey(), algorithmName, initializationVector);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                clientSecret = PortalSecurity.Instance.Decrypt(Config.GetDecryptionkey(), clientSecret);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        return
        [
            new SmtpOAuthSetting
            {
                Name = Constants.TenantIdSettingName,
                Value = tenantId,
                Label = Localization.GetString("TenantId", Constants.LocalResourcesFile),
                Help = Localization.GetString("TenantId.Help", Constants.LocalResourcesFile),
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
        ];
    }

    private IList<SmtpOAuthSetting> GetSettingsFromHost()
    {
        var tenantId = this.hostSettingsService.GetString(Constants.TenantIdSettingName, string.Empty);
        var clientId = this.hostSettingsService.GetString(Constants.ClientIdSettingName, string.Empty);
        var clientSecret = this.hostSettingsService.GetString(Constants.ClientSecretSettingName, string.Empty);
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            var algorithmName = this.hostSettingsService.GetString(Constants.ClientSecretEncryptionAlgorithm, string.Empty);
            if (!string.IsNullOrWhiteSpace(algorithmName))
            {
                var initializationVector = this.hostSettingsService.GetString(Constants.ClientSecretInitializationVector, string.Empty);
                clientSecret = this.cryptographyProvider.DecryptString(clientSecret, Config.GetDecryptionkey(), algorithmName, initializationVector);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                clientSecret = PortalSecurity.Instance.Decrypt(Config.GetDecryptionkey(), clientSecret);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        return new List<SmtpOAuthSetting>
        {
            new SmtpOAuthSetting
            {
                Name = Constants.TenantIdSettingName,
                Value = tenantId,
                Label = Localization.GetString("TenantId", Constants.LocalResourcesFile),
                Help = Localization.GetString("TenantId.Help", Constants.LocalResourcesFile),
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
        var tenantId = this.GetSetting(Null.NullInteger, Constants.TenantIdSettingName);
        var clientId = this.GetSetting(Null.NullInteger, Constants.ClientIdSettingName);
        var clientSecret = this.GetSetting(Null.NullInteger, Constants.ClientSecretSettingName);
        var clientSecretEncryptionAlgorithm = this.GetSetting(Null.NullInteger, Constants.ClientSecretEncryptionAlgorithm);

        var changed = false;
        if (settings.ContainsKey(Constants.TenantIdSettingName) && settings[Constants.TenantIdSettingName] != tenantId)
        {
            this.hostSettingsService.Update(Constants.TenantIdSettingName, settings[Constants.TenantIdSettingName], false);
            changed = true;
        }

        if (settings.ContainsKey(Constants.ClientIdSettingName) && settings[Constants.ClientIdSettingName] != clientId)
        {
            this.hostSettingsService.Update(Constants.ClientIdSettingName, settings[Constants.ClientIdSettingName], false);
            changed = true;
        }

        if (settings.ContainsKey(Constants.ClientSecretSettingName))
        {
            if (settings[Constants.ClientSecretSettingName] != clientSecret || string.IsNullOrWhiteSpace(clientSecretEncryptionAlgorithm))
            {
                var (encryptedSecret, algorithmName, initializationVector) = this.cryptographyProvider.EncryptString(settings[Constants.ClientSecretSettingName], Config.GetDecryptionkey());
                this.hostSettingsService.Update(Constants.ClientSecretSettingName, encryptedSecret, false);
                this.hostSettingsService.Update(Constants.ClientSecretEncryptionAlgorithm, algorithmName, false);
                this.hostSettingsService.Update(Constants.ClientSecretInitializationVector, initializationVector, false);
                changed = true;
            }
        }

        if (changed)
        {
            DataCache.ClearCache();
        }

        return changed;
    }

    private bool UpdatePortalSettings(int portalId, IDictionary<string, string> settings)
    {
        var tenantId = this.GetSetting(portalId, Constants.TenantIdSettingName);
        var clientId = this.GetSetting(portalId, Constants.ClientIdSettingName);
        var clientSecret = this.GetSetting(portalId, Constants.ClientSecretSettingName);
        var clientSecretEncryptionAlgorithm = this.GetSetting(portalId, Constants.ClientSecretEncryptionAlgorithm);

        var changed = false;
        if (settings.ContainsKey(Constants.TenantIdSettingName) && settings[Constants.TenantIdSettingName] != tenantId)
        {
            PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.TenantIdSettingName, settings[Constants.TenantIdSettingName], false);
            changed = true;
        }

        if (settings.ContainsKey(Constants.ClientIdSettingName) && settings[Constants.ClientIdSettingName] != clientId)
        {
            PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.ClientIdSettingName, settings[Constants.ClientIdSettingName], false);
            changed = true;
        }

        if (settings.ContainsKey(Constants.ClientSecretSettingName))
        {
            if (settings[Constants.ClientSecretSettingName] != clientSecret || string.IsNullOrWhiteSpace(clientSecretEncryptionAlgorithm))
            {
                var (encryptedSecret, algorithmName, initializationVector) = this.cryptographyProvider.EncryptString(settings[Constants.ClientSecretSettingName], Config.GetDecryptionkey());
                PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.ClientSecretSettingName, encryptedSecret, false);
                PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.ClientSecretEncryptionAlgorithm, algorithmName, false);
                PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.ClientSecretInitializationVector, initializationVector, false);
                changed = true;
            }
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
