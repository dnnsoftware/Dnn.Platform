// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
