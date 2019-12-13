﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Workflow.Entities;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    internal class WorkflowLogRepository : ServiceLocator<IWorkflowLogRepository, WorkflowLogRepository> , IWorkflowLogRepository
    {
        #region Public Methods
        public IEnumerable<WorkflowLog> GetWorkflowLogs(int contentItemId, int workflowId)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowLog>();
                var workflowLogs = rep.Find("WHERE (ContentItemID = @0 AND WorkflowID = @1)", contentItemId, workflowId).ToArray();
                
                return workflowLogs;
            }
        }

        public void DeleteWorkflowLogs(int contentItemId, int workflowId)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowLog>();
                rep.Delete("WHERE (ContentItemID = @0 AND WorkflowID = @1)", contentItemId, workflowId);
            }
        }

        public void AddWorkflowLog(WorkflowLog workflowLog)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowLog>();

                rep.Insert(workflowLog);
            }
        }
        #endregion

        #region Service Locator
        protected override Func<IWorkflowLogRepository> GetFactory()
        {
            return () => new WorkflowLogRepository();
        }
        #endregion
    }    
}
