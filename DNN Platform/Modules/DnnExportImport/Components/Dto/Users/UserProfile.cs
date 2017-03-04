using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Users
{
    [JsonObject]
    [Serializable]
    [TableName("UserProfile")]
    [PrimaryKey("ProfileID")]
    public class UserProfile : BasicExportImportDto
    {
        [JsonProperty(PropertyName = "ProfileID")]
        [ColumnName("ProfileID")]
        public int ProfileId { get; set; }

        [JsonProperty(PropertyName = "UserID")]
        [ColumnName("UserID")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "PropertyDefinitionID")]
        [ColumnName("PropertyDefinitionID")]
        public int PropertyDefinitionId { get; set; }

        public string PropertyValue { get; set; }
        public string PropertyText { get; set; }
        public int Visibility { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string ExtendedVisibility { get; set; }
    }
}