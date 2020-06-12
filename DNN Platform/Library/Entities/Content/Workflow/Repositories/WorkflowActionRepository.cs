// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Linq;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Workflow.Actions;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    internal class WorkflowActionRepository : ServiceLocator<IWorkflowActionRepository, WorkflowActionRepository>, IWorkflowActionRepository
    {
        protected override Func<IWorkflowActionRepository> GetFactory()
        {
            return () => new WorkflowActionRepository();
        }

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
    }
}
