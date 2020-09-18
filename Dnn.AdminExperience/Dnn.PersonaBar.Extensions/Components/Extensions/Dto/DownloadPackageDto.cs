// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    using Newtonsoft.Json;

    [JsonObject]
    public class DownloadPackageDto
    {
        public string PackageType { get; set; }
        public string FileName { get; set; }
    }
}
