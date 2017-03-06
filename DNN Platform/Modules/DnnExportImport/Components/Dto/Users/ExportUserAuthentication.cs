using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Users
{
    [JsonObject]
    [Serializable]
    [TableName("UserAuthentication")]
    [PrimaryKey("UserAuthenticationID")]
    public class ExportUserAuthentication:BasicExportImportDto
    {
        [ColumnName("UserAuthenticationID")]
        [JsonProperty(PropertyName = "UserAuthenticationID")]
        public int UserAuthenticationId { get; set; }

        [ColumnName("UserID")]
        [JsonProperty(PropertyName = "UserID")]
        public int UserId { get; set; }

        public string AuthenticationType { get; set; }
        public string AuthenticationToken { get; set; }

        [ColumnName("CreatedByUserID")]
        [JsonProperty(PropertyName = "CreatedByUserID")]
        public int CreatedByUserId { get; set; }

        [IgnoreColumn]
        public string CreatedByUserName { get; set; } //This could be used to find "CreatedByUserId"

        public DateTime CreatedOnDate { get; set; }

        [ColumnName("LastModifiedByUserID")]
        [JsonProperty(PropertyName = "LastModifiedByUserID")]
        public int LastModifiedByUserId { get; set; }

        [IgnoreColumn]
        public string LastModifiedByUserName { get; set; } //This could be used to find "LastModifiedByUserId"
        public DateTime LastModifiedOnDate { get; set; }
    }
}