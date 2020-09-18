// Licensed to the .NET Foundation under one or more agreements.
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
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Web.Api;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class ServerSettingsSmtpHostController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerSettingsSmtpHostController));

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
                        smtpServer = HostController.Instance.GetString("SMTPServer"),
                        smtpConnectionLimit = HostController.Instance.GetInteger("SMTPConnectionLimit", 2),
                        smtpMaxIdleTime = HostController.Instance.GetInteger("SMTPMaxIdleTime", 100000),
                        smtpAuthentication = HostController.Instance.GetString("SMTPAuthentication"),
                        enableSmtpSsl = HostController.Instance.GetBoolean("SMTPEnableSSL", false),
                        smtpUserName = HostController.Instance.GetString("SMTPUsername"),
                        smtpPassword = GetSmtpPassword(),
                        smtpHostEmail = HostController.Instance.GetString("HostEmail"),
                        messageSchedulerBatchSize = Host.MessageSchedulerBatchSize
                    },
                    site = new
                    {
                        smtpServer = PortalController.GetPortalSetting("SMTPServer", portalId, string.Empty),
                        smtpConnectionLimit = PortalController.GetPortalSettingAsInteger("SMTPConnectionLimit", portalId, 2),
                        smtpMaxIdleTime = PortalController.GetPortalSettingAsInteger("SMTPMaxIdleTime", portalId, 100000),
                        smtpAuthentication = PortalController.GetPortalSetting("SMTPAuthentication", portalId, "0"),
                        enableSmtpSsl = PortalController.GetPortalSetting("SMTPEnableSSL", portalId, string.Empty) == "Y",
                        smtpUserName = PortalController.GetPortalSetting("SMTPUsername", portalId, string.Empty),
                        smtpPassword = PortalController.GetEncryptedString("SMTPPassword", portalId, Config.GetDecryptionkey())
                    },
                    portalName = PortalSettings.Current.PortalName
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, smtpSettings);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

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
                    HostController.Instance.Update("SMTPServer", request.SmtpServer, false);
                    HostController.Instance.Update("SMTPConnectionLimit", request.SmtpConnectionLimit, false);
                    HostController.Instance.Update("SMTPMaxIdleTime", request.SmtpMaxIdleTime, false);
                    HostController.Instance.Update("SMTPAuthentication", request.SmtpAuthentication.ToString(), false);
                    HostController.Instance.Update("SMTPUsername", request.SmtpUsername, false);
                    HostController.Instance.UpdateEncryptedString("SMTPPassword", request.SmtpPassword,
                        Config.GetDecryptionkey());
                    HostController.Instance.Update("HostEmail", request.SmtpHostEmail);
                    HostController.Instance.Update("SMTPEnableSSL", request.EnableSmtpSsl ? "Y" : "N", false);
                    HostController.Instance.Update("MessageSchedulerBatchSize",
                        request.MessageSchedulerBatchSize.ToString(), false);
                }
                else
                {
                    PortalController.UpdatePortalSetting(portalId, "SMTPServer", request.SmtpServer, false);
                    PortalController.UpdatePortalSetting(portalId, "SMTPConnectionLimit", request.SmtpConnectionLimit, false);
                    PortalController.UpdatePortalSetting(portalId, "SMTPMaxIdleTime", request.SmtpMaxIdleTime, false);
                    PortalController.UpdatePortalSetting(portalId, "SMTPAuthentication",
                        request.SmtpAuthentication.ToString(), false);
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

        /// POST: api/Servers/SendTestEmail
        /// <summary>
        /// Tests SMTP settings.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SendTestEmail(SendTestEmailRequest request)
        {
            try
            {
                var mailFrom = Host.HostEmail;
                var mailTo = request.SmtpServerMode == "h" ? Host.HostEmail : this.PortalSettings.UserInfo.Email;

                var errMessage = Mail.SendMail(mailFrom,
                    mailTo,
                    "",
                    "",
                    MailPriority.Normal,
                    Localization.GetSystemMessage(this.PortalSettings, "EMAIL_SMTP_TEST_SUBJECT"),
                    MailFormat.Text,
                    Encoding.UTF8,
                    "",
                    "",
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
                    confirmationMessage = success ?
                        string.Format(Localization.GetString("EmailSentMessage", Components.Constants.ServersResourcersPath),
                        mailFrom, mailTo) : Localization.GetString("errorMessageSendingTestEmail")
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static string GetSmtpPassword()
        {
            string decryptedText;
            try
            {
                decryptedText = HostController.Instance.GetEncryptedString("SMTPPassword", Config.GetDecryptionkey());
            }
            catch (Exception)
            {
                //fixes case where smtppassword failed to encrypt due to failing upgrade
                var current = HostController.Instance.GetString("SMTPPassword");
                if (!string.IsNullOrEmpty(current))
                {
                    HostController.Instance.UpdateEncryptedString("SMTPPassword", current, Config.GetDecryptionkey());
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
