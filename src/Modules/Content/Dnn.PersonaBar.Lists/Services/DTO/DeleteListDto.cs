using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Lists.Services.DTO
{
    public class DeleteListDto
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "portalId")]
        public int PortalId { get; set; }

        [JsonProperty(PropertyName = "parentKey")]
        public string ParentKey { get; set; }
    }
}