// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Framework;

    public class WorkflowActionManager : ServiceLocator<IWorkflowActionManager, WorkflowActionManager>, IWorkflowActionManager
    {
        private readonly IWorkflowActionRepository _workflowActionRepository;

        public WorkflowActionManager()
        {
            this._workflowActionRepository = WorkflowActionRepository.Instance;
        }

        public IWorkflowAction GetWorkflowActionInstance(int contentTypeId, WorkflowActionTypes actionType)
        {
            var action = this._workflowActionRepository.GetWorkflowAction(contentTypeId, actionType.ToString());
            if (action == null)
            {
                return null;
            }

            return Reflection.CreateInstance(Reflection.CreateType(action.ActionSource)) as IWorkflowAction;
        }

        public void RegisterWorkflowAction(WorkflowAction workflowAction)
        {
            Requires.NotNull("workflowAction", workflowAction);

            var action = Reflection.CreateInstance(Reflection.CreateType(workflowAction.ActionSource)) as IWorkflowAction;
            if (action == null)
            {
                throw new ArgumentException("The specified ActionSource does not implement the IWorkflowAction interface");
            }

            this._workflowActionRepository.AddWorkflowAction(workflowAction);
        }

        protected override Func<IWorkflowActionManager> GetFactory()
        {
            return () => new WorkflowActionManager();
        }
    }
}
