// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Dto.Pages
{
    public class ExportTabUrl : BasicExportImportDto
    {
        public int TabID { get; set; }
        public int SeqNum { get; set; }
        public string Url { get; set; }
        public string QueryString { get; set; }
        public string HttpStatus { get; set; }
        public string CultureCode { get; set; }
        public bool IsSystem { get; set; }
        public int? PortalAliasId { get; set; }
        public int? PortalAliasUsage { get; set; } // PortalAliasUsageType
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserID { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }

        public string HTTPAlias { get; set; }
        public string CreatedByUserName { get; set; }
        public string LastModifiedByUserName { get; set; }
    }
}
