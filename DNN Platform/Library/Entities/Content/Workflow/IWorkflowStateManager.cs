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

namespace DotNetNuke.Entities.Content.Workflow
{
    /// <summary>
    /// This class is responsible to the management of the Workflow States
    /// </summary>
    public interface IWorkflowStateManager
    {
        /// <summary>
        /// This method returns the list of States of a Workflow ordered by State Order ascending
        /// </summary>
        /// <param name="workflowId">Workflow Id</param>
        /// <returns>List of workflow States ordered by State Order ascending</returns>
        IEnumerable<WorkflowState> GetWorkflowStates(int workflowId);

        /// <summary>
        /// This method returns the total number of Content Items that are associated with the State
        /// </summary>
        /// <param name="stateId">State Id</param>
        /// <returns>Total count of Content Items that are using the specified state</returns>
        int GetContentWorkflowStateUsageCount(int stateId);

        /// <summary>
        /// This method returns a workflow State by Id
        /// </summary>
        /// <param name="stateId">State Id</param>
        /// <returns>State entity</returns>
        WorkflowState GetWorkflowState(int stateId);

        /// <summary>
        /// This method adds a new State to a workflow. The new state is always added as next to last state. 
        /// </summary>
        /// <remarks>This method also takes care on state reordering.</remarks>
        /// <param name="state">State entity</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowDoesNotExistException">Thrown when adding a state to a workflow that does not exist</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when adding a state to a system workflow</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowStateNameAlreadyExistsException">Thrown when already exist a state in the workflow with the same name</exception>
        void AddWorkflowState(WorkflowState state);

        /// <summary>
        /// This method updates a State.
        /// </summary>
        /// <remarks>This method does not update the Order of the state. Use MoveWorkflowStateDown and MoveWorkflowStateUp for this operation.</remarks>
        /// <param name="state">State entity</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowDoesNotExistException">Thrown when updating a state that does not exist</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowStateNameAlreadyExistsException">Thrown when already exist a state in the workflow with the same name</exception>
        void UpdateWorkflowState(WorkflowState state);

        /// <summary>
        /// This method hard deletes a state
        /// </summary>
        /// <remarks>This method takes care of state reordering.</remarks>
        /// <param name="state">State entity</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when deleting a system state workflow (i.e.: Draft, Published) or if the workflow state is beign used</exception>
        void DeleteWorkflowState(WorkflowState state);

        /// <summary>
        /// This method move the state down to 1 position in the workflow state order
        /// </summary>
        /// <remarks>This method takes care of state reordering.</remarks>
        /// <param name="stateId">State Id</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowDoesNotExistException">Thrown when moving a state that does not exist</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when state cannot be moved (i.e.: is the first/last state, etc...)</exception>
        void MoveWorkflowStateDown(int stateId);

        /// <summary>
        /// This method move the state up to 1 position in the workflow state order
        /// </summary>
        /// <remarks>This method takes care of state reordering.</remarks>
        /// <param name="stateId">State Id</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowDoesNotExistException">Thrown when moving a state that does not exist</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when state cannot be moved (i.e.: is the first/last state, etc...)</exception>
        void MoveWorkflowStateUp(int stateId);

        /// <summary>
        /// This method move the state up to index position in the workflow state order
        /// </summary>
        /// <remarks>This method takes care of state reordering.</remarks>
        /// <param name="stateId">State Id</param>
        /// <param name="index">Index where the stateId will be moved. Since first and last states can't be moved, this index has to be a number between 1 and the number of states minus 2.</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowDoesNotExistException">Thrown when moving a state that does not exist</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when state cannot be moved (i.e.: is the first/last state, etc...)</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when index is not a valid value (index = 0 or index >= number of states - 2)</exception>
        void MoveState(int stateId, int index);
        
        /// <summary>
        /// This method returns the list of State Permission of a specific state
        /// </summary>
        /// <param name="stateId">State Id</param>
        /// <returns>List of state permissions</returns>
        IEnumerable<WorkflowStatePermission> GetWorkflowStatePermissionByState(int stateId);

        /// <summary>
        /// This method add a new workflow state permission
        /// </summary>
        /// <param name="permission">Permission</param>
        /// <param name="userId">User Id of the user that perform the action</param>
        void AddWorkflowStatePermission(WorkflowStatePermission permission, int userId);

        /// <summary>
        /// This method deletes a workflow state permission by Id
        /// </summary>
        /// <param name="workflowStatePermissionId">Workflow state permission Id</param>
        void DeleteWorkflowStatePermission(int workflowStatePermissionId);
    }
}