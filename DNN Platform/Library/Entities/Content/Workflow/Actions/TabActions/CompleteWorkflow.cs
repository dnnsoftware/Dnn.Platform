// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions.TabActions
{
    using DotNetNuke.Entities.Content.Workflow.Actions;
    using DotNetNuke.Entities.Content.Workflow.Dto;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Tabs.TabVersions;

    /// <summary>
    /// Completes the workflow, no matter what the current state is.
    /// It also sends a system notification to the user who submitted the workflow to
    /// inform them about the complete workflow action.
    /// </summary>
    internal class CompleteWorkflow : TabActionBase, IWorkflowAction
    {
        /// <summary>
        /// Publishes the tab version.
        /// </summary>
        /// <param name="stateTransaction">The state transaction.</param>
        public void DoActionOnStateChanging(StateTransaction stateTransaction)
        {
            var tab = GetTab(stateTransaction.ContentItemId);
            if (tab == null)
            {
                return;
            }

            TabVersionBuilder.Instance.Publish(tab.PortalID, tab.TabID, stateTransaction.UserId);
        }

        /// <inheritdoc />
        public void DoActionOnStateChanged(StateTransaction stateTransaction)
            => RemoveCache(stateTransaction);

        /// <inheritdoc />
        public ActionMessage GetActionMessage(StateTransaction stateTransaction, WorkflowState currentState)
            => new ()
            {
                Subject = GetString($"{nameof(CompleteWorkflow)}{nameof(ActionMessage.Subject)}", "Page Published"),
                Body = GetString($"{nameof(CompleteWorkflow)}{nameof(ActionMessage.Body)}", "Page '{0}' has been published and is now live.", GetTab(stateTransaction.ContentItemId).LocalizedTabName),
            };
    }
}
