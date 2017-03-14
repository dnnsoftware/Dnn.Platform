using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Assets
{
    [JsonObject]
    [Serializable]
    [TableName("Folders")]
    [PrimaryKey("FolderID")]
    public class ExportFolder : BasicExportImportDto
    {
        [ColumnName("FolderID")]
        [JsonProperty(PropertyName = "FolderID")]
        public int FolderId { get; set; }

        [ColumnName("PortalID")]
        [JsonProperty(PropertyName = "PortalID")]
        public int PortalId { get; set; }

        public string FolderPath { get; set; }
        public int StorageLocation { get; set; }
        public bool IsProtected { get; set; }
        public bool IsCached { get; set; }
        public DateTime? LastUpdated { get; set; }

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
        public Guid UniqueId { get; set; }
        public Guid VersionGuid { get; set; }

        [ColumnName("FolderMappingID")]
        [JsonProperty(PropertyName = "FolderMappingID")]
        public int FolderMappingId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string FolderMappingName { get; set; }

        [ColumnName("ParentID")]
        [JsonProperty(PropertyName = "ParentID")]
        public int? ParentId { get; set; }

        public bool IsVersioned { get; set; }

        [ColumnName("WorkflowID")]
        [JsonProperty(PropertyName = "WorkflowID")]
        public int? WorkflowId { get; set; }

        public string MappedPath { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public int? UserId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string Username { get; set; }
    }
}