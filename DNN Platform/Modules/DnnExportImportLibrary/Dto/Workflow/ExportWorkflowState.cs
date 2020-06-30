// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Dto.Workflow
{
    public class ExportWorkflowState : BasicExportImportDto
    {
        public int StateID { get; set; }

        public int WorkflowID { get; set; }

        public string StateName { get; set; }

        public int Order { get; set; }

        public bool IsActive { get; set; }

        public bool SendEmail { get; set; }

        public bool SendMessage { get; set; }

        public bool IsDisposalState { get; set; }

        public string OnCompleteMessageSubject { get; set; }

        public string OnCompleteMessageBody { get; set; }

        public string OnDiscardMessageSubject { get; set; }

        public string OnDiscardMessageBody { get; set; }

        public bool IsSystem { get; set; }

        public bool SendNotification { get; set; }

        public bool SendNotificationToAdministrators { get; set; }
    }
}
