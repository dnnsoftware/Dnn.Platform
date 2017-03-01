using System;
using System.Collections;
using System.Collections.Generic;
using LiteDB;

namespace Dnn.ExportImport.Components.Dto.Users
{
    public class Users : BasicExportObject
    {
        [BsonIgnore]
        public int Total { get; set; }
        [BsonIgnore]
        public int rowid { get; set; }
        [BsonIgnore]
        public int rowiddesc { get; set; }

        [BsonField(Name = "UserID")]
        public int UserId { get; set; }

        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsSuperUser { get; set; }
        public int AffiliateId { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool UpdatePassword { get; set; }

        [BsonField(Name = "LastIPAddress")]
        public string LastIpAddress { get; set; }

        public bool IsDeleted { get; set; }

        [BsonField(Name = "CreatedByUserID")]
        public int CreatedByUserId { get; set; } //How do we insert this value?
        public string CreatedByUserName { get; set; }//This could be used to find "CreatedByUserId"
        public DateTime? CreatedOnDate { get; set; }

        [BsonField(Name = "LastModifiedByUserID")]
        public int LastModifiedByUserId { get; set; } //How do we insert this value?
        public string LastModifiedByUserName { get; set; }//This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }
        public Guid PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiration { get; set; }
        public string LowerEmail { get; set; }

        [BsonRef("aspnetusers")] // where "aspnetusers" are AspnetUsers collection name
        public AspnetUsers AspnetUsers { get; set; }

        [BsonRef("aspnetmembership")] // where "aspnetmembership" are AspnetMembership collection name
        public AspnetMembership AspnetMembership { get; set; }

        public IEnumerable<UserRoles> UserRoles { get; set; }

        [BsonRef("userportals")] // where "userportals" are UserPortals collection name
        public UserPortals UserPortals { get; set; }
    }
}
