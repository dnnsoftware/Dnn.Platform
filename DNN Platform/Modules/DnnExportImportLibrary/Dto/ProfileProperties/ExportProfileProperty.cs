// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Dto.ProfileProperties
{
    using System;

    public class ExportProfileProperty : BasicExportImportDto
    {
        public int PropertyDefinitionId { get; set; }

        public int? ModuleDefId { get; set; }

        public bool Deleted { get; set; }

        public int? DataType { get; set; }

        public string DefaultValue { get; set; }

        public string PropertyCategory { get; set; }

        public string PropertyName { get; set; }

        public int Length { get; set; }

        public bool Required { get; set; }

        public string ValidationExpression { get; set; }

        public int ViewOrder { get; set; }

        public bool Visible { get; set; }

        public int? CreatedByUserId { get; set; }

        public string CreatedByUserName { get; set; } // This could be used to find "CreatedByUserId"

        public DateTime? CreatedOnDate { get; set; }

        public int? LastModifiedByUserId { get; set; }

        public string LastModifiedByUserName { get; set; } // This could be used to find "LastModifiedByUserId"

        public DateTime? LastModifiedOnDate { get; set; }

        public int DefaultVisibility { get; set; }

        public bool ReadOnly { get; set; }
    }
}
