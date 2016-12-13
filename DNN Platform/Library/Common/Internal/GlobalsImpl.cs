#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Text;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI.UserControls;

namespace DotNetNuke.Common.Internal
{
    public class GlobalsImpl : IGlobals
    {
        public string ApplicationPath
        {
            get { return Globals.ApplicationPath; }
        }

        public string HostMapPath
        {
            get { return Globals.HostMapPath; }
        }

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
            return Globals.LinkClick(link, tabID, moduleID, trackClicks, forceDownload, portalId, enableUrlLanguage,portalGuid);
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
            return GetDomainName(requestedUri, false);
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
            if (!String.IsNullOrEmpty(hostHeader))
            {
                uri = uri.ToLower().Replace(hostHeader.ToLower(), "");
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
                switch (url[queryIndex].ToLower())
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
                        if ((url[queryIndex].Length >= ".aspx".Length))
                        {
                            if (url[queryIndex].ToLower().LastIndexOf(".aspx", StringComparison.Ordinal) == (url[queryIndex].Length - (".aspx".Length)) ||
                                url[queryIndex].ToLower().LastIndexOf(".axd", StringComparison.Ordinal) == (url[queryIndex].Length - (".axd".Length)) ||
                                url[queryIndex].ToLower().LastIndexOf(".ashx", StringComparison.Ordinal) == (url[queryIndex].Length - (".ashx".Length)))
                            {
                                break;
                            }
                        }
                        // non of the exclusionary names found
                        domainName.Append((!String.IsNullOrEmpty(domainName.ToString()) ? "/" : "") + url[queryIndex]);
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
                        domainName = domainName.Replace(":" + requestedUri.Port, "");
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
            return Globals.NavigateURL();
        }

        public string NavigateURL(int tabID)
        {
            return Globals.NavigateURL(tabID);
        }

        public string NavigateURL(int tabID, bool isSuperTab)
        {
            return Globals.NavigateURL(tabID, isSuperTab);
        }

        public string NavigateURL(string controlKey)
        {
            return Globals.NavigateURL(controlKey);
        }

        public string NavigateURL(string controlKey, params string[] additionalParameters)
        {
            return Globals.NavigateURL(controlKey, additionalParameters);
        }

        public string NavigateURL(int tabID, string controlKey)
        {
            return Globals.NavigateURL(tabID, controlKey);
        }

        public string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        {
            return Globals.NavigateURL(tabID, controlKey, additionalParameters);
        }

        public string NavigateURL(int tabID, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return Globals.NavigateURL(tabID, settings, controlKey, additionalParameters);
        }

        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return Globals.NavigateURL(tabID, isSuperTab, settings, controlKey, additionalParameters);
        }

        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        {
            return Globals.NavigateURL(tabID, isSuperTab, settings, controlKey, language, additionalParameters);
        }

        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        {
            return Globals.NavigateURL(tabID, isSuperTab, settings, controlKey, language, pageName, additionalParameters);
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