﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Content.Workflow.Dto;
using DotNetNuke.Entities.Content.Workflow.Entities;

namespace DotNetNuke.Entities.Content.Workflow.Actions
{
    /// <summary>
    /// This interface represents a point of extension that third party can implement to inject behavior on workflow action inside the Workflow Engine.
    /// i.e.: Discard State, Complete State, Discard Workflow, Complete Workflow
    /// Third party can implement the interface for each one of the 4 actions and then register it using the <see cref="IWorkflowActionManager"/>
    /// </summary>
    public interface IWorkflowAction
    {
        /// <summary>
        /// The method gets the action message that will be sent to notify the users depending on the type of action
        /// i.e.: with action type Complete Workflow the action message represents the notification sent to the user that started the workflow
        /// </summary>
        /// <param name="stateTransaction">State transaction dto</param>
        /// <param name="currentState">Workflow state that represent the current state after the workflow action</param>
        /// <returns>Action message that the engine will use to send the notification</returns>
        ActionMessage GetActionMessage(StateTransaction stateTransaction, WorkflowState currentState);

        /// <summary>
        /// This method implements some action on state changed.
        /// i.e.: on Complete Workflow user can implement clear a cache or log some info
        /// </summary>
        /// <param name="stateTransaction">State transaction dto</param>
        void DoActionOnStateChanged(StateTransaction stateTransaction);

        /// <summary>
        /// This method implements some action on state changed.
        /// i.e.: on Complete Workflow user can implement the publish of a pending content version.
        /// </summary>
        /// <param name="stateTransaction">State transaction dto</param>
        void DoActionOnStateChanging(StateTransaction stateTransaction);
    }
}
