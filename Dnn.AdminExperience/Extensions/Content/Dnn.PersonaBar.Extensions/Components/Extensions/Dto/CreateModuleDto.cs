// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
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
