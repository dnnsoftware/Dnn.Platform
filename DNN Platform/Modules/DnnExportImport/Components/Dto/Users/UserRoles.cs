using System;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Users
{
    public class UserRoles : BasicExportImportDto
    {
        [JsonProperty(PropertyName= "UserRoleID")]
        public int UserRoleId { get; set; }

        [JsonProperty(PropertyName= "UserID")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName= "RoleID")]
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public DateTime ExpiryDate { get; set; }
        public bool IsTrialUsed { get; set; }
        public DateTime EffectiveDate { get; set; }

        [JsonProperty(PropertyName= "CreatedByUserID")]
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }//This could be used to find "CreatedByUserId"
        public DateTime CreatedOnDate { get; set; }

        [JsonProperty(PropertyName= "LastModifiedByUserID")]
        public int LastModifiedByUserId { get; set; }
        public string LastModifiedByUserName { get; set; }//This could be used to find "LastModifiedByUserId"
        public DateTime LastModifiedOnDate { get; set; }
        public int Status { get; set; }
        public bool IsOwner { get; set; }
    }
}
