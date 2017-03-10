using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Portal
{
    [JsonObject]
    [Serializable]
    [TableName("PortalLanguages")]
    [PrimaryKey("PortalLanguageID")]

    public class ExportPortalLanguage : BasicExportImportDto
    {
        [ColumnName("PortalLanguageID")]
        [JsonProperty(PropertyName = "PortalLanguageID")]
        public int PortalLanguageId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string CultureCode { get; set; }

        [ColumnName("PortalID")]
        [JsonProperty(PropertyName = "PortalID")]
        public int PortalId { get; set; }

        [ColumnName("LanguageID")]
        [JsonProperty(PropertyName = "LanguageID")]
        public int LanguageId { get; set; }

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
        public bool IsPublished { get; set; }
    }
}