// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using DotNetNuke.Entities.Tabs.Dto;

    /// <summary>
    /// Class responsible to provide settings for Tab Changes.
    /// </summary>
    public interface ITabChangeSettings
    {
        /// <summary>
        /// Get the status of the tab changes control in a specific tab and its master portal.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="tabId">Tab Id.</param>
        /// <returns>Returns true if changes control is available for both Portal and Tab, false otherwise.</returns>
        bool IsChangeControlEnabled(int portalId, int tabId);

        /// <summary>
        /// Get the full state of the tab changes control in a specific tab and its master portal.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="tabId">Tab Id.</param>
        /// <returns></returns>
        ChangeControlState GetChangeControlState(int portalId, int tabId);
    }
}
