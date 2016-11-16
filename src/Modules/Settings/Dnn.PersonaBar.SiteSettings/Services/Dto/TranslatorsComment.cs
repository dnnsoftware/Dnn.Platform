using Newtonsoft.Json;

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    [JsonObject]
    public class TranslatorsComment
    {
        public int TabId { get; set; }
        public string Text { get; set; }
    }
}