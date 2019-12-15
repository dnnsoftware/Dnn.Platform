// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    /// <summary>
    /// This class is responsible to persist and retrieve workflow entity
    /// </summary>
    internal interface IWorkflowRepository
    {
        /// <summary>
        /// This method gets the list of system portal workflows
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>List of system workflows of the portal</returns>
        IEnumerable<Entities.Workflow> GetSystemWorkflows(int portalId);

        /// <summary>
        /// This method gets the list of portal workflows
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>List of workflows of the portal</returns>
        IEnumerable<Entities.Workflow> GetWorkflows(int portalId);

        /// <summary>
        /// This method gets the Workflow by Id
        /// </summary>
        /// <param name="workflowId">Workflow Id</param>
        /// <returns>Workflow entity</returns>
        Entities.Workflow GetWorkflow(int workflowId);

        /// <summary>
        /// This method gets the Workflow by Content Item Id
        /// </summary>
        /// <param name="contentItem">Content Item entity</param>
        /// <returns>Workflow entity</returns>
        Entities.Workflow GetWorkflow(ContentItem contentItem);

        /// <summary>
        /// This method persists a new workflow entity
        /// </summary>
        /// <param name="workflow">Workflow entity</param>
        void AddWorkflow(Entities.Workflow workflow);

        /// <summary>
        /// This method persists changes for a workflow entity
        /// </summary>
        /// <param name="workflow">Workflow entity</param>
        void UpdateWorkflow(Entities.Workflow workflow);

        /// <summary>
        /// This method hard deletes a workflow
        /// </summary>
        /// <param name="workflow">Workflow entity</param>
        void DeleteWorkflow(Entities.Workflow workflow);
    }
}
