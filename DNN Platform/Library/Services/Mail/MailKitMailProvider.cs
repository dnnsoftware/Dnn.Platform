// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;

    using MailKit.Net.Smtp;

    using MimeKit;

    using Localize = DotNetNuke.Services.Localization.Localization;

    /// <summary>A <see cref="MailProvider"/> implementation using <see cref="SmtpClient"/>).</summary>
    public class MailKitMailProvider : MailProvider
    {
        private static readonly Regex SmtpServerRegex = new Regex("^[^:]+(:[0-9]{1,5})?$", RegexOptions.Compiled);

        /// <inheritdoc />
        public override string SendMail(MailInfo mailInfo, SmtpInfo smtpInfo = null)
        {
            // validate smtp server
            if (smtpInfo == null || string.IsNullOrEmpty(smtpInfo.Server))
            {
                if (string.IsNullOrWhiteSpace(Host.SMTPServer))
                {
                    return "SMTP Server not configured";
                }

                smtpInfo = new SmtpInfo
                {
                    Server = Host.SMTPServer,
                    Authentication = Host.SMTPAuthentication,
                    Username = Host.SMTPUsername,
                    Password = Host.SMTPPassword,
                    EnableSSL = Host.EnableSMTPSSL,
                };
            }

            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress(mailInfo.FromName, mailInfo.From));
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

            mailMessage.Priority = (MessagePriority)mailInfo.Priority;

            // Only modify senderAddress if smtpAuthentication is enabled
            // Can be "0", empty or Null - anonymous, "1" - basic, "2" - NTLM.
            if (smtpInfo.Authentication == "1" || smtpInfo.Authentication == "2")
            {
                // if the senderAddress is the email address of the Host then switch it smtpUsername if different
                // if display name of senderAddress is empty, then use Host.HostTitle for it
                if (mailMessage.Sender != null)
                {
                    var senderAddress = mailInfo.Sender;
                    var senderDisplayName = mailInfo.FromName;
                    var needUpdateSender = false;
                    if (smtpInfo.Username.Contains("@") && senderAddress == Host.HostEmail &&
                        !senderAddress.Equals(smtpInfo.Username, StringComparison.InvariantCultureIgnoreCase))
                    {
                        senderAddress = smtpInfo.Username;
                        needUpdateSender = true;
                    }

                    if (string.IsNullOrEmpty(senderDisplayName))
                    {
                        senderDisplayName = Host.SMTPPortalEnabled ? PortalSettings.Current.PortalName : Host.HostTitle;
                        needUpdateSender = true;
                    }

                    if (needUpdateSender)
                    {
                        mailMessage.Sender = new MailboxAddress(senderDisplayName, senderAddress);
                    }
                }
                else if (smtpInfo.Username.Contains("@"))
                {
                    mailMessage.Sender = new MailboxAddress(Host.SMTPPortalEnabled ? PortalSettings.Current.PortalName : Host.HostTitle, smtpInfo.Username);
                }
            }

            var builder = new BodyBuilder
            {
                TextBody = Mail.ConvertToText(mailInfo.Body),
            };

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

            smtpInfo.Server = smtpInfo.Server.Trim();

            if (!SmtpServerRegex.IsMatch(smtpInfo.Server))
            {
                return Localize.GetString("SMTPConfigurationProblem");
            }

            try
            {
                var smtpHostParts = smtpInfo.Server.Split(':');
                var host = smtpHostParts[0];
                var port = 25;

                if (smtpHostParts.Length > 1)
                {
                    // port is guaranteed to be of max 5 digits numeric by the RegEx check
                    port = int.Parse(smtpHostParts[1]);
                    if (port < 1 || port > 65535)
                    {
                        return Localize.GetString("SmtpInvalidPort");
                    }
                }

                // to workaround problem in 4.0 need to specify host name
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(host, port, smtpInfo.EnableSSL);

                    switch (smtpInfo.Authentication)
                    {
                        case "":
                        case "0": // anonymous
                            break;
                        case "1": // basic
                            if (!string.IsNullOrEmpty(smtpInfo.Username)
                                && !string.IsNullOrEmpty(smtpInfo.Password))
                            {
                                smtpClient.Authenticate(smtpInfo.Username, smtpInfo.Password);
                            }

                            break;
                        case "2": // NTLM (Not Supported by MailKit)
                            throw new NotSupportedException("NTLM authentication is not supported by MailKit provider");
                    }

                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }

                return string.Empty;
            }
            catch (Exception exc)
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
        }
    }
}
