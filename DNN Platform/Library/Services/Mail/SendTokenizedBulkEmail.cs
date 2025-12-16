// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Text;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Messaging.Data;
    using DotNetNuke.Services.Tokens;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>
    /// SendTokenizedBulkEmail Class is a class to manage the sending of bulk mails
    /// that contains tokens, which might be replaced with individual user properties.
    /// </summary>
    public class SendTokenizedBulkEmail : IDisposable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SendTokenizedBulkEmail));

        // ReSharper restore InconsistentNaming
        private readonly List<string> addressedRoles = new List<string>();
        private readonly List<UserInfo> addressedUsers = new List<UserInfo>();
        private readonly List<Attachment> attachments = new List<Attachment>();

        private UserInfo replyToUser;
        private bool smtpEnableSSL;
        private TokenReplace tokenReplace;
        private PortalSettings portalSettings;
        private UserInfo sendingUser;
        private string body = string.Empty;
        private string confirmBodyHTML;
        private string confirmBodyText;
        private string confirmSubject;
        private string noError;
        private string relayEmail;
        private string smtpAuthenticationMethod = string.Empty;
        private string smtpPassword = string.Empty;
        private string smtpServer = string.Empty;
        private string smtpUsername = string.Empty;
        private string strSenderLanguage;
        private bool isDisposed;

        /// <summary>Initializes a new instance of the <see cref="SendTokenizedBulkEmail"/> class.</summary>
        public SendTokenizedBulkEmail()
        {
            this.ReportRecipients = true;
            this.AddressMethod = AddressMethods.Send_TO;
            this.BodyFormat = MailFormat.Text;
            this.Subject = string.Empty;
            this.Priority = MailPriority.Normal;
            this.Initialize();
        }

        /// <summary>Initializes a new instance of the <see cref="SendTokenizedBulkEmail"/> class.</summary>
        /// <param name="addressedRoles">The names of the roles to which the email is addressed.</param>
        /// <param name="addressedUsers">The users to which the email is addressed.</param>
        /// <param name="removeDuplicates">Whether to remove duplicate recipients.</param>
        /// <param name="subject">The email's subject.</param>
        /// <param name="body">The email's body.</param>
        public SendTokenizedBulkEmail(List<string> addressedRoles, List<UserInfo> addressedUsers, bool removeDuplicates, string subject, string body)
        {
            this.ReportRecipients = true;
            this.AddressMethod = AddressMethods.Send_TO;
            this.BodyFormat = MailFormat.Text;
            this.Priority = MailPriority.Normal;
            this.addressedRoles = addressedRoles;
            this.addressedUsers = addressedUsers;
            this.RemoveDuplicates = removeDuplicates;
            this.Subject = subject;
            this.Body = body;
            this.Initialize();
        }

        /// <summary>Finalizes an instance of the <see cref="SendTokenizedBulkEmail"/> class.</summary>
        ~SendTokenizedBulkEmail()
        {
            this.Dispose(false);
        }

        /// <summary>Addressing Methods (personalized or hidden).</summary>
        // ReSharper disable InconsistentNaming
        // Existing public API
        public enum AddressMethods
        {
            /// <summary>Put the recipient's email address in the TO field.</summary>
            Send_TO = 1,

            /// <summary>Put the recipient's email address in the BCC field.</summary>
            Send_BCC = 2,

            /// <summary>Send via an email relay address.</summary>
            Send_Relay = 3,
        }

        /// <summary>Gets or sets priority of emails to be sent.</summary>
        public MailPriority Priority { get; set; }

        /// <summary>Gets or sets subject of the emails to be sent.</summary>
        /// <remarks>may contain tokens.</remarks>
        public string Subject { get; set; }

        /// <summary>Gets or sets body text of the email to be sent.</summary>
        /// <remarks>may contain HTML tags and tokens. Side effect: sets BodyFormat autmatically.</remarks>
        public string Body
        {
            get
            {
                return this.body;
            }

            set
            {
                this.body = value;
                this.BodyFormat = HtmlUtils.IsHtml(this.body) ? MailFormat.Html : MailFormat.Text;
            }
        }

        /// <summary>Gets or sets format of body text for the email to be sent.</summary>
        /// <remarks>by default activated, if tokens are found in Body and subject.</remarks>
        public MailFormat BodyFormat { get; set; }

        /// <summary>Gets or sets address method for the email to be sent (TO or BCC).</summary>
        /// <remarks>TO is default value.</remarks>
        public AddressMethods AddressMethod { get; set; }

        /// <summary>Gets or sets portal alias http path to be used for links to images, ...</summary>
        public string PortalAlias { get; set; }

        /// <summary>Gets or sets userInfo of the user sending the mail.</summary>
        /// <remarks>if not set explicitely, currentuser will be used.</remarks>
        public UserInfo SendingUser
        {
            get
            {
                return this.sendingUser;
            }

            set
            {
                this.sendingUser = value;
                if (this.sendingUser.Profile.PreferredLocale != null)
                {
                    this.strSenderLanguage = this.sendingUser.Profile.PreferredLocale;
                }
                else
                {
                    PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                    this.strSenderLanguage = portalSettings.DefaultLanguage;
                }
            }
        }

        /// <summary>Gets or sets email of the user to be shown in the mail as replyTo address.</summary>
        /// <remarks>if not set explicitely, sendingUser will be used.</remarks>
        public UserInfo ReplyTo
        {
            get
            {
                return this.replyToUser ?? this.SendingUser;
            }

            set
            {
                this.replyToUser = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether duplicate email addresses shall be ignored? (default value: false).</summary>
        /// <remarks>Duplicate Users (e.g. from multiple role selections) will always be ignored.</remarks>
        public bool RemoveDuplicates { get; set; }

        /// <summary>Gets or sets a value indicating whether automatic TokenReplace shall be prohibited?.</summary>
        /// <remarks>default value: false.</remarks>
        public bool SuppressTokenReplace { get; set; }

        /// <summary>Gets or sets a value indicating whether List of recipients shall be appended to confirmation report?.</summary>
        /// <remarks>enabled by default.</remarks>
        public bool ReportRecipients { get; set; }

        public string RelayEmailAddress
        {
            get
            {
                return this.AddressMethod == AddressMethods.Send_Relay ? this.relayEmail : string.Empty;
            }

            set
            {
                this.relayEmail = value;
            }
        }

        public string[] LanguageFilter { get; set; }

        /// <summary>Specify SMTP server to be used.</summary>
        /// <param name="smtpServer">name of the SMTP server.</param>
        /// <param name="smtpAuthentication">authentication string (0: anonymous, 1: basic, 2: NTLM).</param>
        /// <param name="smtpUsername">username to log in SMTP server.</param>
        /// <param name="smtpPassword">password to log in SMTP server.</param>
        /// <param name="smtpEnableSSL">SSL used to connect tp SMTP server.</param>
        /// <returns>always true.</returns>
        /// <remarks>if not called, values will be taken from host settings.</remarks>
        public bool SetSMTPServer(string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            this.EnsureNotDisposed();

            this.smtpServer = smtpServer;
            this.smtpAuthenticationMethod = smtpAuthentication;
            this.smtpUsername = smtpUsername;
            this.smtpPassword = smtpPassword;
            this.smtpEnableSSL = smtpEnableSSL;
            return true;
        }

        /// <summary>Add a single attachment file to the email.</summary>
        /// <param name="localPath">path to file to attach.</param>
        /// <remarks>only local stored files can be added with a path.</remarks>
        public void AddAttachment(string localPath)
        {
            this.EnsureNotDisposed();
            this.attachments.Add(new Attachment(localPath));
        }

        public void AddAttachment(Stream contentStream, ContentType contentType)
        {
            this.EnsureNotDisposed();
            this.attachments.Add(new Attachment(contentStream, contentType));
        }

        /// <summary>Add a single recipient.</summary>
        /// <param name="recipient">userinfo of user to add.</param>
        /// <remarks>emaiol will be used for addressing, other properties might be used for TokenReplace.</remarks>
        public void AddAddressedUser(UserInfo recipient)
        {
            this.EnsureNotDisposed();
            this.addressedUsers.Add(recipient);
        }

        /// <summary>Add all members of a role to recipient list.</summary>
        /// <param name="roleName">name of a role, whose members shall be added to recipients.</param>
        /// <remarks>emaiol will be used for addressing, other properties might be used for TokenReplace.</remarks>
        public void AddAddressedRole(string roleName)
        {
            this.EnsureNotDisposed();
            this.addressedRoles.Add(roleName);
        }

        /// <summary>All bulk mail recipients, derived from role names and individual adressees. </summary>
        /// <returns>List of userInfo objects, who receive the bulk mail. </returns>
        /// <remarks>user.Email used for sending, other properties might be used for TokenReplace.</remarks>
        public List<UserInfo> Recipients()
        {
            this.EnsureNotDisposed();

            var userList = new List<UserInfo>();
            var keyList = new List<string>();

            foreach (string roleName in this.addressedRoles)
            {
                string role = roleName;
                var roleInfo = RoleController.Instance.GetRole(this.portalSettings.PortalId, r => r.RoleName == role);

                foreach (UserInfo objUser in RoleController.Instance.GetUsersByRole(this.portalSettings.PortalId, roleName))
                {
                    UserInfo user = objUser;
                    ProfileController.GetUserProfile(ref user);
                    var userRole = RoleController.Instance.GetUserRole(this.portalSettings.PortalId, objUser.UserID, roleInfo.RoleID);

                    // only add if user role has not expired and effectivedate has been passed
                    if ((userRole.EffectiveDate <= DateTime.Now || Null.IsNull(userRole.EffectiveDate)) && (userRole.ExpiryDate >= DateTime.Now || Null.IsNull(userRole.ExpiryDate)))
                    {
                        this.ConditionallyAddUser(objUser, ref keyList, ref userList);
                    }
                }
            }

            foreach (UserInfo objUser in this.addressedUsers)
            {
                this.ConditionallyAddUser(objUser, ref keyList, ref userList);
            }

            return userList;
        }

        /// <summary>Send bulkmail to all recipients according to settings.</summary>
        /// <returns>Number of emails sent, null.integer if not determinable.</returns>
        /// <remarks>Detailed status report is sent by email to sending user.</remarks>
        public int SendMails()
        {
            this.EnsureNotDisposed();

            int recipients = 0;
            int messagesSent = 0;
            int errors = 0;

            try
            {
                // send to recipients
                string body = this.body;
                if (this.BodyFormat == MailFormat.Html)
                {
                    // Add Base Href for any images inserted in to the email.
                    var host = this.PortalAlias.Contains("/") ? this.PortalAlias.Substring(0, this.PortalAlias.IndexOf('/')) : this.PortalAlias;
                    body = "<html><head><base href='http://" + host + "'><title>" + this.Subject + "</title></head><body>" + body + "</body></html>";
                }

                string subject = this.Subject;
                string startedAt = DateTime.Now.ToString(CultureInfo.InvariantCulture);

                bool replaceTokens = !this.SuppressTokenReplace && (this.tokenReplace.ContainsTokens(this.Subject) || this.tokenReplace.ContainsTokens(this.body));
                bool individualSubj = false;
                bool individualBody = false;

                var mailErrors = new StringBuilder();
                var mailRecipients = new StringBuilder();

                switch (this.AddressMethod)
                {
                    case AddressMethods.Send_TO:
                    case AddressMethods.Send_Relay:
                        // optimization:
                        if (replaceTokens)
                        {
                            individualBody = this.tokenReplace.Cacheability(this.body) == CacheLevel.notCacheable;
                            individualSubj = this.tokenReplace.Cacheability(this.Subject) == CacheLevel.notCacheable;
                            if (!individualBody)
                            {
                                body = this.tokenReplace.ReplaceEnvironmentTokens(body);
                            }

                            if (!individualSubj)
                            {
                                subject = this.tokenReplace.ReplaceEnvironmentTokens(subject);
                            }
                        }

                        foreach (UserInfo user in this.Recipients())
                        {
                            recipients += 1;
                            if (individualBody || individualSubj)
                            {
                                this.tokenReplace.User = user;
                                this.tokenReplace.AccessingUser = user;
                                if (individualBody)
                                {
                                    body = this.tokenReplace.ReplaceEnvironmentTokens(this.body);
                                }

                                if (individualSubj)
                                {
                                    subject = this.tokenReplace.ReplaceEnvironmentTokens(this.Subject);
                                }
                            }

                            string recipient = this.AddressMethod == AddressMethods.Send_TO ? user.Email : this.RelayEmailAddress;

                            string mailError = Mail.SendMail(
                                this.sendingUser.Email,
                                recipient,
                                string.Empty,
                                string.Empty,
                                this.ReplyTo.Email,
                                this.Priority,
                                subject,
                                this.BodyFormat,
                                Encoding.UTF8,
                                body,
                                this.LoadAttachments(),
                                this.smtpServer,
                                this.smtpAuthenticationMethod,
                                this.smtpUsername,
                                this.smtpPassword,
                                this.smtpEnableSSL);
                            if (!string.IsNullOrEmpty(mailError))
                            {
                                mailErrors.Append(mailError);
                                mailErrors.AppendLine();
                                errors += 1;
                            }
                            else
                            {
                                mailRecipients.Append(user.Email);
                                mailRecipients.Append(this.BodyFormat == MailFormat.Html ? "<br />" : Environment.NewLine);
                                messagesSent += 1;
                            }
                        }

                        break;
                    case AddressMethods.Send_BCC:
                        var distributionList = new StringBuilder();
                        messagesSent = Null.NullInteger;
                        foreach (UserInfo user in this.Recipients())
                        {
                            recipients += 1;
                            distributionList.Append(user.Email + "; ");
                            mailRecipients.Append(user.Email);
                            mailRecipients.Append(this.BodyFormat == MailFormat.Html ? "<br />" : Environment.NewLine);
                        }

                        if (distributionList.Length > 2)
                        {
                            if (replaceTokens)
                            {
                                // no access to User properties possible!
                                var tr = new TokenReplace(Scope.Configuration);
                                body = tr.ReplaceEnvironmentTokens(this.body);
                                subject = tr.ReplaceEnvironmentTokens(this.Subject);
                            }
                            else
                            {
                                body = this.body;
                                subject = this.Subject;
                            }

                            string mailError = Mail.SendMail(
                                this.sendingUser.Email,
                                this.sendingUser.Email,
                                string.Empty,
                                distributionList.ToString(0, distributionList.Length - 2),
                                this.ReplyTo.Email,
                                this.Priority,
                                subject,
                                this.BodyFormat,
                                Encoding.UTF8,
                                body,
                                this.LoadAttachments(),
                                this.smtpServer,
                                this.smtpAuthenticationMethod,
                                this.smtpUsername,
                                this.smtpPassword,
                                this.smtpEnableSSL);
                            if (mailError == string.Empty)
                            {
                                messagesSent = 1;
                            }
                            else
                            {
                                mailErrors.Append(mailError);
                                errors += 1;
                            }
                        }

                        break;
                }

                if (mailErrors.Length > 0)
                {
                    mailRecipients = new StringBuilder();
                }

                this.SendConfirmationMail(recipients, messagesSent, errors, subject, startedAt, mailErrors.ToString(), mailRecipients.ToString());
            }
            catch (Exception exc)
            {
                // send mail failure
                Logger.Error(exc);

                Debug.Write(exc.Message);
            }
            finally
            {
                foreach (var attachment in this.attachments)
                {
                    attachment.Dispose();
                }
            }

            return messagesSent;
        }

        /// <summary>Wrapper for Function SendMails.</summary>
        public void Send()
        {
            this.EnsureNotDisposed();
            this.SendMails();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    // get rid of managed resources
                    foreach (Attachment attachment in this.attachments)
                    {
                        attachment.Dispose();
                        this.isDisposed = true;
                    }
                }

                // get rid of unmanaged resources
            }
        }

        /// <summary>internal method to initialize used objects, depending on parameters of construct method.</summary>
        private void Initialize()
        {
            this.portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            this.PortalAlias = this.portalSettings.PortalAlias.HTTPAlias;
            this.SendingUser = (UserInfo)HttpContext.Current.Items["UserInfo"];
            this.tokenReplace = new TokenReplace();
            this.confirmBodyHTML = Localization.GetString("EMAIL_BulkMailConf_Html_Body", Localization.GlobalResourceFile, this.strSenderLanguage);
            this.confirmBodyText = Localization.GetString("EMAIL_BulkMailConf_Text_Body", Localization.GlobalResourceFile, this.strSenderLanguage);
            this.confirmSubject = Localization.GetString("EMAIL_BulkMailConf_Subject", Localization.GlobalResourceFile, this.strSenderLanguage);
            this.noError = Localization.GetString("NoErrorsSending", Localization.GlobalResourceFile, this.strSenderLanguage);
            this.smtpEnableSSL = Host.EnableSMTPSSL;
        }

        /// <summary>Send bulkmail confirmation to admin.</summary>
        /// <param name="numRecipients">number of email recipients.</param>
        /// <param name="numMessages">number of messages sent, -1 if not determinable.</param>
        /// <param name="numErrors">number of emails not sent.</param>
        /// <param name="subject">Subject of BulkMail sent (to be used as reference).</param>
        /// <param name="startedAt">date/time, sendout started.</param>
        /// <param name="mailErrors">mail error texts.</param>
        /// <param name="recipientList">List of recipients as formatted string.</param>
        private void SendConfirmationMail(int numRecipients, int numMessages, int numErrors, string subject, string startedAt, string mailErrors, string recipientList)
        {
            // send confirmation, use resource string like:
            // Operation started at: [Custom:0]<br>
            // EmailRecipients:      [Custom:1]<b
            // EmailMessages sent:   [Custom:2]<br>
            // Operation Completed:  [Custom:3]<br>
            // Number of Errors:     [Custom:4]<br>
            // Error Report:<br>
            // [Custom:5]
            //--------------------------------------
            // Recipients:
            // [custom:6]
            var parameters = new ArrayList
            {
                startedAt,
                numRecipients.ToString(CultureInfo.InvariantCulture),
                numMessages >= 0 ? numMessages.ToString(CultureInfo.InvariantCulture) : "***",
                DateTime.Now.ToString(CultureInfo.InvariantCulture),
                numErrors > 0 ? numErrors.ToString(CultureInfo.InvariantCulture) : string.Empty,
                mailErrors != string.Empty ? mailErrors : this.noError,
                this.ReportRecipients ? recipientList : string.Empty,
            };
            this.tokenReplace.User = this.sendingUser;
            string body = this.tokenReplace.ReplaceEnvironmentTokens(this.BodyFormat == MailFormat.Html ? this.confirmBodyHTML : this.confirmBodyText, parameters, "Custom");
            string strSubject = string.Format(CultureInfo.CurrentCulture, this.confirmSubject, subject);
            if (!this.SuppressTokenReplace)
            {
                strSubject = this.tokenReplace.ReplaceEnvironmentTokens(strSubject);
            }

            var message = new Message { FromUserID = this.sendingUser.UserID, ToUserID = this.sendingUser.UserID, Subject = strSubject, Body = body, Status = MessageStatusType.Unread };

            Mail.SendEmail(this.sendingUser.Email, this.sendingUser.Email, message.Subject, message.Body);
        }

        /// <summary>check, if the user's language matches the current language filter.</summary>
        /// <param name="userLanguage">Language of the user.</param>
        /// <returns><paramref name="userLanguage"></paramref> matches current <see cref="LanguageFilter"/>.</returns>
        /// <remarks>if filter not set, true is returned.</remarks>
        private bool MatchLanguageFilter(string userLanguage)
        {
            if (this.LanguageFilter == null || this.LanguageFilter.Length == 0)
            {
                return true;
            }

            if (string.IsNullOrEmpty(userLanguage))
            {
                userLanguage = this.portalSettings.DefaultLanguage;
            }

            return this.LanguageFilter.Any(s => userLanguage.StartsWith(s, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>add a user to the userlist, if it is not already in there.</summary>
        /// <param name="user">user to add.</param>
        /// <param name="keyList">list of key (either email addresses or userid's).</param>
        /// <param name="userList">List of users.</param>
        /// <remarks>for use by Recipients method only.</remarks>
        private void ConditionallyAddUser(UserInfo user, ref List<string> keyList, ref List<UserInfo> userList)
        {
            if (((user.UserID <= 0 || user.Membership.Approved) && user.Email != string.Empty) && this.MatchLanguageFilter(user.Profile.PreferredLocale))
            {
                string key;
                if (this.RemoveDuplicates || user.UserID == Null.NullInteger)
                {
                    key = user.Email;
                }
                else
                {
                    key = user.UserID.ToString(CultureInfo.InvariantCulture);
                }

                if (key != string.Empty && !keyList.Contains(key))
                {
                    userList.Add(user);
                    keyList.Add(key);
                }
            }
        }

        private List<Attachment> LoadAttachments()
        {
            var attachments = new List<Attachment>();
            foreach (var attachment in this.attachments)
            {
                Attachment newAttachment;
                MemoryStream memoryStream = null;
                var buffer = new byte[4096];
                try
                {
                    memoryStream = new MemoryStream();
                    while (true)
                    {
                        var read = attachment.ContentStream.Read(buffer, 0, 4096);
                        if (read <= 0)
                        {
                            break;
                        }

                        memoryStream.Write(buffer, 0, read);
                    }

                    newAttachment = new Attachment(memoryStream, attachment.ContentType);
                    newAttachment.ContentStream.Position = 0;
                    attachments.Add(newAttachment);

                    // reset original position
                    attachment.ContentStream.Position = 0;
                    memoryStream = null;
                }
                finally
                {
                    memoryStream?.Dispose();
                }
            }

            return attachments;
        }

        private void EnsureNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("SharedDictionary");
            }
        }
    }
}
