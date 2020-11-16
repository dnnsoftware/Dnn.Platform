// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable InconsistentNaming
namespace Dnn.ExportImport.Dto.Pages
{
    using System;

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
