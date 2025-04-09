// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    using System;
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow.Actions;
    using DotNetNuke.Entities.Content.Workflow.Actions.TabActions;
    using DotNetNuke.Framework;

    internal class WorkflowActionRepository : ServiceLocator<IWorkflowActionRepository, WorkflowActionRepository>, IWorkflowActionRepository
    {
        /// <inheritdoc/>
        public WorkflowAction GetWorkflowAction(int contentTypeId, string type)
        {
            using var context = DataContext.Instance();
            var rep = context.GetRepository<WorkflowAction>();
            return rep.Find("WHERE ContentTypeId = @0 AND ActionType = @1", contentTypeId, type).SingleOrDefault()
                ?? this.GetWorkflowActionsDefaultsOrNull(contentTypeId, type); // fallback to default action (not in db)
        }

        /// <inheritdoc/>
        public void AddWorkflowAction(WorkflowAction action)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowAction>();
                rep.Insert(action);
            }
        }

        /// <inheritdoc/>
        protected override Func<IWorkflowActionRepository> GetFactory()
        {
            return () => new WorkflowActionRepository();
        }

        private WorkflowAction GetWorkflowActionsDefaultsOrNull(int contentTypeId, string type)
        {
            // only "Tab" ContentType is supported
            if (contentTypeId != ContentType.Tab.ContentTypeId)
            {
                return null;
            }

            // only known ActionTypes are supported
            if (!Enum.TryParse<WorkflowActionTypes>(type, out var workflowActionType))
            {
                return null;
            }

            var actionSource = workflowActionType switch
            {
                WorkflowActionTypes.DiscardWorkflow => typeof(DiscardWorkflow).AssemblyQualifiedName,
                WorkflowActionTypes.CompleteWorkflow => typeof(CompleteWorkflow).AssemblyQualifiedName,
                WorkflowActionTypes.DiscardState => typeof(DiscardState).AssemblyQualifiedName,
                WorkflowActionTypes.CompleteState => typeof(CompleteState).AssemblyQualifiedName,
                WorkflowActionTypes.StartWorkflow => typeof(StartWorkflow).AssemblyQualifiedName,
                _ => throw new ArgumentOutOfRangeException(),
            };

            return new WorkflowAction()
            {
                ActionId = -1, // not in DB
                ContentTypeId = contentTypeId,
                ActionType = type,
                ActionSource = actionSource,
            };
        }
    }
}
