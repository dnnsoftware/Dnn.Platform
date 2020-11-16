// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Dto.Users
{
    using System;

    public class ExportUser : BasicExportImportDto
    {
        public int RowId { get; set; }

        public int UserId { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsSuperUser { get; set; }

        public int? AffiliateId { get; set; }

        public string Email { get; set; }

        public string DisplayName { get; set; }

        public bool UpdatePassword { get; set; }

        public string LastIpAddress { get; set; }

        public bool IsDeletedPortal { get; set; }

        public int CreatedByUserId { get; set; } // How do we insert this value?

        public string CreatedByUserName { get; set; }// This could be used to find "CreatedByUserId"

        public DateTime? CreatedOnDate { get; set; }

        public int LastModifiedByUserId { get; set; } // How do we insert this value?

        public string LastModifiedByUserName { get; set; }// This could be used to find "LastModifiedByUserId"

        public DateTime? LastModifiedOnDate { get; set; }

        public Guid? PasswordResetToken { get; set; }

        public DateTime? PasswordResetExpiration { get; set; }
    }
}
