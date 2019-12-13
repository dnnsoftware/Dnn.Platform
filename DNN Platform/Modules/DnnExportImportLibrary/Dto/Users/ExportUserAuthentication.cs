// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace Dnn.ExportImport.Dto.Users
{
    public class ExportUserAuthentication:BasicExportImportDto
    {
        public int UserAuthenticationId { get; set; }

        public int UserId { get; set; }

        public string AuthenticationType { get; set; }
        public string AuthenticationToken { get; set; }
        public int CreatedByUserId { get; set; }

        public string CreatedByUserName { get; set; } //This could be used to find "CreatedByUserId"

        public DateTime CreatedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }

        public string LastModifiedByUserName { get; set; } //This could be used to find "LastModifiedByUserId"
        public DateTime LastModifiedOnDate { get; set; }
    }
}
