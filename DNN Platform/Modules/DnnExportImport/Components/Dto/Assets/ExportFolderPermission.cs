using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Assets
{
    [JsonObject]
    [Serializable]
    [TableName("FolderPermission")]
    [PrimaryKey("FolderPermissionID")]
    public class ExportFolderPermission:BasicExportImportDto
    {
        [ColumnName("FolderPermissionID")]
        [JsonProperty(PropertyName = "FolderPermissionID")]
        public int FolderPermissionId { get; set; }

        [ColumnName("FolderID")]
        [JsonProperty(PropertyName = "FolderID")]
        public int FolderId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string FolderPath { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        [ColumnName("PortalID")]
        [JsonProperty(PropertyName = "PortalID")]
        public string PortalId { get; set; }


        [ColumnName("PermissionID")]
        [JsonProperty(PropertyName = "PermissionID")]
        public int PermissionId { get; set; }

        public bool AllowAccess { get; set; }

        [ColumnName("RoleID")]
        [JsonProperty(PropertyName = "RoleID")]
        public int? RoleId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string RoleName { get; set; }

        [ColumnName("UserID")]
        [JsonProperty(PropertyName = "UserID")]
        public int? UserId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string Username { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string DisplayName { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string PermissionCode { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        [ColumnName("ModuleDefID")]
        [JsonProperty(PropertyName = "ModuleDefID")]
        public int ModuleDefId { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string PermissionKey { get; set; }

        [JsonIgnore]
        [IgnoreColumn]
        public string PermissionName { get; set; }

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

    }
}