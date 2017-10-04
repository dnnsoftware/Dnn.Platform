#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using DotNetNuke.Entities.Content.Workflow.Entities;
using System.Collections.Generic;

namespace DotNetNuke.Entities.Content.Workflow
{
    /// <summary>
    /// This class is responsible to manage the workflows of the portal. 
    /// It provides CRUD operation methods and methods to know the usage of the workflow 
    /// </summary>
    public interface IWorkflowManager
    {
        /// <summary>
        /// This method returns the paginated list of Items that are associated with a workflow 
        /// </summary>
        /// <param name="workflowId">Workflow Id</param>
        /// <param name="pageIndex">Page index (where 1 is the index of the first page)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of Usage Items</returns>
        IEnumerable<WorkflowUsageItem> GetWorkflowUsage(int workflowId, int pageIndex, int pageSize);

        /// <summary>
        /// This method returns the total number of Content Items that are associated with any State of a workflow (even the Published state)
        /// </summary>
        /// <param name="workflowId">Workflow Id</param>
        /// <returns>Total count of Content Items that are using the specified workflow</returns>
        int GetWorkflowUsageCount(int workflowId);
        
        /// <summary>
        /// This method return the list of the Workflows defined for the portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>List of the Workflows for the portal</returns>
        IEnumerable<Entities.Workflow> GetWorkflows(int portalId);
        
        /// <summary>
        /// This method adds a new workflow. It automatically add two system states: "Draft" and "Published"
        /// </summary>
        /// <param name="workflow">Workflow Entity</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowNameAlreadyExistsException">Thrown when a workflow with the same name already exist for the portal</exception>
        void AddWorkflow(Entities.Workflow workflow);

        /// <summary>
        /// this method update a existing workflow.
        /// </summary>
        /// <param name="workflow">Workflow Entity</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowNameAlreadyExistsException">Thrown when a workflow with the same name already exist for the portal</exception>
        void UpdateWorkflow(Entities.Workflow workflow);

        /// <summary>
        /// This method hard deletes a workflow
        /// </summary>
        /// <param name="workflow">Workflow Entity</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when a workflow is in use or is a system workflow</exception>
        void DeleteWorkflow(Entities.Workflow workflow);

        /// <summary>
        /// This method returns a workflow entity by Id
        /// </summary>
        /// <param name="workflowId">Workflow Id</param>
        /// <returns>Workflow Entity</returns>
        Entities.Workflow GetWorkflow(int workflowId);

        /// <summary>
        /// This method returns a workflow entity by Content Item Id. It returns null if the Content Item is not under workflow.
        /// </summary>
        /// <param name="contentItem">Content Item</param>
        /// <returns>Workflow Entity</returns>
        Entities.Workflow GetWorkflow(ContentItem contentItem);

    }
}