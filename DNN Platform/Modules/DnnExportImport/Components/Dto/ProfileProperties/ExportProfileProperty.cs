using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.ProfileProperties
{
    [JsonObject]
    [Serializable]
    [TableName("ProfilePropertyDefinition")]
    [PrimaryKey("PropertyDefinitionID")]
    public class ExportProfileProperty : BasicExportImportDto
    {
        [ColumnName("PropertyDefinitionID")]
        [JsonProperty(PropertyName = "PropertyDefinitionID")]
        public int PropertyDefinitionId { get; set; }

        [ColumnName("PortalID")]
        [JsonProperty(PropertyName = "PortalID")]
        public int? PortalId { get; set; }

        [ColumnName("ModuleDefID")]
        [JsonProperty(PropertyName = "ModuleDefID")]
        public int ModuleDefId { get; set; }

        public bool Deleted { get; set; }
        public int? DataType { get; set; }
        public string DefaultValue { get; set; }
        public string PropertyCategory { get; set; }
        public string PropertyName { get; set; }
        public int Length { get; set; }
        public bool Required { get; set; }
        public string ValidationExpression { get; set; }
        public int ViewOrder { get; set; }
        public bool Visible { get; set; }

        [ColumnName("CreatedByUserID")]
        [JsonProperty(PropertyName = "CreatedByUserID")]
        public int? CreatedByUserId { get; set; }

        [IgnoreColumn]
        public string CreatedByUserName { get; set; } //This could be used to find "CreatedByUserId"
        public DateTime? CreatedOnDate { get; set; }

        [ColumnName("LastModifiedByUserID")]
        [JsonProperty(PropertyName = "LastModifiedByUserID")]
        public int? LastModifiedByUserId { get; set; }

        [IgnoreColumn]
        public string LastModifiedByUserName { get; set; } //This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }
        public int DefaultVisibility { get; set; }
        public bool ReadOnly { get; set; }
    }
}