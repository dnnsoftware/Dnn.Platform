// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
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

    [MenuPermission(Scope = ServiceScope.Admin)]
    public class ServerSettingsSmtpAdminController : PersonaBarApiController
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
                    host = new { },
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

                //admins can only change site settings
                PortalController.UpdatePortalSetting(portalId, "SMTPServer", request.SmtpServer, false);
                PortalController.UpdatePortalSetting(portalId, "SMTPConnectionLimit", request.SmtpConnectionLimit, false);
                PortalController.UpdatePortalSetting(portalId, "SMTPMaxIdleTime", request.SmtpMaxIdleTime, false);
                PortalController.UpdatePortalSetting(portalId, "SMTPAuthentication",
                    request.SmtpAuthentication.ToString(), false);
                PortalController.UpdatePortalSetting(portalId, "SMTPUsername", request.SmtpUsername, false);
                PortalController.UpdateEncryptedString(portalId, "SMTPPassword", request.SmtpPassword,
                    Config.GetDecryptionkey());
                PortalController.UpdatePortalSetting(portalId, "SMTPEnableSSL", request.EnableSmtpSsl ? "Y" : "N", false);

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
                var smtpHostMode = request.SmtpServerMode == "h";
                var mailFrom = smtpHostMode ? Host.HostEmail : this.PortalSettings.Email;
                var mailTo = this.UserInfo.Email;

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
                    smtpHostMode ? HostController.Instance.GetString("SMTPServer") : request.SmtpServer,
                    smtpHostMode ? HostController.Instance.GetString("SMTPAuthentication") : request.SmtpAuthentication.ToString(),
                    smtpHostMode ? HostController.Instance.GetString("SMTPUsername") : request.SmtpUsername,
                    smtpHostMode ? HostController.Instance.GetEncryptedString("SMTPPassword", Config.GetDecryptionkey()) : request.SmtpPassword,
                    smtpHostMode ? HostController.Instance.GetBoolean("SMTPEnableSSL", false) : request.EnableSmtpSsl);

                var success = string.IsNullOrEmpty(errMessage);
                return this.Request.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, new
                {
                    success,
                    errMessage,
                    confirmationMessage =
                        string.Format(Localization.GetString("EmailSentMessage", Components.Constants.ServersResourcersPath),
                        mailFrom, mailTo)
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
