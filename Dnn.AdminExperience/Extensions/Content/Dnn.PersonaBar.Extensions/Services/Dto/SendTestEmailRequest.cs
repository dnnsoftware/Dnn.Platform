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
