// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExchangeOnlineAuthProvider.Components
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
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Identity.Client;

    /// <inheritdoc/>
    public class ExchangeOnlineOAuthProvider : ISmtpOAuthProvider
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
            var clientApplication = CreateClientApplication(portalId);
            if (clientApplication == null)
            {
                return false;
            }

            var accounts = clientApplication.GetAccountsAsync().Result;

            return accounts?.Any() ?? false;
        }

        /// <summary>
        /// Get the authorize url.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>The authorize url.</returns>
        public string GetAuthorizeUrl(int portalId)
        {
            var clientId = this.GetSetting(portalId, Constants.ClientIdSettingName);
            var clientSecret = this.GetSetting(portalId, Constants.ClientSecretSettingName);

            if (string.IsNullOrWhiteSpace(clientId)
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
                this.DeleteSetting(portalId, Constants.AuthenticationSettingName);
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
            if (!this.IsAuthorized(portalId))
            {
                return;
            }

            var clientApplication = CreateClientApplication(portalId);
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

        /// <summary>
        /// Authorize the smtp client.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="smtpClient">The smtp client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> indicating completion.</returns>
        public Task AuthorizeAsync(int portalId, IOAuth2SmtpClient smtpClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                this.Authorize(portalId, smtpClient);
            });
        }

        /// <summary>
        /// Create the authentication client application.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>The client application.</returns>
        internal static ConfidentialClientApplication CreateClientApplication(int portalId)
        {
            var authProvider = SmtpOAuthController.Instance.GetOAuthProvider(Constants.Name);

            var settings = authProvider.GetSettings(portalId);
            var tenantId = settings.FirstOrDefault(i => i.Name == Constants.TenantIdSettingName)?.Value ?? string.Empty;
            var clientId = settings.FirstOrDefault(i => i.Name == Constants.ClientIdSettingName)?.Value ?? string.Empty;
            var clientSecret = settings.FirstOrDefault(i => i.Name == Constants.ClientSecretSettingName)?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(tenantId)
                || string.IsNullOrWhiteSpace(clientId)
                || string.IsNullOrWhiteSpace(clientSecret))
            {
                return null;
            }

            var redirectUrl = authProvider.GetAuthorizeUrl(portalId);
            if (redirectUrl.Contains("?"))
            {
                redirectUrl = redirectUrl.Substring(0, redirectUrl.IndexOf("?"));
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

            var tokenCacheHelper = new TokenCacheHelper(portalId);
            tokenCacheHelper.EnableSerialization(clientApplication.UserTokenCache);

            return clientApplication;
        }

        /// <summary>
        /// Get the authentication scopes list.
        /// </summary>
        /// <returns>The scopes.</returns>
        internal static IList<string> GetAuthenticationScopes()
        {
            return new string[]
            {
                "https://outlook.office365.com/.default",
            };
        }

        private static IServiceProvider GetServiceProvider()
        {
            return HttpContextSource.Current?.GetScope()?.ServiceProvider ??
                DependencyInjectionInitialize.BuildServiceProvider();
        }

        private T GetService<T>()
            where T : class
        {
            return (T)this.serviceProvider.GetService(typeof(T));
        }

        private IList<SmtpOAuthSetting> GetSettingsFromPortal(int portalId)
        {
            var portalSettings = PortalController.Instance.GetPortalSettings(portalId);
            if (portalSettings == null)
            {
                throw new ArgumentException("Invalid portal Id.");
            }

            var tenantId = portalSettings.GetValueOrDefault<string>(Constants.TenantIdSettingName, string.Empty);
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

        private IList<SmtpOAuthSetting> GetSettingsFromHost()
        {
            var hostSettings = this.GetService<IHostSettingsService>().GetSettingsDictionary().ToDictionary(i => i.Key, i => i.Value);

            var tenantId = hostSettings.GetValueOrDefault<string>(Constants.TenantIdSettingName, string.Empty);
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

            var changed = false;
            if (settings.ContainsKey(Constants.TenantIdSettingName) && settings[Constants.TenantIdSettingName] != tenantId)
            {
                this.GetService<IHostSettingsService>().Update(Constants.TenantIdSettingName, settings[Constants.TenantIdSettingName], false);
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
            var tenantId = this.GetSetting(portalId, Constants.TenantIdSettingName);
            var clientId = this.GetSetting(portalId, Constants.ClientIdSettingName);
            var clientSecret = this.GetSetting(portalId, Constants.ClientSecretSettingName);

            var changed = false;
            if (settings.ContainsKey(Constants.TenantIdSettingName) && settings[Constants.TenantIdSettingName] != tenantId)
            {
                PortalController.UpdatePortalSetting(portalId, Constants.TenantIdSettingName, settings[Constants.TenantIdSettingName], false);
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
    }
}
