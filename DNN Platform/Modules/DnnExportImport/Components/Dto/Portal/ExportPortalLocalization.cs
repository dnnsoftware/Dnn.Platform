using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Portal
{
    [JsonObject]
    [Serializable]
    [TableName("PortalLocalization")]
    [PrimaryKey("PortalID,CultureCode", AutoIncrement = false)]

    public class ExportPortalLocalization : BasicExportImportDto
    {
        [ColumnName("PortalID")]
        [JsonProperty(PropertyName = "PortalID")]
        public int PortalId { get; set; }

        public string CultureCode { get; set; }
        public string PortalName { get; set; }
        public string LogoFile { get; set; }
        public string FooterText { get; set; }
        public string Description { get; set; }
        public string KeyWords { get; set; }
        public string BackgroundFile { get; set; }
        public int? HomeTabId { get; set; }
        public int? LoginTabId { get; set; }
        public int? UserTabId { get; set; }
        public int? AdminTabId { get; set; }
        public int? SplashTabId { get; set; }

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
        public int? RegisterTabId { get; set; }
        public int? SearchTabId { get; set; }
        public int? Custom404TabId { get; set; }
        public int? Custom500TabId { get; set; }
    }
}