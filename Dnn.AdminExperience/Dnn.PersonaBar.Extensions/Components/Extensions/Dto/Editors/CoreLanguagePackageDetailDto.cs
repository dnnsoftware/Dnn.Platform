// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    using System.Collections.Generic;

    using DotNetNuke.Services.Installer.Packages;
    using Newtonsoft.Json;

    [JsonObject]
    public class CoreLanguagePackageDetailDto : PackageInfoDto
    {
        public CoreLanguagePackageDetailDto()
        {
        }

        public CoreLanguagePackageDetailDto(int portalId, PackageInfo package) : base(portalId, package)
        {
        }

        [JsonProperty("locales")]
        public IEnumerable<ListItemDto> Locales { get; set; }

        [JsonProperty("languageId")]
        public int LanguageId { get; set; }

        [JsonProperty("editUrlFormat")]
        public string EditUrlFormat { get; set; }

        [JsonProperty("packages")]
        public IEnumerable<ListItemDto> Packages { get; set; }
    }
}
