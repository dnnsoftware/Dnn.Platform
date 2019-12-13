// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class PackageSettingsDto
    {
        [JsonProperty("packageId")]
        public int PackageId { get; set; }

        [JsonProperty("portalId")]
        public int PortalId { get; set; }

        [JsonProperty("settings")]
        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        [JsonProperty("editorActions")]
        public IDictionary<string, string> EditorActions { get; set; } = new Dictionary<string, string>();
    }
}
