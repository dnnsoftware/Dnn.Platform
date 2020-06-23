﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    using Newtonsoft.Json;

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
