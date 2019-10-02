using Newtonsoft.Json;

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    [JsonObject]
    public class LocalizationEntry
    {
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public string NewValue { get; set; }
    }
}