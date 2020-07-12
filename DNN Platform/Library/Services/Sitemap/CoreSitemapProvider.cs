// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Sitemap
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;

    public class CoreSitemapProvider : SitemapProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CoreSitemapProvider));
        private bool includeHiddenPages;
        private float minPagePriority;

        private bool useLevelBasedPagePriority;

        /// <summary>
        ///   Includes page urls on the sitemap.
        /// </summary>
        /// <remarks>
        ///   Pages that are included:
        ///   - are not deleted
        ///   - are not disabled
        ///   - are normal pages (not links,...)
        ///   - are visible (based on date and permissions).
        /// </remarks>
        /// <returns></returns>
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
                currentLanguage = Localization.GetPageLocale(ps).Name;
            }

            var languagePublished = LocaleController.Instance.GetLocale(ps.PortalId, currentLanguage).IsPublished;
            var tabs = TabController.Instance.GetTabsByPortal(portalId).Values
                        .Where(t => (!t.IsSystem
                                    && !ps.ContentLocalizationEnabled) || (languagePublished && t.CultureCode.Equals(currentLanguage, StringComparison.InvariantCultureIgnoreCase)));
            foreach (TabInfo tab in tabs)
            {
                try
                {
                    if (!tab.IsDeleted && !tab.DisableLink && tab.TabType == TabType.Normal &&
                        (Null.IsNull(tab.StartDate) || tab.StartDate < DateTime.Now) &&
                        (Null.IsNull(tab.EndDate) || tab.EndDate > DateTime.Now) && this.IsTabPublic(tab.TabPermissions))
                    {
                        if ((this.includeHiddenPages || tab.IsVisible) && tab.HasBeenPublished && tab.Indexed)
                        {
                            try
                            {
                                pageUrl = this.GetPageUrl(tab, currentLanguage, ps);
                                urls.Add(pageUrl);
                            }
                            catch (Exception)
                            {
                                Logger.ErrorFormat("Error has occurred getting PageUrl for {0}", tab.TabName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Services.Exceptions.Exceptions.LogException(new Exception(Localization.GetExceptionMessage(
                        "SitemapUrlGenerationError",
                        "URL sitemap generation for page '{0} - {1}' caused an exception: {2}",
                        tab.TabID, tab.TabName, ex.Message)));
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
                foreach (string role in roles.Split(new[] { ';' }))
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        // Deny permission
                        if (role.StartsWith("!"))
                        {
                            string denyRole = role.Replace("!", string.Empty);
                            if (denyRole == Globals.glbRoleUnauthUserName || denyRole == Globals.glbRoleAllUsersName)
                            {
                                hasPublicRole = false;
                                break;
                            }

                            // Grant permission
                        }
                        else
                        {
                            if (role == Globals.glbRoleUnauthUserName || role == Globals.glbRoleAllUsersName)
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

        /// <summary>
        ///   When page level priority is used, the priority for each page will be computed from
        ///   the hierarchy level of the page.
        ///   Top level pages will have a value of 1, second level 0.9, third level 0.8, ...
        /// </summary>
        /// <param name = "objTab">The page being indexed.</param>
        /// <returns>The priority assigned to the page.</returns>
        /// <remarks>
        /// </remarks>
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

        /// <summary>
        ///   Return the sitemap url node for the page.
        /// </summary>
        /// <param name = "objTab">The page being indexed.</param>
        /// <param name="language">Culture code to use in the URL.</param>
        /// <returns>A SitemapUrl object for the current page.</returns>
        /// <remarks>
        /// </remarks>
        private SitemapUrl GetPageUrl(TabInfo objTab, string language, PortalSettings ps)
        {
            var pageUrl = new SitemapUrl();
            var url = TestableGlobals.Instance.NavigateURL(objTab.TabID, objTab.IsSuperTab, ps, string.Empty, language);
            if ((ps.SSLEnforced || (objTab.IsSecure && ps.SSLEnabled)) && url.StartsWith("http://"))
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
                List<AlternateUrl> alternates = new List<AlternateUrl>();
                TabInfo currentTab = objTab;

                if (!objTab.IsDefaultLanguage)
                {
                    currentTab = objTab.DefaultLanguageTab;
                }

                foreach (TabInfo localized in currentTab.LocalizedTabs.Values)
                {
                    if ((!localized.IsDeleted && !localized.DisableLink && localized.TabType == TabType.Normal) &&
                        (Null.IsNull(localized.StartDate) || localized.StartDate < DateTime.Now) &&
                        (Null.IsNull(localized.EndDate) || localized.EndDate > DateTime.Now) &&
                        this.IsTabPublic(localized.TabPermissions) &&
                        (this.includeHiddenPages || localized.IsVisible) && localized.HasBeenPublished)
                    {
                        string alternateUrl = TestableGlobals.Instance.NavigateURL(localized.TabID, localized.IsSuperTab, ps, string.Empty, localized.CultureCode);
                        alternates.Add(new AlternateUrl()
                        {
                            Url = alternateUrl,
                            Language = localized.CultureCode,
                        });
                    }
                }

                if (alternates.Count > 0)
                {
                    // add default language to the list
                    string alternateUrl = TestableGlobals.Instance.NavigateURL(currentTab.TabID, currentTab.IsSuperTab, ps, string.Empty, currentTab.CultureCode);
                    alternates.Add(new AlternateUrl()
                    {
                        Url = alternateUrl,
                        Language = currentTab.CultureCode,
                    });

                    pageUrl.AlternateUrls = alternates;
                }
            }

            return pageUrl;
        }
    }
}
