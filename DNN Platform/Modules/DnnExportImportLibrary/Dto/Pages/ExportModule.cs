// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable InconsistentNaming
namespace Dnn.ExportImport.Dto.Pages
{
    using System;

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
