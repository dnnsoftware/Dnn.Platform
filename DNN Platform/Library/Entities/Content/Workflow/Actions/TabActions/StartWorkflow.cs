// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions.TabActions
{
    using DotNetNuke.Entities.Content.Workflow.Actions;
    using DotNetNuke.Entities.Content.Workflow.Dto;
    using DotNetNuke.Entities.Content.Workflow.Entities;

    /// <summary>
    /// Starts a tab workflow.
    /// </summary>
    internal class StartWorkflow : TabActionBase, IWorkflowAction
    {
        /// <inheritdoc />
        public void DoActionOnStateChanging(StateTransaction stateTransaction)
        {
            // nothing
        }

        /// <inheritdoc />
        public void DoActionOnStateChanged(StateTransaction stateTransaction)
            => RemoveCache(stateTransaction);

        /// <inheritdoc />
        public ActionMessage GetActionMessage(StateTransaction stateTransaction, WorkflowState currentState)
            => new ()
            {
                Subject = GetString($"{nameof(StartWorkflow)}{nameof(ActionMessage.Subject)}", "Page Submitted"),
                Body = GetString($"{nameof(StartWorkflow)}{nameof(ActionMessage.Body)}", "Page '{0}' is in the '{1}' state and waiting to be submitted.", GetTab(stateTransaction.ContentItemId).LocalizedTabName, currentState.StateName),
            };
    }
}
