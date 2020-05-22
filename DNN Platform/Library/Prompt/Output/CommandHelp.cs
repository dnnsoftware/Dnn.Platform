// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Abstractions.Prompt;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DotNetNuke.Prompt.Output
{
    [Serializable]
    [JsonObject]
    public class CommandHelp : ICommandHelp
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "options")]
        public IEnumerable<ICommandOption> Options { get; set; }

        [JsonProperty(PropertyName = "resultHtml")]
        public string ResultHtml { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}
