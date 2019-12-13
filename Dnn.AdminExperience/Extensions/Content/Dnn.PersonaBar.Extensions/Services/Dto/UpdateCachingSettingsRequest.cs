namespace Dnn.PersonaBar.Servers.Services.Dto
{
    public class UpdateCachingSettingsRequest
    {
        public string CachingProvider { get; set; }

        public bool UseSSL { get; set; }
    }
}
