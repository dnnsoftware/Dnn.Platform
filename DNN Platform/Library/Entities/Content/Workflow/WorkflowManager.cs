// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow;

using System.Collections.Generic;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Workflow.Entities;
using DotNetNuke.Entities.Content.Workflow.Exceptions;
using DotNetNuke.Entities.Content.Workflow.Repositories;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

public class WorkflowManager : ServiceLocator<IWorkflowManager, WorkflowManager>, IWorkflowManager
{
    private readonly DataProvider dataProvider;
    private readonly IWorkflowRepository workflowRepository;
    private readonly IWorkflowStateRepository workflowStateRepository;
    private readonly ISystemWorkflowManager systemWorkflowManager;

    /// <summary>Initializes a new instance of the <see cref="WorkflowManager"/> class.</summary>
    public WorkflowManager()
    {
        this.dataProvider = DataProvider.Instance();
        this.workflowRepository = WorkflowRepository.Instance;
        this.workflowStateRepository = WorkflowStateRepository.Instance;
        this.systemWorkflowManager = SystemWorkflowManager.Instance;
    }

    /// <inheritdoc/>
    public void DeleteWorkflow(Entities.Workflow workflow)
    {
        var workflowToDelete = this.workflowRepository.GetWorkflow(workflow.WorkflowID);
        if (workflowToDelete == null)
        {
            return;
        }

        if (workflowToDelete.IsSystem)
        {
            throw new WorkflowInvalidOperationException(Localization.GetString("SystemWorkflowDeletionException", Localization.ExceptionsResourceFile));
        }

        var usageCount = this.GetWorkflowUsageCount(workflowToDelete.WorkflowID);
        if (usageCount > 0)
        {
            throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowInUsageException", Localization.ExceptionsResourceFile));
        }

        this.workflowRepository.DeleteWorkflow(workflowToDelete);
    }

    /// <inheritdoc/>
    public Entities.Workflow GetWorkflow(int workflowId)
    {
        return this.workflowRepository.GetWorkflow(workflowId);
    }

    /// <inheritdoc/>
    public Entities.Workflow GetWorkflow(ContentItem contentItem)
    {
        if (contentItem.StateID == Null.NullInteger)
        {
            return null;
        }

        var state = WorkflowStateRepository.Instance.GetWorkflowStateByID(contentItem.StateID);
        return state == null ? null : this.GetWorkflow(state.WorkflowID);
    }

    /// <inheritdoc/>
    public IEnumerable<Entities.Workflow> GetWorkflows(int portalId)
    {
        return this.workflowRepository.GetWorkflows(portalId);
    }

    /// <inheritdoc/>
    public void AddWorkflow(Entities.Workflow workflow)
    {
        this.workflowRepository.AddWorkflow(workflow);

        var firstDefaultState = this.systemWorkflowManager.GetDraftStateDefinition(1);
        var lastDefaultState = this.systemWorkflowManager.GetPublishedStateDefinition(2);

        firstDefaultState.WorkflowID = workflow.WorkflowID;
        lastDefaultState.WorkflowID = workflow.WorkflowID;

        this.workflowStateRepository.AddWorkflowState(firstDefaultState);
        this.workflowStateRepository.AddWorkflowState(lastDefaultState);

        workflow.States = new List<WorkflowState>
        {
            firstDefaultState,
            lastDefaultState,
        };
    }

    /// <inheritdoc/>
    public void UpdateWorkflow(Entities.Workflow workflow)
    {
        this.workflowRepository.UpdateWorkflow(workflow);
    }

    /// <inheritdoc/>
    public IEnumerable<WorkflowUsageItem> GetWorkflowUsage(int workflowId, int pageIndex, int pageSize)
    {
        return CBO.FillCollection<WorkflowUsageItem>(this.dataProvider.GetContentWorkflowUsage(workflowId, pageIndex, pageSize));
    }

    /// <inheritdoc/>
    public int GetWorkflowUsageCount(int workflowId)
    {
        return this.dataProvider.GetContentWorkflowUsageCount(workflowId);
    }

    /// <inheritdoc/>
    protected override System.Func<IWorkflowManager> GetFactory()
    {
        return () => new WorkflowManager();
    }
}
