using DotNetNuke.Abstractions.Prompt;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DotNetNuke.Prompt
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
