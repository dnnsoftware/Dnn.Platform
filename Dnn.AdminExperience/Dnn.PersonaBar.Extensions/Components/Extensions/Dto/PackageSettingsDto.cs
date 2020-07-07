// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

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
