#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

#region Usings

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

#endregion

namespace DotNetNuke.Services.Mail
{
    public class Mail
    {
        #region Private Methods

        private static string SendMailInternal(MailMessage mailMessage, string subject, string body, MailPriority priority,  
                                MailFormat bodyFormat, Encoding bodyEncoding, IEnumerable<Attachment> attachments, 
                                string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            string retValue;

            mailMessage.Priority = (System.Net.Mail.MailPriority)priority;
            mailMessage.IsBodyHtml = (bodyFormat == MailFormat.Html);

            //attachments
            foreach (var attachment in attachments)
            {
                mailMessage.Attachments.Add(attachment);
            }

            //message
            mailMessage.SubjectEncoding = bodyEncoding;
            mailMessage.Subject = HtmlUtils.StripWhiteSpace(subject, true);
            mailMessage.BodyEncoding = bodyEncoding;

            //added support for multipart html messages
            //add text part as alternate view
            var PlainView = AlternateView.CreateAlternateViewFromString(ConvertToText(body), null, "text/plain");
            mailMessage.AlternateViews.Add(PlainView);
            if (mailMessage.IsBodyHtml)
            {
                var HTMLView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                mailMessage.AlternateViews.Add(HTMLView);
            }
            
            if (!String.IsNullOrEmpty(smtpServer))
            {
                try
                {
                    var smtpClient = new SmtpClient();

                    var smtpHostParts = smtpServer.Split(':');
                    smtpClient.Host = smtpHostParts[0];
                    if (smtpHostParts.Length > 1)
                    {
                        smtpClient.Port = Convert.ToInt32(smtpHostParts[1]);
                    }

                    switch (smtpAuthentication)
                    {
                        case "":
                        case "0": //anonymous
                            break;
                        case "1": //basic
                            if (!String.IsNullOrEmpty(smtpUsername) && !String.IsNullOrEmpty(smtpPassword))
                            {
                                smtpClient.UseDefaultCredentials = false;
                                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                            }
                            break;
                        case "2": //NTLM
                            smtpClient.UseDefaultCredentials = true;
                            break;
                    }
                    smtpClient.EnableSsl = smtpEnableSSL;
                    smtpClient.Send(mailMessage);
                    retValue = "";
                }
                catch (SmtpFailedRecipientException exc)
                {
                    retValue = string.Format(Localize.GetString("FailedRecipient"), exc.FailedRecipient);
                    Exceptions.Exceptions.LogException(exc);
                }
                catch (SmtpException exc)
                {
                    retValue = Localize.GetString("SMTPConfigurationProblem");
                    Exceptions.Exceptions.LogException(exc);
                }
                catch (Exception exc)
                {
                    //mail configuration problem
                    if (exc.InnerException != null)
                    {
                        retValue = string.Concat(exc.Message, Environment.NewLine, exc.InnerException.Message);
                        Exceptions.Exceptions.LogException(exc.InnerException);
                    }
                    else
                    {
                        retValue = exc.Message;
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

        #endregion

        #region Public Methods

        public static string ConvertToText(string sHTML)
        {
            string sContent = sHTML;
            sContent = sContent.Replace("<br />", Environment.NewLine);
            sContent = sContent.Replace("<br>", Environment.NewLine);
            sContent = HtmlUtils.FormatText(sContent, true);
            return HtmlUtils.StripTags(sContent, true);
        }

        public static bool IsValidEmailAddress(string Email, int portalid)
        {
            string pattern = Null.NullString;
            //During install Wizard we may not have a valid PortalID
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
            if ((string.IsNullOrEmpty(Host.SMTPServer)))
            {
                return;
            }

            var emailMessage = new MailMessage(fromAddress, toAddress) { Sender = new MailAddress(senderAddress) };

            SendMailInternal(emailMessage, subject, body, MailPriority.Normal,
                                    HtmlUtils.IsHtml(body) ? MailFormat.Html : MailFormat.Text,
                                    Encoding.UTF8, new List<Attachment>(),
                                    Host.SMTPServer, Host.SMTPAuthentication, Host.SMTPUsername,
                                    Host.SMTPPassword, Host.EnableSMTPSSL);
        }

        public static string SendEmail(string fromAddress, string senderAddress, string toAddress, string subject, string body, List<Attachment> attachments)
        {
            if ((string.IsNullOrEmpty(Host.SMTPServer)))
            {
                return "SMTP Server not configured";
            }

            var emailMessage = new MailMessage(fromAddress, toAddress) { Sender = new MailAddress(senderAddress) };

            return SendMailInternal(emailMessage, subject, body, MailPriority.Normal,
                                    HtmlUtils.IsHtml(body) ? MailFormat.Html : MailFormat.Text,
                                    Encoding.UTF8, attachments,
                                    Host.SMTPServer, Host.SMTPAuthentication, Host.SMTPUsername,
                                    Host.SMTPPassword, Host.EnableSMTPSSL);

        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// <summary>Send an email notification</summary>
        /// </summary>
        /// <param name="user">The user to whom the message is being sent</param>
        /// <param name="msgType">The type of message being sent</param>
        /// <param name="settings">Portal Settings</param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        ///     [cnurse]        09/29/2005  Moved to Mail class
        ///     [sLeupold]      02/07/2008 language used for admin mails corrected
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string SendMail(UserInfo user, MessageType msgType, PortalSettings settings)
        {
			//Send Notification to User
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
                                         HttpContext.Current.Server.UrlEncode(user.Username),
                                         HttpContext.Current.Server.UrlEncode(user.GetProperty("verificationcode", String.Empty, null, user, Scope.SystemMessages, ref propertyNotFound))
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
                default:
                    subject = "EMAIL_USER_UPDATED_OWN_PASSWORD_SUBJECT";
                    body = "EMAIL_USER_UPDATED_OWN_PASSWORD_BODY";
                    break;
            }
          
            subject = Localize.GetSystemMessage(locale, settings, subject, user, Localize.GlobalResourceFile, custom, "", settings.AdministratorId);
            body = Localize.GetSystemMessage(locale, settings, body, user, Localize.GlobalResourceFile, custom, "", settings.AdministratorId);
        
            SendEmail(settings.Email, UserController.GetUserById(settings.PortalId, toUser).Email, subject, body);

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
        /// <history>
        ///     [cnurse]        09/29/2005  Moved to Mail class
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string SendMail(string mailFrom, string mailTo, string bcc, string subject, string body, string attachment, string bodyType, string smtpServer, string smtpAuthentication,
                                      string smtpUsername, string smtpPassword)
        {
            MailFormat bodyFormat = MailFormat.Text;
            if (!String.IsNullOrEmpty(bodyType))
            {
                switch (bodyType.ToLower())
                {
                    case "html":
                        bodyFormat = MailFormat.Html;
                        break;
                    case "text":
                        bodyFormat = MailFormat.Text;
                        break;
                }
            }
            return SendMail(mailFrom, mailTo, "", bcc, MailPriority.Normal, subject, bodyFormat, Encoding.UTF8, body, attachment, smtpServer, smtpAuthentication, smtpUsername, smtpPassword);
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
        /// <history>
        /// 	[Nik Kalyani]	10/15/2004	Replaced brackets in member names
        ///     [cnurse]        09/29/2005  Moved to Mail class
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body,
                                      string attachment, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword)
        {
            return SendMail(mailFrom, mailTo, cc, bcc, priority, subject, bodyFormat, bodyEncoding, body, attachment, smtpServer, smtpAuthentication, smtpUsername, smtpPassword, Host.EnableSMTPSSL);
        }

        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding, string body,
                                      string attachment, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            return SendMail(mailFrom,
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
            return SendMail(mailFrom,
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

        public static string SendMail(string mailFrom, string mailTo, string cc, string bcc, string replyTo, MailPriority priority, string subject, MailFormat bodyFormat, Encoding bodyEncoding,
                                      string body, string[] attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            var attachmentList = (from attachment in attachments 
                                  where !String.IsNullOrEmpty(attachment) 
                                  select new Attachment(attachment))
                                  .ToList();

            return SendMail(mailFrom,
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
            //SMTP server configuration
            if (string.IsNullOrEmpty(smtpServer) && !string.IsNullOrEmpty(Host.SMTPServer))
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
			
            //translate semi-colon delimiters to commas as ASP.NET 2.0 does not support semi-colons
            mailTo = mailTo.Replace(";", ",");
            cc = cc.Replace(";", ",");
            bcc = bcc.Replace(";", ",");

            MailMessage mailMessage = null;
            mailMessage = new MailMessage { From = new MailAddress(mailFrom) };
            if (!String.IsNullOrEmpty(mailTo))
            {
                mailMessage.To.Add(mailTo);
            }
            if (!String.IsNullOrEmpty(cc))
            {
                mailMessage.CC.Add(cc);
            }
            if (!String.IsNullOrEmpty(bcc))
            {
                mailMessage.Bcc.Add(bcc);
            }
            if (replyTo != string.Empty)
            {
                mailMessage.ReplyToList.Add(new MailAddress(replyTo));
            }

            return SendMailInternal(mailMessage, subject, body, priority, bodyFormat, bodyEncoding,
                attachments, smtpServer, smtpAuthentication, smtpUsername,smtpPassword, smtpEnableSSL);
        }

        #endregion

        #region Obsolete Methods

        [Obsolete("Obsoleted in DotNetNuke 5.5. Use DotNetNuke.Common.Utilities.HtmlUtils.IsHtml()")]
        public static bool IsHTMLMail(string Body)
        {
            return HtmlUtils.IsHtml(Body);
        }

        #endregion

    }
}
