using System;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Users
{
    [Serializable]
    public class UserProfile : BasicExportImportDto
    {
        [JsonProperty(PropertyName = "ProfileID")]
        public int ProfileId { get; set; }

        [JsonProperty(PropertyName = "UserID")]

        public int UserI { get; set; }

        [JsonProperty(PropertyName = "PropertyDefinitionID")]
        public int PropertyDefinitionId { get; set; }

        public string PropertyValue { get; set; }
        public string PropertyText { get; set; }
        public int Visibility { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string ExtendedVisibility { get; set; }
    }
}