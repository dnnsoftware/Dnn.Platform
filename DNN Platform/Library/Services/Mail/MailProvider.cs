// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using DotNetNuke.ComponentModel;

namespace DotNetNuke.Services.Mail
{
    public abstract class MailProvider
    {
        public abstract string SendMail(MailInfo mailInfo, SmtpInfo smtpInfo = null);

        // return the provider
        public static MailProvider Instance()
        {
            return ComponentFactory.GetComponent<MailProvider>();
        }
    }
}
