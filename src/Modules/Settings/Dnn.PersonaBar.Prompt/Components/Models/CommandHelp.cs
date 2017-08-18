using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    [Serializable]
    [JsonObject]
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