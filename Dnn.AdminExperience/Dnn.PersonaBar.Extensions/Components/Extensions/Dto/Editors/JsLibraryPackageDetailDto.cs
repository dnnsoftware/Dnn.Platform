// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    using System.Collections.Generic;

    using DotNetNuke.Services.Installer.Packages;
    using Newtonsoft.Json;

    [JsonObject]
    public class JsLibraryPackageDetailDto : PackageInfoDto
    {
        public JsLibraryPackageDetailDto()
        {
        }

        public JsLibraryPackageDetailDto(int portalId, PackageInfo package) : base(portalId, package)
        {
        }

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
    }
}
