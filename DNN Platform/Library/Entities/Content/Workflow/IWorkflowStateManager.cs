// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Content.Workflow.Entities;

    /// <summary>
    /// This class is responsible to the management of the Workflow States.
    /// </summary>
    public interface IWorkflowStateManager
    {
        /// <summary>
        /// This method returns the list of States of a Workflow ordered by State Order ascending.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <returns>List of workflow States ordered by State Order ascending.</returns>
        IEnumerable<WorkflowState> GetWorkflowStates(int workflowId);

        /// <summary>
        /// This method returns the total number of Content Items that are associated with the State.
        /// </summary>
        /// <param name="stateId">State Id.</param>
        /// <returns>Total count of Content Items that are using the specified state.</returns>
        int GetContentWorkflowStateUsageCount(int stateId);

        /// <summary>
        /// This method returns a workflow State by Id.
        /// </summary>
        /// <param name="stateId">State Id.</param>
        /// <returns>State entity.</returns>
        WorkflowState GetWorkflowState(int stateId);

        /// <summary>
        /// This method adds a new State to a workflow. The new state is always added as next to last state.
        /// </summary>
        /// <remarks>This method also takes care on state reordering.</remarks>
        /// <param name="state">State entity.</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowDoesNotExistException">Thrown when adding a state to a workflow that does not exist.</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when adding a state to a system workflow.</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowStateNameAlreadyExistsException">Thrown when already exist a state in the workflow with the same name.</exception>
        void AddWorkflowState(WorkflowState state);

        /// <summary>
        /// This method updates a State.
        /// </summary>
        /// <remarks>This method does not update the Order of the state. Use MoveWorkflowStateDown and MoveWorkflowStateUp for this operation.</remarks>
        /// <param name="state">State entity.</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowDoesNotExistException">Thrown when updating a state that does not exist.</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowStateNameAlreadyExistsException">Thrown when already exist a state in the workflow with the same name.</exception>
        void UpdateWorkflowState(WorkflowState state);

        /// <summary>
        /// This method hard deletes a state.
        /// </summary>
        /// <remarks>This method takes care of state reordering.</remarks>
        /// <param name="state">State entity.</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when deleting a system state workflow (i.e.: Draft, Published) or if the workflow state is beign used.</exception>
        void DeleteWorkflowState(WorkflowState state);

        /// <summary>
        /// This method move the state down to 1 position in the workflow state order.
        /// </summary>
        /// <remarks>This method takes care of state reordering.</remarks>
        /// <param name="stateId">State Id.</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowDoesNotExistException">Thrown when moving a state that does not exist.</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when state cannot be moved (i.e.: is the first/last state, etc...)</exception>
        void MoveWorkflowStateDown(int stateId);

        /// <summary>
        /// This method move the state up to 1 position in the workflow state order.
        /// </summary>
        /// <remarks>This method takes care of state reordering.</remarks>
        /// <param name="stateId">State Id.</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowDoesNotExistException">Thrown when moving a state that does not exist.</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when state cannot be moved (i.e.: is the first/last state, etc...)</exception>
        void MoveWorkflowStateUp(int stateId);

        /// <summary>
        /// This method move the state up to index position in the workflow state order.
        /// </summary>
        /// <remarks>This method takes care of state reordering.</remarks>
        /// <param name="stateId">State Id.</param>
        /// <param name="index">Index where the stateId will be moved. Since first and last states can't be moved, this index has to be a number between 1 and the number of states minus 2.</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowDoesNotExistException">Thrown when moving a state that does not exist.</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when state cannot be moved (i.e.: is the first/last state, etc...)</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when index is not a valid value (index = 0 or index >= number of states - 2).</exception>
        void MoveState(int stateId, int index);

        /// <summary>
        /// This method returns the list of State Permission of a specific state.
        /// </summary>
        /// <param name="stateId">State Id.</param>
        /// <returns>List of state permissions.</returns>
        IEnumerable<WorkflowStatePermission> GetWorkflowStatePermissionByState(int stateId);

        /// <summary>
        /// This method add a new workflow state permission.
        /// </summary>
        /// <param name="permission">Permission.</param>
        /// <param name="userId">User Id of the user that perform the action.</param>
        void AddWorkflowStatePermission(WorkflowStatePermission permission, int userId);

        /// <summary>
        /// This method deletes a workflow state permission by Id.
        /// </summary>
        /// <param name="workflowStatePermissionId">Workflow state permission Id.</param>
        void DeleteWorkflowStatePermission(int workflowStatePermissionId);
    }
}
