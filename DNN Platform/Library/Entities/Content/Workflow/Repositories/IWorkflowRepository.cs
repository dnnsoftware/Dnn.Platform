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
