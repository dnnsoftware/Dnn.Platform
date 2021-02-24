// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail
{
    using DotNetNuke.ComponentModel;

    /// <summary>A provider with the ability to send emails.</summary>
    public abstract class MailProvider
    {
        /// <summary>Sends an email.</summary>
        /// <param name="mailInfo">Information about the message to send.</param>
        /// <param name="smtpInfo">Information about the SMTP server via which to send the message.</param>
        /// <returns><see cref="string.Empty"/> if the message send successfully, otherwise an error message.</returns>
        public abstract string SendMail(MailInfo mailInfo, SmtpInfo smtpInfo = null);

        /// <summary>Gets the currently configured <see cref="MailProvider"/> instance.</summary>
        /// <returns>A <see cref="MailProvider"/> instance.</returns>
        public static MailProvider Instance()
        {
            return ComponentFactory.GetComponent<MailProvider>();
        }
    }
}
