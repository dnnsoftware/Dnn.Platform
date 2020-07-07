// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using DotNetNuke.Entities.Tabs;

    public interface IPageManagementController
    {
        string GetCreatedInfo(TabInfo tab);

        string GetTabHierarchy(TabInfo tab);

        string GetTabUrl(TabInfo tab);

        /// <summary>Gets a value indicating whether the given <paramref name="tabInfo"/> has children.</summary>
        /// <param name="tabInfo">Tab info object.</param>
        /// <returns>Returns true if tab has children, false otherwise.</returns>
        bool TabHasChildren(TabInfo tabInfo);
    }
}
