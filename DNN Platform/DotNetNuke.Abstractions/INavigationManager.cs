// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions
{
    using DotNetNuke.Abstractions.Portals;

    public interface INavigationManager
    {
        /// <summary>
        /// Gets the URL to the current page.
        /// </summary>
        /// <returns>Formatted URL.</returns>
        string NavigateURL();

        /// <summary>
        /// Gets the URL to the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <returns>Formatted URL.</returns>
        string NavigateURL(int tabID);

        /// <summary>
        /// Gets the URL to the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> the page is a "super-tab," i.e. a host-level page.</param>
        /// <returns>Formatted URL.</returns>
        string NavigateURL(int tabID, bool isSuperTab);

        /// <summary>
        /// Gets the URL to show the control associated with the given control key.
        /// </summary>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <returns>Formatted URL.</returns>
        string NavigateURL(string controlKey);

        /// <summary>
        /// Gets the URL to show the control associated with the given control key.
        /// </summary>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters, in <c>"key=value"</c> format.</param>
        /// <returns>Formatted URL.</returns>
        string NavigateURL(string controlKey, params string[] additionalParameters);

        /// <summary>
        /// Gets the URL to show the control associated with the given control key on the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <returns>Formatted URL.</returns>
        string NavigateURL(int tabID, string controlKey);

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        string NavigateURL(int tabID, string controlKey, params string[] additionalParameters);

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        string NavigateURL(int tabID, IPortalSettings settings, string controlKey, params string[] additionalParameters);

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, params string[] additionalParameters);

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="language">The language code.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, string language, params string[] additionalParameters);

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="language">The language code.</param>
        /// <param name="pageName">The page name to pass to <see cref="FriendlyUrl(DotNetNuke.Entities.Tabs.TabInfo,string,string)"/>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted url.</returns>
        string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters);
    }
}
