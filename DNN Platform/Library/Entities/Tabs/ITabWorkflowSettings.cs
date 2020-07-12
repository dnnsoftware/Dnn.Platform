// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    /// <summary>
    /// This interface is responsible to provide the tab workflow settings.
    /// </summary>
    public interface ITabWorkflowSettings
    {
        /// <summary>
        /// This method returns the default tab workflow of the portal.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <remarks>If no default workflow is defined for a portal the method returns the Direct Publish system workflow.</remarks>
        /// <returns>The workflow Id of the portal default workflow.</returns>
        int GetDefaultTabWorkflowId(int portalId);

        /// <summary>
        /// This method sets the default workflow for a portal.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="workflowId">Workflow Id.</param>
        void SetDefaultTabWorkflowId(int portalId, int workflowId);

        /// <summary>
        /// This method enables or disables the tab workflow for the entire portal.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="enabled">true for enable it, false for disable it.</param>
        void SetWorkflowEnabled(int portalId, bool enabled);

        /// <summary>
        /// This method enables or disables the tab workflow for a specific tab.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="tabId">Tab Id.</param>
        /// <param name="enabled">true for enable it, false for disable it.</param>
        /// <remarks>this won't enable workflow of a tab if the tab workflow is disabled at portal level.</remarks>
        void SetWorkflowEnabled(int portalId, int tabId, bool enabled);

        /// <summary>
        /// The method returns true if the workflow is enabled for a tab.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="tabId">Tab Id.</param>
        /// <returns>True if the workflow is enabled, false otherwise.</returns>
        bool IsWorkflowEnabled(int portalId, int tabId);

        /// <summary>
        /// The method returns true is the workflow is enabled at portal level.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <returns>True if the workflow is enabled, false otherwise.</returns>
        bool IsWorkflowEnabled(int portalId);
    }
}
