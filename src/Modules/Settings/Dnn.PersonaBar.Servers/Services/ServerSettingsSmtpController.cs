#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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
using System.Globalization;
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
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Servers.Services
{
    [ServiceScope(Scope = ServiceScope.Host)]
    public class ServerSettingsSmtpController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerSettingsSmtpController));
        
        [HttpGet]
        public HttpResponseMessage GetSmtpSettings()
        {
            try
            {
                var smtpSettings = new
                {
                    smtpServer = Host.SMTPServer,
                    smtpConnectionLimit = Host.SMTPConnectionLimit.ToString(CultureInfo.InvariantCulture),
                    smtpMaxIdleTime = Host.SMTPMaxIdleTime.ToString(CultureInfo.InvariantCulture),
                    smtpAuthentication = Host.SMTPAuthentication,
                    enableSmtpSsl = Host.EnableSMTPSSL,
                    smtpUserName = Host.SMTPUsername,
                    smtpPassword = new Regex(".").Replace(Host.SMTPPassword, "*"),
                    messageSchedulerBatchSize = Host.MessageSchedulerBatchSize
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
        public HttpResponseMessage UpdateSmtpSettings(UpdateSMTPSettingsRequest request)
        {
            try
            {
                HostController.Instance.Update("SMTPServer", request.SMTPServer, false);
                HostController.Instance.Update("SMTPConnectionLimit", request.SMTPConnectionLimit, false);
                HostController.Instance.Update("SMTPMaxIdleTime", request.SMTPMaxIdleTime, false);
                HostController.Instance.Update("SMTPAuthentication", request.SMTPAuthentication.ToString(), false);
                HostController.Instance.Update("SMTPUsername", request.SMTPUsername, false);
                if (request.SMTPPassword != new Regex(".").Replace(Host.SMTPPassword, "*"))
                {
                    HostController.Instance.UpdateEncryptedString("SMTPPassword", request.SMTPPassword, Config.GetDecryptionkey());
                }
                HostController.Instance.Update("SMTPEnableSSL", request.EnableSMTPSSL ? "Y" : "N", false);
                HostController.Instance.Update("MessageSchedulerBatchSize", request.MessageSchedulerBatchSize.ToString(), false);
                DataCache.ClearCache();

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
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
                var password = request.SMTPPassword;
                if (request.SMTPPassword == new Regex(".").Replace(Host.SMTPPassword, "*"))
                {
                    password = Host.SMTPPassword;
                }
                var strMessage = Mail.SendMail(Host.HostEmail,
                                                  Host.HostEmail,
                                                  "",
                                                  "",
                                                  MailPriority.Normal,
                                                  Localization.GetSystemMessage(PortalSettings, "EMAIL_SMTP_TEST_SUBJECT"),
                                                  MailFormat.Text,
                                                  Encoding.UTF8,
                                                  "",
                                                  "",
                                                  request.SMTPServer,
                                                  request.SMTPAuthentication.ToString(),
                                                  request.SMTPUsername,
                                                  password,
                                                  request.EnableSMTPSSL);
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = string.IsNullOrEmpty(strMessage)
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