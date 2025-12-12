// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;

    using Dnn.PersonaBar.Library.Dto;
    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Pages.Components.Dto;
    using Dnn.PersonaBar.Pages.Components.Exceptions;
    using Dnn.PersonaBar.Pages.Services.Dto;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;

    using Microsoft.Extensions.DependencyInjection;

    using PermissionsNotMetException = DotNetNuke.Entities.Tabs.PermissionsNotMetException;

    public class PagesControllerImpl : IPagesController
    {
        public const string PageTagsVocabulary = "PageTags";

        private static readonly IList<string> TabSettingKeys = new List<string> { "CustomStylesheet" };
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly ITabController tabController;
        private readonly IModuleController moduleController;
        private readonly IPageUrlsController pageUrlsController;
        private readonly ITemplateController templateController;
        private readonly IDefaultPortalThemeController defaultPortalThemeController;
        private readonly ICloneModuleExecutionContext cloneModuleExecutionContext;
        private readonly IUrlRewriterUtilsWrapper urlRewriterUtilsWrapper;
        private readonly IFriendlyUrlWrapper friendlyUrlWrapper;
        private readonly IContentVerifier contentVerifier;
        private readonly IPortalController portalController;
        private readonly PersonalizationController personalizationController;

        public PagesControllerImpl(IBusinessControllerProvider businessControllerProvider, ITemplateController templateController)
            : this(
                  businessControllerProvider,
                  TabController.Instance,
                  ModuleController.Instance,
                  PageUrlsController.Instance,
                  templateController,
                  DefaultPortalThemeController.Instance,
                  CloneModuleExecutionContext.Instance,
                  new UrlRewriterUtilsWrapper(),
                  new FriendlyUrlWrapper(),
                  new ContentVerifier(),
                  PortalController.Instance)
        {
        }

        public PagesControllerImpl(
            IBusinessControllerProvider businessControllerProvider,
            ITabController tabController,
            IModuleController moduleController,
            IPageUrlsController pageUrlsController,
            ITemplateController templateController,
            IDefaultPortalThemeController defaultPortalThemeController,
            ICloneModuleExecutionContext cloneModuleExecutionContext,
            IUrlRewriterUtilsWrapper urlRewriterUtilsWrapper,
            IFriendlyUrlWrapper friendlyUrlWrapper,
            IContentVerifier contentVerifier,
            IPortalController portalController)
            : this(
                businessControllerProvider,
                tabController,
                moduleController,
                pageUrlsController,
                templateController,
                defaultPortalThemeController,
                cloneModuleExecutionContext,
                urlRewriterUtilsWrapper,
                friendlyUrlWrapper,
                contentVerifier,
                portalController,
                null)
        {
        }

        public PagesControllerImpl(
            IBusinessControllerProvider businessControllerProvider,
            ITabController tabController,
            IModuleController moduleController,
            IPageUrlsController pageUrlsController,
            ITemplateController templateController,
            IDefaultPortalThemeController defaultPortalThemeController,
            ICloneModuleExecutionContext cloneModuleExecutionContext,
            IUrlRewriterUtilsWrapper urlRewriterUtilsWrapper,
            IFriendlyUrlWrapper friendlyUrlWrapper,
            IContentVerifier contentVerifier,
            IPortalController portalController,
            PersonalizationController personalizationController)
        {
            this.businessControllerProvider = businessControllerProvider;
            this.tabController = tabController;
            this.moduleController = moduleController;
            this.pageUrlsController = pageUrlsController;
            this.templateController = templateController;
            this.defaultPortalThemeController = defaultPortalThemeController;
            this.cloneModuleExecutionContext = cloneModuleExecutionContext;
            this.urlRewriterUtilsWrapper = urlRewriterUtilsWrapper;
            this.friendlyUrlWrapper = friendlyUrlWrapper;
            this.contentVerifier = contentVerifier;
            this.portalController = portalController;
            this.personalizationController = personalizationController ?? Globals.GetCurrentServiceProvider().GetRequiredService<PersonalizationController>();
        }

        private PortalSettings PortalSettings { get; set; }

        /// <inheritdoc/>
        public bool IsValidTabPath(TabInfo tab, string newTabPath, string newTabName, out string errorMessage)
        {
            var portalSettings = this.PortalSettings ?? PortalController.Instance.GetCurrentPortalSettings();
            var valid = true;
            errorMessage = string.Empty;

            // get default culture if the tab's culture is null
            var cultureCode = tab != null ? tab.CultureCode : string.Empty;
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = portalSettings.DefaultLanguage;
            }

            // Validate Tab Path
            var tabId = TabController.GetTabByTabPath(portalSettings.PortalId, newTabPath, cultureCode);
            if (tabId != Null.NullInteger && (tab == null || tabId != tab.TabID))
            {
                var existingTab = this.tabController.GetTab(tabId, portalSettings.PortalId, false);
                if (existingTab != null && existingTab.IsDeleted)
                {
                    errorMessage = Localization.GetString("TabRecycled");
                }
                else
                {
                    errorMessage = Localization.GetString("TabExists");
                }

                valid = false;
            }

            // check whether have conflict between tab path and portal alias.
            if (valid && TabController.IsDuplicateWithPortalAlias(portalSettings.PortalId, newTabPath))
            {
                errorMessage = string.Format(Localization.GetString("PathDuplicateWithAlias"), newTabName, newTabPath);
                valid = false;
            }

            if (valid)
            {
                bool modified;
                FriendlyUrlController.ValidateUrl(newTabPath.TrimStart('/'), tab?.TabID ?? Null.NullInteger, portalSettings, out modified);
                if (modified)
                {
                    errorMessage = string.Format(Localization.GetString("PathDuplicateWithPage"), newTabPath);
                    valid = false;
                }
            }

            return valid;
        }

        /// <inheritdoc/>
        public List<int> GetPageHierarchy(int pageId)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var tab = TabController.Instance.GetTab(pageId, portalSettings.PortalId);
            if (tab == null)
            {
                throw new PageNotFoundException();
            }

            var paths = new List<int> { tab.TabID };
            while (tab.ParentId != Null.NullInteger)
            {
                tab = TabController.Instance.GetTab(tab.ParentId, portalSettings.PortalId);
                if (tab != null)
                {
                    paths.Insert(0, tab.TabID);
                }
            }

            return paths;
        }

        /// <inheritdoc/>
        public TabInfo MovePage(PageMoveRequest request)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var tab = TabController.Instance.GetTab(request.PageId, portalSettings.PortalId);
            if (tab == null)
            {
                throw new PageNotFoundException();
            }

            if (request.Action == "parent" && tab.ParentId != request.ParentId)
            {
                string errorMessage;

                if (!this.IsValidTabPath(tab, Globals.GenerateTabPath(request.ParentId, tab.TabName), tab.TabName, out errorMessage))
                {
                    throw new PageException(errorMessage);
                }
            }
            else if (request.Action == "before" || request.Action == "after")
            {
                var relatedTab = TabController.Instance.GetTab(request.RelatedPageId, portalSettings.PortalId);
                if (relatedTab == null)
                {
                    throw new PageNotFoundException();
                }

                string errorMessage;

                if (tab.ParentId != relatedTab.ParentId && !this.IsValidTabPath(tab, Globals.GenerateTabPath(relatedTab.ParentId, tab.TabName), tab.TabName, out errorMessage))
                {
                    throw new PageException(errorMessage);
                }
            }

            switch (request.Action)
            {
                case "before":
                    TabController.Instance.MoveTabBefore(tab, request.RelatedPageId);
                    break;
                case "after":
                    TabController.Instance.MoveTabAfter(tab, request.RelatedPageId);
                    break;
                case "parent":
                    // avoid move tab into its child page
                    if (this.IsChild(portalSettings.PortalId, tab.TabID, request.ParentId))
                    {
                        throw new PageException("DragInvalid");
                    }

                    TabController.Instance.MoveTabToParent(tab, request.ParentId);
                    break;
            }

            // as tab's parent may changed, url need refresh.
            return TabController.Instance.GetTab(request.PageId, portalSettings.PortalId);
        }

        /// <inheritdoc/>
        public void DeletePage(PageItem page, PortalSettings portalSettings = null)
        {
            this.DeletePage(page, false, portalSettings);
        }

        /// <inheritdoc/>
        public void DeletePage(PageItem page, bool hardDelete, PortalSettings portalSettings = null)
        {
            var currentPortal = portalSettings ?? PortalController.Instance.GetCurrentPortalSettings();
            var tab = TabController.Instance.GetTab(page.Id, currentPortal.PortalId);
            if (tab == null)
            {
                throw new PageNotFoundException();
            }

            if (TabPermissionController.CanDeletePage(tab))
            {
                if (TabController.IsSpecialTab(tab.TabID, currentPortal.PortalId))
                {
                    throw new PageException(Localization.GetString("CannotDeleteSpecialPage"));
                }
                else
                {
                    if (this.contentVerifier.IsContentExistsForRequestedPortal(tab.PortalID, currentPortal))
                    {
                        if (hardDelete)
                        {
                            TabController.Instance.DeleteTab(tab.TabID, currentPortal.PortalId);
                        }
                        else
                        {
                            TabController.Instance.SoftDeleteTab(tab.TabID, currentPortal);
                        }
                    }
                    else
                    {
                        throw new PageNotFoundException();
                    }
                }
            }
            else
            {
                throw new PageException(Localization.GetString("NoPermissionDeletePage"));
            }
        }

        /// <inheritdoc/>
        public void EditModeForPage(int pageId, int userId)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var newCookie = new HttpCookie("LastPageId", $"{portalSettings.PortalId}:{pageId}")
            {
                Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
            };
            HttpContext.Current.Response.Cookies.Add(newCookie);

            if (Personalization.GetUserMode() != PortalSettings.Mode.Edit)
            {
                var personalization = this.personalizationController.LoadProfile(userId, portalSettings.PortalId);
                personalization.Profile["Usability:UserMode" + portalSettings.PortalId] = "EDIT";
                personalization.IsModified = true;
                this.personalizationController.SaveProfile(personalization);
            }
        }

        /// <inheritdoc/>
        public TabInfo SavePageDetails(PortalSettings settings, PageSettings pageSettings)
        {
            this.PortalSettings = settings ?? PortalController.Instance.GetCurrentPortalSettings();
            TabInfo tab = null;
            if (pageSettings.TabId > 0)
            {
                tab = TabController.Instance.GetTab(pageSettings.TabId, this.PortalSettings.PortalId);
                if (tab == null)
                {
                    throw new PageNotFoundException();
                }
            }

            string errorMessage;
            string field;
            if (!this.ValidatePageSettingsData(this.PortalSettings, pageSettings, tab, out field, out errorMessage))
            {
                throw new PageValidationException(field, errorMessage);
            }

            var tabId = pageSettings.TabId <= 0
                ? this.AddTab(this.PortalSettings, pageSettings)
                : this.UpdateTab(tab, pageSettings);

            return TabController.Instance.GetTab(tabId, this.PortalSettings.PortalId);
        }

        /// <inheritdoc/>
        public IEnumerable<TabInfo> GetPageList(PortalSettings settings, int parentId = -1, string searchKey = "", bool includeHidden = true, bool includeDeleted = false, bool includeSubpages = false)
        {
            var portalSettings = settings ?? PortalController.Instance.GetCurrentPortalSettings();
            var adminTabId = portalSettings.AdminTabId;

            var tabs = TabController.GetPortalTabs(portalSettings.PortalId, adminTabId, false, includeHidden, includeDeleted, true);
            var pages = from t in tabs
                        where (t.ParentId != adminTabId || t.ParentId == Null.NullInteger) &&
                                !t.IsSystem &&
                                    ((string.IsNullOrEmpty(searchKey) && (includeSubpages || t.ParentId == parentId))
                                        || (!string.IsNullOrEmpty(searchKey) &&
                                                (t.TabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger
                                                    || t.LocalizedTabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger)))
                        select t;

            return includeSubpages ? pages.OrderBy(x => x.ParentId > -1 ? x.ParentId : x.TabID).ThenBy(x => x.TabID) : pages;
        }

        /// <inheritdoc/>
        public IEnumerable<TabInfo> GetPageList(PortalSettings portalSettings, bool? deleted, string tabName, string tabTitle, string tabPath, string tabSkin, bool? visible, int parentId, out int total, string searchKey = "", int pageIndex = -1, int pageSize = 10, bool includeSubpages = false)
        {
            pageIndex = pageIndex <= 0 ? 0 : pageIndex;
            pageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 10;
            var tabs = this.GetPageList(portalSettings, parentId, searchKey, true, deleted ?? false, includeSubpages);
            var finalList = new List<TabInfo>();
            if (deleted.HasValue)
            {
                tabs = tabs.Where(tab => tab.IsDeleted == deleted);
            }

            if (visible.HasValue)
            {
                tabs = tabs.Where(tab => tab.IsVisible == visible);
            }

            if (!string.IsNullOrEmpty(tabTitle) || !string.IsNullOrEmpty(tabName) || !string.IsNullOrEmpty(tabPath) ||
                !string.IsNullOrEmpty(tabSkin))
            {
                foreach (var tab in tabs)
                {
                    var bIsMatch = true;
                    if (!string.IsNullOrEmpty(tabTitle))
                    {
                        bIsMatch = bIsMatch &
                                   Regex.IsMatch(tab.Title, tabTitle.Replace("*", ".*"), RegexOptions.IgnoreCase);
                    }

                    if (!string.IsNullOrEmpty(tabName))
                    {
                        bIsMatch = bIsMatch &
                                   Regex.IsMatch(tab.TabName, tabName.Replace("*", ".*"), RegexOptions.IgnoreCase);
                    }

                    if (!string.IsNullOrEmpty(tabPath))
                    {
                        bIsMatch = bIsMatch &
                                   Regex.IsMatch(tab.TabPath, tabPath.Replace("*", ".*"), RegexOptions.IgnoreCase);
                    }

                    if (!string.IsNullOrEmpty(tabSkin))
                    {
                        var escapedString = Regex.Replace(tabSkin, "([^\\w^\\*\\s]+)+", @"\$1", RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        bIsMatch = bIsMatch &
                                   Regex.IsMatch(tab.SkinSrc, escapedString.Replace("*", ".*"), RegexOptions.IgnoreCase);
                    }

                    if (bIsMatch)
                    {
                        finalList.Add(tab);
                    }
                }
            }
            else
            {
                finalList.AddRange(tabs);
            }

            total = finalList.Count;
            return finalList.Skip(pageIndex * pageSize).Take(pageSize);
        }

        /// <inheritdoc/>
        public IEnumerable<TabInfo> SearchPages(out int totalRecords, string searchKey = "", string pageType = "", string tags = "", string publishStatus = "", string publishDateStart = "", string publishDateEnd = "", int workflowId = -1, int pageIndex = -1, int pageSize = -1)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var adminTabId = portalSettings.AdminTabId;

            var tabs = TabController.GetPortalTabs(portalSettings.PortalId, adminTabId, false, true, false, true);
            var pages = from t in tabs
                        where (t.ParentId != adminTabId || t.ParentId == Null.NullInteger) &&
                                !t.IsSystem &&
                                    (string.IsNullOrEmpty(searchKey)
                                        || (!string.IsNullOrEmpty(searchKey) &&
                                                (t.TabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger
                                                    || t.LocalizedTabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger)))
                        select t;

            if (!string.IsNullOrEmpty(pageType))
            {
                pages = pages.Where(p => string.Equals(Globals.GetURLType(p.Url).ToString(), pageType, StringComparison.CurrentCultureIgnoreCase));
            }

            if (!string.IsNullOrEmpty(tags))
            {
                pages = pages.Where(p => HasTags(tags, p.Terms));
            }

            PublishStatus status;
            if (Enum.TryParse(publishStatus, true, out status))
            {
                switch (status)
                {
                    case PublishStatus.Published:
                        pages = pages.Where(tab => tab.HasBeenPublished && WorkflowHelper.IsWorkflowCompleted(tab));
                        break;
                    case PublishStatus.Draft:
                        pages = pages.Where(tab => !tab.HasBeenPublished || !WorkflowHelper.IsWorkflowCompleted(tab));
                        break;
                    case PublishStatus.All:
                        break;
                    default:
                        break;
                }
            }

            DateTime startDate;
            if (!string.IsNullOrEmpty(publishDateStart))
            {
                startDate = DateTime.ParseExact(publishDateStart, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                pages = pages.Where(p => WorkflowHelper.GetTabLastPublishedOn(p) >= startDate);
            }

            DateTime endDate;
            if (!string.IsNullOrEmpty(publishDateEnd))
            {
                endDate = DateTime.ParseExact(publishDateEnd, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                pages = pages.Where(p => WorkflowHelper.GetTabLastPublishedOn(p) <= endDate);
            }

            if (workflowId != -1)
            {
                pages = pages.Where(p => WorkflowHelper.GetTabWorkflowId(p) == workflowId);
            }

            totalRecords = pages.Count();
            return pageIndex == -1 || pageSize == -1 ? pages : pages.Skip(pageIndex * pageSize).Take(pageSize);
        }

        /// <inheritdoc/>
        public IEnumerable<ModuleInfo> GetModules(int pageId)
        {
            var tabModules = this.moduleController.GetTabModules(pageId);
            return tabModules.Values.Where(m => !m.IsDeleted);
        }

        public bool ValidatePageUrlSettings(PortalSettings portalSettings, PageSettings pageSettings, TabInfo tab, ref string invalidField, ref string errorMessage)
        {
            var urlPath = pageSettings.Url;

            if (string.IsNullOrEmpty(urlPath))
            {
                return true;
            }

            bool modified;

            // Clean Url
            var options = this.urlRewriterUtilsWrapper.GetExtendOptionsForURLs(portalSettings.PortalId);
            urlPath = this.GetLocalPath(urlPath);
            urlPath = this.friendlyUrlWrapper.CleanNameForUrl(urlPath, options, out modified);
            if (modified)
            {
                errorMessage = Localization.GetString("UrlPathCleaned");
                invalidField = "url";
                return false;
            }

            // Validate for uniqueness
            this.friendlyUrlWrapper.ValidateUrl(urlPath, tab?.TabID ?? Null.NullInteger, portalSettings, out modified);
            if (modified)
            {
                errorMessage = Localization.GetString("UrlPathNotUnique");
                invalidField = "url";
                return false;
            }

            return true;
        }

        public virtual int AddTab(PortalSettings settings, PageSettings pageSettings)
        {
            var portalSettings = settings ?? PortalController.Instance.GetCurrentPortalSettings();
            var portalId = portalSettings.PortalId;
            var tab = new TabInfo { PortalID = portalId, ParentId = pageSettings.ParentId ?? Null.NullInteger };
            this.UpdateTabInfoFromPageSettings(tab, pageSettings);

            if (portalSettings.ContentLocalizationEnabled)
            {
                tab.CultureCode = portalSettings.CultureCode;
            }

            this.SavePagePermissions(tab, pageSettings.Permissions);

            var tabId = this.tabController.AddTab(tab);

            this.CreateOrUpdateContentItem(tab);

            this.tabController.UpdateTab(tab);

            tab = this.tabController.GetTab(tabId, portalId);

            this.UpdateTabWorkflowFromPageSettings(tab, pageSettings);

            if (pageSettings.TemplateTabId > 0)
            {
                this.CopyContentFromSourceTab(tab, pageSettings.TemplateTabId, pageSettings.Modules);
            }

            if (pageSettings.TemplateId > 0)
            {
                try
                {
                    this.templateController.CreatePageFromTemplate(pageSettings.TemplateId, tab, portalId);
                }
                catch (PageException)
                {
                    this.tabController.DeleteTab(tab.TabID, portalId);
                    throw;
                }
            }

            this.SaveTabUrl(tab, pageSettings);

            this.MovePageIfNeeded(pageSettings, tab);

            this.tabController.ClearCache(portalId);
            return tab.TabID;
        }

        public void SaveTabUrl(TabInfo tab, PageSettings pageSettings)
        {
            if (!pageSettings.CustomUrlEnabled)
            {
                return;
            }

            if (tab.IsSuperTab)
            {
                return;
            }

            var url = pageSettings.Url;
            var tabUrl = tab.TabUrls.SingleOrDefault(t => t.IsSystem
                                                          && t.HttpStatus == "200"
                                                          && t.SeqNum == 0);

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (!string.IsNullOrEmpty(url) && url != "/")
            {
                url = this.CleanTabUrl(url);

                string currentUrl = string.Empty;
                var friendlyUrlSettings = new FriendlyUrlSettings(portalSettings.PortalId);
                if (tab.TabID > -1)
                {
                    var baseUrl = Globals.AddHTTP(portalSettings.PortalAlias.HTTPAlias) + "/Default.aspx?TabId=" + tab.TabID;
                    var path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                        tab,
                        baseUrl,
                        Globals.glbDefaultPage,
                        portalSettings.PortalAlias.HTTPAlias,
                        false,
                        friendlyUrlSettings,
                        Guid.Empty);

                    currentUrl = path.Replace(Globals.AddHTTP(portalSettings.PortalAlias.HTTPAlias), string.Empty);
                }

                if (url == currentUrl)
                {
                    return;
                }

                if (tabUrl == null)
                {
                    // Add new custom url
                    tabUrl = new TabUrlInfo
                    {
                        TabId = tab.TabID,
                        SeqNum = 0,
                        PortalAliasId = -1,
                        PortalAliasUsage = PortalAliasUsageType.Default,
                        QueryString = string.Empty,
                        Url = url,
                        HttpStatus = "200",
                        CultureCode = string.Empty,
                        IsSystem = true,
                    };

                    // Save url
                    this.tabController.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }
                else
                {
                    // Change the original 200 url to a redirect
                    tabUrl.HttpStatus = "301";
                    tabUrl.SeqNum = tab.TabUrls.Max(t => t.SeqNum) + 1;
                    this.tabController.SaveTabUrl(tabUrl, portalSettings.PortalId, true);

                    // Add new custom url
                    tabUrl.Url = url;
                    tabUrl.HttpStatus = "200";
                    tabUrl.SeqNum = 0;
                    this.tabController.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }

                // Delete any redirects to the same url
                foreach (var redirecturl in this.tabController.GetTabUrls(tab.TabID, tab.PortalID))
                {
                    if (redirecturl.Url == url && redirecturl.HttpStatus != "200")
                    {
                        this.tabController.DeleteTabUrl(redirecturl, tab.PortalID, true);
                    }
                }
            }
            else
            {
                if (url == "/" && tabUrl != null)
                {
                    this.tabController.DeleteTabUrl(tabUrl, portalSettings.PortalId, true);
                }
            }
        }

        /// <inheritdoc/>
        public string CleanTabUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            var urlPath = url.TrimStart('/');
            bool modified;

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var friendlyUrlSettings = new FriendlyUrlSettings(portalSettings.PortalId);
            urlPath = UrlRewriterUtils.CleanExtension(urlPath, friendlyUrlSettings, string.Empty);

            // Clean Url
            var options = UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(friendlyUrlSettings));
            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);

            return '/' + urlPath;
        }

        /// <inheritdoc/>
        public void CopyThemeToDescendantPages(int pageId, Theme theme)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var portalId = portalSettings.PortalId;
            var tab = this.tabController.GetTab(pageId, portalId, false);
            if (tab == null)
            {
                throw new PageNotFoundException();
            }

            TabController.CopyDesignToChildren(tab, theme.SkinSrc, theme.ContainerSrc);
        }

        /// <inheritdoc/>
        public void CopyPermissionsToDescendantPages(int pageId)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var portalId = portalSettings.PortalId;
            var tab = this.tabController.GetTab(pageId, portalId, false);
            if (tab == null)
            {
                throw new PageNotFoundException();
            }

            if (!TabPermissionController.CanManagePage(tab) || tab.IsSuperTab)
            {
                throw new PermissionsNotMetException(tab.TabID, Localization.GetString("CannotCopyPermissionsToDescendantPages"));
            }

            TabController.CopyPermissionsToChildren(tab, tab.TabPermissions);
        }

        /// <inheritdoc/>
        public IEnumerable<Url> GetPageUrls(int tabId)
        {
            var tab = this.GetPageDetails(tabId);
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var portalId = portalSettings.PortalId;
            return this.pageUrlsController.GetPageUrls(tab, portalId);
        }

        /// <inheritdoc/>
        public PageSettings GetPageSettings(int pageId, PortalSettings requestPortalSettings = null)
        {
            var tab = this.GetPageDetails(pageId);

            var portalSettings = requestPortalSettings ?? this.portalController.GetCurrentPortalSettings();

            if (!this.contentVerifier.IsContentExistsForRequestedPortal(tab.PortalID, portalSettings))
            {
                throw new PageNotFoundException();
            }

            var page = Converters.ConvertToPageSettings<PageSettings>(tab);
            page.Modules = this.GetModules(page.TabId).Select(Converters.ConvertToModuleItem);
            page.PageUrls = this.GetPageUrls(page.TabId);
            page.Permissions = this.GetPermissionsData(pageId);
            page.SiteAliases = this.GetSiteAliases(portalSettings.PortalId);
            page.PrimaryAliasId = this.GetPrimaryAliasId(portalSettings.PortalId, portalSettings.CultureCode);
            page.Locales = this.GetLocales(portalSettings.PortalId);
            page.HasParent = tab.ParentId > -1;

            // icons
            var iconFile = string.IsNullOrEmpty(tab.IconFile) ? null : FileManager.Instance.GetFile(tab.PortalID, tab.IconFileRaw);
            if (iconFile != null)
            {
                page.IconFile = new FileDto
                {
                    fileId = iconFile.FileId,
                    fileName = iconFile.FileName,
                    folderId = iconFile.FolderId,
                    folderPath = iconFile.Folder,
                };
            }

            var iconFileLarge = string.IsNullOrEmpty(tab.IconFileLarge) ? null : FileManager.Instance.GetFile(tab.PortalID, tab.IconFileLargeRaw);
            if (iconFileLarge != null)
            {
                page.IconFileLarge = new FileDto
                {
                    fileId = iconFileLarge.FileId,
                    fileName = iconFileLarge.FileName,
                    folderId = iconFileLarge.FolderId,
                    folderPath = iconFileLarge.Folder,
                };
            }

            page.EnabledVersioning = TabVersionSettings.Instance.IsVersioningEnabled(portalSettings.PortalId, pageId);
            page.WorkflowEnabled = TabWorkflowSettings.Instance.IsWorkflowEnabled(portalSettings.PortalId, pageId);
            page.WorkflowId = WorkflowHelper.GetTabWorkflowId(tab);
            page.WorkflowName = WorkflowHelper.GetTabWorkflowName(tab);
            page.StateId = tab.StateID;
            page.StateName = tab.StateID != Null.NullInteger ? WorkflowStateManager.Instance.GetWorkflowState(tab.StateID).StateName : null;
            page.PublishStatus = tab.HasBeenPublished && page.IsWorkflowCompleted ? "Published" : "Draft";
            page.HasAVisibleVersion = tab.HasAVisibleVersion;
            page.HasBeenPublished = tab.HasBeenPublished;
            page.IsWorkflowCompleted = WorkflowHelper.IsWorkflowCompleted(tab);
            page.IsWorkflowOnDraft = WorkflowEngine.Instance.IsWorkflowOnDraft(tab);

            return page;
        }

        /// <inheritdoc/>
        public PageUrlResult CreateCustomUrl(SeoUrl dto)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return this.pageUrlsController.CreateCustomUrl(dto.SaveUrl, this.tabController.GetTab(dto.TabId, portalSettings.PortalId, false));
        }

        /// <inheritdoc/>
        public PageUrlResult UpdateCustomUrl(SeoUrl dto)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return this.pageUrlsController.UpdateCustomUrl(dto.SaveUrl, this.tabController.GetTab(dto.TabId, portalSettings.PortalId, false));
        }

        /// <inheritdoc/>
        public PageUrlResult DeleteCustomUrl(UrlIdDto dto)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return this.pageUrlsController.DeleteCustomUrl(dto.Id, this.tabController.GetTab(dto.TabId, portalSettings.PortalId, false));
        }

        public void CreateOrUpdateContentItem(TabInfo tab)
        {
            var contentController = Util.GetContentController();
            tab.Content = string.IsNullOrEmpty(tab.Title) ? tab.TabName : tab.Title;
            tab.Indexed = false;

            if (tab.ContentItemId != Null.NullInteger)
            {
                contentController.UpdateContentItem(tab);
                return;
            }

            var typeController = new ContentTypeController();
            var contentType =
                (from t in typeController.GetContentTypes()
                 where t.ContentType == "Tab"
                 select t).SingleOrDefault();

            if (contentType != null)
            {
                tab.ContentTypeId = contentType.ContentTypeId;
            }

            contentController.AddContentItem(tab);
        }

        public int UpdateTab(TabInfo tab, PageSettings pageSettings)
        {
            this.UpdateTabInfoFromPageSettings(tab, pageSettings);

            this.UpdateTabWorkflowFromPageSettings(tab, pageSettings);

            this.SavePagePermissions(tab, pageSettings.Permissions);

            this.tabController.UpdateTab(tab);

            this.CreateOrUpdateContentItem(tab);

            this.SaveTabUrl(tab, pageSettings);

            this.MovePageIfNeeded(pageSettings, tab);

            return tab.TabID;
        }

        public void SavePagePermissions(TabInfo tab, PagePermissions permissions)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

            tab.TabPermissions.Clear();

            // add default permissions for administrators if needed
            if (!HasAdminPermissions(permissions))
            {
                // add default permissions
                var permissionsList = PermissionController.GetPermissionsByTab();
                foreach (var permissionInfo in permissionsList)
                {
                    var editPermisison = (PermissionInfo)permissionInfo;
                    var permission = new TabPermissionInfo(editPermisison)
                    {
                        RoleID = portalSettings.AdministratorRoleId,
                        AllowAccess = true,
                        RoleName = portalSettings.AdministratorRoleName,
                    };
                    tab.TabPermissions.Add(permission);
                }
            }

            // add role permissions
            if (permissions.RolePermissions != null)
            {
                foreach (var rolePermission in permissions.RolePermissions.Where(NoLocked()))
                {
                    if (rolePermission.RoleId.ToString() == Globals.glbRoleAllUsers
                        || rolePermission.RoleId.ToString() == Globals.glbRoleUnauthUser
                        || RoleController.Instance.GetRoleById(portalSettings.PortalId, rolePermission.RoleId) != null)
                    {
                        foreach (var permission in rolePermission.Permissions)
                        {
                            tab.TabPermissions.Add(new TabPermissionInfo
                            {
                                PermissionID = permission.PermissionId,
                                RoleID = rolePermission.RoleId,
                                UserID = Null.NullInteger,
                                AllowAccess = permission.AllowAccess,
                            });
                        }
                    }
                }
            }

            // add user permissions
            if (permissions.UserPermissions != null)
            {
                foreach (var userPermission in permissions.UserPermissions)
                {
                    var user = UserController.Instance.GetUserById(portalSettings.PortalId, userPermission.UserId);
                    if (user != null)
                    {
                        if (!int.TryParse(Globals.glbRoleNothing, out var roleId))
                        {
                            roleId = -4;
                        }

                        foreach (var permission in userPermission.Permissions)
                        {
                            tab.TabPermissions.Add(new TabPermissionInfo
                            {
                                PermissionID = permission.PermissionId,
                                RoleID = roleId,
                                UserID = userPermission.UserId,
                                AllowAccess = permission.AllowAccess,
                            });
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual PageSettings GetDefaultSettings(int pageId = 0)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var pageSettings = new PageSettings
            {
                Templates = this.templateController.GetTemplates(),
                Permissions = pageId == 0 ? this.GetPermissionsData(portalSettings.HomeTabId) : this.GetPermissionsData(pageId),
            };

            pageSettings.TemplateId = this.templateController.GetDefaultTemplateId(pageSettings.Templates);

            if (portalSettings.SSLSetup == DotNetNuke.Abstractions.Security.SiteSslSetup.On || (portalSettings.SSLEnabled && portalSettings.SSLEnforced))
            {
                pageSettings.IsSecure = true;
            }

            var tabVersionSettings = TabVersionSettings.Instance;
            var tabWorkflowSettings = TabWorkflowSettings.Instance;
            pageSettings.EnabledVersioning = tabVersionSettings.IsVersioningEnabled(portalSettings.PortalId);
            pageSettings.WorkflowEnabled = tabWorkflowSettings.IsWorkflowEnabled(portalSettings.PortalId);
            pageSettings.WorkflowId = tabWorkflowSettings.GetDefaultTabWorkflowId(portalSettings.PortalId);

            return pageSettings;
        }

        /// <inheritdoc/>
        public PagePermissions GetPermissionsData(int pageId)
        {
            var permissions = new PagePermissions(true);
            if (pageId > 0)
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                var tab = TabController.Instance.GetTab(pageId, portalSettings.PortalId);
                if (tab != null)
                {
                    foreach (TabPermissionInfo permission in tab.TabPermissions)
                    {
                        if (permission.UserID != Null.NullInteger)
                        {
                            permissions.AddUserPermission(permission);
                        }
                        else
                        {
                            permissions.AddRolePermission(permission);
                        }
                    }

                    permissions.RolePermissions =
                        permissions.RolePermissions
                            .Select(
                                p =>
                                {
                                    p.RoleName = DotNetNuke.Services.Localization.Localization.LocalizeRole(p.RoleName);
                                    return p;
                                })
                            .OrderByDescending(p => p.Locked)
                            .ThenByDescending(p => p.IsDefault)
                            .ThenBy(p => p.RoleName)
                            .ToList();
                    permissions.UserPermissions = permissions.UserPermissions.OrderBy(p => p.DisplayName).ToList();
                }
            }

            return permissions;
        }

        /// <inheritdoc/>
        public void DeleteTabModule(int pageId, int moduleId)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var tab = this.tabController.GetTab(pageId, portalSettings.PortalId);
            if (tab == null)
            {
                throw new PageModuleNotFoundException();
            }

            var tabModule = this.moduleController.GetModule(moduleId, pageId, false);
            if (tabModule == null)
            {
                throw new PageModuleNotFoundException();
            }

            if (!TabPermissionController.CanAddContentToPage(tab))
            {
                throw new SecurityException("You do not have permission to delete module on this page");
            }

            this.moduleController.DeleteTabModule(pageId, moduleId, true);
            this.moduleController.ClearCache(pageId);
        }

        public void CopyContentFromSourceTab(TabInfo tab, int sourceTabId, IEnumerable<ModuleItem> includedModules)
        {
            var sourceTab = this.tabController.GetTab(sourceTabId, tab.PortalID);
            if (sourceTab == null || sourceTab.IsDeleted)
            {
                return;
            }

            // Copy Properties
            this.CopySourceTabProperties(tab, sourceTab);

            // Copy Modules
            this.CopyModulesFromSourceTab(tab, sourceTab, includedModules);
        }

        protected virtual bool ValidatePageSettingsData(PortalSettings portalSettings, PageSettings pageSettings, TabInfo tab, out string invalidField, out string errorMessage)
        {
            errorMessage = string.Empty;
            invalidField = string.Empty;

            var isValid = !string.IsNullOrEmpty(pageSettings.Name) && TabController.IsValidTabName(pageSettings.Name, out errorMessage);
            if (!isValid)
            {
                invalidField = pageSettings.PageType == "template" ? "templateName" : "name";
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = Localization.GetString("EmptyTabName");
                }
                else if (errorMessage.Equals("InvalidTabName", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = string.Format(Localization.GetString("InvalidTabName"), pageSettings.Name);
                }
                else
                {
                    errorMessage = Localization.GetString(errorMessage);
                }

                return false;
            }

            var parentId = pageSettings.ParentId ?? tab?.ParentId ?? Null.NullInteger;

            if (pageSettings.PageType == "template")
            {
                parentId = this.GetTemplateParentId(tab?.PortalID ?? portalSettings.PortalId);
            }

            isValid = this.IsValidTabPath(tab, Globals.GenerateTabPath(parentId, pageSettings.Name), pageSettings.Name, out errorMessage);
            if (!isValid)
            {
                invalidField = pageSettings.PageType == "template" ? "templateName" : "name";
                errorMessage = (pageSettings.PageType == "template" ? "templates_" : string.Empty) + errorMessage;
                return false;
            }

            if (pageSettings.StartDate.HasValue && pageSettings.EndDate.HasValue && pageSettings.StartDate > pageSettings.EndDate)
            {
                errorMessage = Localization.GetString("StartDateAfterEndDate");
                invalidField = "endDate";
                return false;
            }

            switch (pageSettings.PageType)
            {
                case "tab":
                    if (!int.TryParse(pageSettings.ExistingTabRedirection, out var existingTabRedirectionId) || existingTabRedirectionId <= 0)
                    {
                        errorMessage = Localization.GetString("TabToRedirectIsRequired");
                        invalidField = "ExistingTabRedirection";
                        return false;
                    }

                    if (!TabPermissionController.CanViewPage(TabController.Instance.GetTab(
                        existingTabRedirectionId,
                        portalSettings.PortalId)))
                    {
                        errorMessage = Localization.GetString("NoPermissionViewRedirectPage");
                        invalidField = "ExistingTabRedirection";
                        return false;
                    }

                    break;
                case "url":
                    if (string.IsNullOrEmpty(pageSettings.ExternalRedirection))
                    {
                        errorMessage = Localization.GetString("ExternalRedirectionUrlRequired");
                        invalidField = "ExternalRedirection";
                        return false;
                    }

                    break;
                case "file":
                    var fileIdRedirectionId = pageSettings.FileIdRedirection ?? 0;
                    var file = pageSettings.FileIdRedirection != null ? FileManager.Instance.GetFile(pageSettings.FileIdRedirection.Value) : null;
                    if (fileIdRedirectionId <= 0 || file == null)
                    {
                        errorMessage = Localization.GetString("ValidFileIsRequired");
                        invalidField = "FileIdRedirection";
                        return false;
                    }

                    break;
            }

            return this.ValidatePageUrlSettings(portalSettings, pageSettings, tab, ref invalidField, ref errorMessage);
        }

        protected virtual int GetTemplateParentId(int portalId)
        {
            return Null.NullInteger;
        }

        protected virtual void UpdateTabInfoFromPageSettings(TabInfo tab, PageSettings pageSettings)
        {
            tab.TabName = pageSettings.Name;
            tab.TabPath = Globals.GenerateTabPath(tab.ParentId, tab.TabName);
            tab.Title = pageSettings.Title;
            tab.Description = this.GetTabDescription(pageSettings);
            tab.KeyWords = this.GetKeyWords(pageSettings);
            tab.IsVisible = pageSettings.IncludeInMenu;
            tab.DisableLink = pageSettings.DisableLink;

            tab.StartDate = pageSettings.StartDate ?? Null.NullDate;
            tab.EndDate = pageSettings.EndDate ?? Null.NullDate;

            tab.IsSecure = pageSettings.IsSecure;
            tab.TabSettings["AllowIndex"] = pageSettings.AllowIndex;

            tab.SiteMapPriority = pageSettings.SiteMapPriority;
            tab.PageHeadText = pageSettings.PageHeadText;

            tab.PermanentRedirect = pageSettings.PermanentRedirect;
            tab.Url = this.GetInternalUrl(pageSettings);

            tab.TabSettings["CacheProvider"] = pageSettings.CacheProvider;
            if (pageSettings.CacheProvider != null)
            {
                tab.TabSettings["CacheDuration"] = pageSettings.CacheDuration;
                if (pageSettings.CacheIncludeExclude.HasValue)
                {
                    if (pageSettings.CacheIncludeExclude.Value)
                    {
                        tab.TabSettings["CacheIncludeExclude"] = "1";
                        tab.TabSettings["IncludeVaryBy"] = null;
                        tab.TabSettings["ExcludeVaryBy"] = pageSettings.CacheExcludeVaryBy;
                    }
                    else
                    {
                        tab.TabSettings["CacheIncludeExclude"] = "0";
                        tab.TabSettings["IncludeVaryBy"] = pageSettings.CacheIncludeVaryBy;
                        tab.TabSettings["ExcludeVaryBy"] = null;
                    }

                    tab.TabSettings["MaxVaryByCount"] = pageSettings.CacheMaxVaryByCount;
                }
            }
            else
            {
                tab.TabSettings["CacheDuration"] = null;
                tab.TabSettings["CacheIncludeExclude"] = null;
                tab.TabSettings["IncludeVaryBy"] = null;
                tab.TabSettings["ExcludeVaryBy"] = null;
                tab.TabSettings["MaxVaryByCount"] = null;
            }

            tab.TabSettings["LinkNewWindow"] = pageSettings.LinkNewWindow.ToString();
            tab.TabSettings["CustomStylesheet"] = pageSettings.PageStyleSheet;

            // Tab Skin
            tab.SkinSrc = this.GetSkinSrc(pageSettings);
            tab.ContainerSrc = this.GetContainerSrc(pageSettings);

            if (pageSettings.PageType == "template")
            {
                tab.ParentId = this.GetTemplateParentId(tab.PortalID);
                tab.IsSystem = true;
            }

            tab.Terms.Clear();
            if (!string.IsNullOrEmpty(pageSettings.Tags))
            {
                tab.Terms.Clear();
                var termController = new TermController();
                var vocabularyController = Util.GetVocabularyController();
                var vocabulary =
                    vocabularyController.GetVocabularies()
                        .Cast<Vocabulary>()
                        .Where(v => v.Name == PageTagsVocabulary && v.ScopeId == tab.PortalID)
                        .SingleOrDefault();

                int vocabularyId;
                if (vocabulary == null)
                {
                    var scopeType = Util.GetScopeTypeController().GetScopeTypes().SingleOrDefault(s => s.ScopeType == "Portal");
                    if (scopeType == null)
                    {
                        throw new ScopeNotFoundException("Can't create default vocabulary as scope type 'Portal' can't be found.");
                    }

                    vocabularyId = vocabularyController.AddVocabulary(
                        new Vocabulary(PageTagsVocabulary, string.Empty, VocabularyType.Simple)
                        {
                            ScopeTypeId = scopeType.ScopeTypeId,
                            ScopeId = tab.PortalID,
                        });
                }
                else
                {
                    vocabularyId = vocabulary.VocabularyId;
                }

                // get all terms info
                var allTerms = new List<Term>();
                var vocabularies = from v in vocabularyController.GetVocabularies()
                                   where v.ScopeType.ScopeType == "Portal" && v.ScopeId == tab.PortalID && !v.Name.Equals("Tags", StringComparison.OrdinalIgnoreCase)
                                   select v;
                foreach (var v in vocabularies)
                {
                    allTerms.AddRange(termController.GetTermsByVocabulary(v.VocabularyId));
                }

                foreach (var tag in pageSettings.Tags.Trim().Split(','))
                {
                    if (!string.IsNullOrEmpty(tag) && tab.Terms.All(t => !t.Name.Equals(tag, StringComparison.OrdinalIgnoreCase)))
                    {
                        var term = allTerms.FirstOrDefault(t => t.Name.Equals(tag, StringComparison.OrdinalIgnoreCase));
                        if (term == null)
                        {
                            var termId = termController.AddTerm(new Term(tag, string.Empty, vocabularyId));
                            term = termController.GetTerm(termId);
                        }

                        tab.Terms.Add(term);
                    }
                }
            }

            // icons
            if (pageSettings.IconFile != null && pageSettings.IconFile.fileId > 0)
            {
                tab.IconFile = FileManager.Instance.GetFile(pageSettings.IconFile.fileId).RelativePath;
            }
            else
            {
                tab.IconFile = null;
            }

            if (pageSettings.IconFileLarge != null && pageSettings.IconFileLarge.fileId > 0)
            {
                tab.IconFileLarge = FileManager.Instance.GetFile(pageSettings.IconFileLarge.fileId).RelativePath;
            }
            else
            {
                tab.IconFileLarge = null;
            }
        }

        protected void UpdateTabWorkflowFromPageSettings(TabInfo tab, PageSettings pageSettings)
        {
            var tabVersionSettings = TabVersionSettings.Instance;
            var tabWorkflowSettings = TabWorkflowSettings.Instance;

            if (pageSettings.EnabledVersioning.HasValue && tabVersionSettings.IsVersioningEnabled(tab.PortalID))
            {
                tabVersionSettings.SetEnabledVersioningForTab(tab.TabID, pageSettings.EnabledVersioning.Value);
            }

            if (pageSettings.WorkflowEnabled.HasValue && tabWorkflowSettings.IsWorkflowEnabled(tab.PortalID))
            {
                tabWorkflowSettings.SetWorkflowEnabled(tab.PortalID, tab.TabID, pageSettings.WorkflowEnabled.Value);
            }

            ChangeContentWorkflow(tab, pageSettings);
        }

        protected IOrderedEnumerable<KeyValuePair<int, string>> GetLocales(int portalId)
        {
            var locales = new Lazy<Dictionary<string, Locale>>(() => LocaleController.Instance.GetLocales(portalId));
            return locales.Value.Values.Select(local => new KeyValuePair<int, string>(local.KeyID, local.EnglishName)).OrderBy(x => x.Value);
        }

        protected IEnumerable<KeyValuePair<int, string>> GetSiteAliases(int portalId)
        {
            var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
            return aliases.Select(alias => new KeyValuePair<int, string>(alias.KeyID, alias.HTTPAlias)).OrderBy(x => x.Value);
        }

        protected int? GetPrimaryAliasId(int portalId, string cultureCode)
        {
            var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
            var primary = aliases.Where(a => a.IsPrimary
                                             && (a.CultureCode == cultureCode || string.IsNullOrEmpty(a.CultureCode)))
                .OrderByDescending(a => a.CultureCode)
                .FirstOrDefault();
            return primary == null ? (int?)null : primary.KeyID;
        }

        private static string GetExternalUrlRedirection(string url)
        {
            if (url == null)
            {
                return null;
            }

            if (url.ToLower() == "http://")
            {
                return string.Empty;
            }

            if (url.StartsWith("//"))
            {
                return url;
            }

            if (url.IndexOf("://") != -1)
            {
                return Globals.AddHTTP(url);
            }

            return url;
        }

        private static Func<RolePermission, bool> NoLocked()
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return r => !(r.Locked && r.RoleId != portalSettings.AdministratorRoleId);
        }

        private static bool HasAdminPermissions(PagePermissions permissions)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return permissions.RolePermissions != null && (bool)permissions.RolePermissions?.Any(permission =>
                permission.RoleId == portalSettings.AdministratorRoleId &&
                permission.Permissions.Count != 0);
        }

        private static bool HasTags(string tags, IEnumerable<Term> terms)
        {
            return tags.Split(',').All(tag => terms.Any(t => string.Equals(t.Name, tag, StringComparison.CurrentCultureIgnoreCase)));
        }

        private static void ChangeContentWorkflow(TabInfo tab, PageSettings pageSettings)
        {
            var currentState = WorkflowStateRepository.Instance.GetWorkflowStateByID(tab.StateID);
            if (pageSettings.WorkflowId == currentState?.WorkflowID)
            {
                return;
            }

            var newWorkflow = WorkflowManager.Instance.GetWorkflow(pageSettings.WorkflowId);

            // Can't find workflow. This is not expected.
            if (newWorkflow == null)
            {
                return;
            }

            // Change to new workflow
            tab.StateID = WorkflowEngine.Instance.IsWorkflowCompleted(tab.ContentItemId)
                ? newWorkflow.LastState.StateID // Workflow is completed, just change to last state ("Published") of new workflow
                : newWorkflow.FirstState.StateID; // Workflow bot completed, just change to first state ("Draft") of new workflow
        }

        private bool IsChild(int portalId, int tabId, int parentId)
        {
            if (parentId == Null.NullInteger)
            {
                return false;
            }

            if (tabId == parentId)
            {
                return true;
            }

            var tab = TabController.Instance.GetTab(parentId, portalId);
            while (tab != null && tab.ParentId != Null.NullInteger)
            {
                if (tab.ParentId == tabId)
                {
                    return true;
                }

                tab = TabController.Instance.GetTab(tab.ParentId, portalId);
            }

            return false;
        }

        private TabInfo GetPageDetails(int pageId)
        {
            var portalSettings = this.portalController.GetCurrentPortalSettings();
            var tab = this.tabController.GetTab(pageId, portalSettings.PortalId);
            if (tab == null)
            {
                throw new PageNotFoundException();
            }

            return tab;
        }

        private void MovePageIfNeeded(PageSettings pageSettings, TabInfo tab)
        {
            if (pageSettings.ParentId.HasValue && pageSettings.ParentId.Value != tab.ParentId)
            {
                var request = new PageMoveRequest
                {
                    Action = "parent",
                    PageId = tab.TabID,
                    ParentId = pageSettings.ParentId.Value,
                };

                this.MovePage(request);
            }
        }

        private string GetContainerSrc(PageSettings pageSettings)
        {
            var defaultContainer = this.defaultPortalThemeController.GetDefaultPortalContainer();
            if (pageSettings.ContainerSrc != null &&
                pageSettings.ContainerSrc.Equals(
                    defaultContainer,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return pageSettings.ContainerSrc;
        }

        private string GetSkinSrc(PageSettings pageSettings)
        {
            var defaultSkin = this.defaultPortalThemeController.GetDefaultPortalLayout();
            if (pageSettings.SkinSrc != null &&
                pageSettings.SkinSrc.Equals(
                    defaultSkin,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return pageSettings.SkinSrc;
        }

        private string GetInternalUrl(PageSettings pageSettings)
        {
            switch (pageSettings.PageType)
            {
                case "tab":
                    return pageSettings.ExistingTabRedirection;
                case "url":
                    return GetExternalUrlRedirection(pageSettings.ExternalRedirection);
                case "file":
                    return pageSettings.FileIdRedirection.HasValue ? "FileId=" + pageSettings.FileIdRedirection : null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// If the tab description is equal to the portal description
        /// we store null so the system will serve the portal description instead.
        /// </summary>
        /// <param name="pageSettings">The page settings.</param>
        /// <returns>Tab Description value to be stored.</returns>
        private string GetTabDescription(PageSettings pageSettings)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return pageSettings.Description != portalSettings.Description
                ? pageSettings.Description : null;
        }

        /// <summary>
        /// If the tab keywords is equal to the portal keywords
        /// we store null so the system will serve the portal keywords instead.
        /// </summary>
        /// <param name="pageSettings">The page settings.</param>
        /// <returns>Tab Keywords value to be stored.</returns>
        private string GetKeyWords(PageSettings pageSettings)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return pageSettings.Keywords != portalSettings.KeyWords
                ? pageSettings.Keywords : null;
        }

        private string GetLocalPath(string url)
        {
            url = url.TrimEnd(new[] { '/' });
            if (url.Length > 1 && url.IndexOf('/') > -1)
            {
                url = url.Remove(0, url.LastIndexOf('/'));
            }

            return url;
        }

        private void CopySourceTabProperties(TabInfo tab, TabInfo sourceTab)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            tab.IconFile = sourceTab.IconFile;
            tab.IconFileLarge = sourceTab.IconFileLarge;
            tab.PageHeadText = sourceTab.PageHeadText;
            tab.RefreshInterval = sourceTab.RefreshInterval;
            this.tabController.UpdateTab(tab);

            // update need tab settings.
            foreach (var key in TabSettingKeys)
            {
                if (sourceTab.TabSettings.ContainsKey(key))
                {
                    this.tabController.UpdateTabSetting(tab.TabID, key, Convert.ToString(sourceTab.TabSettings[key]));
                }
            }
        }

        private void CopyModulesFromSourceTab(TabInfo tab, TabInfo sourceTab, IEnumerable<ModuleItem> includedModules)
        {
            includedModules = includedModules ?? sourceTab.ChildModules.Values.Select(Converters.ConvertToModuleItem);
            foreach (var module in includedModules)
            {
                var includedInCopy = module.IncludedInCopy ?? true;
                if (!includedInCopy || !sourceTab.ChildModules.Values.Any(m => !m.IsDeleted && !m.AllTabs && m.ModuleID == module.Id))
                {
                    continue;
                }

                var copyType = module.CopyType ?? ModuleCopyType.Copy;
                var objModule = ModuleController.Instance.GetModule(module.Id, sourceTab.TabID, false);
                ModuleInfo newModule = null;
                if (objModule != null)
                {
                    // Clone module as it exists in the cache and changes we make will update the cached object
                    newModule = objModule.Clone();
                    newModule.TabID = tab.TabID;
                    newModule.DefaultLanguageGuid = Null.NullGuid;
                    newModule.CultureCode = tab.CultureCode;
                    newModule.ModuleTitle = module.Title;

                    if (copyType != ModuleCopyType.Reference)
                    {
                        newModule.ModuleID = Null.NullInteger;
                        ModuleController.Instance.InitialModulePermission(newModule, newModule.TabID, 0);
                        newModule.InheritViewPermissions = objModule.InheritViewPermissions;
                    }

                    newModule.ModuleID = ModuleController.Instance.AddModule(newModule);

                    // copy permissions from source module
                    foreach (ModulePermissionInfo permission in objModule.ModulePermissions)
                    {
                        newModule.ModulePermissions.Add(
                            new ModulePermissionInfo
                            {
                                ModuleID = newModule.ModuleID,
                                PermissionID = permission.PermissionID,
                                RoleID = permission.RoleID,
                                UserID = permission.UserID,
                                PermissionKey = permission.PermissionKey,
                                AllowAccess = permission.AllowAccess,
                            },
                            true);
                    }

                    ModulePermissionController.SaveModulePermissions(newModule);

                    if (copyType == ModuleCopyType.Copy)
                    {
                        var controller = this.businessControllerProvider.GetInstance<IPortable>(newModule);
                        if (controller is not null)
                        {
                            try
                            {
                                this.cloneModuleExecutionContext.SetCloneModuleContext(true);
                                var content = controller.ExportModule(module.Id);
                                if (!string.IsNullOrEmpty(content))
                                {
                                    controller.ImportModule(
                                        newModule.ModuleID,
                                        content,
                                        newModule.DesktopModule.Version,
                                        UserController.Instance.GetCurrentUserInfo().UserID);
                                }
                            }
                            finally
                            {
                                this.cloneModuleExecutionContext.SetCloneModuleContext(false);
                            }
                        }
                    }
                }

                if (copyType != ModuleCopyType.Reference && objModule != null)
                {
                    // Make reference copies on secondary language
                    foreach (var m in objModule.LocalizedModules.Values)
                    {
                        if (tab.LocalizedTabs.ContainsKey(m.CultureCode))
                        {
                            var newLocalizedModule = m.Clone();
                            var localizedTab = tab.LocalizedTabs[m.CultureCode];
                            newLocalizedModule.TabID = localizedTab.TabID;
                            newLocalizedModule.CultureCode = localizedTab.CultureCode;
                            newLocalizedModule.ModuleTitle = module.Title;
                            newLocalizedModule.DefaultLanguageGuid = newModule.UniqueId;
                            newLocalizedModule.ModuleID = ModuleController.Instance.AddModule(newLocalizedModule);
                        }
                    }
                }
            }
        }
    }
}
