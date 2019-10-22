using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using System;
using System.Linq;
using System.Threading;

namespace DotNetNuke.Common
{
    internal class NavigationManager : INavigationManager
    {
        private readonly IPortalController _portalController;
        public NavigationManager(IPortalController portalController)
        {
            _portalController = portalController;
        }

        /// <summary>
        /// Gets the URL to the current page.
        /// </summary>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL()
        {
            PortalSettings portalSettings = _portalController.GetCurrentPortalSettings();
            return NavigateURL(portalSettings.ActiveTab.TabID, Null.NullString);
        }

        /// <summary>
        /// Gets the URL to the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID)
        {
            return NavigateURL(tabID, Null.NullString);
        }

        /// <summary>
        /// Gets the URL to the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> the page is a "super-tab," i.e. a host-level page.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID, bool isSuperTab)
        {
            IPortalSettings _portalSettings = _portalController.GetCurrentSettings();
            string cultureCode = Globals.GetCultureCode(tabID, isSuperTab, _portalSettings);
            return NavigateURL(tabID, isSuperTab, _portalSettings, Null.NullString, cultureCode);
        }

        /// <summary>
        /// Gets the URL to show the control associated with the given control key.
        /// </summary>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(string controlKey)
        {
            if (controlKey == "Access Denied")
            {
                return Globals.AccessDeniedURL();
            }
            else
            {
                PortalSettings _portalSettings = _portalController.GetCurrentPortalSettings();
                return NavigateURL(_portalSettings.ActiveTab.TabID, controlKey);
            }
        }

        /// <summary>
        /// Gets the URL to show the control associated with the given control key.
        /// </summary>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters, in <c>"key=value"</c> format.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(string controlKey, params string[] additionalParameters)
        {
            PortalSettings _portalSettings = _portalController.GetCurrentPortalSettings();
            return NavigateURL(_portalSettings?.ActiveTab?.TabID ?? -1, controlKey, additionalParameters);
        }

        /// <summary>
        /// Gets the URL to show the control associated with the given control key on the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID, string controlKey)
        {
            PortalSettings _portalSettings = _portalController.GetCurrentPortalSettings();
            return NavigateURL(tabID, _portalSettings, controlKey, null);
        }

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        {
            PortalSettings _portalSettings = _portalController.GetCurrentPortalSettings();
            return NavigateURL(tabID, _portalSettings, controlKey, additionalParameters);
        }

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID, IPortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            bool isSuperTab = Globals.IsHostTab(tabID);

            return NavigateURL(tabID, isSuperTab, settings, controlKey, additionalParameters);
        }

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            string cultureCode = Globals.GetCultureCode(tabID, isSuperTab, settings);
            return NavigateURL(tabID, isSuperTab, settings, controlKey, cultureCode, additionalParameters);
        }

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
        public string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        {
            return NavigateURL(tabID, isSuperTab, settings, controlKey, language, Globals.glbDefaultPage, additionalParameters);
        }

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
        public string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        {
            string url = tabID == Null.NullInteger ? Globals.ApplicationURL() : Globals.ApplicationURL(tabID);
            if (!String.IsNullOrEmpty(controlKey))
            {
                url += "&ctl=" + controlKey;
            }
            if (additionalParameters != null)
            {
                url = additionalParameters.Where(parameter => !string.IsNullOrEmpty(parameter)).Aggregate(url, (current, parameter) => current + ("&" + parameter));
            }
            if (isSuperTab)
            {
                url += "&portalid=" + settings.PortalId;
            }

            TabInfo tab = null;

            if (settings != null)
            {
                tab = TabController.Instance.GetTab(tabID, isSuperTab ? Null.NullInteger : settings.PortalId, false);
            }

            //only add language to url if more than one locale is enabled
            if (settings != null && language != null && LocaleController.Instance.GetLocales(settings.PortalId).Count > 1)
            {
                if (settings.ContentLocalizationEnabled)
                {
                    if (language == "")
                    {
                        if (tab != null && !string.IsNullOrEmpty(tab.CultureCode))
                        {
                            url += "&language=" + tab.CultureCode;
                        }
                    }
                    else
                    {
                        url += "&language=" + language;
                    }
                }
                else if (settings.EnableUrlLanguage)
                {
                    //legacy pre 5.5 behavior
                    if (language == "")
                    {
                        url += "&language=" + Thread.CurrentThread.CurrentCulture.Name;
                    }
                    else
                    {
                        url += "&language=" + language;
                    }
                }
            }

            if (Host.UseFriendlyUrls || Config.GetFriendlyUrlProvider() == "advanced")
            {
                if (String.IsNullOrEmpty(pageName))
                {
                    pageName = Globals.glbDefaultPage;
                }

                url = (settings == null) ? Globals.FriendlyUrl(tab, url, pageName) : Globals.FriendlyUrl(tab, url, pageName, settings);
            }
            else
            {
                url = Globals.ResolveUrl(url);
            }

            return url;
        }
    }
}
