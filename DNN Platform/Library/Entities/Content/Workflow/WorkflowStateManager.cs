// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow;

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
    private readonly DataProvider dataProvider;
    private readonly IWorkflowRepository workflowRepository = WorkflowRepository.Instance;
    private readonly IWorkflowStateRepository workflowStateRepository = WorkflowStateRepository.Instance;
    private readonly IWorkflowStatePermissionsRepository workflowStatePermissionsRepository = WorkflowStatePermissionsRepository.Instance;

    /// <summary>Initializes a new instance of the <see cref="WorkflowStateManager"/> class.</summary>
    public WorkflowStateManager()
    {
        this.dataProvider = DataProvider.Instance();
    }

    /// <inheritdoc/>
    public IEnumerable<WorkflowState> GetWorkflowStates(int workflowId)
    {
        return this.workflowStateRepository.GetWorkflowStates(workflowId);
    }

    /// <inheritdoc/>
    public WorkflowState GetWorkflowState(int stateId)
    {
        return this.workflowStateRepository.GetWorkflowStateByID(stateId);
    }

    /// <inheritdoc/>
    public void AddWorkflowState(WorkflowState state)
    {
        var workflow = this.workflowRepository.GetWorkflow(state.WorkflowID);
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
        this.workflowStateRepository.AddWorkflowState(state);
        this.workflowStateRepository.UpdateWorkflowState(lastState); // Update last state order
    }

    /// <inheritdoc/>
    public void DeleteWorkflowState(WorkflowState state)
    {
        var stateToDelete = this.workflowStateRepository.GetWorkflowStateByID(state.StateID);
        if (stateToDelete == null)
        {
            return;
        }

        if (stateToDelete.IsSystem)
        {
            throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowSystemWorkflowStateCannotBeDeleted", Localization.ExceptionsResourceFile));
        }

        if (this.dataProvider.GetContentWorkflowStateUsageCount(state.StateID) > 0)
        {
            throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateInUsageException", Localization.ExceptionsResourceFile));
        }

        this.workflowStateRepository.DeleteWorkflowState(stateToDelete);

        // Reorder states order
        using (var context = DataContext.Instance())
        {
            var rep = context.GetRepository<WorkflowState>();
            rep.Update("SET [Order] = [Order] - 1 WHERE WorkflowID = @0 AND [Order] > @1", stateToDelete.WorkflowID, stateToDelete.Order);
        }
    }

    /// <inheritdoc/>
    public void UpdateWorkflowState(WorkflowState state)
    {
        var workflowState = this.workflowStateRepository.GetWorkflowStateByID(state.StateID);
        if (workflowState == null)
        {
            throw new WorkflowDoesNotExistException();
        }

        this.workflowStateRepository.UpdateWorkflowState(state);
    }

    /// <inheritdoc/>
    public void MoveWorkflowStateDown(int stateId)
    {
        var state = this.workflowStateRepository.GetWorkflowStateByID(stateId);
        if (state == null)
        {
            throw new WorkflowDoesNotExistException();
        }

        var states = this.workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();

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

        this.workflowStateRepository.UpdateWorkflowState(stateToMoveUp);
        this.workflowStateRepository.UpdateWorkflowState(stateToMoveDown);
    }

    /// <inheritdoc/>
    public void MoveWorkflowStateUp(int stateId)
    {
        var state = this.workflowStateRepository.GetWorkflowStateByID(stateId);
        if (state == null)
        {
            throw new WorkflowDoesNotExistException();
        }

        var states = this.workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();

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

        this.workflowStateRepository.UpdateWorkflowState(stateToMoveUp);
        this.workflowStateRepository.UpdateWorkflowState(stateToMoveDown);
    }

    /// <inheritdoc/>
    public void MoveState(int stateId, int index)
    {
        var state = this.workflowStateRepository.GetWorkflowStateByID(stateId);
        if (state == null)
        {
            throw new WorkflowDoesNotExistException();
        }

        var states = this.workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();
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

    /// <inheritdoc/>
    public IEnumerable<WorkflowStatePermission> GetWorkflowStatePermissionByState(int stateId)
    {
        return this.workflowStatePermissionsRepository.GetWorkflowStatePermissionByState(stateId);
    }

    /// <inheritdoc/>
    public void AddWorkflowStatePermission(WorkflowStatePermission permission, int userId)
    {
        permission.WorkflowStatePermissionID = this.workflowStatePermissionsRepository.AddWorkflowStatePermission(permission, userId);
    }

    /// <inheritdoc/>
    public void DeleteWorkflowStatePermission(int workflowStatePermissionId)
    {
        this.workflowStatePermissionsRepository.DeleteWorkflowStatePermission(workflowStatePermissionId);
    }

    /// <inheritdoc/>
    public int GetContentWorkflowStateUsageCount(int stateId)
    {
        return this.dataProvider.GetContentWorkflowStateUsageCount(stateId);
    }

    /// <inheritdoc/>
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
