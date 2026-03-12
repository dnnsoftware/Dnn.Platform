// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Mail.OAuth;

    using MailKit.Net.Smtp;
    using MailKit.Security;

    using Microsoft.Extensions.DependencyInjection;

    using MimeKit;

    using Localize = DotNetNuke.Services.Localization.Localization;

    /// <summary>A <see cref="MailProvider"/> implementation using <see cref="SmtpClient"/>).</summary>
    public class MailKitMailProvider : MailProvider
    {
        private static readonly Regex SmtpServerRegex = new Regex("^[^:]+(:[0-9]{1,5})?$", RegexOptions.Compiled);

        private readonly Lazy<ISmtpOAuthController> smtpOAuthController;
        private readonly IMailSettings mailSettings;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="MailKitMailProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.13.0. Use overload taking Lazy<ISmtpOAuthProvider>. Scheduled removal in v11.0.0.")]
        public MailKitMailProvider()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MailKitMailProvider"/> class.</summary>
        /// <param name="smtpOAuthController">The SMTP OAuth controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IMailSettings. Scheduled removal in v12.0.0.")]
        public MailKitMailProvider(Lazy<ISmtpOAuthController> smtpOAuthController)
            : this(smtpOAuthController, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MailKitMailProvider"/> class.</summary>
        /// <param name="smtpOAuthController">The SMTP OAuth controller.</param>
        /// <param name="mailSettings">The mail settings.</param>
        /// <param name="hostSettings">The host settings.</param>
        public MailKitMailProvider(Lazy<ISmtpOAuthController> smtpOAuthController, IMailSettings mailSettings, IHostSettings hostSettings)
        {
            this.smtpOAuthController = smtpOAuthController ?? Globals.GetCurrentServiceProvider().GetRequiredService<Lazy<ISmtpOAuthController>>();
            this.mailSettings = mailSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IMailSettings>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        /// <inheritdoc />
        public override bool SupportsOAuth => true;

        private static int CurrentPortalId => PortalSettings.Current?.PortalId ?? Null.NullInteger;

        private static string CurrentPortalName => PortalSettings.Current?.PortalName ?? string.Empty;

        /// <inheritdoc />
        public override string SendMail(MailInfo mailInfo, SmtpInfo smtpInfo = null)
        {
            try
            {
                var (host, port, errorMessage) = ParseSmtpServer(this.mailSettings, ref smtpInfo);
                if (errorMessage != null)
                {
                    return errorMessage;
                }

                var mailMessage = CreateMailMessage(this.mailSettings, this.hostSettings, mailInfo, smtpInfo);

                using var smtpClient = new SmtpClient();
                smtpClient.Connect(host, port, SecureSocketOptions.Auto);

                if (smtpInfo.Authentication == "1" && !string.IsNullOrEmpty(smtpInfo.Username) && !string.IsNullOrEmpty(smtpInfo.Password))
                {
                    smtpClient.Authenticate(smtpInfo.Username, smtpInfo.Password);
                }

                var (provider, portalId) = this.GetOAuthProvider(smtpClient, smtpInfo);
                provider?.Authorize(portalId, new OAuthSmtpClient(smtpClient));

                smtpClient.Send(mailMessage);
                smtpClient.Disconnect(true);

                return string.Empty;
            }
            catch (Exception exc)
            {
                return HandleException(exc);
            }
        }

        /// <inheritdoc />
        public override async Task<string> SendMailAsync(MailInfo mailInfo, SmtpInfo smtpInfo = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var (host, port, errorMessage) = ParseSmtpServer(this.mailSettings, ref smtpInfo);
            if (errorMessage != null)
            {
                return errorMessage;
            }

            var mailMessage = CreateMailMessage(this.mailSettings, this.hostSettings, mailInfo, smtpInfo);

            try
            {
                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.Auto, cancellationToken);

                if (smtpInfo.Authentication == "1" && !string.IsNullOrEmpty(smtpInfo.Username) && !string.IsNullOrEmpty(smtpInfo.Password))
                {
                    await smtpClient.AuthenticateAsync(smtpInfo.Username, smtpInfo.Password, cancellationToken);
                }

                var (provider, portalId) = this.GetOAuthProvider(smtpClient, smtpInfo);
                if (provider != null)
                {
                    await provider.AuthorizeAsync(portalId, new OAuthSmtpClient(smtpClient), cancellationToken);
                }

                await smtpClient.SendAsync(mailMessage, cancellationToken);
                await smtpClient.DisconnectAsync(true, cancellationToken);

                return string.Empty;
            }
            catch (Exception exc)
            {
                return HandleException(exc);
            }
        }

        private static (string Host, int Port, string ErrorMessage) ParseSmtpServer(IMailSettings mailSettings, ref SmtpInfo smtpInfo)
        {
            var port = 25;
            if (smtpInfo == null || string.IsNullOrEmpty(smtpInfo.Server))
            {
                if (string.IsNullOrWhiteSpace(mailSettings.GetServer(CurrentPortalId)))
                {
                    return (null, port, "SMTP Server not configured");
                }

                smtpInfo = new SmtpInfo
                           {
                               Server = mailSettings.GetServer(CurrentPortalId),
                               Authentication = mailSettings.GetAuthentication(CurrentPortalId),
                               Username = mailSettings.GetUsername(CurrentPortalId),
                               Password = mailSettings.GetPassword(CurrentPortalId),
                               EnableSSL = mailSettings.GetSecureConnectionEnabled(CurrentPortalId),
                               AuthProvider = mailSettings.GetAuthProvider(CurrentPortalId),
                           };
            }

            if (smtpInfo.Authentication == "2")
            {
                throw new NotSupportedException("NTLM authentication is not supported by MailKit provider");
            }

            smtpInfo.Server = smtpInfo.Server.Trim();
            if (!SmtpServerRegex.IsMatch(smtpInfo.Server))
            {
                return (null, port, Localize.GetString("SMTPConfigurationProblem"));
            }

            var smtpHostParts = smtpInfo.Server.Split(':');
            var host = smtpHostParts[0];
            if (smtpHostParts.Length <= 1)
            {
                return (host, port, null);
            }

            // port is guaranteed to be of max 5 digits numeric by the RegEx check
            port = int.Parse(smtpHostParts[1], CultureInfo.InvariantCulture);
            var errorMessage = port is < 1 or > 65535 ? Localize.GetString("SmtpInvalidPort") : null;
            return (host, port, errorMessage);
        }

        private static string HandleException(Exception exc)
        {
            var retValue = Localize.GetString("SMTPConfigurationProblem") + " ";

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

            return retValue;
        }

        private static MimeMessage CreateMailMessage(IMailSettings mailSettings, IHostSettings hostSettings, MailInfo mailInfo, SmtpInfo smtpInfo)
        {
            var mailMessage = new MimeMessage();

            mailMessage.From.Add(ParseAddressWithDisplayName(displayName: mailInfo.FromName, address: mailInfo.From));
            if (!string.IsNullOrEmpty(mailInfo.Sender))
            {
                mailMessage.Sender = MailboxAddress.Parse(mailInfo.Sender);
            }

            // translate semi-colon delimiters to commas as ASP.NET 2.0 does not support semi-colons
            if (!string.IsNullOrEmpty(mailInfo.To))
            {
                mailInfo.To = mailInfo.To.Replace(";", ",");
                mailMessage.To.AddRange(InternetAddressList.Parse(mailInfo.To));
            }

            if (!string.IsNullOrEmpty(mailInfo.CC))
            {
                mailInfo.CC = mailInfo.CC.Replace(";", ",");
                mailMessage.Cc.AddRange(InternetAddressList.Parse(mailInfo.CC));
            }

            if (!string.IsNullOrEmpty(mailInfo.BCC))
            {
                mailInfo.BCC = mailInfo.BCC.Replace(";", ",");
                mailMessage.Bcc.AddRange(InternetAddressList.Parse(mailInfo.BCC));
            }

            if (!string.IsNullOrEmpty(mailInfo.ReplyTo))
            {
                mailInfo.ReplyTo = mailInfo.ReplyTo.Replace(";", ",");
                mailMessage.ReplyTo.AddRange(InternetAddressList.Parse(mailInfo.ReplyTo));
            }

            mailMessage.Priority = ToMessagePriority(mailInfo.Priority);

            // Only modify senderAddress if smtpAuthentication is enabled
            // Can be "0", empty or Null - anonymous, "1" - basic, "2" - NTLM.
            if (smtpInfo.Authentication is "1" or "2")
            {
                // if the senderAddress is the email address of the Host then switch it smtpUsername if different
                // if display name of senderAddress is empty, then use Host.HostTitle for it
                if (mailMessage.Sender != null)
                {
                    var senderAddress = mailInfo.Sender;
                    var senderDisplayName = mailInfo.FromName;
                    var needUpdateSender = false;
                    if (smtpInfo.Username.Contains("@")
                        && senderAddress == hostSettings.HostEmail
                        && !senderAddress.Equals(smtpInfo.Username, StringComparison.OrdinalIgnoreCase))
                    {
                        senderAddress = smtpInfo.Username;
                        needUpdateSender = true;
                    }

                    if (string.IsNullOrEmpty(senderDisplayName))
                    {
                        senderDisplayName = mailSettings.IsPortalEnabled(CurrentPortalId) ? CurrentPortalName : hostSettings.HostTitle;
                        needUpdateSender = true;
                    }

                    if (needUpdateSender)
                    {
                        mailMessage.Sender = ParseAddressWithDisplayName(
                            displayName: senderDisplayName,
                            address: senderAddress);
                    }
                }
                else if (smtpInfo.Username.Contains("@"))
                {
                    mailMessage.Sender = ParseAddressWithDisplayName(
                        displayName: mailSettings.IsPortalEnabled(CurrentPortalId) ? CurrentPortalName : hostSettings.HostTitle,
                        address: smtpInfo.Username);
                }
            }

            var builder = new BodyBuilder { TextBody = Mail.ConvertToText(mailInfo.Body), };

            if (mailInfo.BodyFormat == MailFormat.Html)
            {
                builder.HtmlBody = mailInfo.Body;
            }

            // attachments
            if (mailInfo.Attachments != null)
            {
                foreach (var attachment in mailInfo.Attachments.Where(attachment => attachment.Content != null))
                {
                    builder.Attachments.Add(attachment.Filename, attachment.Content, ContentType.Parse(attachment.ContentType));
                }
            }

            // message
            mailMessage.Subject = HtmlUtils.StripWhiteSpace(mailInfo.Subject, true);
            mailMessage.Body = builder.ToMessageBody();
            return mailMessage;
        }

        private static MailboxAddress ParseAddressWithDisplayName(string displayName, string address)
        {
            var mailboxAddress = MailboxAddress.Parse(address);
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                mailboxAddress.Name = displayName;
            }

            return mailboxAddress;
        }

        private static MessagePriority ToMessagePriority(MailPriority priority) =>
            priority switch
            {
                MailPriority.Low => MessagePriority.NonUrgent,
                MailPriority.Normal => MessagePriority.Normal,
                MailPriority.High => MessagePriority.Urgent,
                _ => throw new ArgumentException($"Invalid MailPriority value: {priority}", nameof(priority)),
            };

        private (ISmtpOAuthProvider AuthProvider, int PortalId) GetOAuthProvider(SmtpClient smtpClient, SmtpInfo smtpInfo)
        {
            var usingOAuth = smtpInfo.Authentication == "3";
            if (!usingOAuth)
            {
                return (null, Null.NullInteger);
            }

            var authProvider = this.smtpOAuthController.Value.GetOAuthProvider(smtpInfo.AuthProvider);
            if (authProvider == null)
            {
                return (null, Null.NullInteger);
            }

            var portalId = Null.NullInteger;
            if (this.mailSettings.IsPortalEnabled(CurrentPortalId))
            {
                portalId = CurrentPortalId;
            }

            return (authProvider, portalId);
        }

        private class OAuthSmtpClient : IOAuth2SmtpClient
        {
            public OAuthSmtpClient(ISmtpClient smtpClient)
            {
                this.SmtpClient = smtpClient;
            }

            public ISmtpClient SmtpClient { get; }

            public void Authenticate(string username, string token)
            {
                this.SmtpClient.Authenticate(new SaslMechanismOAuth2(username, token));
            }

            public Task AuthenticateAsync(string username, string token, CancellationToken cancellationToken = default(CancellationToken))
            {
                return this.SmtpClient.AuthenticateAsync(new SaslMechanismOAuth2(username, token), cancellationToken);
            }
        }
    }
}
