﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
