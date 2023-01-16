﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Services
{
    using System;
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
    using DotNetNuke.Web.Api;

    /// <summary>Provides the APIs for SMTP settings management.</summary>
    [MenuPermission(Scope = ServiceScope.Host)]
    public class ServerSettingsSmtpHostController : PersonaBarApiController
    {
        private const string ObfuscateString = "*****";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerSettingsSmtpHostController));
        private readonly IHostSettingsService hostSettingsService;

        /// <summary>Initializes a new instance of the <see cref="ServerSettingsSmtpHostController"/> class.</summary>
        /// <param name="hostSettingsService">A service to manage host settings.</param>
        public ServerSettingsSmtpHostController(
            IHostSettingsService hostSettingsService)
        {
            this.hostSettingsService = hostSettingsService;
        }

        /// <summary>Gets the SMTP settings for the host.</summary>
        /// <returns>An object representing the host smtp settings.</returns>
        [HttpGet]
        public HttpResponseMessage GetSmtpSettings()
        {
            try
            {
                var portalId = PortalSettings.Current.PortalId;

                var smtpSettings = new
                {
                    smtpServerMode = PortalController.GetPortalSetting("SMTPmode", portalId, "h"),
                    host = new
                    {
                        smtpServer = this.hostSettingsService.GetString("SMTPServer"),
                        smtpConnectionLimit = this.hostSettingsService.GetInteger("SMTPConnectionLimit", 2),
                        smtpMaxIdleTime = this.hostSettingsService.GetInteger("SMTPMaxIdleTime", 100000),
                        smtpAuthentication = this.hostSettingsService.GetString("SMTPAuthentication"),
                        enableSmtpSsl = this.hostSettingsService.GetBoolean("SMTPEnableSSL", false),
                        smtpUserName = this.hostSettingsService.GetString("SMTPUsername"),
                        smtpPassword = this.GetSmtpPassword(-1, true),
                        smtpHostEmail = this.hostSettingsService.GetString("HostEmail"),
                        messageSchedulerBatchSize = Host.MessageSchedulerBatchSize,
                    },
                    site = new
                    {
                        smtpServer = PortalController.GetPortalSetting("SMTPServer", portalId, string.Empty),
                        smtpConnectionLimit = PortalController.GetPortalSettingAsInteger("SMTPConnectionLimit", portalId, 2),
                        smtpMaxIdleTime = PortalController.GetPortalSettingAsInteger("SMTPMaxIdleTime", portalId, 100000),
                        smtpAuthentication = PortalController.GetPortalSetting("SMTPAuthentication", portalId, "0"),
                        enableSmtpSsl = PortalController.GetPortalSetting("SMTPEnableSSL", portalId, string.Empty) == "Y",
                        smtpUserName = PortalController.GetPortalSetting("SMTPUsername", portalId, string.Empty),
                        smtpPassword = this.GetSmtpPassword(portalId, true),
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

        /// <summary>Updates the SMTP settings.</summary>
        /// <param name="request">The request.</param>
        /// <returns>A value indicating whether the operation succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSmtpSettings(UpdateSmtpSettingsRequest request)
        {
            try
            {
                var portalId = PortalSettings.Current.PortalId;
                PortalController.UpdatePortalSetting(portalId, "SMTPmode", request.SmtpServerMode, false);

                if (request.SmtpServerMode == "h")
                {
                    if (request.SmtpPassword == ObfuscateString)
                    {
                        request.SmtpPassword = this.GetHostSmtpPassword();
                    }

                    this.hostSettingsService.Update("SMTPServer", request.SmtpServer, false);
                    this.hostSettingsService.Update("SMTPConnectionLimit", request.SmtpConnectionLimit, false);
                    this.hostSettingsService.Update("SMTPMaxIdleTime", request.SmtpMaxIdleTime, false);
                    this.hostSettingsService.Update("SMTPAuthentication", request.SmtpAuthentication.ToString(), false);
                    this.hostSettingsService.Update("SMTPUsername", request.SmtpUsername, false);
                    this.hostSettingsService.UpdateEncryptedString("SMTPPassword", request.SmtpPassword, Config.GetDecryptionkey());
                    this.hostSettingsService.Update("HostEmail", request.SmtpHostEmail);
                    this.hostSettingsService.Update("SMTPEnableSSL", request.EnableSmtpSsl ? "Y" : "N", false);
                    this.hostSettingsService.Update("MessageSchedulerBatchSize", request.MessageSchedulerBatchSize.ToString(), false);
                }
                else
                {
                    if (request.SmtpPassword == ObfuscateString)
                    {
                        request.SmtpPassword = this.GetSmtpPassword(portalId, false);
                    }

                    PortalController.UpdatePortalSetting(portalId, "SMTPServer", request.SmtpServer, false);
                    PortalController.UpdatePortalSetting(portalId, "SMTPConnectionLimit", request.SmtpConnectionLimit, false);
                    PortalController.UpdatePortalSetting(portalId, "SMTPMaxIdleTime", request.SmtpMaxIdleTime, false);
                    PortalController.UpdatePortalSetting(
                        portalId,
                        "SMTPAuthentication",
                        request.SmtpAuthentication.ToString(),
                        false);
                    PortalController.UpdatePortalSetting(portalId, "SMTPUsername", request.SmtpUsername, false);
                    PortalController.UpdateEncryptedString(portalId, "SMTPPassword", request.SmtpPassword, Config.GetDecryptionkey());
                    PortalController.UpdatePortalSetting(portalId, "SMTPEnableSSL", request.EnableSmtpSsl ? "Y" : "N", false);
                }

                DataCache.ClearCache();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>Sends an email to test the SMTP settings.</summary>
        /// <param name="request"><see cref="SendTestEmailRequest"/>.</param>
        /// <returns>A localized result message object.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SendTestEmail(SendTestEmailRequest request)
        {
            try
            {
                var mailFrom = request.SmtpServerMode == "h" ? Host.HostEmail : this.PortalSettings.Email;
                var mailTo = this.UserInfo.Email;
                if (request.SmtpPassword == ObfuscateString)
                {
                    request.SmtpPassword = this.GetHostSmtpPassword();
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
                    request.SmtpServer,
                    request.SmtpAuthentication.ToString(),
                    request.SmtpUsername,
                    request.SmtpPassword,
                    request.EnableSmtpSsl);

                var success = string.IsNullOrEmpty(errMessage);
                return this.Request.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, new
                {
                    success,
                    errMessage,
                    confirmationMessage =
                        success
                            ? string.Format(
                                Localization.GetString(
                                    "EmailSentMessage",
                                    Components.Constants.ServersResourcersPath),
                                mailFrom,
                                mailTo)
                            : Localization.GetString("errorMessageSendingTestEmail"),
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private string GetSmtpPassword(int portalId, bool obfuscate)
        {
            var pwd = string.Empty;
            if (portalId == -1)
            {
                pwd = this.GetHostSmtpPassword();
            }
            else
            {
                pwd = PortalController.GetEncryptedString("SMTPPassword", portalId, Config.GetDecryptionkey());
            }

            if (obfuscate && !string.IsNullOrEmpty(pwd))
            {
                return ObfuscateString;
            }

            return pwd;
        }

        private string GetHostSmtpPassword()
        {
            string decryptedText;
            try
            {
                decryptedText = this.hostSettingsService.GetEncryptedString("SMTPPassword", Config.GetDecryptionkey());
            }
            catch (Exception)
            {
                // fixes case where smtppassword failed to encrypt due to failing upgrade
                var current = this.hostSettingsService.GetString("SMTPPassword");
                if (!string.IsNullOrEmpty(current))
                {
                    this.hostSettingsService.UpdateEncryptedString("SMTPPassword", current, Config.GetDecryptionkey());
                    decryptedText = current;
                }
                else
                {
                    decryptedText = string.Empty;
                }
            }

            return decryptedText;
        }
    }
}
