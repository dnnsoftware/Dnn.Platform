using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class SkinPackageDetailDto : PackageInfoDto
    {
        public SkinPackageDetailDto()
        {

        }

        public SkinPackageDetailDto(int portalId, PackageInfo package) : base(portalId, package)
        {

        }
    }
}