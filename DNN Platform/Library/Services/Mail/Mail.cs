// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;

    using Localize = DotNetNuke.Services.Localization.Localization;

    public class Mail
    {
        private static readonly Regex SmtpServerRegex = new Regex("^[^:]+(:[0-9]{1,5})?$", RegexOptions.Compiled);

        public static string ConvertToText(string sHTML)
        {
            var formattedHtml = HtmlUtils.FormatText(sHTML, true);
            var styleLessHtml = HtmlUtils.RemoveInlineStyle(formattedHtml);
            return HtmlUtils.StripTags(styleLessHtml, true);
        }

        public static bool IsValidEmailAddress(string Email, int portalid)
        {
            string pattern = Null.NullString;

            // During install Wizard we may not have a valid PortalID
            if (portalid != Null.NullInteger)
            {
                pattern = Convert.ToString(UserController.GetUserSettings(portalid)["Security_EmailValidation"]);
            }

            pattern = string.IsNullOrEmpty(pattern) ? Globals.glbEmailRegEx : pattern;
            return Regex.Match(Email, pattern).Success;
        }

        public static void SendEmail(string fromAddress, string toAddress, string subject, string body)
        {
            SendEmail(fromAddress, fromAddress, toAddress, subject, body);
        }

        public static void SendEmail(string fromAddress, string senderAddress, string toAddress, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(Host.SMTPServer) || string.IsNullOrEmpty(fromAddress) || string.IsNullOrEmpty(senderAddress) || string.IsNullOrEmpty(toAddress))
            {
                return;
            }

            using (var emailMessage = new MailMessage(fromAddress, toAddress) { Sender = new MailAddress(senderAddress) })
            {
                SendMailInternal(emailMessage, subject, body, MailPriority.Normal,
                    HtmlUtils.IsHtml(body) ? MailFormat.Html : MailFormat.Text,
                    Encoding.UTF8, new List<Attachment>(),
                    Host.SMTPServer, Host.SMTPAuthentication, Host.SMTPUsername,
                    Host.SMTPPassword, Host.EnableSMTPSSL);
            }
        }

        public static string SendEmail(string fromAddress, string senderAddress, string toAddress, string subject, string body, List<Attachment> attachments)
        {
            if (string.IsNullOrWhiteSpace(Host.SMTPServer))
            {
                return "SMTP Server not configured";
            }

            using (var emailMessage = new MailMessage(fromAddress, toAddress) { Sender = new MailAddress(senderAddress) })
            {
                return SendMailInternal(emailMessage, subject, body, MailPriority.Normal,
                    HtmlUtils.IsHtml(body) ? MailFormat.Html : MailFormat.Text,
                    Encoding.UTF8, attachments,
                    Host.SMTPServer, Host.SMTPAuthentication, Host.SMTPUsername,
                    Host.SMTPPassword, Host.EnableSMTPSSL);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// <summary>Send an email notification</summary>
        /// </summary>
        /// <param name="user">The user to whom the message is being sent.</param>
        /// <param name="msgType">The type of message being sent.</param>
        /// <param name="settings">Portal Settings.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static string SendMail(UserInfo user, MessageType msgType, PortalSettings settings)
        {
            return SendMail(user.PortalID, user.UserID, msgType, settings);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// <summary>Send an email notification</summary>
        /// </summary>
        /// <param name="portalId">The PortalId of the user to whom the message is being sent.</param>
        /// <param name="userId">The UserId of the user to whom the message is being sent.</param>
        /// <param name="msgType">The type of message being sent.</param>
        /// <param name="settings">Portal Settings.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static string SendMail(int portalId, int userId, MessageType msgType, PortalSettings settings)
        {
            // Send Notification to User
            var user = UserController.Instance.GetUserById(portalId, userId);
            int toUser = user.UserID;
            string locale = user.Profile.PreferredLocale;
            string subject;
            string body;
            ArrayList custom = null;
            switch (msgType)
            {
                case MessageType.UserRegistrationAdmin:
                    subject = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_SUBJECT";
                    body = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_BODY";
                    toUser = settings.AdministratorId;
                    UserInfo admin = UserController.GetUserById(settings.PortalId, settings.AdministratorId);
                    locale = admin.Profile.PreferredLocale;
                    break;
                case MessageType.UserRegistrationPrivate:
                    subject = "EMAIL_USER_REGISTRATION_PRIVATE_SUBJECT";
                    body = "EMAIL_USER_REGISTRATION_PRIVATE_BODY";
                    break;
                case MessageType.UserRegistrationPrivateNoApprovalRequired:
                    subject = "EMAIL_USER_REGISTRATION_PUBLIC_SUBJECT";
                    body = "EMAIL_USER_REGISTRATION_PUBLIC_BODY";
                    break;
                case MessageType.UserRegistrationPublic:
                    subject = "EMAIL_USER_REGISTRATION_PUBLIC_SUBJECT";
                    body = "EMAIL_USER_REGISTRATION_PUBLIC_BODY";
                    break;
                case MessageType.UserRegistrationVerified:
                    subject = "EMAIL_USER_REGISTRATION_VERIFIED_SUBJECT";
                    body = "EMAIL_USER_REGISTRATION_VERIFIED_BODY";
                    var propertyNotFound = false;
                    if (HttpContext.Current != null)
                    {
                        custom = new ArrayList
                        {
                            HttpContext.Current.Server.HtmlEncode(HttpContext.Current.Server.UrlEncode(user.Username)),
                            HttpContext.Current.Server.UrlEncode(user.GetProperty("verificationcode", string.Empty, null,
                                user, Scope.SystemMessages, ref propertyNotFound)),
                        };
                    }

                    break;
                case MessageType.PasswordReminder:
                    subject = "EMAIL_PASSWORD_REMINDER_SUBJECT";
                    body = "EMAIL_PASSWORD_REMINDER_BODY";
                    break;
                case MessageType.ProfileUpdated:
                    subject = "EMAIL_PROFILE_UPDATED_SUBJECT";
                    body = "EMAIL_PROFILE_UPDATED_BODY";
                    break;
                case MessageType.PasswordUpdated:
                    subject = "EMAIL_PASSWORD_UPDATED_SUBJECT";
                    body = "EMAIL_PASSWORD_UPDATED_BODY";
                    break;
                case MessageType.PasswordReminderUserIsNotApproved:
                    subject = "EMAIL_PASSWORD_REMINDER_USER_ISNOT_APPROVED_SUBJECT";
                    body = "EMAIL_PASSWORD_REMINDER_USER_ISNOT_APPROVED_BODY";
                    break;
                case MessageType.UserAuthorized:
                    subject = "EMAIL_USER_AUTHORIZED_SUBJECT";
                    body = "EMAIL_USER_AUTHORIZED_BODY";
                    break;
                case MessageType.UserUnAuthorized:
                    subject = "EMAIL_USER_UNAUTHORIZED_SUBJECT";
                    body = "EMAIL_USER_UNAUTHORIZED_BODY";
                    break;
                default:
                    subject = "EMAIL_USER_UPDATED_OWN_PASSWORD_SUBJECT";
                    body = "EMAIL_USER_UPDATED_OWN_PASSWORD_BODY";
                    break;
            }

            subject = Localize.GetSystemMessage(locale, settings, subject, user, Localize.GlobalResourceFile, custom, string.Empty, settings.AdministratorId);
            body = Localize.GetSystemMessage(locale, settings, body, user, Localize.GlobalResourceFile, custom, string.Empty, settings.AdministratorId);

            var fromUser = (UserController.GetUserByEmail(settings.PortalId, settings.Email) != null) ?
                string.Format("{0} < {1} >", UserController.GetUserByEmail(settings.PortalId, settings.Email).DisplayName, settings.Email) : settings.Email;
            SendEmail(fromUser, UserController.GetUserById(settings.PortalId, toUser).Email, subject, body);

            return Null.NullString;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// <summary>Send a simple email.</summary>
        /// </summary>
        /// <param name="mailFrom"></param>
        /// <param name="mailTo"></param>
        /// <param name="bcc"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachment"></param>
        /// <param name="bodyType"></param>
        /// <param name="smtpServer"></param>
        /// <param name="smtpAuthentication"></param>
        /// <param name="smtpUsername"></param>
        /// <param name="smtpPassword"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static string SendMail(string mailFrom, string mailTo, string bcc, string subject, string body, string attachment, string bodyType, string smtpServer, string smtpAuthentication,
                                      string smtpUsername, string smtpPassword)
        {
            MailFormat bodyFormat = MailFormat.Text;
            if (!string.IsNullOrEmpty(bodyType))
            {
                switch (bodyType.ToLowerInvariant())
                {
                    case "html":
                        bodyFormat = MailFormat.Html;
                        break;
                    case "text":
                        bodyFormat = MailFormat.Text;
                        break;
                }
            }

            return SendMail(mailFrom, mailTo, string.Empty, bcc, MailPriority.Normal, subject, bodyFormat, Encoding.UTF8, body, attachment, smtpServer, smtpAuthentication, smtpUsername, smtpPassword);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>Send a simple email.</summary>
        /// <param name="mailFrom"></param>
        /// <param name="mailTo"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <param name="priority"></param>
        /// <param name="subject"></param>
        /// <param name="bodyFormat"></param>
        /// <param name="bodyEncoding"></param>
        /// <param name="body"></param>
        /// <param name="attachment"></param>
        /// <param name="smtpServer"></param>
        /// <param name="smtpAuthentication"></param>
        /// <param name="smtpUsername"></param>
        /// <param name="smtpPassword"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body,
                                      string attachment, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword)
        {
            return SendMail(mailFrom, mailTo, cc, bcc, priority, subject, bodyFormat, bodyEncoding, body, attachment, smtpServer, smtpAuthentication, smtpUsername, smtpPassword, Host.EnableSMTPSSL);
        }

        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body,
                                      string attachment, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            return SendMail(
                mailFrom,
                mailTo,
                cc,
                bcc,
                mailFrom,
                priority,
                subject,
                bodyFormat,
                bodyEncoding,
                body,
                attachment.Split('|'),
                smtpServer,
                smtpAuthentication,
                smtpUsername,
                smtpPassword,
                smtpEnableSSL);
        }

        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body,
                                      string[] attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            return SendMail(
                mailFrom,
                mailTo,
                cc,
                bcc,
                mailFrom,
                priority,
                subject,
                bodyFormat,
                bodyEncoding,
                body,
                attachments,
                smtpServer,
                smtpAuthentication,
                smtpUsername,
                smtpPassword,
                smtpEnableSSL);
        }

        /// <summary>
        /// Sends an email based on params.
        /// </summary>
        /// <param name="mailFrom">Email sender.</param>
        /// <param name="mailTo">Recipients, can be more then one separated by semi-colons.</param>
        /// <param name="cc">CC-recipients, can be more then one separated by semi-colons.</param>
        /// <param name="bcc">BCC-recipients, can be more then one separated by semi-colons.</param>
        /// <param name="replyTo">Reply-to email to be displayed for recipients.</param>
        /// <param name="priority"><see cref="DotNetNuke.Services.Mail.MailPriority"/>.</param>
        /// <param name="subject">Subject of email.</param>
        /// <param name="bodyFormat"><see cref="DotNetNuke.Services.Mail.MailFormat"/>.</param>
        /// <param name="bodyEncoding">Email Encoding from System.Text.Encoding.</param>
        /// <param name="body">Body of email.</param>
        /// <param name="attachments">List of filenames to attach to email.</param>
        /// <param name="smtpServer">IP or ServerName of the SMTP server. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpAuthentication">SMTP authentication method. Can be "0" - anonymous, "1" - basic, "2" - NTLM. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpUsername">SMTP authentication UserName. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpPassword">SMTP authentication Password. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpEnableSSL">Enable or disable SSL.</param>
        /// <returns>Returns an empty string on success mail sending. Otherwise returns an error description.</returns>
        /// <example>SendMail(  "admin@email.com",
        ///                         "user@email.com",
        ///                         "user1@email.com;user2@email.com",
        ///                         "user3@email.com",
        ///                         "no-reply@email.com",
        ///                         MailPriority.Low,
        ///                         "This is test email",
        ///                         MailFormat.Text,
        ///                         Encoding.UTF8,
        ///                         "Test body. Test body. Test body.",
        ///                         new string[] {"d:\documents\doc1.doc","d:\documents\doc2.doc"},
        ///                         "mail.email.com",
        ///                         "1",
        ///                         "admin@email.com",
        ///                         "AdminPassword",
        ///                         false).
        ///     </example>
        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding,
                                      string body, string[] attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            var attachmentList = (from attachment in attachments
                                  where !string.IsNullOrEmpty(attachment)
                                  select new Attachment(attachment))
                                  .ToList();

            return SendMail(
                mailFrom,
                mailTo,
                cc,
                bcc,
                replyTo,
                priority,
                subject,
                bodyFormat,
                bodyEncoding,
                body,
                attachmentList,
                smtpServer,
                smtpAuthentication,
                smtpUsername,
                smtpPassword,
                smtpEnableSSL);
        }

        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding,
                              string body, List<Attachment> attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            return SendMail(
                mailFrom,
                string.Empty,
                mailTo,
                cc,
                bcc,
                replyTo,
                priority,
                subject,
                bodyFormat,
                bodyEncoding,
                body,
                attachments,
                smtpServer,
                smtpAuthentication,
                smtpUsername,
                smtpPassword,
                smtpEnableSSL);
        }

        public static string SendMail(string mailFrom, string mailSender, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding,
                                      string body, List<Attachment> attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            // SMTP server configuration
            if (string.IsNullOrWhiteSpace(smtpServer) && !string.IsNullOrWhiteSpace(Host.SMTPServer))
            {
                smtpServer = Host.SMTPServer;
            }

            if (string.IsNullOrEmpty(smtpAuthentication) && !string.IsNullOrEmpty(Host.SMTPAuthentication))
            {
                smtpAuthentication = Host.SMTPAuthentication;
            }

            if (string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(Host.SMTPUsername))
            {
                smtpUsername = Host.SMTPUsername;
            }

            if (string.IsNullOrEmpty(smtpPassword) && !string.IsNullOrEmpty(Host.SMTPPassword))
            {
                smtpPassword = Host.SMTPPassword;
            }

            MailMessage mailMessage = null;
            if (PortalSettings.Current != null)
            {
                mailMessage = (UserController.GetUserByEmail(PortalSettings.Current.PortalId, mailFrom) != null)
                    ? new MailMessage
                    {
                        From =
                            new MailAddress(
                                mailFrom,
                                UserController.GetUserByEmail(PortalSettings.Current.PortalId, mailFrom).DisplayName),
                    }
                    : new MailMessage { From = new MailAddress(mailFrom) };
            }
            else
            {
                mailMessage = new MailMessage { From = new MailAddress(mailFrom) };
            }

            if (!string.IsNullOrEmpty(mailSender))
            {
                mailMessage.Sender = new MailAddress(mailSender);
            }

            if (!string.IsNullOrEmpty(mailTo))
            {
                // translate semi-colon delimiters to commas as ASP.NET 2.0 does not support semi-colons
                mailTo = mailTo.Replace(";", ",");
                mailMessage.To.Add(mailTo);
            }

            if (!string.IsNullOrEmpty(cc))
            {
                // translate semi-colon delimiters to commas as ASP.NET 2.0 does not support semi-colons
                cc = cc.Replace(";", ",");
                mailMessage.CC.Add(cc);
            }

            if (!string.IsNullOrEmpty(bcc))
            {
                // translate semi-colon delimiters to commas as ASP.NET 2.0 does not support semi-colons
                bcc = bcc.Replace(";", ",");
                mailMessage.Bcc.Add(bcc);
            }

            if (replyTo != string.Empty)
            {
                mailMessage.ReplyToList.Add(new MailAddress(replyTo));
            }

            using (mailMessage)
            {
                return SendMailInternal(mailMessage, subject, body, priority, bodyFormat, bodyEncoding,
                    attachments, smtpServer, smtpAuthentication, smtpUsername, smtpPassword, smtpEnableSSL);
            }
        }

        private static string SendMailInternal(MailMessage mailMessage, string subject, string body, MailPriority priority,
                                MailFormat bodyFormat, Encoding bodyEncoding, IEnumerable<Attachment> attachments,
                                string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            string retValue = string.Empty;

            mailMessage.Priority = (System.Net.Mail.MailPriority)priority;
            mailMessage.IsBodyHtml = bodyFormat == MailFormat.Html;

            // Only modify senderAdress if smtpAuthentication is enabled
            // Can be "0", empty or Null - anonymous, "1" - basic, "2" - NTLM.
            if (smtpAuthentication == "1" || smtpAuthentication == "2")
            {
                // if the senderAddress is the email address of the Host then switch it smtpUsername if different
                // if display name of senderAddress is empty, then use Host.HostTitle for it
                if (mailMessage.Sender != null)
                {
                    var senderAddress = mailMessage.Sender.Address;
                    var senderDisplayName = mailMessage.Sender.DisplayName;
                    var needUpdateSender = false;
                    if (smtpUsername.Contains("@") && senderAddress == Host.HostEmail &&
                        !senderAddress.Equals(smtpUsername, StringComparison.InvariantCultureIgnoreCase))
                    {
                        senderAddress = smtpUsername;
                        needUpdateSender = true;
                    }

                    if (string.IsNullOrEmpty(senderDisplayName))
                    {
                        senderDisplayName = Host.SMTPPortalEnabled ? PortalSettings.Current.PortalName : Host.HostTitle;
                        needUpdateSender = true;
                    }

                    if (needUpdateSender)
                    {
                        mailMessage.Sender = new MailAddress(senderAddress, senderDisplayName);
                    }
                }
                else if (smtpUsername.Contains("@"))
                {
                    mailMessage.Sender = new MailAddress(smtpUsername, Host.SMTPPortalEnabled ? PortalSettings.Current.PortalName : Host.HostTitle);
                }
            }

            // attachments
            foreach (var attachment in attachments)
            {
                mailMessage.Attachments.Add(attachment);
            }

            // message
            mailMessage.SubjectEncoding = bodyEncoding;
            mailMessage.Subject = HtmlUtils.StripWhiteSpace(subject, true);
            mailMessage.BodyEncoding = bodyEncoding;

            // added support for multipart html messages
            // add text part as alternate view
            var PlainView = AlternateView.CreateAlternateViewFromString(ConvertToText(body), null, "text/plain");
            mailMessage.AlternateViews.Add(PlainView);
            if (mailMessage.IsBodyHtml)
            {
                var HTMLView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                mailMessage.AlternateViews.Add(HTMLView);
            }

            smtpServer = smtpServer.Trim();
            if (SmtpServerRegex.IsMatch(smtpServer))
            {
                try
                {
                    // to workaround problem in 4.0 need to specify host name
                    using (var smtpClient = new SmtpClient())
                    {
                        var smtpHostParts = smtpServer.Split(':');
                        smtpClient.Host = smtpHostParts[0];
                        if (smtpHostParts.Length > 1)
                        {
                            // port is guaranteed to be of max 5 digits numeric by the RegEx check
                            var port = Convert.ToInt32(smtpHostParts[1]);
                            if (port < 1 || port > 65535)
                            {
                                return Localize.GetString("SmtpInvalidPort");
                            }

                            smtpClient.Port = port;
                        }

                        // else the port defaults to 25 by .NET when not set
                        smtpClient.ServicePoint.MaxIdleTime = Host.SMTPMaxIdleTime;
                        smtpClient.ServicePoint.ConnectionLimit = Host.SMTPConnectionLimit;

                        switch (smtpAuthentication)
                        {
                            case "":
                            case "0": // anonymous
                                break;
                            case "1": // basic
                                if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
                                {
                                    smtpClient.UseDefaultCredentials = false;
                                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                                }

                                break;
                            case "2": // NTLM
                                smtpClient.UseDefaultCredentials = true;
                                break;
                        }

                        smtpClient.EnableSsl = smtpEnableSSL;
                        smtpClient.Send(mailMessage);
                        smtpClient.Dispose();
                    }
                }
                catch (Exception exc)
                {
                    var exc2 = exc as SmtpFailedRecipientException;
                    if (exc2 != null)
                    {
                        retValue = string.Format(Localize.GetString("FailedRecipient"), exc2.FailedRecipient) + " ";
                    }
                    else if (exc is SmtpException)
                    {
                        retValue = Localize.GetString("SMTPConfigurationProblem") + " ";
                    }

                    // mail configuration problem
                    if (exc.InnerException != null)
                    {
                        retValue += string.Concat(exc.Message, Environment.NewLine, exc.InnerException.Message);
                        Exceptions.Exceptions.LogException(exc.InnerException);
                    }
                    else
                    {
                        retValue += exc.Message;
                        Exceptions.Exceptions.LogException(exc);
                    }
                }
                finally
                {
                    mailMessage.Dispose();
                }
            }
            else
            {
                retValue = Localize.GetString("SMTPConfigurationProblem");
            }

            return retValue;
        }
    }
}
