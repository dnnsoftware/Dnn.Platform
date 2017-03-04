// ReSharper disable InconsistentNaming

using System;

namespace Dnn.ExportImport.Components.Dto.Roles
{
    public class ExportRoleGroup : BasicExportImportDto
    {
        public int RoleGroupID { get; set; }
        public int PortalID { get; set; }
        public string RoleGroupName { get; set; }
        public string Description { get; set; }
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserID { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }

        public string CreatedByUserName { get; set; }
        public string LastModifiedByUserName { get; set; }
    }
}