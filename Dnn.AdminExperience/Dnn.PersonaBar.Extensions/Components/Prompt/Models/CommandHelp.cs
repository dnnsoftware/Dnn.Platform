// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    [Serializable]
    [JsonObject]
    [Obsolete("Moved to DotNetNuke.Prompt in the core library project. Will be removed in DNN 11.", false)]
    public class CommandHelp
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "options")]
        public IEnumerable<CommandOption> Options { get; set; }

        [JsonProperty(PropertyName = "resultHtml")]
        public string ResultHtml { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}
