﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace Dnn.ExportImport.Dto.Users
{
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

        public int CreatedByUserId { get; set; } //How do we insert this value?
        public string CreatedByUserName { get; set; }//This could be used to find "CreatedByUserId"
        public DateTime? CreatedOnDate { get; set; }

        public int LastModifiedByUserId { get; set; } //How do we insert this value?
        public string LastModifiedByUserName { get; set; }//This could be used to find "LastModifiedByUserId"
        public DateTime? LastModifiedOnDate { get; set; }
        public Guid? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiration { get; set; }
    }
}
