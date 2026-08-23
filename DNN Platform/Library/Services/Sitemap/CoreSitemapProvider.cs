// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Sitemap
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.Logging;

    public class CoreSitemapProvider : SitemapProvider
    {
        private static readonly ILogger Logger = DnnLoggingController.GetLogger<CoreSitemapProvider>();
        private bool includeHiddenPages;
        private float minPagePriority;

        private bool useLevelBasedPagePriority;

        /// <inheritdoc />
        public override List<SitemapUrl> GetUrls(int portalId, PortalSettings ps, string version)
        {
            SitemapUrl pageUrl = null;
            var urls = new List<SitemapUrl>();

            this.useLevelBasedPagePriority = bool.Parse(PortalController.GetPortalSetting("SitemapLevelMode", portalId, "False"));
            this.minPagePriority = float.Parse(PortalController.GetPortalSetting("SitemapMinPriority", portalId, "0.1"), CultureInfo.InvariantCulture);
            this.includeHiddenPages = bool.Parse(PortalController.GetPortalSetting("SitemapIncludeHidden", portalId, "True"));

            var currentLanguage = ps.CultureCode;
            if (string.IsNullOrEmpty(currentLanguage))
            {
                currentLanguage = Localization.GetPageLocale((IPortalSettings)ps).Name;
            }

            var languagePublished = LocaleController.Instance.GetLocale(ps.PortalId, currentLanguage).IsPublished;
            var tabs = TabController.Instance.GetTabsByPortal(portalId).Values
                        .Where(t => (!t.IsSystem
                                    && !ps.ContentLocalizationEnabled) || (languagePublished && t.CultureCode.Equals(currentLanguage, StringComparison.OrdinalIgnoreCase)));
            foreach (TabInfo tab in tabs)
            {
                try
                {
                    if (this.IsTabEligibleForSitemap(tab, DateTime.Now))
                    {
                        try
                        {
                            pageUrl = this.GetPageUrl(tab, currentLanguage, ps);
                            urls.Add(pageUrl);
                        }
                        catch (Exception exception)
                        {
                            Logger.CoreSitemapProviderErrorGettingPageUrl(exception, tab.TabName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var exceptionMessage = Localization.GetExceptionMessage(
                        "SitemapUrlGenerationError",
                        "URL sitemap generation for page '{0} - {1}' caused an exception: {2}",
                        tab.TabID,
                        tab.TabName,
                        ex.Message);
                    Services.Exceptions.Exceptions.LogException(new SitemapException(exceptionMessage, ex));
                }
            }

            return urls;
        }

        public virtual bool IsTabPublic(TabPermissionCollection objTabPermissions)
        {
            string roles = objTabPermissions.ToString("VIEW");
            bool hasPublicRole = false;

            if (roles != null)
            {
                // permissions strings are encoded with Deny permissions at the beginning and Grant permissions at the end for optimal performance
                foreach (string role in roles.Split(';'))
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        // Deny permission
                        if (role.StartsWith("!", StringComparison.Ordinal))
                        {
                            string denyRole = role.Replace("!", string.Empty);
                            if (denyRole is Globals.glbRoleUnauthUserName or Globals.glbRoleAllUsersName)
                            {
                                hasPublicRole = false;
                                break;
                            }

                            // Grant permission
                        }
                        else
                        {
                            if (role is Globals.glbRoleUnauthUserName or Globals.glbRoleAllUsersName)
                            {
                                hasPublicRole = true;
                                break;
                            }
                        }
                    }
                }
            }

            return hasPublicRole;
        }

        /// <summary>Determines whether a page is eligible to be included in sitemap output.</summary>
        /// <param name="tab">The page to evaluate.</param>
        /// <param name="now">The date and time used to evaluate the page publication window.</param>
        /// <returns><see langword="true"/> when the page is eligible; otherwise, <see langword="false"/>.</returns>
        internal bool IsTabEligibleForSitemap(TabInfo tab, DateTime now)
        {
            return tab != null &&
                   !tab.IsDeleted &&
                   !tab.DisableLink &&
                   tab.TabType == TabType.Normal &&
                   (Null.IsNull(tab.StartDate) || tab.StartDate < now) &&
                   (Null.IsNull(tab.EndDate) || tab.EndDate > now) &&
                   this.IsTabPublic(tab.TabPermissions) &&
                   (this.includeHiddenPages || tab.IsVisible) &&
                   tab.HasBeenPublished &&
                   tab.AllowIndex;
        }

        /// <summary>Gets the eligible localized URLs for an hreflang group.</summary>
        /// <param name="defaultLanguageTab">The default-language page.</param>
        /// <param name="localizedTabs">The localized versions of the page.</param>
        /// <param name="ps">The current portal settings.</param>
        /// <param name="now">The date and time used to evaluate page publication windows.</param>
        /// <returns>The eligible alternate URLs.</returns>
        internal List<AlternateUrl> GetAlternateUrls(
            TabInfo defaultLanguageTab,
            IEnumerable<TabInfo> localizedTabs,
            PortalSettings ps,
            DateTime now)
        {
            var alternates = new List<AlternateUrl>();
            List<TabInfo> eligibleLocalizedTabs = localizedTabs.Where(localizedTab => this.IsTabEligibleForSitemap(localizedTab, now)).ToList();
            bool isDefaultLanguageTabEligible = this.IsTabEligibleForSitemap(defaultLanguageTab, now);

            // A single self-reference is not an alternate-language relationship.
            if (!isDefaultLanguageTabEligible && eligibleLocalizedTabs.Count < 2)
            {
                return alternates;
            }

            foreach (TabInfo localizedTab in eligibleLocalizedTabs)
            {
                alternates.Add(new AlternateUrl
                {
                    Url = TestableGlobals.Instance.NavigateURL(localizedTab.TabID, localizedTab.IsSuperTab, ps, string.Empty, localizedTab.CultureCode),
                    Language = localizedTab.CultureCode,
                });
            }

            if (alternates.Count > 0 && isDefaultLanguageTabEligible)
            {
                string defaultUrl = TestableGlobals.Instance.NavigateURL(defaultLanguageTab.TabID, defaultLanguageTab.IsSuperTab, ps, string.Empty, defaultLanguageTab.CultureCode);
                alternates.Add(new AlternateUrl
                {
                    Url = defaultUrl,
                    Language = defaultLanguageTab.CultureCode,
                });
            }

            return alternates;
        }

        /// <summary>
        ///   When page level priority is used, the priority for each page will be computed from
        ///   the hierarchy level of the page.
        ///   Top level pages will have a value of 1, second level 0.9, third level 0.8, ...
        /// </summary>
        /// <param name="objTab">The page being indexed.</param>
        /// <returns>The priority assigned to the page.</returns>
        protected float GetPriority(TabInfo objTab)
        {
            float priority = objTab.SiteMapPriority;

            if (this.useLevelBasedPagePriority)
            {
                if (objTab.Level >= 9)
                {
                    priority = 0.1F;
                }
                else
                {
                    priority = Convert.ToSingle(1 - (objTab.Level * 0.1));
                }

                if (priority < this.minPagePriority)
                {
                    priority = this.minPagePriority;
                }
            }

            return priority;
        }

        /// <summary>Return the sitemap url node for the page.</summary>
        /// <param name="objTab">The page being indexed.</param>
        /// <param name="language">Culture code to use in the URL.</param>
        /// <param name="ps">The portal settings.</param>
        /// <returns>A SitemapUrl object for the current page.</returns>
        private SitemapUrl GetPageUrl(TabInfo objTab, string language, PortalSettings ps)
        {
            var pageUrl = new SitemapUrl();
            var url = TestableGlobals.Instance.NavigateURL(objTab.TabID, objTab.IsSuperTab, ps, string.Empty, language);
            if ((ps.SSLSetup == Abstractions.Security.SiteSslSetup.On || ps.SSLEnforced || (objTab.IsSecure && ps.SSLEnabled)) && url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url.Substring("http://".Length);
            }

            pageUrl.Url = url;
            pageUrl.Priority = this.GetPriority(objTab);
            pageUrl.LastModified = objTab.LastModifiedOnDate;
            foreach (ModuleInfo m in ModuleController.Instance.GetTabModules(objTab.TabID).Values)
            {
                if (m.LastModifiedOnDate > objTab.LastModifiedOnDate)
                {
                    pageUrl.LastModified = m.LastModifiedOnDate;
                }
            }

            pageUrl.ChangeFrequency = SitemapChangeFrequency.Daily;

            // support for alternate pages: https://support.google.com/webmasters/answer/2620865?hl=en
            if (ps.ContentLocalizationEnabled && !objTab.IsNeutralCulture)
            {
                TabInfo defaultLanguageTab = objTab.IsDefaultLanguage ? objTab : objTab.DefaultLanguageTab;
                if (defaultLanguageTab != null)
                {
                    List<AlternateUrl> alternates = this.GetAlternateUrls(defaultLanguageTab, defaultLanguageTab.LocalizedTabs.Values, ps, DateTime.Now);
                    if (alternates.Count > 0)
                    {
                        pageUrl.AlternateUrls = alternates;
                    }
                }
            }

            return pageUrl;
        }
    }
}
