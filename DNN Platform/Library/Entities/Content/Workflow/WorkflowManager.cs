#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
