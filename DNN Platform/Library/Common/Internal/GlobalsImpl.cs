// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Internal
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A collection of Dnn global methods and properties.</summary>
    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public class GlobalsImpl : IGlobals
    {
        /// <summary>Initializes a new instance of the <see cref="GlobalsImpl"/> class.</summary>
        public GlobalsImpl()
        {
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
        }

        /// <inheritdoc/>
        public string ApplicationPath
        {
            get { return Globals.ApplicationPath; }
        }

        /// <inheritdoc/>
        public string HostMapPath
        {
            get { return Globals.HostMapPath; }
        }

        /// <summary>Gets a navigation manager to provide navigation services.</summary>
        protected INavigationManager NavigationManager { get; }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public string GetSubFolderPath(string strFileNamePath, int portalId)
        {
            return Globals.GetSubFolderPath(strFileNamePath, portalId);
        }

        /// <inheritdoc/>
        public string LinkClick(string link, int tabId, int moduleId)
        {
            return Globals.LinkClick(link, tabId, moduleId);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public string LinkClick(string link, int tabID, int moduleID, bool trackClicks, bool forceDownload, int portalId, bool enableUrlLanguage, string portalGuid)
        {
            return Globals.LinkClick(link, tabID, moduleID, trackClicks, forceDownload, portalId, enableUrlLanguage, portalGuid);
        }

        /// <inheritdoc/>
        public string ResolveUrl(string url)
        {
            return Globals.ResolveUrl(url);
        }

        /// <inheritdoc/>
        public bool IsHostTab(int tabId)
        {
            return Globals.IsHostTab(tabId);
        }

        /// <inheritdoc/>
        public string AddHTTP(string strURL)
        {
            return Globals.AddHTTP(strURL);
        }

        /// <inheritdoc/>
        public string GetPortalDomainName(string strPortalAlias, HttpRequest request, bool blnAddHTTP)
        {
            return Globals.GetPortalDomainName(strPortalAlias, request, blnAddHTTP);
        }

        /// <inheritdoc/>
        public string GetDomainName(Uri requestedUri)
        {
            return this.GetDomainName(requestedUri, false);
        }

        /// <inheritdoc/>
        public string GetDomainName(Uri requestedUri, bool parsePortNumber)
        {
            var domainName = new StringBuilder();

            // split both URL separater, and parameter separator
            // We trim right of '?' so test for filename extensions can occur at END of URL-componenet.
            // Test:   'www.aspxforum.net'  should be returned as a valid domain name.
            // just consider left of '?' in URI
            // Binary, else '?' isn't taken literally; only interested in one (left) string
            string uri = requestedUri.ToString();
            string hostHeader = Config.GetSetting("HostHeader");
            if (!string.IsNullOrEmpty(hostHeader))
            {
                uri = uri.ToLowerInvariant().Replace(hostHeader.ToLowerInvariant(), string.Empty);
            }

            int queryIndex = uri.IndexOf("?", StringComparison.Ordinal);
            if (queryIndex > -1)
            {
                uri = uri.Substring(0, queryIndex);
            }

            string[] url = uri.Split('/');
            for (queryIndex = 2; queryIndex <= url.GetUpperBound(0); queryIndex++)
            {
                bool needExit = false;
                switch (url[queryIndex].ToLowerInvariant())
                {
                    case "":
                        continue;
                    case "admin":
                    case "controls":
                    case "desktopmodules":
                    case "mobilemodules":
                    case "premiummodules":
                    case "providers":
                    case "api":
                        needExit = true;
                        break;
                    default:
                        // exclude filenames ENDing in ".aspx" or ".axd" ---
                        //   we'll use reverse match,
                        //   - but that means we are checking position of left end of the match;
                        //   - and to do that, we need to ensure the string we test against is long enough;
                        if (url[queryIndex].Length >= ".aspx".Length)
                        {
                            if (url[queryIndex].LastIndexOf(".aspx", StringComparison.OrdinalIgnoreCase) == (url[queryIndex].Length - ".aspx".Length) ||
                                url[queryIndex].LastIndexOf(".axd", StringComparison.OrdinalIgnoreCase) == (url[queryIndex].Length - ".axd".Length) ||
                                url[queryIndex].LastIndexOf(".ashx", StringComparison.OrdinalIgnoreCase) == (url[queryIndex].Length - ".ashx".Length))
                            {
                                break;
                            }
                        }

                        // non of the exclusionary names found
                        domainName.Append((!string.IsNullOrEmpty(domainName.ToString()) ? "/" : string.Empty) + url[queryIndex]);
                        break;
                }

                if (needExit)
                {
                    break;
                }
            }

            if (parsePortNumber)
            {
                if (domainName.ToString().IndexOf(":", StringComparison.Ordinal) != -1)
                {
                    if (!Globals.UsePortNumber())
                    {
                        domainName = domainName.Replace(":" + requestedUri.Port, string.Empty);
                    }
                }
            }

            return domainName.ToString();
        }

        /// <inheritdoc/>
        public string FormatHelpUrl(string helpUrl, PortalSettings objPortalSettings, string name)
        {
            return Globals.FormatHelpUrl(helpUrl, objPortalSettings, name);
        }

        /// <inheritdoc/>
        public string FormatHelpUrl(string helpUrl, PortalSettings objPortalSettings, string name, string version)
        {
            return Globals.FormatHelpUrl(helpUrl, objPortalSettings, name, version);
        }

        /// <inheritdoc/>
        public string AccessDeniedURL()
        {
            return Globals.AccessDeniedURL();
        }

        /// <inheritdoc/>
        public string AccessDeniedURL(string message)
        {
            return Globals.AccessDeniedURL(message);
        }

        /// <inheritdoc/>
        public string LoginURL(string returnURL, bool @override)
        {
            return Globals.LoginURL(returnURL, @override);
        }

        /// <inheritdoc/>
        public string NavigateURL()
        {
            return this.NavigationManager.NavigateURL();
        }

        /// <inheritdoc/>
        public string NavigateURL(int tabID)
        {
            return this.NavigationManager.NavigateURL(tabID);
        }

        /// <inheritdoc/>
        public string NavigateURL(int tabID, bool isSuperTab)
        {
            return this.NavigationManager.NavigateURL(tabID, isSuperTab);
        }

        /// <inheritdoc/>
        public string NavigateURL(string controlKey)
        {
            return this.NavigationManager.NavigateURL(controlKey);
        }

        /// <inheritdoc/>
        public string NavigateURL(string controlKey, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(controlKey, additionalParameters);
        }

        /// <inheritdoc/>
        public string NavigateURL(int tabID, string controlKey)
        {
            return this.NavigationManager.NavigateURL(tabID, controlKey);
        }

        /// <inheritdoc/>
        public string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, controlKey, additionalParameters);
        }

        /// <inheritdoc/>
        public string NavigateURL(int tabID, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, settings, controlKey, additionalParameters);
        }

        /// <inheritdoc/>
        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, additionalParameters);
        }

        /// <inheritdoc/>
        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, language, additionalParameters);
        }

        /// <inheritdoc/>
        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, language, pageName, additionalParameters);
        }

        /// <inheritdoc/>
        public string FriendlyUrl(TabInfo tab, string path)
        {
            return Globals.FriendlyUrl(tab, path);
        }

        /// <inheritdoc/>
        public string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
            return Globals.FriendlyUrl(tab, path, pageName);
        }

        /// <inheritdoc/>
        public string FriendlyUrl(TabInfo tab, string path, PortalSettings settings)
        {
            return Globals.FriendlyUrl(tab, path, (IPortalSettings)settings);
        }

        /// <inheritdoc/>
        public string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        {
            return Globals.FriendlyUrl(tab, path, pageName, (IPortalSettings)settings);
        }

        /// <inheritdoc/>
        public string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
            return Globals.FriendlyUrl(tab, path, pageName, portalAlias);
        }
    }
}
