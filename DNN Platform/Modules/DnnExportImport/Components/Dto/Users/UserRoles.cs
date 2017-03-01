using System;
using LiteDB;

namespace Dnn.ExportImport.Components.Dto.Users
{
    public class UserRoles : BasicExportObject
    {
        public int ReferenceUserId { get; set; }

        [BsonField(Name = "UserRoleID")]
        public int UserRoleId { get; set; }

        [BsonField(Name = "UserID")]
        public int UserId { get; set; }

        [BsonField(Name = "RoleID")]
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public DateTime ExpiryDate { get; set; }
        public bool IsTrialUsed { get; set; }
        public DateTime EffectiveDate { get; set; }

        [BsonField(Name = "CreatedByUserID")]
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }//This could be used to find "CreatedByUserId"
        public DateTime CreatedOnDate { get; set; }

        [BsonField(Name = "LastModifiedByUserID")]
        public int LastModifiedByUserId { get; set; }
        public string LastModifiedByUserName { get; set; }//This could be used to find "LastModifiedByUserId"
        public DateTime LastModifiedOnDate { get; set; }
        public int Status { get; set; }
        public bool IsOwner { get; set; }
    }
}
