// ReSharper disable InconsistentNaming

using System;

namespace Dnn.ExportImport.Dto.Workflow
{
    public class ExportWorkflowStatePermission : BasicExportImportDto
    {
        public int WorkflowStatePermissionID { get; set; }
        public int StateID { get; set; }
        public int PermissionID { get; set; }
        public string PermissionCode { get; set; }
        public string PermissionKey { get; set; }
        public string PermissionName { get; set; }
        public bool AllowAccess { get; set; }
        public int? RoleID { get; set; }
        public string RoleName { get; set; }
        public int? UserID { get; set; }
        public string Username { get; set; }
        //public int? ModuleDefID { get; set; }

        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserID { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }

        public string CreatedByUserName { get; set; }
        public string LastModifiedByUserName { get; set; }
    }
}