// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
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
        private readonly DataProvider _dataProvider;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowStateRepository _workflowStateRepository;
        private readonly ISystemWorkflowManager _systemWorkflowManager;

        public WorkflowManager()
        {
            this._dataProvider = DataProvider.Instance();
            this._workflowRepository = WorkflowRepository.Instance;
            this._workflowStateRepository = WorkflowStateRepository.Instance;
            this._systemWorkflowManager = SystemWorkflowManager.Instance;
        }

        public void DeleteWorkflow(Entities.Workflow workflow)
        {
            var workflowToDelete = this._workflowRepository.GetWorkflow(workflow.WorkflowID);
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

            this._workflowRepository.DeleteWorkflow(workflowToDelete);
        }

        public Entities.Workflow GetWorkflow(int workflowId)
        {
            return this._workflowRepository.GetWorkflow(workflowId);
        }

        public Entities.Workflow GetWorkflow(ContentItem contentItem)
        {
            if (contentItem.StateID == Null.NullInteger)
            {
                return null;
            }

            var state = WorkflowStateRepository.Instance.GetWorkflowStateByID(contentItem.StateID);
            return state == null ? null : this.GetWorkflow(state.WorkflowID);
        }

        public IEnumerable<Entities.Workflow> GetWorkflows(int portalId)
        {
            return this._workflowRepository.GetWorkflows(portalId);
        }

        public void AddWorkflow(Entities.Workflow workflow)
        {
            this._workflowRepository.AddWorkflow(workflow);

            var firstDefaultState = this._systemWorkflowManager.GetDraftStateDefinition(1);
            var lastDefaultState = this._systemWorkflowManager.GetPublishedStateDefinition(2);

            firstDefaultState.WorkflowID = workflow.WorkflowID;
            lastDefaultState.WorkflowID = workflow.WorkflowID;

            this._workflowStateRepository.AddWorkflowState(firstDefaultState);
            this._workflowStateRepository.AddWorkflowState(lastDefaultState);

            workflow.States = new List<WorkflowState>
                              {
                                  firstDefaultState,
                                  lastDefaultState,
                              };
        }

        public void UpdateWorkflow(Entities.Workflow workflow)
        {
            this._workflowRepository.UpdateWorkflow(workflow);
        }

        public IEnumerable<WorkflowUsageItem> GetWorkflowUsage(int workflowId, int pageIndex, int pageSize)
        {
            return CBO.FillCollection<WorkflowUsageItem>(this._dataProvider.GetContentWorkflowUsage(workflowId, pageIndex, pageSize));
        }

        public int GetWorkflowUsageCount(int workflowId)
        {
            return this._dataProvider.GetContentWorkflowUsageCount(workflowId);
        }

        protected override System.Func<IWorkflowManager> GetFactory()
        {
            return () => new WorkflowManager();
        }
    }
}
