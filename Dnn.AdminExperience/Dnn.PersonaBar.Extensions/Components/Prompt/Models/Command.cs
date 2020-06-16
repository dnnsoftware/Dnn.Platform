// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    [JsonObject]
    public class Command
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }

        [JsonIgnore]
        public Type CommandType { get; set; }
    }
}
