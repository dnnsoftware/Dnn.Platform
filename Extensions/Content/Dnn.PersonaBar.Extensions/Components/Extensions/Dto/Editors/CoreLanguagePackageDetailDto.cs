using System.Collections.Generic;
using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class CoreLanguagePackageDetailDto : PackageInfoDto
    {
        [JsonProperty("locales")]
        public IEnumerable<ListItemDto> Locales { get; set; }

        [JsonProperty("languageId")]
        public int LanguageId { get; set; }

        [JsonProperty("editUrlFormat")]
        public string EditUrlFormat { get; set; }

        [JsonProperty("packages")]
        public IEnumerable<ListItemDto> Packages { get; set; }

        public CoreLanguagePackageDetailDto()
        {

        }

        public CoreLanguagePackageDetailDto(int portalId, PackageInfo package) : base(portalId, package)
        {

        }
    }
}