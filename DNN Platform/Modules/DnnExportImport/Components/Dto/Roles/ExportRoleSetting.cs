// ReSharper disable InconsistentNaming

using System;

namespace Dnn.ExportImport.Components.Dto.Roles
{
    public class ExportRoleSetting : BasicExportImportDto
    {
        public int RoleSettingID { get; set; }
        public int PortalID { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserID { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }

        public string CreatedByUserName { get; set; }
        public string LastModifiedByUserName { get; set; }
    }
}