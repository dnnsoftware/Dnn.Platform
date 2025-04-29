// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail;

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;

using Localize = DotNetNuke.Services.Localization.Localization;

/// <summary>A <see cref="MailProvider"/> implementation using the original core DNN logic (via <see cref="SmtpClient"/>).</summary>
public class CoreMailProvider : MailProvider
{
    private static readonly Regex SmtpServerRegex = new Regex("^[^:]+(:[0-9]{1,5})?$", RegexOptions.Compiled);

    /// <inheritdoc />
    public override string SendMail(MailInfo mailInfo, SmtpInfo smtpInfo = null)
    {
        var (host, port, errorMessage) = ParseSmtpServer(ref smtpInfo);
        if (errorMessage != null)
        {
            return errorMessage;
        }

        using (var mailMessage = CreateMailMessage(mailInfo, smtpInfo))
        {
            try
            {
                using (var smtpClient = CreateSmtpClient(smtpInfo, host, port))
                {
                    smtpClient.Send(mailMessage);
                }

                return string.Empty;
            }
            catch (Exception exc)
            {
                return HandleException(exc);
            }
        }
    }

    /// <inheritdoc />
    public override async Task<string> SendMailAsync(MailInfo mailInfo, SmtpInfo smtpInfo = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var (host, port, errorMessage) = ParseSmtpServer(ref smtpInfo);
        if (errorMessage != null)
        {
            return errorMessage;
        }

        using (var mailMessage = CreateMailMessage(mailInfo, smtpInfo))
        {
            try
            {
                using (var smtpClient = CreateSmtpClient(smtpInfo, host, port))
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }

                return string.Empty;
            }
            catch (Exception exc)
            {
                return HandleException(exc);
            }
        }
    }

    /// <summary>Adds alternate views to the <paramref name="mailMessage"/>.</summary>
    /// <param name="mailMessage">The message to which the alternate views should be added.</param>
    /// <param name="body">The message body.</param>
    /// <param name="bodyEncoding">The encoding of the message body.</param>
    internal static void AddAlternateView(MailMessage mailMessage, string body, Encoding bodyEncoding)
    {
        // added support for multipart html messages
        // add text part as alternate view
        var plainView = AlternateView.CreateAlternateViewFromString(Mail.ConvertToText(body), bodyEncoding, "text/plain");
        mailMessage.AlternateViews.Add(plainView);
        if (mailMessage.IsBodyHtml)
        {
            var htmlView = AlternateView.CreateAlternateViewFromString(body, bodyEncoding, "text/html");
            mailMessage.AlternateViews.Add(htmlView);
        }
    }

    private static string ValidateSmtpInfo(SmtpInfo smtpInfo)
    {
        if (smtpInfo != null && !string.IsNullOrEmpty(smtpInfo.Server))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(Host.SMTPServer))
        {
            return null;
        }

        return "SMTP Server not configured";
    }

    private static SmtpInfo GetDefaultSmtpInfo(SmtpInfo smtpInfo)
    {
        if (smtpInfo != null && !string.IsNullOrEmpty(smtpInfo.Server))
        {
            return smtpInfo;
        }

        return new SmtpInfo
        {
            Server = Host.SMTPServer,
            Authentication = Host.SMTPAuthentication,
            Username = Host.SMTPUsername,
            Password = Host.SMTPPassword,
            EnableSSL = Host.EnableSMTPSSL,
            AuthProvider = Host.SMTPAuthProvider,
        };
    }

    private static MailMessage CreateMailMessage(MailInfo mailInfo, SmtpInfo smtpInfo)
    {
        // translate semi-colon delimiters to commas as ASP.NET 2.0 does not support semi-colons
        if (!string.IsNullOrEmpty(mailInfo.To))
        {
            mailInfo.To = mailInfo.To.Replace(";", ",");
        }

        var mailMessage = new MailMessage(mailInfo.From, mailInfo.To);

        if (!string.IsNullOrEmpty(mailInfo.Sender))
        {
            mailMessage.Sender = new MailAddress(mailInfo.Sender);
        }

        if (!string.IsNullOrEmpty(mailInfo.CC))
        {
            mailInfo.CC = mailInfo.CC.Replace(";", ",");
            mailMessage.CC.Add(mailInfo.CC);
        }

        if (!string.IsNullOrEmpty(mailInfo.BCC))
        {
            mailInfo.BCC = mailInfo.BCC.Replace(";", ",");
            mailMessage.Bcc.Add(mailInfo.BCC);
        }

        if (!string.IsNullOrEmpty(mailInfo.ReplyTo))
        {
            mailInfo.ReplyTo = mailInfo.ReplyTo.Replace(";", ",");
            mailMessage.ReplyToList.Add(mailInfo.ReplyTo);
        }

        mailMessage.Priority = (System.Net.Mail.MailPriority)mailInfo.Priority;
        mailMessage.IsBodyHtml = mailInfo.BodyFormat == MailFormat.Html;

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
                if (smtpInfo.Username.Contains("@")
                    && senderAddress == Host.HostEmail
                    && !senderAddress.Equals(smtpInfo.Username, StringComparison.InvariantCultureIgnoreCase))
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
                    mailMessage.Sender = new MailAddress(senderAddress, senderDisplayName);
                }
            }
            else if (smtpInfo.Username.Contains("@"))
            {
                mailMessage.Sender = new MailAddress(
                    smtpInfo.Username,
                    Host.SMTPPortalEnabled ? PortalSettings.Current.PortalName : Host.HostTitle);
            }
        }

        // attachments
        if (mailInfo.Attachments != null)
        {
            foreach (var attachment in mailInfo.Attachments.Where(attachment => attachment.Content != null))
            {
                mailMessage.Attachments.Add(
                    new Attachment(
                        new MemoryStream(attachment.Content, writable: false),
                        attachment.Filename,
                        attachment.ContentType));
            }
        }

        // message
        mailMessage.SubjectEncoding = mailInfo.BodyEncoding;
        mailMessage.Subject = HtmlUtils.StripWhiteSpace(mailInfo.Subject, true);
        mailMessage.BodyEncoding = mailInfo.BodyEncoding;
        mailMessage.Body = mailInfo.Body;

        AddAlternateView(mailMessage, mailInfo.Body, mailInfo.BodyEncoding);

        return mailMessage;
    }

    private static (string Host, int? Port, string ErrorMessage) ParseSmtpServer(ref SmtpInfo smtpInfo)
    {
        var errorMessage = ValidateSmtpInfo(smtpInfo);
        if (errorMessage != null)
        {
            return (null, null, errorMessage);
        }

        smtpInfo = GetDefaultSmtpInfo(smtpInfo);

        smtpInfo.Server = smtpInfo.Server.Trim();
        if (!SmtpServerRegex.IsMatch(smtpInfo.Server))
        {
            return (null, null, Localize.GetString("SMTPConfigurationProblem"));
        }

        var smtpHostParts = smtpInfo.Server.Split(':');
        var host = smtpHostParts[0];
        if (smtpHostParts.Length <= 1)
        {
            return (host, null, null);
        }

        // port is guaranteed to be of max 5 digits numeric by the RegEx check
        var port = int.Parse(smtpHostParts[1]);
        if (port < 1 || port > 65535)
        {
            return (null, null, Localize.GetString("SmtpInvalidPort"));
        }

        return (host, port, null);
    }

    private static SmtpClient CreateSmtpClient(SmtpInfo smtpInfo, string host, int? port)
    {
        SmtpClient client = null;
        try
        {
            client = new SmtpClient();
            client.Host = host;
            if (port != null)
            {
                client.Port = port.Value;
            }

            SetSmtpClientAuthentication(smtpInfo, client);

            client.EnableSsl = smtpInfo.EnableSSL;

            var returnedClient = client;
            client = null;

            return returnedClient;
        }
        finally
        {
            client?.Dispose();
        }
    }

    private static void SetSmtpClientAuthentication(SmtpInfo smtpInfo, SmtpClient smtpClient)
    {
        switch (smtpInfo.Authentication)
        {
            case "":
            case "0": // anonymous
                break;
            case "1": // basic
                if (!string.IsNullOrEmpty(smtpInfo.Username) && !string.IsNullOrEmpty(smtpInfo.Password))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(smtpInfo.Username, smtpInfo.Password);
                }

                break;
            case "2": // NTLM
                smtpClient.UseDefaultCredentials = true;
                break;
        }
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
}
