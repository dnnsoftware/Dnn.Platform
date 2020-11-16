// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Content.Workflow.Entities;

    /// <summary>
    /// This class is responsible to manage the workflow state permission entity.
    /// </summary>
    internal interface IWorkflowStatePermissionsRepository
    {
        /// <summary>
        /// Gets the registered permissions set for a specific state.
        /// </summary>
        /// <param name="stateId">State Id.</param>
        /// <returns>List of Workflow State Permission entities.</returns>
        IEnumerable<WorkflowStatePermission> GetWorkflowStatePermissionByState(int stateId);

        /// <summary>
        /// Persists a new Workflow State Permission entity.
        /// </summary>
        /// <param name="permission">Workflow State Permission entity.</param>
        /// <param name="lastModifiedByUserId">User Id who modifies the permissions set.</param>
        /// <returns></returns>
        int AddWorkflowStatePermission(WorkflowStatePermission permission, int lastModifiedByUserId);

        /// <summary>
        /// Deletes a specific Workflow State Permission entity.
        /// </summary>
        /// <param name="workflowStatePermissionId">Workflow State Permission Id.</param>
        void DeleteWorkflowStatePermission(int workflowStatePermissionId);
    }
}
