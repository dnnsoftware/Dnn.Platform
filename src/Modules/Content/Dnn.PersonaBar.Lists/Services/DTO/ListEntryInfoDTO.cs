using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Lists.Services.DTO
{
    public class ListEntryInfoDto
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "listName")]
        public string ListName { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "localizedText")]
        public string LocalizedText { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "hasChildren")]
        public bool HasChildren { get; set; }

        [JsonProperty(PropertyName = "parentId")]
        public int ParentId { get; set; }

        [JsonProperty(PropertyName = "parentKey")]
        public string ParentKey { get; set; }

        [JsonProperty(PropertyName = "parent")]
        public string Parent { get; set; }

        [JsonProperty(PropertyName = "level")]
        public int Level { get; set; }

        [JsonProperty(PropertyName = "sort")]
        public int Sort { get; set; }

        [JsonProperty(PropertyName = "definitionId")]
        public int DefinitionId { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "portalId")]
        public int PortalId { get; set; }

        [JsonProperty(PropertyName = "isSystem")]
        public bool IsSystem { get; set; }
    }
}