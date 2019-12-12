// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.ConfigConsole.Services.Dto
{
    [JsonObject]
    public class ConfigFileDto
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("fileContent")]
        public string FileContent { get; set; }
    }
}
