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
    public class ModuleFolderDto
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("isSpecial")]
        public bool IsSpecial { get; set; }

        [JsonProperty("specialType")]
        public string SpecialType { get; set; }
    }
}
