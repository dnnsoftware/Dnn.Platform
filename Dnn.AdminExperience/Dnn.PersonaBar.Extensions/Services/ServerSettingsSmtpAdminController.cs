// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Servers.Services.Dto;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.Mail.OAuth;
    using DotNetNuke.Web.Api;

    /// <summary>Provides the APIs for SMTP settings management.</summary>
    [MenuPermission(Scope = ServiceScope.Admin)]
    public class ServerSettingsSmtpAdminController : PersonaBarApiController
    {
        private const string ObfuscateString = "*****";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerSettingsSmtpHostController));
        private readonly IHostSettingsService hostSettingsService;
        private readonly ISmtpOAuthController smtpOAuthController;

        /// <summary>Initializes a new instance of the <see cref="ServerSettingsSmtpAdminController"/> class.</summary>
        /// <param name="hostSettingsService">A service to manage host settings.</param>
        /// <param name="smtpOAuthController">A controller for SMTP OAuth providers.</param>
        public ServerSettingsSmtpAdminController(
            IHostSettingsService hostSettingsService,
            ISmtpOAuthController smtpOAuthController)
        {
            this.hostSettingsService = hostSettingsService;
            this.smtpOAuthController = smtpOAuthController;
        }

        /// <summary>Gets the SMTP settings.</summary>
        /// <returns>An object representing the SMTP settings for the current portal.</returns>
        [HttpGet]
        public HttpResponseMessage GetSmtpSettings()
        {
            try
            {
                var portalId = PortalSettings.Current.PortalId;

                var smtpSettings = new
                {
                    smtpServerMode = PortalController.GetPortalSetting("SMTPmode", portalId, "h"),
                    host = new { },
                    site = new
                    {
                        smtpServer = PortalController.GetPortalSetting("SMTPServer", portalId, string.Empty),
                        smtpConnectionLimit = PortalController.GetPortalSettingAsInteger("SMTPConnectionLimit", portalId, 2),
                        smtpMaxIdleTime = PortalController.GetPortalSettingAsInteger("SMTPMaxIdleTime", portalId, 100000),
                        smtpAuthentication = PortalController.GetPortalSetting("SMTPAuthentication", portalId, "0"),
                        enableSmtpSsl = PortalController.GetPortalSetting("SMTPEnableSSL", portalId, string.Empty) == "Y",
                        smtpUserName = PortalController.GetPortalSetting("SMTPUsername", portalId, string.Empty),
                        smtpPassword = GetSmtpPassword(portalId, true),
                        authProvider = PortalController.GetPortalSetting("SMTPAuthProvider", portalId, string.Empty),
                    },
                    portalName = PortalSettings.Current.PortalName,
                    hideCoreSettings = ProviderConfiguration.GetProviderConfiguration("mail").GetDefaultProvider().Attributes.GetValueOrDefault("hideCoreSettings", false),
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, smtpSettings);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>Updates the SMTP settings for the current portal.</summary>
        /// <param name="request"><see cref="UpdateSmtpSettingsRequest"/>.</param>
        /// <returns>A value indicating whether the operation succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSmtpSettings(UpdateSmtpSettingsRequest request)
        {
            try
            {
                IList<string> errorMessages = new List<string>();

                var portalId = PortalSettings.Current.PortalId;

                PortalController.UpdatePortalSetting(portalId, "SMTPmode", request.SmtpServerMode, false);

                if (request.SmtpPassword == ObfuscateString)
                {
                    request.SmtpPassword = GetSmtpPassword(portalId, false);
                }

                // admins can only change site settings
                PortalController.UpdatePortalSetting(portalId, "SMTPServer", request.SmtpServer, false);
                PortalController.UpdatePortalSetting(portalId, "SMTPConnectionLimit", request.SmtpConnectionLimit, false);
                PortalController.UpdatePortalSetting(portalId, "SMTPMaxIdleTime", request.SmtpMaxIdleTime, false);
                PortalController.UpdatePortalSetting(
                    portalId,
                    "SMTPAuthentication",
                    request.SmtpAuthentication.ToString(),
                    false);
                PortalController.UpdatePortalSetting(portalId, "SMTPUsername", request.SmtpUsername, false);
                PortalController.UpdateEncryptedString(
                    portalId,
                    "SMTPPassword",
                    request.SmtpPassword,
                    Config.GetDecryptionkey());
                PortalController.UpdatePortalSetting(portalId, "SMTPEnableSSL", request.EnableSmtpSsl ? "Y" : "N", false);

                var providerChanged = false;

                // oauth authentication
                if (request.SmtpAuthentication == 3)
                {
                    // Only the mail kit provider supports oauth.
                    EnsureMailProviderSupportOAuth();

                    var authProvider = PortalController.GetPortalSetting("SMTPAuthProvider", portalId, string.Empty);
                    if (authProvider != request.AuthProvider)
                    {
                        PortalController.UpdatePortalSetting(portalId, "SMTPAuthProvider", request.AuthProvider, false);
                        providerChanged = true;
                    }

                    var provider = this.smtpOAuthController.GetOAuthProvider(request.AuthProvider);
                    if (provider != null)
                    {
                        providerChanged = provider.UpdateSettings(portalId, request.AuthProviderSettings, out errorMessages);
                    }
                }

                DataCache.ClearCache();
                if (errorMessages.Any())
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { success = false, errors = errorMessages });
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { success = true, providerChanged });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>Sends a test email to validate the settings work.</summary>
        /// <param name="request"><see cref="SendTestEmailRequest"/>.</param>
        /// <returns>A localized test result message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SendTestEmail(SendTestEmailRequest request)
        {
            try
            {
                var smtpHostMode = request.SmtpServerMode == "h";
                var mailFrom = smtpHostMode ? Host.HostEmail : this.PortalSettings.Email;
                var mailTo = this.UserInfo.Email;
                var portalId = PortalSettings.Current.PortalId;
                if (request.SmtpPassword == ObfuscateString)
                {
                    request.SmtpPassword = GetSmtpPassword(portalId, false);
                }

                var errMessage = Mail.SendMail(
                    mailFrom,
                    mailTo,
                    string.Empty,
                    string.Empty,
                    MailPriority.Normal,
                    Localization.GetSystemMessage(this.PortalSettings, "EMAIL_SMTP_TEST_SUBJECT"),
                    MailFormat.Text,
                    Encoding.UTF8,
                    string.Empty,
                    string.Empty,
                    smtpHostMode ? this.hostSettingsService.GetString("SMTPServer") : request.SmtpServer,
                    smtpHostMode ? this.hostSettingsService.GetString("SMTPAuthentication") : request.SmtpAuthentication.ToString(),
                    smtpHostMode ? this.hostSettingsService.GetString("SMTPUsername") : request.SmtpUsername,
                    smtpHostMode ? this.hostSettingsService.GetEncryptedString("SMTPPassword", Config.GetDecryptionkey()) : request.SmtpPassword,
                    smtpHostMode ? this.hostSettingsService.GetBoolean("SMTPEnableSSL", false) : request.EnableSmtpSsl,
                    smtpHostMode ? this.hostSettingsService.GetString("SMTPAuthProvider", string.Empty) : request.AuthProvider);

                var success = string.IsNullOrEmpty(errMessage);
                return this.Request.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, new
                {
                    success,
                    errMessage,
                    confirmationMessage =
                        string.Format(
                            Localization.GetString(
                                "EmailSentMessage",
                                Components.Constants.ServersResourcersPath),
                            mailFrom,
                            mailTo),
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>
        /// Get smtp oauth providers.
        /// </summary>
        /// <returns>smtp oauth providers.</returns>
        [HttpGet]
        public HttpResponseMessage GetSmtpOAuthProviders()
        {
            try
            {
                var portalId = PortalSettings.Current.PortalId;
                var providers = this.smtpOAuthController.GetOAuthProviders();
                var result = providers.Select(i => new
                {
                    name = i.Name,
                    localizedName = i.LocalizedName,
                    settings = i.GetSettings(portalId).Where(s => !s.IsBackground),
                    isAuthorized = i.IsAuthorized(portalId),
                    authorizeUrl = i.GetAuthorizeUrl(portalId),
                });

                return this.Request.CreateResponse(HttpStatusCode.OK, new { host = Array.Empty<object>(), site = result });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static void EnsureMailProviderSupportOAuth()
        {
            if (MailProvider.Instance().SupportsOAuth)
            {
                return;
            }

            const string providerName = "MailKitMailProvider";

            var xmlConfig = Config.Load();

            var provider = xmlConfig.SelectSingleNode("configuration/dotnetnuke/mail/providers/add[@name='" + providerName + "']");
            if (provider == null)
            {
                return;
            }

            var mailNode = xmlConfig.SelectSingleNode("configuration/dotnetnuke/mail");
            if (mailNode?.Attributes == null)
            {
                return;
            }

            var defaultProvider = mailNode.Attributes["defaultProvider"].Value;
            if (defaultProvider == providerName)
            {
                return;
            }

            XmlUtils.UpdateAttribute(mailNode, "defaultProvider", providerName);
            Config.Save(xmlConfig);
        }

        private static string GetSmtpPassword(int portalId, bool obfuscate)
        {
            var pwd = PortalController.GetEncryptedString("SMTPPassword", portalId, Config.GetDecryptionkey());

            if (obfuscate && !string.IsNullOrEmpty(pwd))
            {
                return ObfuscateString;
            }

            return pwd;
        }
    }
}
