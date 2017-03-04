using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Users
{
    [JsonObject]
    [Serializable]
    [TableName("UserRoles")]
    [PrimaryKey("UserRoleID")]
    public class UserRoles : BasicExportImportDto
    {
        [JsonProperty(PropertyName = "UserRoleID")]
        [ColumnName("UserRoleID")]
        public int UserRoleId { get; set; }

        [JsonProperty(PropertyName = "UserID")]
        [ColumnName("UserID")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "RoleID")]
        [ColumnName("RoleID")]
        public int RoleId { get; set; }

        [IgnoreColumn]
        [JsonIgnore]
        public string RoleName { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public bool IsTrialUsed { get; set; }
        public DateTime? EffectiveDate { get; set; }

        [JsonProperty(PropertyName = "CreatedByUserID")]
        [ColumnName("CreatedByUserID")]
        public int CreatedByUserId { get; set; }

        [IgnoreColumn]
        [JsonIgnore]
        public string CreatedByUserName { get; set; } //This could be used to find "CreatedByUserId"
        public DateTime CreatedOnDate { get; set; }

        [JsonProperty(PropertyName = "LastModifiedByUserID")]
        [ColumnName("LastModifiedByUserID")]
        public int LastModifiedByUserId { get; set; }

        [IgnoreColumn]
        [JsonIgnore]
        public string LastModifiedByUserName { get; set; } //This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }
        public int Status { get; set; }
        public bool IsOwner { get; set; }
    }
}