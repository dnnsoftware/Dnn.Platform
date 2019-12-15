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
    public class DeletePackageDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("deleteFiles")]
        public bool DeleteFiles { get; set; }
    }
}
