using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class SkinPackageDetailDto : PackageInfoDto
    {
        [JsonProperty("themePackageName")]
        public string ThemePackageName { get; set; }

        public SkinPackageDetailDto()
        {

        }

        public SkinPackageDetailDto(int portalId, PackageInfo package) : base(portalId, package)
        {

        }
    }
}