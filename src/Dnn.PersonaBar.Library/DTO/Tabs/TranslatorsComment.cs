using Newtonsoft.Json;

namespace Dnn.PersonaBar.Library.DTO.Tabs
{
    [JsonObject]
    public class TranslatorsComment
    {
        public int TabId { get; set; }
        public string Text { get; set; }
    }
}