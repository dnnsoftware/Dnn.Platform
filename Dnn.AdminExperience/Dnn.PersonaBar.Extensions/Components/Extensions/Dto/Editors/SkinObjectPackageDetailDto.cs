// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
