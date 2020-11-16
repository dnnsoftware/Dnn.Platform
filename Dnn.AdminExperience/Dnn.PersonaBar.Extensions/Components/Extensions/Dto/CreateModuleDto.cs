// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    using Newtonsoft.Json;

    [JsonObject]
    public class CreateModuleDto
    {
        [JsonProperty("type")]
        public CreateModuleType Type { get; set; }

        [JsonProperty("ownerFolder")]
        public string OwnerFolder { get; set; }

        [JsonProperty("moduleFolder")]
        public string ModuleFolder { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("moduleName")]
        public string ModuleName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("manifest")]
        public string Manifest { get; set; }

        [JsonProperty("addPage")]
        public bool AddPage { get; set; }
    }
}
