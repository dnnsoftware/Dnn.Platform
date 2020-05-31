// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
