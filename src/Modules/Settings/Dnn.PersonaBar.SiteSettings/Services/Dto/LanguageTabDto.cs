using Newtonsoft.Json;

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    [JsonObject]
    public class LanguageTabDto
    {
        public int PageId { get; set; }
        public string PageName { get; set; }
        public string ViewUrl { get; set; }
    }
}
