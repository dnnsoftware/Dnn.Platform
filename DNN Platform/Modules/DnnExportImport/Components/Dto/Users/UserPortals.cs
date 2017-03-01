using System;
using LiteDB;

namespace Dnn.ExportImport.Components.Dto.Users
{
    public class UserPortals : BasicExportObject
    {
        public int UserId { get; set; }

        [BsonIndex]
        public override int PortalId { get; set; }

        public int UserPortalId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Authorised { get; set; }
        public bool IsDeleted { get; set; }
        public bool RefreshRoles { get; set; }
        public string VanityUrl { get; set; }
    }
}