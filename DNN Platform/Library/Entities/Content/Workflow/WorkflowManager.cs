// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Workflow.Entities;
using DotNetNuke.Entities.Content.Workflow.Exceptions;
using DotNetNuke.Entities.Content.Workflow.Repositories;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.Workflow
{
    public class WorkflowManager : ServiceLocator<IWorkflowManager, WorkflowManager>, IWorkflowManager
    {
        #region Members
        private readonly DataProvider _dataProvider;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowStateRepository _workflowStateRepository;
        private readonly ISystemWorkflowManager _systemWorkflowManager;
        #endregion

        #region Constructor
        public WorkflowManager()
        {
            _dataProvider = DataProvider.Instance();
            _workflowRepository = WorkflowRepository.Instance;
            _workflowStateRepository = WorkflowStateRepository.Instance;
            _systemWorkflowManager = SystemWorkflowManager.Instance;
        }
        #endregion

        #region Public Methods

        public void DeleteWorkflow(Entities.Workflow workflow)
        {
            var workflowToDelete = _workflowRepository.GetWorkflow(workflow.WorkflowID);
            if (workflowToDelete == null)
            {
                return;
            }

            if (workflowToDelete.IsSystem)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("SystemWorkflowDeletionException", Localization.ExceptionsResourceFile));
            }

            var usageCount = GetWorkflowUsageCount(workflowToDelete.WorkflowID);
            if (usageCount > 0)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowInUsageException", Localization.ExceptionsResourceFile));
            }

            _workflowRepository.DeleteWorkflow(workflowToDelete);
        }

        public Entities.Workflow GetWorkflow(int workflowId)
        {
            return _workflowRepository.GetWorkflow(workflowId);
        }

        public Entities.Workflow GetWorkflow(ContentItem contentItem)
        {
            if (contentItem.StateID == Null.NullInteger)
            {
                return null;
            }
            var state = WorkflowStateRepository.Instance.GetWorkflowStateByID(contentItem.StateID);
            return state == null ? null : GetWorkflow(state.WorkflowID);
        }

        public IEnumerable<Entities.Workflow> GetWorkflows(int portalId)
        {
            return _workflowRepository.GetWorkflows(portalId);
        }

        public void AddWorkflow(Entities.Workflow workflow)
        {
            _workflowRepository.AddWorkflow(workflow);

            var firstDefaultState = _systemWorkflowManager.GetDraftStateDefinition(1);
            var lastDefaultState = _systemWorkflowManager.GetPublishedStateDefinition(2);

            firstDefaultState.WorkflowID = workflow.WorkflowID;
            lastDefaultState.WorkflowID = workflow.WorkflowID;

            _workflowStateRepository.AddWorkflowState(firstDefaultState);
            _workflowStateRepository.AddWorkflowState(lastDefaultState);

            workflow.States = new List<WorkflowState>
                              {
                                  firstDefaultState,
                                  lastDefaultState
                              };
        }

        public void UpdateWorkflow(Entities.Workflow workflow)
        {
            _workflowRepository.UpdateWorkflow(workflow);
        }

        public IEnumerable<WorkflowUsageItem> GetWorkflowUsage(int workflowId, int pageIndex, int pageSize)
        {
            return CBO.FillCollection<WorkflowUsageItem>(_dataProvider.GetContentWorkflowUsage(workflowId, pageIndex, pageSize));
        }

        public int GetWorkflowUsageCount(int workflowId)
        {
            return _dataProvider.GetContentWorkflowUsageCount(workflowId);
        }
        #endregion

        #region Service Locator
        protected override System.Func<IWorkflowManager> GetFactory()
        {
            return () => new WorkflowManager();
        }
        #endregion
    }
}
