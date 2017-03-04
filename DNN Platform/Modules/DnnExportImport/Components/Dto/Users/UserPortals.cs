using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Users
{
    [JsonObject]
    [Serializable]
    [TableName("UserPortals")]
    //[PrimaryKey("UserId,PortalId", AutoIncrement = false)]
    [PrimaryKey("UserPortalId")]
    public class UserPortals : BasicExportImportDto
    {
        public int UserId { get; set; }

        public int PortalId { get; set; }

        public int UserPortalId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Authorised { get; set; }
        public bool IsDeleted { get; set; }
        public bool RefreshRoles { get; set; }
        public string VanityUrl { get; set; }
    }
}