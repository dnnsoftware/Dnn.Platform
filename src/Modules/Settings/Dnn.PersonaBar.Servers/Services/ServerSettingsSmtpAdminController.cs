#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

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

namespace Dnn.PersonaBar.Servers.Services
{
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
                    host = new{},
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
                return Request.CreateResponse(HttpStatusCode.OK, smtpSettings);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
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
                return Request.CreateResponse(HttpStatusCode.OK, new {success = true});
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Servers/SendTestEmail
        /// <summary>
        /// Tests SMTP settings
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
                var mailFrom = Host.HostEmail;
                var mailTo = smtpHostMode ? Host.HostEmail : PortalSettings.UserInfo.Email;

                var errMessage = Mail.SendMail(mailFrom,
                    mailTo,
                    "",
                    "",
                    MailPriority.Normal,
                    Localization.GetSystemMessage(PortalSettings, "EMAIL_SMTP_TEST_SUBJECT"),
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
                return Request.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, new
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
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}