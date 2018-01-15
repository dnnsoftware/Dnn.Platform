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

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Workflow.Entities;
using DotNetNuke.Entities.Content.Workflow.Exceptions;
using DotNetNuke.Entities.Content.Workflow.Repositories;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.Workflow
{
    public class WorkflowStateManager : ServiceLocator<IWorkflowStateManager, WorkflowStateManager>, IWorkflowStateManager
    {
        #region Members
        private readonly DataProvider _dataProvider;
        private readonly IWorkflowRepository _workflowRepository = WorkflowRepository.Instance;
        private readonly IWorkflowStateRepository _workflowStateRepository = WorkflowStateRepository.Instance;
        private readonly IWorkflowStatePermissionsRepository _workflowStatePermissionsRepository = WorkflowStatePermissionsRepository.Instance;
        #endregion

        #region Constructor
        public WorkflowStateManager()
        {
            _dataProvider = DataProvider.Instance();
        }
        #endregion

        #region Public Methods
        public IEnumerable<WorkflowState> GetWorkflowStates(int workflowId)
        {
            return _workflowStateRepository.GetWorkflowStates(workflowId);
        }

        public WorkflowState GetWorkflowState(int stateId)
        {
            return _workflowStateRepository.GetWorkflowStateByID(stateId);
        }

        public void AddWorkflowState(WorkflowState state)
        {
            var workflow = _workflowRepository.GetWorkflow(state.WorkflowID);
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
            _workflowStateRepository.AddWorkflowState(state);
            _workflowStateRepository.UpdateWorkflowState(lastState); // Update last state order
        }

        public void DeleteWorkflowState(WorkflowState state)
        {
            var stateToDelete = _workflowStateRepository.GetWorkflowStateByID(state.StateID);
            if (stateToDelete == null)
            {
                return;
            }

            if (stateToDelete.IsSystem)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowSystemWorkflowStateCannotBeDeleted", Localization.ExceptionsResourceFile));
            }

            if (_dataProvider.GetContentWorkflowStateUsageCount(state.StateID) > 0)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateInUsageException", Localization.ExceptionsResourceFile));
            }

            _workflowStateRepository.DeleteWorkflowState(stateToDelete);

            // Reorder states order
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowState>();
                rep.Update("SET [Order] = [Order] - 1 WHERE WorkflowID = @0 AND [Order] > @1", stateToDelete.WorkflowID, stateToDelete.Order);
            }
        }

        public void UpdateWorkflowState(WorkflowState state)
        {
            var workflowState = _workflowStateRepository.GetWorkflowStateByID(state.StateID);
            if (workflowState == null)
            {
                throw new WorkflowDoesNotExistException();
            }
            _workflowStateRepository.UpdateWorkflowState(state);
        }

        public void MoveWorkflowStateDown(int stateId)
        {
            var state = _workflowStateRepository.GetWorkflowStateByID(stateId);
            if (state == null)
            {
                throw new WorkflowDoesNotExistException();
            }

            var states = _workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();

            if (states.Length == 3)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            WorkflowState stateToMoveUp = null;
            WorkflowState stateToMoveDown = null;

            for (var i = 0; i < states.Length; i++)
            {
                if (states[i].StateID != stateId) continue;

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

            _workflowStateRepository.UpdateWorkflowState(stateToMoveUp);
            _workflowStateRepository.UpdateWorkflowState(stateToMoveDown);
        }

        public void MoveWorkflowStateUp(int stateId)
        {
            var state = _workflowStateRepository.GetWorkflowStateByID(stateId);
            if (state == null)
            {
                throw new WorkflowDoesNotExistException();
            }

            var states = _workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();

            if (states.Length == 3)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            WorkflowState stateToMoveUp = null;
            WorkflowState stateToMoveDown = null;

            for (var i = 0; i < states.Length; i++)
            {
                if (states[i].StateID != stateId) continue;

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

            _workflowStateRepository.UpdateWorkflowState(stateToMoveUp);
            _workflowStateRepository.UpdateWorkflowState(stateToMoveDown);
        }

        public void MoveState(int stateId, int index)
        {
            var state = _workflowStateRepository.GetWorkflowStateByID(stateId);
            if (state == null)
            {
                throw new WorkflowDoesNotExistException();
            }

            var states = _workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();
            if (index < 1 || index > states.Length - 2)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            var currentIndex = GetStateIndex(states, state);

            if (currentIndex == 0
                || currentIndex == states.Length - 1)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            MoveState(state, index, currentIndex);
        }

        public IEnumerable<WorkflowStatePermission> GetWorkflowStatePermissionByState(int stateId)
        {
            return _workflowStatePermissionsRepository.GetWorkflowStatePermissionByState(stateId);
        }

        public void AddWorkflowStatePermission(WorkflowStatePermission permission, int userId)
        {
            permission.WorkflowStatePermissionID = _workflowStatePermissionsRepository.AddWorkflowStatePermission(permission, userId);
        }

        public void DeleteWorkflowStatePermission(int workflowStatePermissionId)
        {
            _workflowStatePermissionsRepository.DeleteWorkflowStatePermission(workflowStatePermissionId);
        }

        public int GetContentWorkflowStateUsageCount(int stateId)
        {
            return _dataProvider.GetContentWorkflowStateUsageCount(stateId);
        }
        #endregion

        #region Private Methods
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
                    MoveWorkflowStateUp(state.StateID);
                }
                else
                {
                    MoveWorkflowStateDown(state.StateID);
                }
            }
        }
        #endregion
        #region Service Locator
        protected override Func<IWorkflowStateManager> GetFactory()
        {
            return () => new WorkflowStateManager();
        }
        #endregion
    }
}
