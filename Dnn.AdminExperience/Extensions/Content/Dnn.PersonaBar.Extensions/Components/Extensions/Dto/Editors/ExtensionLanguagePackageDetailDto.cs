using System.Collections.Generic;
using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class ExtensionLanguagePackageDetailDto : CoreLanguagePackageDetailDto
    {
        [JsonProperty("dependentPackageId")]
        public int DependentPackageId { get; set; }

        public ExtensionLanguagePackageDetailDto()
        {

        }

        public ExtensionLanguagePackageDetailDto(int portalId, PackageInfo package) : base(portalId, package)
        {

        }
    }
}