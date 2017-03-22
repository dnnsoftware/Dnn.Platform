using System;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Components.Dto.Pages
{
    [JsonObject]
    public class ExportTabPermission : BasicExportImportDto
    {
        public int TabPermissionId { get; set; }
        public int TabId { get; set; }
        public int PermissionID { get; set; }
        public string PermissionCode { get; set; }
        public string PermissionKey { get; set; }
        public bool AllowAccess { get; set; }
        public int? RoleId { get; set; }
        public int? UserID { get; set; }
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserID { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }
        public string CreatedByUserName { get; set; }
        public string LastModifiedByUserName { get; set; }
    }
}