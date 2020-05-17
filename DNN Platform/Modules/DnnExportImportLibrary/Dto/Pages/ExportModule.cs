// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Dto.Pages
{
    public class ExportModule : BasicExportImportDto
    {
        public int ModuleID { get; set; }
        public int ModuleDefID { get; set; }
        public string FriendlyName { get; set; } // this is ModuleDefinition.FriendlyName
        public bool AllTabs { get; set; }
        public bool IsDeleted { get; set; }
        public bool? InheritViewPermissions { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? LastContentModifiedOnDate { get; set; }
        public int? ContentItemID { get; set; }
        public bool IsShareable { get; set; }
        public bool IsShareableViewOnly { get; set; }

        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserID { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }

        public string CreatedByUserName { get; set; }
        public string LastModifiedByUserName { get; set; }
    }
}
