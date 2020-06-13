// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Internal
{
    using System;
    using System.Web;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;

    public interface IGlobals
    {
        /// <summary>
        /// Gets the application path.
        /// </summary>
        string ApplicationPath { get; }

        /// <summary>
        /// Gets the host map path.
        /// </summary>
        /// <value>ApplicationMapPath + "Portals\_default\".</value>
        string HostMapPath { get; }

        /// <summary>
        /// Returns the folder path under the root for the portal.
        /// </summary>
        /// <param name="fileNamePath">The folder the absolute path.</param>
        /// <param name="portalId">Portal Id.</param>
        /// <returns></returns>
        string GetSubFolderPath(string fileNamePath, int portalId);

        /// <summary>
        /// Gets Link click url.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="moduleId">The module ID.</param>
        /// <returns>Formatted url.</returns>
        string LinkClick(string link, int tabId, int moduleId);

        /// <summary>
        /// Gets Link click url.
        /// </summary>
        /// <param name="Link">The link.</param>
        /// <param name="TabID">The Tab ID.</param>
        /// <param name="ModuleID">The Module ID.</param>
        /// <param name="TrackClicks">Check whether it has to track clicks.</param>
        /// <param name="ForceDownload">Check whether it has to force the download.</param>
        /// <param name="PortalId">The Portal ID.</param>
        /// <param name="EnableUrlLanguage">Check whether the portal has enabled  ulr languages.</param>
        /// <param name="portalGuid">The Portal GUID.</param>
        /// <returns></returns>
        string LinkClick(string Link, int TabID, int ModuleID, bool TrackClicks, bool ForceDownload, int PortalId,
                         bool EnableUrlLanguage, string portalGuid);

        /// <summary>
        /// Generates the correctly formatted url.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="url">The url to format.</param>
        /// <returns>The formatted (resolved) url.</returns>
        string ResolveUrl(string url);

        /// <summary>
        /// Check whether the specific page is a host page.
        /// </summary>
        /// <param name="tabId">The tab ID.</param>
        /// <returns>if <c>true</c> the tab is a host page; otherwise, it is not a host page.</returns>
        bool IsHostTab(int tabId);

        /// <summary>
        /// Adds the current request's protocol (<c>"http://"</c> or <c>"https://"</c>) to the given URL, if it does not already have a protocol specified.
        /// </summary>
        /// <param name="strURL">The URL.</param>
        /// <returns>The formatted URL.</returns>
        string AddHTTP(string strURL);

        /// <summary>
        /// Gets the portal domain name.
        /// </summary>
        /// <param name="strPortalAlias">The portal alias.</param>
        /// <param name="Request">The request or <c>null</c>.</param>
        /// <param name="blnAddHTTP">if set to <c>true</c> calls <see cref="AddHTTP"/> on the result.</param>
        /// <returns>domain name.</returns>
        string GetPortalDomainName(string strPortalAlias, HttpRequest Request, bool blnAddHTTP);

        /// <summary>
        /// Gets the name of the domain.
        /// </summary>
        /// <param name="requestedUri">The requested Uri.</param>
        /// <returns>domain name.</returns>
        string GetDomainName(Uri requestedUri);

        /// <summary>
        /// returns the domain name of the current request ( ie. www.domain.com or 207.132.12.123 or www.domain.com/directory if subhost ).
        /// </summary>
        /// <param name="requestedUri">The requested Uri.</param>
        /// <param name="parsePortNumber">if set to <c>true</c> [parse port number].</param>
        /// <returns>domain name.</returns>
        string GetDomainName(Uri requestedUri, bool parsePortNumber);

        /// <summary>
        /// Formats the help URL, adding query-string parameters and a protocol (if missing).
        /// </summary>
        /// <param name="HelpUrl">The help URL.</param>
        /// <param name="objPortalSettings">The portal settings.</param>
        /// <param name="Name">The name of the module.</param>
        /// <returns>Formatted URL.</returns>
        string FormatHelpUrl(string HelpUrl, PortalSettings objPortalSettings, string Name);

        /// <summary>
        /// Formats the help URL, adding query-string parameters and a protocol (if missing).
        /// </summary>
        /// <param name="HelpUrl">The help URL.</param>
        /// <param name="objPortalSettings">The portal settings.</param>
        /// <param name="Name">The name of the module.</param>
        /// <param name="Version">The version of the module.</param>
        /// <returns>Formatted URL.</returns>
        string FormatHelpUrl(string HelpUrl, PortalSettings objPortalSettings, string Name, string Version);

        /// <summary>
        /// Get the URL to show the "access denied" message.
        /// </summary>
        /// <returns>URL to access denied view.</returns>
        string AccessDeniedURL();

        /// <summary>
        /// Get the URL to show the "access denied" message.
        /// </summary>
        /// <param name="Message">The message to display.</param>
        /// <returns>URL to access denied view.</returns>
        string AccessDeniedURL(string Message);

        /// <summary>
        /// Gets the login URL.
        /// </summary>
        /// <param name="returnURL">The URL to redirect to after logging in.</param>
        /// <param name="override">if set to <c>true</c>, show the login control on the current page, even if there is a login page defined for the site.</param>
        /// <returns>Formatted URL.</returns>
        string LoginURL(string returnURL, bool @override);

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
        string NavigateURL(int tabID, PortalSettings settings, string controlKey, params string[] additionalParameters);

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, params string[] additionalParameters);

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
        string NavigateURL(
            int tabID,
            bool isSuperTab,
            PortalSettings settings,
            string controlKey,
            string language,
            params string[] additionalParameters);

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
        string NavigateURL(
            int tabID,
            bool isSuperTab,
            PortalSettings settings,
            string controlKey,
            string language,
            string pageName,
            params string[] additionalParameters);

        /// <summary>
        /// Generates the correctly formatted friendly URL.
        /// </summary>
        /// <remarks>
        /// Assumes Default.aspx, and that portalsettings are saved to Context.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <returns>The formatted (friendly) URL.</returns>
        string FriendlyUrl(TabInfo tab, string path);

        /// <summary>
        /// Generates the correctly formatted friendly URL.
        /// </summary>
        /// <remarks>
        /// This overload includes an optional page to include in the url.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page to include in the url.</param>
        /// <returns>The formatted (friendly) URL.</returns>
        string FriendlyUrl(TabInfo tab, string path, string pageName);

        /// <summary>
        /// Generates the correctly formatted friendly URL.
        /// </summary>
        /// <remarks>
        /// This overload includes the portal settings for the site.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="settings">The portal settings.</param>
        /// <returns>The formatted (friendly) URL.</returns>
        string FriendlyUrl(TabInfo tab, string path, PortalSettings settings);

        /// <summary>
        /// Generates the correctly formatted friendly URL.
        /// </summary>
        /// <remarks>
        /// This overload includes an optional page to include in the URL, and the portal
        /// settings for the site.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page to include in the URL.</param>
        /// <param name="settings">The portal settings.</param>
        /// <returns>The formatted (friendly) url.</returns>
        string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings);

        /// <summary>
        /// Generates the correctly formatted friendly url.
        /// </summary>
        /// <remarks>
        /// This overload includes an optional page to include in the url, and the portal
        /// alias for the site.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page to include in the URL.</param>
        /// <param name="portalAlias">The portal alias for the site.</param>
        /// <returns>The formatted (friendly) URL.</returns>
        string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias);
    }
}
