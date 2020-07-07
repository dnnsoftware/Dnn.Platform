// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    using DotNetNuke.Services.Installer.Packages;
    using Newtonsoft.Json;

    [JsonObject]
    public class SkinObjectPackageDetailDto : PackageInfoDto
    {
        public SkinObjectPackageDetailDto()
        {
        }

        public SkinObjectPackageDetailDto(int portalId, PackageInfo package) : base(portalId, package)
        {
        }

        [JsonProperty("controlKey")]
        public string ControlKey { get; set; }

        [JsonProperty("controlSrc")]
        public string ControlSrc { get; set; }

        [JsonProperty("supportsPartialRendering")]
        public bool SupportsPartialRendering { get; set; }
    }
}
