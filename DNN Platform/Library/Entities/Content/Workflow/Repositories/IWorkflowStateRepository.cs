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

using System.Collections.Generic;
using DotNetNuke.Entities.Content.Workflow.Entities;

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    /// <summary>
    /// This class is responsible to persist and retrieve workflow state entity
    /// </summary>
    internal interface IWorkflowStateRepository
    {
        /// <summary>
        /// Get all states for a specific workflow
        /// </summary>
        /// <param name="workflowId">Workflow Id</param>
        /// <returns>List of workflow states</returns>
        IEnumerable<WorkflowState> GetWorkflowStates(int workflowId);

        /// <summary>
        /// Get a workflow state by Id
        /// </summary>
        /// <param name="stateID">State Id</param>
        /// <returns>Workflow State entity</returns>
        WorkflowState GetWorkflowStateByID(int stateID);

        /// <summary>
        /// Persists a new workflow state entity
        /// </summary>
        /// <param name="state">Workflow State entity</param>
        void AddWorkflowState(WorkflowState state);

        /// <summary>
        /// Persists changes for a workflow state entity
        /// </summary>
        /// <param name="state">Workflow State entity</param>
        void UpdateWorkflowState(WorkflowState state);

        /// <summary>
        /// This method hard deletes a workflow state entity
        /// </summary>
        /// <param name="state">Workflow State entity</param>
        void DeleteWorkflowState(WorkflowState state);
    }
}
