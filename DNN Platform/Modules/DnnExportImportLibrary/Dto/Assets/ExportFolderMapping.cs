// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Dto.Assets
{
    using System;

    public class ExportFolderMapping : BasicExportImportDto
    {
        public int FolderMappingId { get; set; }

        public string MappingName { get; set; }

        public string FolderProviderType { get; set; }

        public int? Priority { get; set; }

        public int? CreatedByUserId { get; set; }

        public string CreatedByUserName { get; set; } // This could be used to find "CreatedByUserId"

        public DateTime? CreatedOnDate { get; set; }

        public int? LastModifiedByUserId { get; set; }

        public string LastModifiedByUserName { get; set; } // This could be used to find "LastModifiedByUserId"

        public DateTime? LastModifiedOnDate { get; set; }
    }
}
