using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class PackageFilesQueryDto : PackageInfoDto
    {
        [JsonProperty("packageFolder")]
        public string PackageFolder { get; set; }

        [JsonProperty("includeSource")]
        public bool IncludeSource { get; set; }

        [JsonProperty("includeAppCode")]
        public bool IncludeAppCode { get; set; }
    }
}