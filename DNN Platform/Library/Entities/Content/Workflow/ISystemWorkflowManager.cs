﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Content.Workflow.Entities;

namespace DotNetNuke.Entities.Content.Workflow
{
    /// <summary>
    /// This class is responsible to manage the system workflows of the portal. 
    /// It provides creation operation methods and methods to get specifically all system workflows
    /// </summary>
    public interface ISystemWorkflowManager
    {
        /// <summary>
        /// Creates predefined system workflows
        /// </summary>
        /// <param name="portalId">Portal Id where system workflows will be created</param>
        void CreateSystemWorkflows(int portalId);

        /// <summary>
        /// Get the 'Direct Publish' system workflow of a specific portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>The 'Direct Publish' workflow</returns>
        Entities.Workflow GetDirectPublishWorkflow(int portalId);
        
        /// <summary>
        /// Get the 'Save Draft' system workflow of a specific portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>The 'Save Draft' workflow</returns>
        Entities.Workflow GetSaveDraftWorkflow(int portalId);

        /// <summary>
        /// Get the 'Content Approval' system workflow of a specific portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>The 'Content Approval' workflow</returns>
        Entities.Workflow GetContentApprovalWorkflow(int portalId);
        
        /// <summary>
        /// Gets a default definition of the 'Draft' system state
        /// </summary>
        /// <param name="order">Order number to be included in the state definition</param>
        /// <returns>A 'Draft' state definition</returns>
        WorkflowState GetDraftStateDefinition(int order);
        
        /// <summary>
        /// Gets a default definition of the 'Published' system state
        /// </summary>
        /// <param name="order">Order number to be included in the state definition</param>
        /// <returns>A 'Published' state definition</returns>
        WorkflowState GetPublishedStateDefinition(int order);

        /// <summary>
        /// Gets a default definition of the 'Ready for review' system state
        /// </summary>
        /// <param name="order">Order number to be included in the state definition</param>
        /// <returns>A 'Ready for review' state definition</returns>
        WorkflowState GetReadyForReviewStateDefinition(int order);
    }
}
