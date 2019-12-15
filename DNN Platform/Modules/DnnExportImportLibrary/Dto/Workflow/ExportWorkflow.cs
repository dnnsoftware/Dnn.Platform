// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Dto.Workflow
{
    public class ExportWorkflow : BasicExportImportDto
    {
        public int WorkflowID { get; set; }
        public string WorkflowName { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public bool StartAfterCreating { get; set; }
        public bool StartAfterEditing { get; set; }
        public bool DispositionEnabled { get; set; }
        public bool IsSystem { get; set; }
        public string WorkflowKey { get; set; }
        public bool IsDefault { get; set; }
    }
}
