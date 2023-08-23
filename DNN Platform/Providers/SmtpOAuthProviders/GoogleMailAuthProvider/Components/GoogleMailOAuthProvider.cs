// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.GoogleMailAuthProvider.Components
{
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
    using DotNetNuke.Web;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Auth.OAuth2.Flows;
    using Google.Apis.Auth.OAuth2.Responses;
    using Google.Apis.Util;
    using MailKit.Net.Smtp;
    using MailKit.Security;

    /// <inheritdoc/>
    public class GoogleMailOAuthProvider : ISmtpOAuthProvider
    {
        private readonly IServiceProvider serviceProvider = GetServiceProvider();

        /// <summary>
        /// Gets provider name.
        /// </summary>
        public string Name => Constants.Name;

        /// <summary>
        /// Gets the localized name.
        /// </summary>
        public string LocalizedName => Localization.GetSafeJSString(this.Name, Constants.LocalResourcesFile);

        /// <summary>
        /// Whether the provider completed the authorize process.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>status.</returns>
        public bool IsAuthorized(int portalId)
        {
            var accountEmail = this.GetSetting(portalId, Constants.AccountEmailSettingName);
            if (string.IsNullOrWhiteSpace(accountEmail))
            {
                return false;
            }

            var credential = new GoogleCredentialDataStore(portalId).GetAsync<TokenResponse>(accountEmail).Result;

            return !string.IsNullOrWhiteSpace(credential?.AccessToken);
        }

        /// <summary>
        /// Get the authorize url.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>The authorize url.</returns>
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

            var portalSettings = new PortalSettings(portalId == Null.NullInteger ? Host.HostPortalID : portalId);
            var portalAlias = this.GetService<IPortalAliasService>().GetPortalAliasesByPortalId(portalId == Null.NullInteger ? Host.HostPortalID : portalId)
                .OrderByDescending(a => a.IsPrimary)
                .FirstOrDefault();
            var sslEnabled = portalSettings.SSLEnabled && portalSettings.SSLSetup == DotNetNuke.Abstractions.Security.SiteSslSetup.On;

            var siteUrl = $"{(sslEnabled ? "https" : "http")}://{portalAlias.HttpAlias}";

            return string.Format(Constants.CallbackUrl, siteUrl, portalId);
        }

        /// <summary>
        /// Get the provider parameters.
        /// </summary>
        /// <param name="portalId">the portal id of the setting, pass Null.NullInteger if it's a global setting.</param>
        /// <returns>parameters list.</returns>
        public IList<SmtpOAuthSetting> GetSettings(int portalId)
        {
            return portalId > Null.NullInteger ? this.GetSettingsFromPortal(portalId) : this.GetSettingsFromHost();
        }

        /// <summary>
        /// update provider settings.
        /// </summary>
        /// <param name="portalId">the portal id of the setting, pass Null.NullInteger if it's a global setting.</param>
        /// <param name="settings">the settings.</param>
        /// <param name="errorMessages">the errors.</param>
        /// <returns>Whether update the settings successfully.</returns>
        public bool UpdateSettings(int portalId, IDictionary<string, string> settings, out IList<string> errorMessages)
        {
            errorMessages = new List<string>();
            var changed = false;

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

        /// <summary>
        /// Authorize the smtp client.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="smtpClient">The smtp client.</param>
        public void Authorize(int portalId, IOAuth2SmtpClient smtpClient)
        {
            var settings = this.GetSettings(portalId);
            var accountEmail = settings.FirstOrDefault(i => i.Name == Constants.AccountEmailSettingName)?.Value ?? string.Empty;
            var codeFlow = CreateAuthorizationCodeFlow(portalId);
            if (codeFlow != null)
            {
                var response = new GoogleCredentialDataStore(portalId).GetAsync<TokenResponse>(accountEmail).Result;
                var credential = new UserCredential(codeFlow, accountEmail, response);
                if (credential != null)
                {
                    if (credential.Token.IsExpired(SystemClock.Default))
                    {
                        var refreshed = credential.RefreshTokenAsync(CancellationToken.None).Result;
                    }

                    smtpClient.Authenticate(credential.UserId, credential.Token.AccessToken);
                }
            }
        }

        /// <summary>
        /// Authorize the smtp client.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="smtpClient">The smtp client.</param>
        /// <param name="cancellationToken">The cancallation token.</param>
        /// <returns>A <see cref="Task"/> indicating completion.</returns>
        public Task AuthorizeAsync(int portalId, IOAuth2SmtpClient smtpClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                this.Authorize(portalId, smtpClient);
            });
        }

        /// <summary>
        /// Create authorization code flow.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>The authorization code flow.</returns>
        internal static IAuthorizationCodeFlow CreateAuthorizationCodeFlow(int portalId)
        {
            var authProvider = SmtpOAuthController.Instance.GetOAuthProvider(Constants.Name);

            var settings = authProvider.GetSettings(portalId);
            var accountEmail = settings.FirstOrDefault(i => i.Name == Constants.AccountEmailSettingName)?.Value ?? string.Empty;
            var clientId = settings.FirstOrDefault(i => i.Name == Constants.ClientIdSettingName)?.Value ?? string.Empty;
            var clientSecret = settings.FirstOrDefault(i => i.Name == Constants.ClientSecretSettingName)?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(accountEmail)
                || string.IsNullOrWhiteSpace(clientId)
                || string.IsNullOrWhiteSpace(clientSecret))
            {
                return null;
            }

            var clientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
            };

            var codeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                DataStore = new GoogleCredentialDataStore(portalId),
                Scopes = new[] { "https://mail.google.com/" },
                ClientSecrets = clientSecrets,
                Prompt = "consent",
            });

            return codeFlow;
        }

        private static IServiceProvider GetServiceProvider()
        {
            return HttpContextSource.Current?.GetScope()?.ServiceProvider ??
                DependencyInjectionInitialize.BuildServiceProvider();
        }

        private IList<SmtpOAuthSetting> GetSettingsFromPortal(int portalId)
        {
            var portalSettings = PortalController.Instance.GetPortalSettings(portalId);
            if (portalSettings == null)
            {
                throw new ArgumentException("Invalid portal Id.");
            }

            var accountEmail = portalSettings.GetValueOrDefault<string>(Constants.AccountEmailSettingName, string.Empty);
            var clientId = portalSettings.GetValueOrDefault<string>(Constants.ClientIdSettingName, string.Empty);
            var clientSecret = portalSettings.GetValueOrDefault<string>(Constants.ClientSecretSettingName, string.Empty);
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

        private IList<SmtpOAuthSetting> GetSettingsFromHost()
        {
            var hostSettings = this.GetService<IHostSettingsService>().GetSettingsDictionary().ToDictionary(i => i.Key, i => i.Value);

            var accountEmail = hostSettings.GetValueOrDefault<string>(Constants.AccountEmailSettingName, string.Empty);
            var clientId = hostSettings.GetValueOrDefault<string>(Constants.ClientIdSettingName, string.Empty);
            var clientSecret = hostSettings.GetValueOrDefault<string>(Constants.ClientSecretSettingName, string.Empty);
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
                this.GetService<IHostSettingsService>().Update(Constants.AccountEmailSettingName, settings[Constants.AccountEmailSettingName], false);
                changed = true;
            }

            if (settings.ContainsKey(Constants.ClientIdSettingName) && settings[Constants.ClientIdSettingName] != clientId)
            {
                this.GetService<IHostSettingsService>().Update(Constants.ClientIdSettingName, settings[Constants.ClientIdSettingName], false);
                changed = true;
            }

            if (settings.ContainsKey(Constants.ClientSecretSettingName) && settings[Constants.ClientSecretSettingName] != clientSecret)
            {
                var encryptedSecret = PortalSecurity.Instance.Encrypt(Config.GetDecryptionkey(), settings[Constants.ClientSecretSettingName]);
                this.GetService<IHostSettingsService>().Update(Constants.ClientSecretSettingName, encryptedSecret, false);
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
                PortalController.UpdatePortalSetting(portalId, Constants.AccountEmailSettingName, settings[Constants.AccountEmailSettingName], false);
                changed = true;
            }

            if (settings.ContainsKey(Constants.ClientIdSettingName) && settings[Constants.ClientIdSettingName] != clientId)
            {
                PortalController.UpdatePortalSetting(portalId, Constants.ClientIdSettingName, settings[Constants.ClientIdSettingName], false);
                changed = true;
            }

            if (settings.ContainsKey(Constants.ClientSecretSettingName) && settings[Constants.ClientSecretSettingName] != clientSecret)
            {
                var encryptedSecret = PortalSecurity.Instance.Encrypt(Config.GetDecryptionkey(), settings[Constants.ClientSecretSettingName]);
                PortalController.UpdatePortalSetting(portalId, Constants.ClientSecretSettingName, encryptedSecret, false);
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
                this.GetService<IHostSettingsService>().Update(settingName, string.Empty, false);
            }
            else
            {
                PortalController.UpdatePortalSetting(portalId, settingName, string.Empty, false);
            }
        }

        private T GetService<T>()
            where T : class
        {
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
