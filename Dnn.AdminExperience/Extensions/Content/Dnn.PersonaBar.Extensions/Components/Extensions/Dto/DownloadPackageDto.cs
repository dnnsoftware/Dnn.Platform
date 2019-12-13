using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class DownloadPackageDto
    {
        public string PackageType { get; set; }
        public string FileName { get; set; }
    }
}
