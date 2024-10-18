// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions.TabActions
{
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow.Actions;
    using DotNetNuke.Entities.Content.Workflow.Dto;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;

    /// <summary>
    /// Discards the tab workflow, no matter what the current state is. It also sends a system notification to the user
    /// who submitted the workflow to inform them about the discard workflow action.
    /// </summary>
    internal class DiscardWorkflow : TabActionBase, IWorkflowAction
    {
        /// <summary>
        /// Discards the tab version.
        /// </summary>
        /// <param name="stateTransaction">The state transaction.</param>
        public void DoActionOnStateChanging(StateTransaction stateTransaction)
        {
            var tab = GetTab(stateTransaction.ContentItemId);
            if (tab == null)
            {
                return;
            }

            TabVersionBuilder.Instance.Discard(tab.TabID, stateTransaction.UserId);
        }

        /// <summary>
        /// Deletes the tab when necessary.
        /// </summary>
        /// <param name="stateTransaction">The state transaction.</param>
        public void DoActionOnStateChanged(StateTransaction stateTransaction)
        {
            var tab = GetTab(stateTransaction.ContentItemId);
            if (tab == null)
            {
                return;
            }

            if (HasOnlyOneTabVersion(tab))
            {
                TabController.Instance.SoftDeleteTab(tab.TabID, new PortalSettings(tab.PortalID));
                TabController.Instance.DeleteTab(tab.TabID, tab.PortalID);
            }

            DataCache.RemoveCache($"Tab_Tabs{tab.PortalID}");
        }

        /// <inheritdoc />
        public ActionMessage GetActionMessage(StateTransaction stateTransaction, WorkflowState currentState)
            => new ()
            {
                Subject = GetString($"{nameof(DiscardWorkflow)}{nameof(ActionMessage.Subject)}", "Page Discarded"),
                Body = GetString($"{nameof(DiscardWorkflow)}{nameof(ActionMessage.Body)}", "Edits for page '{0}' have been discarded.", GetTab(stateTransaction.ContentItemId).LocalizedTabName),
            };

        /// <summary>
        /// Checks if the tab has only one version.
        /// </summary>
        /// <param name="tabInfo">The tab information.</param>
        /// <returns>True if the tab has only one version; otherwise, false.</returns>
        private static bool HasOnlyOneTabVersion(TabInfo tabInfo)
        {
            var tabVersions = TabVersionController.Instance.GetTabVersions(tabInfo.TabID, true);
            return tabVersions == null || !tabVersions.Any();
        }
    }
}
