// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Content.Workflow.Entities;

    /// <summary>
    /// This class is responsible to manage the workflows of the portal.
    /// It provides CRUD operation methods and methods to know the usage of the workflow.
    /// </summary>
    public interface IWorkflowManager
    {
        /// <summary>
        /// This method returns the paginated list of Items that are associated with a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <param name="pageIndex">Page index (where 1 is the index of the first page).</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>List of Usage Items.</returns>
        IEnumerable<WorkflowUsageItem> GetWorkflowUsage(int workflowId, int pageIndex, int pageSize);

        /// <summary>
        /// This method returns the total number of Content Items that are associated with any State of a workflow (even the Published state).
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <returns>Total count of Content Items that are using the specified workflow.</returns>
        int GetWorkflowUsageCount(int workflowId);

        /// <summary>
        /// This method return the list of the Workflows defined for the portal.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <returns>List of the Workflows for the portal.</returns>
        IEnumerable<Entities.Workflow> GetWorkflows(int portalId);

        /// <summary>
        /// This method adds a new workflow. It automatically add two system states: "Draft" and "Published".
        /// </summary>
        /// <param name="workflow">Workflow Entity.</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowNameAlreadyExistsException">Thrown when a workflow with the same name already exist for the portal.</exception>
        void AddWorkflow(Entities.Workflow workflow);

        /// <summary>
        /// this method update a existing workflow.
        /// </summary>
        /// <param name="workflow">Workflow Entity.</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowNameAlreadyExistsException">Thrown when a workflow with the same name already exist for the portal.</exception>
        void UpdateWorkflow(Entities.Workflow workflow);

        /// <summary>
        /// This method hard deletes a workflow.
        /// </summary>
        /// <param name="workflow">Workflow Entity.</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when a workflow is in use or is a system workflow.</exception>
        void DeleteWorkflow(Entities.Workflow workflow);

        /// <summary>
        /// This method returns a workflow entity by Id.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <returns>Workflow Entity.</returns>
        Entities.Workflow GetWorkflow(int workflowId);

        /// <summary>
        /// This method returns a workflow entity by Content Item Id. It returns null if the Content Item is not under workflow.
        /// </summary>
        /// <param name="contentItem">Content Item.</param>
        /// <returns>Workflow Entity.</returns>
        Entities.Workflow GetWorkflow(ContentItem contentItem);
    }
}
