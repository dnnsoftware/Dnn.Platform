// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    public class WorkflowLogger : ServiceLocator<IWorkflowLogger, WorkflowLogger>, IWorkflowLogger
    {
        private readonly IWorkflowLogRepository _workflowLogRepository;

        public WorkflowLogger()
        {
            this._workflowLogRepository = WorkflowLogRepository.Instance;
        }

        public IEnumerable<WorkflowLog> GetWorkflowLogs(int contentItemId, int workflowId)
        {
            return this._workflowLogRepository.GetWorkflowLogs(contentItemId, workflowId);
        }

        public void AddWorkflowLog(int contentItemId, int workflowId, WorkflowLogType type, string comment, int userId)
        {
            this.AddWorkflowLog(contentItemId, workflowId, type, this.GetWorkflowActionText(type), comment, userId);
        }

        public void AddWorkflowLog(int contentItemId, int workflowId, string action, string comment, int userId)
        {
            this.AddWorkflowLog(contentItemId, workflowId, WorkflowLogType.CommentProvided, action, comment, userId);
        }

        protected override Func<IWorkflowLogger> GetFactory()
        {
            return () => new WorkflowLogger();
        }

        private void AddWorkflowLog(int contentItemId, int workflowId, WorkflowLogType type, string action, string comment, int userId)
        {
            var workflowLog = new WorkflowLog
            {
                ContentItemID = contentItemId,
                WorkflowID = workflowId,
                Type = (int)type,
                Action = action,
                Comment = comment,
                User = userId,
                Date = DateTime.UtcNow,
            };
            this._workflowLogRepository.AddWorkflowLog(workflowLog);
        }

        private string GetWorkflowActionText(WorkflowLogType logType)
        {
            var logName = Enum.GetName(typeof(WorkflowLogType), logType);
            return Localization.GetString(logName + ".Action");
        }
    }
}
