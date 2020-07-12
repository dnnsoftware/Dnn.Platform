// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Dto.Assets
{
    using System;

    public class ExportFolderPermission : BasicExportImportDto
    {
        public int FolderPermissionId { get; set; }

        public int FolderId { get; set; }

        public string FolderPath { get; set; }

        public int PermissionId { get; set; }

        public bool AllowAccess { get; set; }

        public int? RoleId { get; set; }

        public string RoleName { get; set; }

        public int? UserId { get; set; }

        public string Username { get; set; }

        public string DisplayName { get; set; }

        public string PermissionCode { get; set; }

        public int ModuleDefId { get; set; }

        public string PermissionKey { get; set; }

        public string PermissionName { get; set; }

        public int? CreatedByUserId { get; set; }

        public string CreatedByUserName { get; set; } // This could be used to find "CreatedByUserId"

        public DateTime? CreatedOnDate { get; set; }

        public int? LastModifiedByUserId { get; set; }

        public string LastModifiedByUserName { get; set; } // This could be used to find "LastModifiedByUserId"

        public DateTime? LastModifiedOnDate { get; set; }
    }
}
