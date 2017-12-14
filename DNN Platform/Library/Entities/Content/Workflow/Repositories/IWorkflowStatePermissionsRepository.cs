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
    /// This class is responsible to manage the workflow state permission entity
    /// </summary>
    internal interface IWorkflowStatePermissionsRepository
    {
        /// <summary>
        /// Gets the registered permissions set for a specific state
        /// </summary>
        /// <param name="stateId">State Id</param>
        /// <returns>List of Workflow State Permission entities</returns>
        IEnumerable<WorkflowStatePermission> GetWorkflowStatePermissionByState(int stateId);

        /// <summary>
        /// Persists a new Workflow State Permission entity
        /// </summary>
        /// <param name="permission">Workflow State Permission entity</param>
        /// <param name="lastModifiedByUserId">User Id who modifies the permissions set</param>
        int AddWorkflowStatePermission(WorkflowStatePermission permission, int lastModifiedByUserId);

        /// <summary>
        /// Deletes a specific Workflow State Permission entity
        /// </summary>
        /// <param name="workflowStatePermissionId">Workflow State Permission Id</param>
        void DeleteWorkflowStatePermission(int workflowStatePermissionId);
    }
}
