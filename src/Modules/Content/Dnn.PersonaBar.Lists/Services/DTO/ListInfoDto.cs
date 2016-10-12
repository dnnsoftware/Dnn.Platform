using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Lists.Services.DTO
{
    public class ListInfoDto
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "parentId")]
        public int ParentId { get; set; }

        [JsonProperty(PropertyName = "level")]
        public int Level { get; set; }

        [JsonProperty(PropertyName = "definitionId")]
        public int DefinitionId { get; set; }

        [JsonProperty(PropertyName = "portalId")]
        public int PortalId { get; set; }

        [JsonProperty(PropertyName = "isPopulated")]
        public bool IsPopulated { get; set; }

        [JsonProperty(PropertyName = "entryCount")]
        public int EntryCount { get; set; }

        [JsonProperty(PropertyName = "parentKey")]
        public string ParentKey { get; set; }

        [JsonProperty(PropertyName = "parent")]
        public string Parent { get; set; }

        [JsonProperty(PropertyName = "parentList")]
        public string ParentList { get; set; }

        [JsonProperty(PropertyName = "enableSort")]
        public bool EnableSort { get; set; }
    }
}