// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Internal
{
    using System;
    using System.Text;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.UI.UserControls;
    using Microsoft.Extensions.DependencyInjection;

    public class GlobalsImpl : IGlobals
    {
        public GlobalsImpl()
        {
            this.NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string ApplicationPath
        {
            get { return Globals.ApplicationPath; }
        }

        public string HostMapPath
        {
            get { return Globals.HostMapPath; }
        }

        protected INavigationManager NavigationManager { get; }

        public string GetSubFolderPath(string strFileNamePath, int portalId)
        {
            return Globals.GetSubFolderPath(strFileNamePath, portalId);
        }

        public string LinkClick(string link, int tabId, int moduleId)
        {
            return Globals.LinkClick(link, tabId, moduleId);
        }

        public string LinkClick(string link, int tabID, int moduleID, bool trackClicks, bool forceDownload, int portalId, bool enableUrlLanguage, string portalGuid)
        {
            return Globals.LinkClick(link, tabID, moduleID, trackClicks, forceDownload, portalId, enableUrlLanguage, portalGuid);
        }

        public string ResolveUrl(string url)
        {
            return Globals.ResolveUrl(url);
        }

        public bool IsHostTab(int tabId)
        {
            return Globals.IsHostTab(tabId);
        }

        public string AddHTTP(string strURL)
        {
            return Globals.AddHTTP(strURL);
        }

        public string GetPortalDomainName(string strPortalAlias, HttpRequest Request, bool blnAddHTTP)
        {
            return Globals.GetPortalDomainName(strPortalAlias, Request, blnAddHTTP);
        }

        public string GetDomainName(Uri requestedUri)
        {
            return this.GetDomainName(requestedUri, false);
        }

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

        public string FormatHelpUrl(string HelpUrl, PortalSettings objPortalSettings, string Name)
        {
            return Globals.FormatHelpUrl(HelpUrl, objPortalSettings, Name);
        }

        public string FormatHelpUrl(string HelpUrl, PortalSettings objPortalSettings, string Name, string Version)
        {
            return Globals.FormatHelpUrl(HelpUrl, objPortalSettings, Name, Version);
        }

        public string AccessDeniedURL()
        {
            return Globals.AccessDeniedURL();
        }

        public string AccessDeniedURL(string Message)
        {
            return Globals.AccessDeniedURL(Message);
        }

        public string LoginURL(string returnURL, bool @override)
        {
            return Globals.LoginURL(returnURL, @override);
        }

        public string NavigateURL()
        {
            return this.NavigationManager.NavigateURL();
        }

        public string NavigateURL(int tabID)
        {
            return this.NavigationManager.NavigateURL(tabID);
        }

        public string NavigateURL(int tabID, bool isSuperTab)
        {
            return this.NavigationManager.NavigateURL(tabID, isSuperTab);
        }

        public string NavigateURL(string controlKey)
        {
            return this.NavigationManager.NavigateURL(controlKey);
        }

        public string NavigateURL(string controlKey, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(controlKey, additionalParameters);
        }

        public string NavigateURL(int tabID, string controlKey)
        {
            return this.NavigationManager.NavigateURL(tabID, controlKey);
        }

        public string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, controlKey, additionalParameters);
        }

        public string NavigateURL(int tabID, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, settings, controlKey, additionalParameters);
        }

        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, additionalParameters);
        }

        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, language, additionalParameters);
        }

        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, language, pageName, additionalParameters);
        }

        public string FriendlyUrl(TabInfo tab, string path)
        {
            return Globals.FriendlyUrl(tab, path);
        }

        public string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
            return Globals.FriendlyUrl(tab, path, pageName);
        }

        public string FriendlyUrl(TabInfo tab, string path, PortalSettings settings)
        {
            return Globals.FriendlyUrl(tab, path, settings);
        }

        public string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        {
            return Globals.FriendlyUrl(tab, path, pageName, settings);
        }

        public string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
            return Globals.FriendlyUrl(tab, path, pageName, portalAlias);
        }
    }
}
