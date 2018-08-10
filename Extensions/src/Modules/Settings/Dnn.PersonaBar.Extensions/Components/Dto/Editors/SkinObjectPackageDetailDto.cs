using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class SkinObjectPackageDetailDto : PackageInfoDto
    {
        [JsonProperty("controlKey")]
        public string ControlKey { get; set; }

        [JsonProperty("controlSrc")]
        public string ControlSrc { get; set; }

        [JsonProperty("supportsPartialRendering")]
        public bool SupportsPartialRendering { get; set; }

        public SkinObjectPackageDetailDto()
        {

        }

        public SkinObjectPackageDetailDto(int portalId, PackageInfo package) : base(portalId, package)
        {

        }
    }
}