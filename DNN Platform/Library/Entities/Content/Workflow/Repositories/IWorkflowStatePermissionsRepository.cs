// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
