using Newtonsoft.Json;

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    [JsonObject]
    public class NameValueEntry
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}