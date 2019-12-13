// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Linq;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Workflow.Actions;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    internal class WorkflowActionRepository : ServiceLocator<IWorkflowActionRepository, WorkflowActionRepository>, IWorkflowActionRepository
    {
        #region Service Locator
        protected override Func<IWorkflowActionRepository> GetFactory()
        {
            return () => new WorkflowActionRepository();
        }
        #endregion

        #region Public Methods
        public WorkflowAction GetWorkflowAction(int contentTypeId, string type)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowAction>();
                return rep.Find("WHERE ContentTypeId = @0 AND ActionType = @1", contentTypeId, type).SingleOrDefault();
            }
        }

        public void AddWorkflowAction(WorkflowAction action)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowAction>();
                rep.Insert(action);
            }
        }
        #endregion
    }
}
