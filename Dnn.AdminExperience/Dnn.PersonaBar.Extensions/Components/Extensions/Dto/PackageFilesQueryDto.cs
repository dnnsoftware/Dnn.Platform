// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class PackageFilesQueryDto : PackageInfoDto
    {
        [JsonProperty("packageFolder")]
        public string PackageFolder { get; set; }

        [JsonProperty("includeSource")]
        public bool IncludeSource { get; set; }

        [JsonProperty("includeAppCode")]
        public bool IncludeAppCode { get; set; }
    }
}
