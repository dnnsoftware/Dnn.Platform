// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace Dnn.PersonaBar.Servers.Services.Dto
{
    public class UpdateSmtpSettingsRequest
    {
        public string SmtpServerMode { get; set; }

        public string SmtpServer { get; set; }

        public string SmtpConnectionLimit { get; set; }

        public string SmtpMaxIdleTime { get; set; }

        public int SmtpAuthentication { get; set; }

        public string SmtpUsername { get; set; }

        public string SmtpPassword { get; set; }

        public string SmtpHostEmail { get; set; }

        public bool EnableSmtpSsl { get; set; }

        public int MessageSchedulerBatchSize { get; set; }
    }
}
