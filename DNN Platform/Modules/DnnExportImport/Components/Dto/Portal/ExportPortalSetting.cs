using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Portal
{
    [JsonObject]
    [Serializable]
    [TableName("PortalSettings")]
    [PrimaryKey("PortalSettingID")]

    public class ExportPortalSetting : BasicExportImportDto
    {
        [ColumnName("PortalSettingID")]
        [JsonProperty(PropertyName = "PortalSettingID")]
        public int PortalSettingId { get; set; }

        [ColumnName("PortalID")]
        [JsonProperty(PropertyName = "PortalID")]
        public int PortalId { get; set; }

        public string SettingName { get; set; }
        public string SettingValue { get; set; }

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
        public string CultureCode { get; set; }
    }
}