// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Services.Dto
{
    public class SendTestEmailRequest
    {
        public string SmtpServerMode { get; set; }

        public string SmtpServer { get; set; }

        public int SmtpAuthentication { get; set; }

        public string SmtpUsername { get; set; }

        public string SmtpPassword { get; set; }

        public bool EnableSmtpSsl { get; set; }
    }
}
