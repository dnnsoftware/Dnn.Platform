using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Lists.Services.DTO
{
    public class DeleteListEntryDto
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
    }
}