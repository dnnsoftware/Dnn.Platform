// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
