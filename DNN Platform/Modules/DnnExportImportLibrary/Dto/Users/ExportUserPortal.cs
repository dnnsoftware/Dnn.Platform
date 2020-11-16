// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Dto.Users
{
    using System;

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
