using System;
using System.Runtime.CompilerServices;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Services.FileSystem;
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

        public static void MapFromFolderInfo(IFolderInfo folderInfo, ExportFolder folder)
        {
            folder.PortalId = folderInfo.PortalID;
            folder.FolderPath = folderInfo.FolderPath;
            folder.FolderId = folderInfo.FolderID;
            folder.ParentId = folderInfo.ParentID;
            folder.CreatedByUserId = folderInfo.CreatedByUserID;
            folder.CreatedOnDate = folderInfo.CreatedOnDate;
            folder.FolderMappingId = folderInfo.FolderMappingID;
            folder.IsCached = folderInfo.IsCached;
            folder.IsVersioned = folderInfo.IsVersioned;
            folder.IsProtected = folderInfo.IsProtected;
            folder.MappedPath = folderInfo.MappedPath;
            folder.StorageLocation = folderInfo.StorageLocation;
            folder.LastUpdated = folderInfo.LastUpdated;
            folder.UniqueId = folderInfo.UniqueId;
            folder.VersionGuid = folderInfo.VersionGuid;
            folder.WorkflowId = folderInfo.WorkflowID;
            folder.LastModifiedByUserId = folderInfo.LastModifiedByUserID;
            folder.LastModifiedOnDate = folderInfo.LastModifiedOnDate;
        }
    }
}