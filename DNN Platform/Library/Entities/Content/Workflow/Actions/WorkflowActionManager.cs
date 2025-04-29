// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions;

using System;

using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Workflow.Repositories;
using DotNetNuke.Framework;

public class WorkflowActionManager : ServiceLocator<IWorkflowActionManager, WorkflowActionManager>, IWorkflowActionManager
{
    private readonly IWorkflowActionRepository workflowActionRepository;

    /// <summary>Initializes a new instance of the <see cref="WorkflowActionManager"/> class.</summary>
    public WorkflowActionManager()
    {
        this.workflowActionRepository = WorkflowActionRepository.Instance;
    }

    /// <inheritdoc/>
    public IWorkflowAction GetWorkflowActionInstance(int contentTypeId, WorkflowActionTypes actionType)
    {
        var action = this.workflowActionRepository.GetWorkflowAction(contentTypeId, actionType.ToString());
        if (action == null)
        {
            return null;
        }

        return Reflection.CreateInstance(Reflection.CreateType(action.ActionSource)) as IWorkflowAction;
    }

    /// <inheritdoc/>
    public void RegisterWorkflowAction(WorkflowAction workflowAction)
    {
        Requires.NotNull("workflowAction", workflowAction);

        var action = Reflection.CreateInstance(Reflection.CreateType(workflowAction.ActionSource)) as IWorkflowAction;
        if (action == null)
        {
            throw new ArgumentException("The specified ActionSource does not implement the IWorkflowAction interface");
        }

        this.workflowActionRepository.AddWorkflowAction(workflowAction);
    }

    /// <inheritdoc/>
    protected override Func<IWorkflowActionManager> GetFactory()
    {
        return () => new WorkflowActionManager();
    }
}
