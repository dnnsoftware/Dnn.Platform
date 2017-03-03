using System;
using System.Web.UI.WebControls;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Users
{
    [JsonObject]
    [Serializable]
    [TableName("Users")]
    [PrimaryKey("UserID")]
    public class Users : BasicExportImportDto
    {
        [IgnoreColumn]
        [JsonIgnore]
        public int Total { get; set; }
        [JsonIgnore]
        [IgnoreColumn]
        public int RowId { get; set; }
        [JsonIgnore]
        [IgnoreColumn]
        public int RowIdDesc { get; set; }

        [ColumnName("UserID")]
        [JsonProperty(PropertyName= "UserID")]
        public int UserId { get; set; }

        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsSuperUser { get; set; }
        public int AffiliateId { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool UpdatePassword { get; set; }

        [ColumnName("LastIPAddress")]
        [JsonProperty(PropertyName = "LastIPAddress")]
        public string LastIpAddress { get; set; }

        public bool IsDeleted { get; set; }

        [ColumnName("CreatedByUserID")]
        [JsonProperty(PropertyName= "CreatedByUserID")]
        public int CreatedByUserId { get; set; } //How do we insert this value?
        [IgnoreColumn]
        public string CreatedByUserName { get; set; }//This could be used to find "CreatedByUserId"
        public DateTime? CreatedOnDate { get; set; }

        [ColumnName("LastModifiedByUserID")]
        [JsonProperty(PropertyName= "LastModifiedByUserID")]
        public int LastModifiedByUserId { get; set; } //How do we insert this value?
        [IgnoreColumn]
        public string LastModifiedByUserName { get; set; }//This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }
        public Guid PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiration { get; set; }
        [IgnoreColumn]
        public string LowerEmail { get; set; }
    }
}
