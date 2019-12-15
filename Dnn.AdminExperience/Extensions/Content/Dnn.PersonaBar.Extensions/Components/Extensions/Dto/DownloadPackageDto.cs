// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class DownloadPackageDto
    {
        public string PackageType { get; set; }
        public string FileName { get; set; }
    }
}
