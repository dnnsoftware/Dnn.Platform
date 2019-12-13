using System;

namespace Dnn.ExportImport.Dto.Users
{
    public class ExportUserPortal : BasicExportImportDto
    {
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Authorised { get; set; }
        public bool IsDeleted { get; set; }
        public bool RefreshRoles { get; set; }
        public string VanityUrl { get; set; }
    }
}
