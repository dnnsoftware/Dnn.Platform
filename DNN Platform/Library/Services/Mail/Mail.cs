// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Tokens;

    using Localize = DotNetNuke.Services.Localization.Localization;

    /// <summary>Email utility class.</summary>
    public partial class Mail
    {
        public static string ConvertToText(string sHTML)
        {
            var formattedHtml = HtmlUtils.FormatText(sHTML, true);
            var styleLessHtml = HtmlUtils.RemoveInlineStyle(formattedHtml);
            return HtmlUtils.StripTags(styleLessHtml, true);
        }

        public static bool IsValidEmailAddress(string email, int portalid)
        {
            string pattern = Null.NullString;

            // During install Wizard we may not have a valid PortalID
            if (portalid != Null.NullInteger)
            {
                pattern = Convert.ToString(UserController.GetUserSettings(portalid)["Security_EmailValidation"]);
            }

            pattern = string.IsNullOrEmpty(pattern) ? Globals.glbEmailRegEx : pattern;
            return Regex.Match(email, pattern).Success;
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

            var mailInfo = new MailInfo
            {
                From = fromAddress,
                Sender = senderAddress,
                To = toAddress,
                Subject = subject,
                Body = body,
                Priority = MailPriority.Normal,
                BodyFormat = HtmlUtils.IsHtml(body) ? MailFormat.Html : MailFormat.Text,
                BodyEncoding = Encoding.UTF8,
            };

            MailProvider.Instance().SendMail(mailInfo);
        }

        /// <summary>Send a simple email.</summary>
        /// <param name="fromAddress">From email address.</param>
        /// <param name="senderAddress">Sender email address.</param>
        /// <param name="toAddress">Recipients, can be more than one separated by semi-colons.</param>
        /// <param name="subject">Subject of email.</param>
        /// <param name="body">Body of email.</param>
        /// <param name="attachments">A list of attachments.</param>
        /// <returns>Returns an empty string on success mail sending. Otherwise returns an error description.</returns>
        [DnnDeprecated(9, 8, 0, "Please use SendEmail() with ICollection<MailAttachment>")]
        public static partial string SendEmail(string fromAddress, string senderAddress, string toAddress, string subject, string body, List<Attachment> attachments)
        {
            var mailInfo = new MailInfo
            {
                From = fromAddress,
                Sender = senderAddress,
                To = toAddress,
                Subject = subject,
                Body = body,
                Priority = MailPriority.Normal,
                BodyFormat = HtmlUtils.IsHtml(body) ? MailFormat.Html : MailFormat.Text,
                BodyEncoding = Encoding.UTF8,
                Attachments = ConvertAttachments(attachments),
            };

            return MailProvider.Instance().SendMail(mailInfo);
        }

        public static string SendEmail(string fromAddress, string senderAddress, string toAddress, string subject, string body, ICollection<MailAttachment> attachments)
        {
            var mailInfo = new MailInfo
            {
                From = fromAddress,
                Sender = senderAddress,
                To = toAddress,
                Subject = subject,
                Body = body,
                Priority = MailPriority.Normal,
                BodyFormat = HtmlUtils.IsHtml(body) ? MailFormat.Html : MailFormat.Text,
                BodyEncoding = Encoding.UTF8,
                Attachments = attachments,
            };

            return MailProvider.Instance().SendMail(mailInfo);
        }

        /// <summary>Send an email notification.</summary>
        /// <param name="user">The user to whom the message is being sent.</param>
        /// <param name="msgType">The type of message being sent.</param>
        /// <param name="settings">Portal Settings.</param>
        /// <returns><see cref="Null.NullString"/>.</returns>
        public static string SendMail(UserInfo user, MessageType msgType, PortalSettings settings)
        {
            return SendMail(user.PortalID, user.UserID, msgType, settings);
        }

        /// <summary>Send an email notification.</summary>
        /// <param name="portalId">The PortalId of the user to whom the message is being sent.</param>
        /// <param name="userId">The UserId of the user to whom the message is being sent.</param>
        /// <param name="msgType">The type of message being sent.</param>
        /// <param name="settings">Portal Settings.</param>
        /// <returns><see cref="Null.NullString"/>.</returns>
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
                                     HttpContext.Current.Server.HtmlEncode(
                                         HttpContext.Current.Server.UrlEncode(user.Username)),
                                     HttpContext.Current.Server.UrlEncode(
                                         user.GetProperty(
                                             "verificationcode",
                                             string.Empty,
                                             null,
                                             user,
                                             Scope.SystemMessages,
                                             ref propertyNotFound)),
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
                case MessageType.PasswordReminderUserIsNotApprovedAdmin:
                    subject = "EMAIL_PASSWORD_REMINDER_USER_ISNOT_APPROVED_ADMINISTRATOR_SUBJECT";
                    body = "EMAIL_PASSWORD_REMINDER_USER_ISNOT_APPROVED_ADMINISTRATOR_BODY";
                    toUser = settings.AdministratorId;
                    admin = UserController.GetUserById(settings.PortalId, settings.AdministratorId);
                    locale = admin.Profile.PreferredLocale;
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

        /// <summary>Send a simple email.</summary>
        /// <param name="mailFrom">Email sender.</param>
        /// <param name="mailTo">Recipients, can be more than one separated by semicolons.</param>
        /// <param name="bcc">BCC-recipients, can be more than one separated by semicolons.</param>
        /// <param name="subject">Subject of email.</param>
        /// <param name="body">Body of email.</param>
        /// <param name="attachment">A filename to attach to the email (or <see cref="string.Empty"/> for no attachment).</param>
        /// <param name="bodyType"><c>"HTML"</c> if the body should be represented as HTML rather than text.</param>
        /// <param name="smtpServer">IP or ServerName of the SMTP server. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpAuthentication">SMTP authentication method. Can be "0" - anonymous, "1" - basic, "2" - NTLM. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpUsername">SMTP authentication UserName. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpPassword">SMTP authentication Password. When empty or null, then it takes from the HostSettings.</param>
        /// <returns>Returns an empty string on success mail sending. Otherwise returns an error description.</returns>
        public static string SendMail(string mailFrom, string mailTo, string bcc, string subject, string body, string attachment, string bodyType, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword)
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

        /// <summary>Send a simple email.</summary>
        /// <param name="mailFrom">Email sender.</param>
        /// <param name="mailTo">Recipients, can be more than one separated by semi-colons.</param>
        /// <param name="cc">CC-recipients, can be more than one separated by semi-colons.</param>
        /// <param name="bcc">BCC-recipients, can be more than one separated by semi-colons.</param>
        /// <param name="priority"><see cref="DotNetNuke.Services.Mail.MailPriority"/>.</param>
        /// <param name="subject">Subject of email.</param>
        /// <param name="bodyFormat"><see cref="DotNetNuke.Services.Mail.MailFormat"/>.</param>
        /// <param name="bodyEncoding">Email Encoding from System.Text.Encoding.</param>
        /// <param name="body">Body of email.</param>
        /// <param name="attachment">A filename to attach to the email (or <see cref="string.Empty"/> for no attachment).</param>
        /// <param name="smtpServer">IP or ServerName of the SMTP server. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpAuthentication">SMTP authentication method. Can be "0" - anonymous, "1" - basic, "2" - NTLM. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpUsername">SMTP authentication UserName. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpPassword">SMTP authentication Password. When empty or null, then it takes from the HostSettings.</param>
        /// <returns>Returns an empty string on success mail sending. Otherwise returns an error description.</returns>
        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, string attachment, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword)
        {
            return SendMail(mailFrom, mailTo, cc, bcc, priority, subject, bodyFormat, bodyEncoding, body, attachment, smtpServer, smtpAuthentication, smtpUsername, smtpPassword, Host.EnableSMTPSSL);
        }

        /// <summary>Send a simple email.</summary>
        /// <param name="mailFrom">Email sender.</param>
        /// <param name="mailTo">Recipients, can be more than one separated by semi-colons.</param>
        /// <param name="cc">CC-recipients, can be more than one separated by semi-colons.</param>
        /// <param name="bcc">BCC-recipients, can be more than one separated by semi-colons.</param>
        /// <param name="priority"><see cref="DotNetNuke.Services.Mail.MailPriority"/>.</param>
        /// <param name="subject">Subject of email.</param>
        /// <param name="bodyFormat"><see cref="DotNetNuke.Services.Mail.MailFormat"/>.</param>
        /// <param name="bodyEncoding">Email Encoding from System.Text.Encoding.</param>
        /// <param name="body">Body of email.</param>
        /// <param name="attachment">A filename to attach to the email (or <see cref="string.Empty"/> for no attachment).</param>
        /// <param name="smtpServer">IP or ServerName of the SMTP server. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpAuthentication">SMTP authentication method. Can be "0" - anonymous, "1" - basic, "2" - NTLM. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpUsername">SMTP authentication UserName. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpPassword">SMTP authentication Password. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpEnableSSL">Enable or disable SSL.</param>
        /// <returns>Returns an empty string on success mail sending. Otherwise returns an error description.</returns>
        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, string attachment, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
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
                smtpEnableSSL,
                Host.SMTPAuthProvider);
        }

        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, string attachment, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL, string authProvider)
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
                smtpEnableSSL,
                authProvider);
        }

        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, string[] attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
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
                smtpEnableSSL,
                Host.SMTPAuthProvider);
        }

        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, string[] attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL, string authProvider)
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
                smtpEnableSSL,
                authProvider);
        }

        /// <summary>Sends an email based on params.</summary>
        /// <param name="mailFrom">Email sender.</param>
        /// <param name="mailTo">Recipients, can be more than one separated by semi-colons.</param>
        /// <param name="cc">CC-recipients, can be more than one separated by semi-colons.</param>
        /// <param name="bcc">BCC-recipients, can be more than one separated by semi-colons.</param>
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
        /// <example><code>SendMail(  "admin@email.com",
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
        ///                         false)</code>
        ///     </example>
        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, string[] attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
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
                smtpEnableSSL,
                Host.SMTPAuthProvider);
        }

        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, string[] attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL, string authProvider)
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
                smtpEnableSSL,
                authProvider);
        }

        /// <summary>Send a simple email.</summary>
        /// <param name="mailFrom">Email sender.</param>
        /// <param name="mailTo">Recipients, can be more than one separated by semi-colons.</param>
        /// <param name="cc">CC-recipients, can be more than one separated by semi-colons.</param>
        /// <param name="bcc">BCC-recipients, can be more than one separated by semi-colons.</param>
        /// <param name="replyTo">Reply-to email address.</param>
        /// <param name="priority"><see cref="DotNetNuke.Services.Mail.MailPriority"/>.</param>
        /// <param name="subject">Subject of email.</param>
        /// <param name="bodyFormat"><see cref="DotNetNuke.Services.Mail.MailFormat"/>.</param>
        /// <param name="bodyEncoding">Email Encoding from System.Text.Encoding.</param>
        /// <param name="body">Body of email.</param>
        /// <param name="attachments">A list of attachments.</param>
        /// <param name="smtpServer">IP or ServerName of the SMTP server. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpAuthentication">SMTP authentication method. Can be "0" - anonymous, "1" - basic, "2" - NTLM. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpUsername">SMTP authentication UserName. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpPassword">SMTP authentication Password. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpEnableSSL">Enable or disable SSL.</param>
        /// <returns>Returns an empty string on success mail sending. Otherwise returns an error description.</returns>
        [DnnDeprecated(9, 8, 0, "Please use SendEmail() with ICollection<MailAttachment>")]
        public static partial string SendMail(string mailFrom, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, List<Attachment> attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
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
                smtpEnableSSL,
                Host.SMTPAuthProvider);
        }

        [Obsolete("This method has been deprecated. Please use SendEmail() with ICollection<MailAttachment> Scheduled removal in v11.0.0.")]
        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, List<Attachment> attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL, string authProvider)
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
                smtpEnableSSL,
                authProvider);
        }

        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, ICollection<MailAttachment> attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
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
                smtpEnableSSL,
                Host.SMTPAuthProvider);
        }

        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, ICollection<MailAttachment> attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL, string authProvider)
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
                smtpEnableSSL,
                authProvider);
        }

        /// <summary>Send a simple email.</summary>
        /// <param name="mailFrom">From email address.</param>
        /// <param name="mailSender">Sender email address.</param>
        /// <param name="mailTo">Recipients, can be more than one separated by semi-colons.</param>
        /// <param name="cc">CC-recipients, can be more than one separated by semi-colons.</param>
        /// <param name="bcc">BCC-recipients, can be more than one separated by semi-colons.</param>
        /// <param name="replyTo">Reply-to email address.</param>
        /// <param name="priority"><see cref="DotNetNuke.Services.Mail.MailPriority"/>.</param>
        /// <param name="subject">Subject of email.</param>
        /// <param name="bodyFormat"><see cref="DotNetNuke.Services.Mail.MailFormat"/>.</param>
        /// <param name="bodyEncoding">Email Encoding from System.Text.Encoding.</param>
        /// <param name="body">Body of email.</param>
        /// <param name="attachments">A list of attachments.</param>
        /// <param name="smtpServer">IP or ServerName of the SMTP server. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpAuthentication">SMTP authentication method. Can be "0" - anonymous, "1" - basic, "2" - NTLM. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpUsername">SMTP authentication UserName. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpPassword">SMTP authentication Password. When empty or null, then it takes from the HostSettings.</param>
        /// <param name="smtpEnableSSL">Enable or disable SSL.</param>
        /// <returns>Returns an empty string on success mail sending. Otherwise returns an error description.</returns>
        [DnnDeprecated(9, 8, 0, "Please use SendEmail() with ICollection<MailAttachment>")]
        public static partial string SendMail(string mailFrom, string mailSender, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, List<Attachment> attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            return SendMail(
                mailFrom,
                mailSender,
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
                smtpEnableSSL,
                Host.SMTPAuthProvider);
        }

        [Obsolete("This method has been deprecated. Please use SendEmail() with ICollection<MailAttachment> Scheduled removal in v11.0.0.")]
        public static string SendMail(string mailFrom, string mailSender, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, List<Attachment> attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL, string authProvider)
        {
            var smtpInfo = new SmtpInfo
            {
                Server = smtpServer,
                Authentication = smtpAuthentication,
                Username = smtpUsername,
                Password = smtpPassword,
                EnableSSL = smtpEnableSSL,
                AuthProvider = authProvider,
            };

            var mailInfo = new MailInfo
            {
                From = mailFrom,
                Sender = mailSender,
                To = mailTo,
                CC = cc,
                BCC = bcc,
                ReplyTo = replyTo,
                Priority = priority,
                BodyEncoding = bodyEncoding,
                BodyFormat = bodyFormat,
                Body = body,
                Subject = subject,
                Attachments = ConvertAttachments(attachments),
            };

            if (PortalSettings.Current != null && UserController.GetUserByEmail(PortalSettings.Current.PortalId, mailFrom) != null)
            {
                mailInfo.FromName = UserController.GetUserByEmail(PortalSettings.Current.PortalId, mailFrom).DisplayName;
            }

            return MailProvider.Instance().SendMail(mailInfo, smtpInfo);
        }

        public static string SendMail(string mailFrom, string mailSender, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, ICollection<MailAttachment> attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            return SendMail(
                mailFrom,
                mailSender,
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
                smtpEnableSSL,
                Host.SMTPAuthProvider);
        }

        public static string SendMail(string mailFrom, string mailSender, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body, ICollection<MailAttachment> attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL, string authProvider)
        {
            var smtpInfo = new SmtpInfo
            {
                Server = smtpServer,
                Authentication = smtpAuthentication,
                Username = smtpUsername,
                Password = smtpPassword,
                EnableSSL = smtpEnableSSL,
                AuthProvider = authProvider,
            };

            var mailInfo = new MailInfo
            {
                From = mailFrom,
                Sender = mailSender,
                To = mailTo,
                CC = cc,
                BCC = bcc,
                ReplyTo = replyTo,
                Priority = priority,
                BodyEncoding = bodyEncoding,
                BodyFormat = bodyFormat,
                Body = body,
                Subject = subject,
                Attachments = attachments,
            };

            if (PortalSettings.Current != null && UserController.GetUserByEmail(PortalSettings.Current.PortalId, mailFrom) != null)
            {
                mailInfo.FromName = UserController.GetUserByEmail(PortalSettings.Current.PortalId, mailFrom).DisplayName;
            }

            return MailProvider.Instance().SendMail(mailInfo, smtpInfo);
        }

        private static ICollection<MailAttachment> ConvertAttachments(List<Attachment> attachments)
        {
            return attachments.Select(
                    attachment =>
                    {
                        using (var ms = new MemoryStream())
                        {
                            attachment.ContentStream.CopyTo(ms);
                            return new MailAttachment(attachment.Name, ms.ToArray());
                        }
                    })
                .ToList();
        }
    }
}
