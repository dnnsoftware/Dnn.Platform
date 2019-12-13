// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Tabs.Dto;

namespace DotNetNuke.Entities.Tabs
{
    /// <summary>
    /// Class responsible to provide settings for Tab Changes
    /// </summary>
    public interface ITabChangeSettings
    {
        /// <summary>
        /// Get the status of the tab changes control in a specific tab and its master portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="tabId">Tab Id</param>
        /// <returns>Returns true if changes control is available for both Portal and Tab, false otherwise</returns>
        bool IsChangeControlEnabled(int portalId, int tabId);

        /// <summary>
        /// Get the full state of the tab changes control in a specific tab and its master portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="tabId">Tab Id</param>
        ChangeControlState GetChangeControlState(int portalId, int tabId);
    }
}
