// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
﻿using System;
using System.Collections.Generic;
using DotNetNuke.Entities.Content.Workflow.Entities;
using DotNetNuke.Entities.Content.Workflow.Repositories;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.Workflow
{
    public class WorkflowLogger : ServiceLocator<IWorkflowLogger, WorkflowLogger>, IWorkflowLogger
    {
        #region Members
        private readonly IWorkflowLogRepository _workflowLogRepository;
        #endregion

        #region Constructor
        public WorkflowLogger()
        {
            _workflowLogRepository = WorkflowLogRepository.Instance;
        }
        #endregion

        #region Private Methods
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
                Date = DateTime.UtcNow
            };
            _workflowLogRepository.AddWorkflowLog(workflowLog);
        }

        private string GetWorkflowActionText(WorkflowLogType logType)
        {
            var logName = Enum.GetName(typeof(WorkflowLogType), logType);
            return Localization.GetString(logName + ".Action");
        }
        #endregion

        #region Public Methods
        public IEnumerable<WorkflowLog> GetWorkflowLogs(int contentItemId, int workflowId)
        {
            return _workflowLogRepository.GetWorkflowLogs(contentItemId, workflowId);
        }


        public void AddWorkflowLog(int contentItemId, int workflowId, WorkflowLogType type, string comment, int userId)
        {
            AddWorkflowLog(contentItemId, workflowId, type, GetWorkflowActionText(type), comment, userId);
        }

        public void AddWorkflowLog(int contentItemId, int workflowId, string action, string comment, int userId)
        {
            AddWorkflowLog(contentItemId, workflowId, WorkflowLogType.CommentProvided, action, comment, userId);
        }
        #endregion

        #region Service Locator
        protected override Func<IWorkflowLogger> GetFactory()
        {
            return () => new WorkflowLogger();
        }
        #endregion
    }
}
