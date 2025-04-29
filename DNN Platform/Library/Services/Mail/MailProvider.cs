// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail;

using System.Threading;
using System.Threading.Tasks;

using DotNetNuke.ComponentModel;

/// <summary>A provider with the ability to send emails.</summary>
public abstract class MailProvider
{
    /// <summary>Gets a value indicating whether this provider supports OAuth authentication to the SMTP server.</summary>
    public virtual bool SupportsOAuth => false;

    /// <summary>Gets the currently configured <see cref="MailProvider"/> instance.</summary>
    /// <returns>A <see cref="MailProvider"/> instance.</returns>
    public static MailProvider Instance()
    {
        return ComponentFactory.GetComponent<MailProvider>();
    }

    /// <summary>Sends an email.</summary>
    /// <param name="mailInfo">Information about the message to send.</param>
    /// <param name="smtpInfo">Information about the SMTP server via which to send the message.</param>
    /// <returns><see cref="string.Empty"/> if the message send successfully, otherwise an error message.</returns>
    public abstract string SendMail(MailInfo mailInfo, SmtpInfo smtpInfo = null);

    /// <summary>Sends an email.</summary>
    /// <param name="mailInfo">Information about the message to send.</param>
    /// <param name="smtpInfo">Information about the SMTP server via which to send the message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="string.Empty"/> if the message send successfully, otherwise an error message.</returns>
    public virtual Task<string> SendMailAsync(MailInfo mailInfo, SmtpInfo smtpInfo = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        return Task.FromResult(this.SendMail(mailInfo, smtpInfo));
    }
}
