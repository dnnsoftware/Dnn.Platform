using System;

namespace Dnn.ExportImport.Dto.Users
{
    public class ExportUserRole : BasicExportImportDto
    {
        public int UserRoleId { get; set; }

        public int UserId { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public bool IsTrialUsed { get; set; }
        public DateTime? EffectiveDate { get; set; }

        public int CreatedByUserId { get; set; }

        public string CreatedByUserName { get; set; } //This could be used to find "CreatedByUserId"
        public DateTime CreatedOnDate { get; set; }

        public int LastModifiedByUserId { get; set; }

        public string LastModifiedByUserName { get; set; } //This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }
        public int Status { get; set; }
        public bool IsOwner { get; set; }
    }
}
