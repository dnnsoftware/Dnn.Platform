// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Content.Workflow.Entities;

    /// <summary>
    /// This class is responsible to persist and retrieve workflow state entity.
    /// </summary>
    internal interface IWorkflowStateRepository
    {
        /// <summary>
        /// Get all states for a specific workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <returns>List of workflow states.</returns>
        IEnumerable<WorkflowState> GetWorkflowStates(int workflowId);

        /// <summary>
        /// Get a workflow state by Id.
        /// </summary>
        /// <param name="stateID">State Id.</param>
        /// <returns>Workflow State entity.</returns>
        WorkflowState GetWorkflowStateByID(int stateID);

        /// <summary>
        /// Persists a new workflow state entity.
        /// </summary>
        /// <param name="state">Workflow State entity.</param>
        void AddWorkflowState(WorkflowState state);

        /// <summary>
        /// Persists changes for a workflow state entity.
        /// </summary>
        /// <param name="state">Workflow State entity.</param>
        void UpdateWorkflowState(WorkflowState state);

        /// <summary>
        /// This method hard deletes a workflow state entity.
        /// </summary>
        /// <param name="state">Workflow State entity.</param>
        void DeleteWorkflowState(WorkflowState state);
    }
}
