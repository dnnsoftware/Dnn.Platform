using System.Collections.Generic;
using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class LanguagePackageDetailDto : PackageInfoDto
    {
        [JsonProperty("locales")]
        public IEnumerable<ListItemDto> Locales { get; set; }

        [JsonProperty("languageId")]
        public int LanguageId { get; set; }

        [JsonProperty("languagePackageId")]
        public int LanguagePackageId { get; set; }

        [JsonProperty("editUrlFormat")]
        public string EditUrlFormat { get; set; }

        [JsonProperty("packages")]
        public IEnumerable<ListItemDto> Packages { get; set; }

        public LanguagePackageDetailDto()
        {

        }

        public LanguagePackageDetailDto(int portalId, PackageInfo package) : base(portalId, package)
        {

        }
    }
}