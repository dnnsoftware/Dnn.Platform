using System;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Users
{
    public class Users : BasicExportImportDto
    {
        [JsonIgnore]
        public int Total { get; set; }
        [JsonIgnore]
        public int RowId { get; set; }
        [JsonIgnore]
        public int RowIdDesc { get; set; }

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

        [JsonProperty(PropertyName = "LastIPAddress")]
        public string LastIpAddress { get; set; }

        public bool IsDeleted { get; set; }

        [JsonProperty(PropertyName= "CreatedByUserID")]
        public int CreatedByUserId { get; set; } //How do we insert this value?
        public string CreatedByUserName { get; set; }//This could be used to find "CreatedByUserId"
        public DateTime? CreatedOnDate { get; set; }

        [JsonProperty(PropertyName= "LastModifiedByUserID")]
        public int LastModifiedByUserId { get; set; } //How do we insert this value?
        public string LastModifiedByUserName { get; set; }//This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }
        public Guid PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiration { get; set; }
        public string LowerEmail { get; set; }
    }
}
