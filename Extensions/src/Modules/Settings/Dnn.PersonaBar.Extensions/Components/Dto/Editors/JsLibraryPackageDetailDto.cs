using System.Collections.Generic;
using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class JsLibraryPackageDetailDto : PackageInfoDto
    {
        [JsonProperty("objectName")]
        public string ObjectName { get; set; }

        [JsonProperty("defaultCdn")]
        public string DefaultCdn { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("customCdn")]
        public string CustomCdn { get; set; }

        [JsonProperty("dependencies")]
        public IEnumerable<UsedByPackage> Dependencies { get; set; }

        [JsonProperty("usedBy")]
        public IEnumerable<UsedByPackage> UsedBy { get; set; }

        public JsLibraryPackageDetailDto()
        {

        }

        public JsLibraryPackageDetailDto(int portalId, PackageInfo package) : base(portalId, package)
        {

        }
    }
}