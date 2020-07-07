// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Content.Workflow.Exceptions;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    public class WorkflowStateManager : ServiceLocator<IWorkflowStateManager, WorkflowStateManager>, IWorkflowStateManager
    {
        private readonly DataProvider _dataProvider;
        private readonly IWorkflowRepository _workflowRepository = WorkflowRepository.Instance;
        private readonly IWorkflowStateRepository _workflowStateRepository = WorkflowStateRepository.Instance;
        private readonly IWorkflowStatePermissionsRepository _workflowStatePermissionsRepository = WorkflowStatePermissionsRepository.Instance;

        public WorkflowStateManager()
        {
            this._dataProvider = DataProvider.Instance();
        }

        public IEnumerable<WorkflowState> GetWorkflowStates(int workflowId)
        {
            return this._workflowStateRepository.GetWorkflowStates(workflowId);
        }

        public WorkflowState GetWorkflowState(int stateId)
        {
            return this._workflowStateRepository.GetWorkflowStateByID(stateId);
        }

        public void AddWorkflowState(WorkflowState state)
        {
            var workflow = this._workflowRepository.GetWorkflow(state.WorkflowID);
            if (workflow == null)
            {
                throw new WorkflowDoesNotExistException();
            }

            if (workflow.IsSystem)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowNewStateCannotBeAddedToSystemWorkflows", Localization.ExceptionsResourceFile));
            }

            var lastState = workflow.LastState;

            // New States always goes before the last state
            state.Order = lastState.Order;

            lastState.Order++;
            this._workflowStateRepository.AddWorkflowState(state);
            this._workflowStateRepository.UpdateWorkflowState(lastState); // Update last state order
        }

        public void DeleteWorkflowState(WorkflowState state)
        {
            var stateToDelete = this._workflowStateRepository.GetWorkflowStateByID(state.StateID);
            if (stateToDelete == null)
            {
                return;
            }

            if (stateToDelete.IsSystem)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowSystemWorkflowStateCannotBeDeleted", Localization.ExceptionsResourceFile));
            }

            if (this._dataProvider.GetContentWorkflowStateUsageCount(state.StateID) > 0)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateInUsageException", Localization.ExceptionsResourceFile));
            }

            this._workflowStateRepository.DeleteWorkflowState(stateToDelete);

            // Reorder states order
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowState>();
                rep.Update("SET [Order] = [Order] - 1 WHERE WorkflowID = @0 AND [Order] > @1", stateToDelete.WorkflowID, stateToDelete.Order);
            }
        }

        public void UpdateWorkflowState(WorkflowState state)
        {
            var workflowState = this._workflowStateRepository.GetWorkflowStateByID(state.StateID);
            if (workflowState == null)
            {
                throw new WorkflowDoesNotExistException();
            }

            this._workflowStateRepository.UpdateWorkflowState(state);
        }

        public void MoveWorkflowStateDown(int stateId)
        {
            var state = this._workflowStateRepository.GetWorkflowStateByID(stateId);
            if (state == null)
            {
                throw new WorkflowDoesNotExistException();
            }

            var states = this._workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();

            if (states.Length == 3)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            WorkflowState stateToMoveUp = null;
            WorkflowState stateToMoveDown = null;

            for (var i = 0; i < states.Length; i++)
            {
                if (states[i].StateID != stateId)
                {
                    continue;
                }

                // First and Second workflow state cannot be moved down
                if (i <= 1)
                {
                    throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
                }

                stateToMoveUp = states[i - 1];
                stateToMoveDown = states[i];
                break;
            }

            if (stateToMoveUp == null || stateToMoveDown == null)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            var orderTmp = stateToMoveDown.Order;
            stateToMoveDown.Order = stateToMoveUp.Order;
            stateToMoveUp.Order = orderTmp;

            this._workflowStateRepository.UpdateWorkflowState(stateToMoveUp);
            this._workflowStateRepository.UpdateWorkflowState(stateToMoveDown);
        }

        public void MoveWorkflowStateUp(int stateId)
        {
            var state = this._workflowStateRepository.GetWorkflowStateByID(stateId);
            if (state == null)
            {
                throw new WorkflowDoesNotExistException();
            }

            var states = this._workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();

            if (states.Length == 3)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            WorkflowState stateToMoveUp = null;
            WorkflowState stateToMoveDown = null;

            for (var i = 0; i < states.Length; i++)
            {
                if (states[i].StateID != stateId)
                {
                    continue;
                }

                // Last and Next to Last workflow state cannot be moved up
                if (i >= states.Length - 2)
                {
                    throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
                }

                stateToMoveUp = states[i];
                stateToMoveDown = states[i + 1];
                break;
            }

            if (stateToMoveUp == null || stateToMoveDown == null)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            var orderTmp = stateToMoveDown.Order;
            stateToMoveDown.Order = stateToMoveUp.Order;
            stateToMoveUp.Order = orderTmp;

            this._workflowStateRepository.UpdateWorkflowState(stateToMoveUp);
            this._workflowStateRepository.UpdateWorkflowState(stateToMoveDown);
        }

        public void MoveState(int stateId, int index)
        {
            var state = this._workflowStateRepository.GetWorkflowStateByID(stateId);
            if (state == null)
            {
                throw new WorkflowDoesNotExistException();
            }

            var states = this._workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();
            if (index < 1 || index > states.Length - 2)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            var currentIndex = this.GetStateIndex(states, state);

            if (currentIndex == 0
                || currentIndex == states.Length - 1)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            this.MoveState(state, index, currentIndex);
        }

        public IEnumerable<WorkflowStatePermission> GetWorkflowStatePermissionByState(int stateId)
        {
            return this._workflowStatePermissionsRepository.GetWorkflowStatePermissionByState(stateId);
        }

        public void AddWorkflowStatePermission(WorkflowStatePermission permission, int userId)
        {
            permission.WorkflowStatePermissionID = this._workflowStatePermissionsRepository.AddWorkflowStatePermission(permission, userId);
        }

        public void DeleteWorkflowStatePermission(int workflowStatePermissionId)
        {
            this._workflowStatePermissionsRepository.DeleteWorkflowStatePermission(workflowStatePermissionId);
        }

        public int GetContentWorkflowStateUsageCount(int stateId)
        {
            return this._dataProvider.GetContentWorkflowStateUsageCount(stateId);
        }

        protected override Func<IWorkflowStateManager> GetFactory()
        {
            return () => new WorkflowStateManager();
        }

        private int GetStateIndex(WorkflowState[] states, WorkflowState currentState)
        {
            int i = 0;

            foreach (var state in states)
            {
                if (state.StateID == currentState.StateID)
                {
                    return i;
                }

                i++;
            }

            return i;
        }

        private void MoveState(WorkflowState state, int targetIndex, int currentIndex)
        {
            if (currentIndex == targetIndex)
            {
                return;
            }

            var moveUp = currentIndex < targetIndex;
            var numberOfMovements = moveUp ? targetIndex - currentIndex : currentIndex - targetIndex;

            for (var i = 0; i < numberOfMovements; i++)
            {
                if (moveUp)
                {
                    this.MoveWorkflowStateUp(state.StateID);
                }
                else
                {
                    this.MoveWorkflowStateDown(state.StateID);
                }
            }
        }
    }
}
