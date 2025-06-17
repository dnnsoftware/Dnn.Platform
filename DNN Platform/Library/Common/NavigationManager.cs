// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common
{
    using System.Linq;
    using System.Threading;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The default <see cref="INavigationManager"/> implementation.</summary>
    internal class NavigationManager : INavigationManager
    {
        private readonly IPortalController portalController;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="NavigationManager"/> class.</summary>
        /// <param name="portalController">An <see cref="IPortalController"/> instance.</param>
        public NavigationManager(IPortalController portalController)
            : this(portalController, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NavigationManager"/> class.</summary>
        /// <param name="portalController">An <see cref="IPortalController"/> instance.</param>
        /// <param name="hostSettings">An <see cref="IHostSettings"/> instance.</param>
        public NavigationManager(IPortalController portalController, IHostSettings hostSettings)
        {
            this.portalController = portalController;
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        /// <summary>Gets the URL to the current page.</summary>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL()
        {
            return this.NavigateURL(TabController.CurrentPage.TabID, Null.NullString);
        }

        /// <summary>Gets the URL to the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID)
        {
            return this.NavigateURL(tabID, Null.NullString);
        }

        /// <summary>Gets the URL to the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <see langword="true"/> the page is a "super-tab," i.e. a host-level page.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID, bool isSuperTab)
        {
            var portalSettings = this.portalController.GetCurrentSettings();
            var cultureCode = Globals.GetCultureCode(tabID, isSuperTab, portalSettings);
            return this.NavigateURL(tabID, isSuperTab, portalSettings, Null.NullString, cultureCode);
        }

        /// <summary>Gets the URL to show the control associated with the given control key.</summary>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(string controlKey)
        {
            if (controlKey == "Access Denied")
            {
                return Globals.AccessDeniedURL();
            }

            return this.NavigateURL(TabController.CurrentPage.TabID, controlKey);
        }

        /// <summary>Gets the URL to show the control associated with the given control key.</summary>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters, in <c>"key=value"</c> format.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(string controlKey, params string[] additionalParameters)
        {
            return this.NavigateURL(TabController.CurrentPage?.TabID ?? -1, controlKey, additionalParameters);
        }

        /// <summary>Gets the URL to show the control associated with the given control key on the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID, string controlKey)
        {
            return this.NavigateURL(tabID, this.portalController.GetCurrentSettings(), controlKey, null);
        }

        /// <summary>Gets the URL to show the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        {
            return this.NavigateURL(tabID, this.portalController.GetCurrentSettings(), controlKey, additionalParameters);
        }

        /// <summary>Gets the URL to show the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID, IPortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            var isSuperTab = Globals.IsHostTab(tabID);
            return this.NavigateURL(tabID, isSuperTab, settings, controlKey, additionalParameters);
        }

        /// <summary>Gets the URL to show the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <see langword="true"/> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            var cultureCode = Globals.GetCultureCode(tabID, isSuperTab, settings);
            return this.NavigateURL(tabID, isSuperTab, settings, controlKey, cultureCode, additionalParameters);
        }

        /// <summary>Gets the URL to show the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <see langword="true"/> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="language">The language code.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        public string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        {
            return this.NavigateURL(tabID, isSuperTab, settings, controlKey, language, Globals.glbDefaultPage, additionalParameters);
        }

        /// <summary>Gets the URL to show the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <see langword="true"/> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="language">The language code.</param>
        /// <param name="pageName">The page name to pass to <see cref="FriendlyUrl(DotNetNuke.Entities.Tabs.TabInfo,string,string)"/>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted url.</returns>
        public string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        {
            var url = tabID == Null.NullInteger ? Globals.ApplicationURL() : Globals.ApplicationURL(tabID);
            if (!string.IsNullOrEmpty(controlKey))
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

            // only add language to url if more than one locale is enabled
            if (settings != null && language != null && LocaleController.Instance.GetLocales(settings.PortalId).Count > 1)
            {
                if (settings.ContentLocalizationEnabled)
                {
                    if (language == string.Empty)
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
                    // legacy pre 5.5 behavior
                    if (language == string.Empty)
                    {
                        url += "&language=" + Thread.CurrentThread.CurrentCulture.Name;
                    }
                    else
                    {
                        url += "&language=" + language;
                    }
                }
            }

            if (this.hostSettings.UseFriendlyUrls || Config.GetFriendlyUrlProvider() == "advanced")
            {
                if (string.IsNullOrEmpty(pageName))
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
