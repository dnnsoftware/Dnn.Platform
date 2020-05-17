﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Linq;
using DotNetNuke.Entities.Content.Workflow.Entities;
using DotNetNuke.Entities.Content.Workflow.Repositories;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.Workflow
{
    public class SystemWorkflowManager : ServiceLocator<ISystemWorkflowManager, SystemWorkflowManager>, ISystemWorkflowManager
    {
        #region Public Constants
        public const string DirectPublishWorkflowKey = "DirectPublish";
        public const string SaveDraftWorkflowKey = "SaveDraft";
        public const string ContentAprovalWorkflowKey = "ContentApproval";
        #endregion

        #region Members
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowStateRepository _workflowStateRepository;
        #endregion

        #region Constructor
        public SystemWorkflowManager()
        {
            _workflowRepository = WorkflowRepository.Instance;
            _workflowStateRepository = WorkflowStateRepository.Instance;
        }
        #endregion

        #region Private Methods
        private WorkflowState GetDefaultWorkflowState(int order)
        {
            return new WorkflowState
            {
                IsSystem = true,
                SendNotification = true,
                SendNotificationToAdministrators = false,
                Order = order
            };
        }

        private void CreateDirectPublishWorkflow(int portalId)
        {
            var workflow = new Entities.Workflow
            {
                WorkflowName = Localization.GetString("DefaultDirectPublishWorkflowName"),
                Description = Localization.GetString("DefaultDirectPublishWorkflowDescription"),
                WorkflowKey = DirectPublishWorkflowKey,
                IsSystem = true,
                PortalID = portalId
            };
            _workflowRepository.AddWorkflow(workflow);
            var publishedState = GetPublishedStateDefinition(1);
            publishedState.WorkflowID = workflow.WorkflowID;
            _workflowStateRepository.AddWorkflowState(publishedState);
        }

        private void CreateSaveDraftWorkflow(int portalId)
        {
            var workflow = new Entities.Workflow
            {
                WorkflowName = Localization.GetString("DefaultSaveDraftWorkflowName"),
                Description = Localization.GetString("DefaultSaveDraftWorkflowDescription"),
                WorkflowKey = SaveDraftWorkflowKey,
                IsSystem = true,
                PortalID = portalId
            };
            _workflowRepository.AddWorkflow(workflow);

            var state = GetDraftStateDefinition(1);
            state.WorkflowID = workflow.WorkflowID;
            _workflowStateRepository.AddWorkflowState(state);

            state = GetPublishedStateDefinition(2);
            state.WorkflowID = workflow.WorkflowID;
            _workflowStateRepository.AddWorkflowState(state);
        }

        private void CreateContentApprovalWorkflow(int portalId)
        {
            var workflow = new Entities.Workflow
            {
                WorkflowName = Localization.GetString("DefaultWorkflowName"),
                Description = Localization.GetString("DefaultWorkflowDescription"),
                WorkflowKey = ContentAprovalWorkflowKey,
                IsSystem = true,
                PortalID = portalId
            };
            _workflowRepository.AddWorkflow(workflow);

            var state = GetDraftStateDefinition(1);
            state.WorkflowID = workflow.WorkflowID;
            _workflowStateRepository.AddWorkflowState(state);

            state = GetReadyForReviewStateDefinition(2);
            state.WorkflowID = workflow.WorkflowID;
            _workflowStateRepository.AddWorkflowState(state);

            state = GetPublishedStateDefinition(3);
            state.WorkflowID = workflow.WorkflowID;
            _workflowStateRepository.AddWorkflowState(state);
        }
        #endregion

        #region Public Methods
        public void CreateSystemWorkflows(int portalId)
        {
            CreateDirectPublishWorkflow(portalId);
            CreateSaveDraftWorkflow(portalId);
            CreateContentApprovalWorkflow(portalId);
        }

        public Entities.Workflow GetDirectPublishWorkflow(int portalId)
        {
            return _workflowRepository.GetSystemWorkflows(portalId).SingleOrDefault(sw => sw.WorkflowKey == DirectPublishWorkflowKey);
        }

        public Entities.Workflow GetSaveDraftWorkflow(int portalId)
        {
            return _workflowRepository.GetSystemWorkflows(portalId).SingleOrDefault(sw => sw.WorkflowKey == SaveDraftWorkflowKey);
        }

        public Entities.Workflow GetContentApprovalWorkflow(int portalId)
        {
            return _workflowRepository.GetSystemWorkflows(portalId).SingleOrDefault(sw => sw.WorkflowKey == ContentAprovalWorkflowKey);
        }

        public WorkflowState GetDraftStateDefinition(int order)
        {
            var state = GetDefaultWorkflowState(order);
            state.StateName = Localization.GetString("DefaultWorkflowState1.StateName");
            return state;
        }

        public WorkflowState GetPublishedStateDefinition(int order)
        {
            var state = GetDefaultWorkflowState(order);
            state.StateName = Localization.GetString("DefaultWorkflowState3.StateName");
            return state;
        }

        public WorkflowState GetReadyForReviewStateDefinition(int order)
        {
            var state = GetDefaultWorkflowState(order);
            state.StateName = Localization.GetString("DefaultWorkflowState2.StateName");
            state.SendNotification = true;
            state.SendNotificationToAdministrators = true;
            return state;
        }
        #endregion
        
        #region Service Locator
        protected override Func<ISystemWorkflowManager> GetFactory()
        {
            return () => new SystemWorkflowManager();
        }
        #endregion
    }
}
