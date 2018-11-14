#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Dnn.PersonaBar.Library.DTO;
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Pages.Components.Dto;
using Dnn.PersonaBar.Pages.Components.Exceptions;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using PermissionsNotMetException = DotNetNuke.Entities.Tabs.PermissionsNotMetException;

namespace Dnn.PersonaBar.Pages.Components
{
    public class PagesControllerImpl : IPagesController
    {
        private readonly ITabController _tabController;
        private readonly IModuleController _moduleController;
        private readonly IPageUrlsController _pageUrlsController;
        private readonly ITemplateController _templateController;
        private readonly IDefaultPortalThemeController _defaultPortalThemeController;
        private readonly ICloneModuleExecutionContext _cloneModuleExecutionContext;
        private readonly IUrlRewriterUtilsWrapper _urlRewriterUtilsWrapper;
        private readonly IFriendlyUrlWrapper _friendlyUrlWrapper;
        private readonly IContentVerifier _contentVerifier;
        private readonly IPortalController _portalController;

        public const string PageTagsVocabulary = "PageTags";
        private static readonly IList<string> TabSettingKeys = new List<string> { "CustomStylesheet" };        

        private PortalSettings PortalSettings { get; set; }

        public PagesControllerImpl()
            : this(
                  TabController.Instance,
                  ModuleController.Instance,
                  PageUrlsController.Instance,
                  TemplateController.Instance,
                  DefaultPortalThemeController.Instance,
                  CloneModuleExecutionContext.Instance,
                  new UrlRewriterUtilsWrapper(),
                  new FriendlyUrlWrapper(),
                  new ContentVerifier(),
                  PortalController.Instance
                  )
        {
        }

        public PagesControllerImpl(
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
        {
            _tabController = tabController;
            _moduleController = moduleController;
            _pageUrlsController = pageUrlsController;
            _templateController = templateController;
            _defaultPortalThemeController = defaultPortalThemeController;
            _cloneModuleExecutionContext = cloneModuleExecutionContext;
            _urlRewriterUtilsWrapper = urlRewriterUtilsWrapper;
            _friendlyUrlWrapper = friendlyUrlWrapper;
            _contentVerifier = contentVerifier;
            _portalController = portalController;
        }


        public bool IsValidTabPath(TabInfo tab, string newTabPath, string newTabName, out string errorMessage)
        {
            var portalSettings = PortalSettings ?? PortalController.Instance.GetCurrentPortalSettings();
            var valid = true;
            errorMessage = string.Empty;

            //get default culture if the tab's culture is null
            var cultureCode = tab != null ? tab.CultureCode : string.Empty;
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = portalSettings.DefaultLanguage;
            }

            //Validate Tab Path
            var tabId = TabController.GetTabByTabPath(portalSettings.PortalId, newTabPath, cultureCode);
            if (tabId != Null.NullInteger && (tab == null || tabId != tab.TabID))
            {
                var existingTab = _tabController.GetTab(tabId, portalSettings.PortalId, false);
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

            //check whether have conflict between tab path and portal alias.
            if (valid && TabController.IsDuplicateWithPortalAlias(portalSettings.PortalId, newTabPath))
            {
                errorMessage = string.Format(Localization.GetString("PathDuplicateWithAlias"), newTabName, newTabPath);
                valid = false;
            }

            if (valid)
            {
                bool modified;
                FriendlyUrlController.ValidateUrl(newTabPath.TrimStart('/'), tab?.TabID ?? Null.NullInteger,
                    portalSettings, out modified);
                if (modified)
                {
                    errorMessage = string.Format(Localization.GetString("PathDuplicateWithPage"), newTabPath);
                    valid = false;
                }
            }

            return valid;
        }

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

                if (!IsValidTabPath(tab, Globals.GenerateTabPath(request.ParentId, tab.TabName), tab.TabName, out errorMessage))
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

                if (tab.ParentId != relatedTab.ParentId && !IsValidTabPath(tab, Globals.GenerateTabPath(relatedTab.ParentId, tab.TabName), tab.TabName, out errorMessage))
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
                    //avoid move tab into its child page
                    if (IsChild(portalSettings.PortalId, tab.TabID, request.ParentId))
                    {
                        throw new PageException("DragInvalid");
                    }

                    TabController.Instance.MoveTabToParent(tab, request.ParentId);
                    break;
            }

            //as tab's parent may changed, url need refresh.
            return TabController.Instance.GetTab(request.PageId, portalSettings.PortalId);
        }

        public void DeletePage(PageItem page, PortalSettings portalSettings = null)
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
                    if (_contentVerifier.IsContentExistsForRequestedPortal(tab.PortalID, currentPortal))
                    {
                        TabController.Instance.SoftDeleteTab(tab.TabID, currentPortal);
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

        public void EditModeForPage(int pageId, int userId)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var newCookie = new HttpCookie("LastPageId", $"{portalSettings.PortalId}:{pageId}")
            {
                Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/")
            };
            HttpContext.Current.Response.Cookies.Add(newCookie);

            if (portalSettings.UserMode != PortalSettings.Mode.Edit)
            {
                var personalizationController = new PersonalizationController();
                var personalization = personalizationController.LoadProfile(userId, portalSettings.PortalId);
                personalization.Profile["Usability:UserMode" + portalSettings.PortalId] = "EDIT";
                personalization.IsModified = true;
                personalizationController.SaveProfile(personalization);
            }
        }

        public TabInfo SavePageDetails(PortalSettings settings, PageSettings pageSettings)
        {
            PortalSettings = settings ?? PortalController.Instance.GetCurrentPortalSettings();
            TabInfo tab = null;
            if (pageSettings.TabId > 0)
            {
                tab = TabController.Instance.GetTab(pageSettings.TabId, PortalSettings.PortalId);
                if (tab == null)
                {
                    throw new PageNotFoundException();
                }
            }

            string errorMessage;
            string field;
            if (!ValidatePageSettingsData(PortalSettings, pageSettings, tab, out field, out errorMessage))
            {
                throw new PageValidationException(field, errorMessage);
            }

            var tabId = pageSettings.TabId <= 0
                ? AddTab(PortalSettings, pageSettings)
                : UpdateTab(tab, pageSettings);

            return TabController.Instance.GetTab(tabId, PortalSettings.PortalId);
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

        public IEnumerable<TabInfo> GetPageList(PortalSettings portalSettings, bool? deleted, string tabName, string tabTitle, string tabPath,
            string tabSkin, bool? visible, int parentId, out int total, string searchKey = "", int pageIndex = -1, int pageSize = 10, bool includeSubpages = false)
        {
            pageIndex = pageIndex <= 0 ? 0 : pageIndex;
            pageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 10;
            var tabs = GetPageList(portalSettings, parentId, searchKey, true, deleted ?? false, includeSubpages);
            var finalList = new List<TabInfo>();
            if (deleted.HasValue)
                tabs = tabs.Where(tab => tab.IsDeleted == deleted);
            if (visible.HasValue)
                tabs = tabs.Where(tab => tab.IsVisible == visible);

            if (!string.IsNullOrEmpty(tabTitle) || !string.IsNullOrEmpty(tabName) || !string.IsNullOrEmpty(tabPath) ||
                !string.IsNullOrEmpty(tabSkin))
            {
                foreach (var tab in tabs)
                {
                    var bIsMatch = true;
                    if (!string.IsNullOrEmpty(tabTitle))
                        bIsMatch = bIsMatch &
                                   Regex.IsMatch(tab.Title, tabTitle.Replace("*", ".*"), RegexOptions.IgnoreCase);
                    if (!string.IsNullOrEmpty(tabName))
                        bIsMatch = bIsMatch &
                                   Regex.IsMatch(tab.TabName, tabName.Replace("*", ".*"), RegexOptions.IgnoreCase);
                    if (!string.IsNullOrEmpty(tabPath))
                        bIsMatch = bIsMatch &
                                   Regex.IsMatch(tab.TabPath, tabPath.Replace("*", ".*"), RegexOptions.IgnoreCase);
                    if (!string.IsNullOrEmpty(tabSkin))
                    {
                        var escapedString = Regex.Replace(tabSkin, "([^\\w^\\*\\s]+)+", @"\$1", RegexOptions.Compiled|RegexOptions.ECMAScript|RegexOptions.IgnoreCase|RegexOptions.Multiline);
                        bIsMatch = bIsMatch &
                                   Regex.IsMatch(tab.SkinSrc, escapedString.Replace("*", ".*"), RegexOptions.IgnoreCase);
                    }

                    if (bIsMatch)
                        finalList.Add(tab);
                }
            }
            else
            {
                finalList.AddRange(tabs);
            }
            total = finalList.Count;
            return finalList.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public IEnumerable<TabInfo> SearchPages(out int totalRecords, string searchKey = "", string pageType = "", string tags = "", string publishStatus = "",
            string publishDateStart = "", string publishDateEnd = "", int workflowId = -1, int pageIndex = -1, int pageSize = -1)
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
                pages = pages.Where(p => string.Compare(Globals.GetURLType(p.Url).ToString(), pageType, StringComparison.CurrentCultureIgnoreCase) == 0);
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

        private TabInfo GetPageDetails(int pageId)
        {
            var portalSettings = _portalController.GetCurrentPortalSettings();
            var tab = _tabController.GetTab(pageId, portalSettings.PortalId);
            if (tab == null)
            {
                throw new PageNotFoundException();
            }

            return tab;
        }

        public IEnumerable<ModuleInfo> GetModules(int pageId)
        {
            var tabModules = _moduleController.GetTabModules(pageId);
            return tabModules.Values.Where(m => !m.IsDeleted);
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
                parentId = GetTemplateParentId(tab?.PortalID ?? portalSettings.PortalId);
            }

            isValid = IsValidTabPath(tab, Globals.GenerateTabPath(parentId, pageSettings.Name), pageSettings.Name, out errorMessage);
            if (!isValid)
            {
                invalidField = pageSettings.PageType == "template" ? "templateName" : "name";
                errorMessage = (pageSettings.PageType == "template" ? "templates_" : "") + errorMessage;
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
                    var existingTabRedirectionId = 0;
                    int.TryParse(pageSettings.ExistingTabRedirection, out existingTabRedirectionId);
                    if (existingTabRedirectionId <= 0)
                    {
                        errorMessage = Localization.GetString("TabToRedirectIsRequired");
                        invalidField = "ExistingTabRedirection";
                        return false;
                    }
                    if (!TabPermissionController.CanViewPage(TabController.Instance.GetTab(existingTabRedirectionId,
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

            return ValidatePageUrlSettings(portalSettings, pageSettings, tab, ref invalidField, ref errorMessage);
        }

        protected virtual int GetTemplateParentId(int portalId)
        {
            return Null.NullInteger;
        }

        public bool ValidatePageUrlSettings(PortalSettings portalSettings, PageSettings pageSettings, TabInfo tab, ref string invalidField, ref string errorMessage)
        {
            var urlPath = pageSettings.Url;

            if (string.IsNullOrEmpty(urlPath))
            {
                return true;
            }

            bool modified;
            //Clean Url            
            var options = _urlRewriterUtilsWrapper.GetExtendOptionsForURLs(portalSettings.PortalId);
            urlPath = GetLocalPath(urlPath);
            urlPath = _friendlyUrlWrapper.CleanNameForUrl(urlPath, options, out modified);
            if (modified)
            {
                errorMessage = Localization.GetString("UrlPathCleaned");
                invalidField = "url";
                return false;
            }

            //Validate for uniqueness
            _friendlyUrlWrapper.ValidateUrl(urlPath, tab?.TabID ?? Null.NullInteger, portalSettings, out modified);
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
            UpdateTabInfoFromPageSettings(tab, pageSettings);

            if (portalSettings.ContentLocalizationEnabled)
            {
                tab.CultureCode = portalSettings.CultureCode;
            }

            SavePagePermissions(tab, pageSettings.Permissions);

            var tabId = _tabController.AddTab(tab);
            tab = _tabController.GetTab(tabId, portalId);

            CreateOrUpdateContentItem(tab);

            if (pageSettings.TemplateTabId > 0)
            {
                CopyContentFromSourceTab(tab, pageSettings.TemplateTabId, pageSettings.Modules);
            }

            if (pageSettings.TemplateId > 0)
            {
                try
                {
                    _templateController.CreatePageFromTemplate(pageSettings.TemplateId, tab, portalId);
                }
                catch (PageException)
                {
                    _tabController.DeleteTab(tab.TabID, portalId);
                    throw;
                }
            }

            SaveTabUrl(tab, pageSettings);

            MovePageIfNeeded(pageSettings, tab);

            _tabController.ClearCache(portalId);
            return tab.TabID;
        }

        private void MovePageIfNeeded(PageSettings pageSettings, TabInfo tab)
        {
            if (pageSettings.ParentId.HasValue && pageSettings.ParentId.Value != tab.ParentId)
            {
                var request = new PageMoveRequest
                {
                    Action = "parent",
                    PageId = tab.TabID,
                    ParentId = pageSettings.ParentId.Value
                };

                MovePage(request);
            }
        }

        protected virtual void UpdateTabInfoFromPageSettings(TabInfo tab, PageSettings pageSettings)
        {
            tab.TabName = pageSettings.Name;
            tab.TabPath = Globals.GenerateTabPath(tab.ParentId, tab.TabName);
            tab.Title = pageSettings.Title;
            tab.Description = GetTabDescription(pageSettings);
            tab.KeyWords = GetKeyWords(pageSettings);
            tab.IsVisible = pageSettings.IncludeInMenu;
            tab.DisableLink = pageSettings.DisableLink;

            tab.StartDate = pageSettings.StartDate ?? Null.NullDate;
            tab.EndDate = pageSettings.EndDate ?? Null.NullDate;

            tab.IsSecure = pageSettings.IsSecure;
            tab.TabSettings["AllowIndex"] = pageSettings.AllowIndex;

            tab.SiteMapPriority = pageSettings.SiteMapPriority;
            tab.PageHeadText = pageSettings.PageHeadText;

            tab.PermanentRedirect = pageSettings.PermanentRedirect;
            tab.Url = GetInternalUrl(pageSettings);

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
            tab.SkinSrc = GetSkinSrc(pageSettings);
            tab.ContainerSrc = GetContainerSrc(pageSettings);

            if (pageSettings.PageType == "template")
            {
                tab.ParentId = GetTemplateParentId(tab.PortalID);
                tab.IsSystem = true;
            }

            tab.Terms.Clear();
            if (!string.IsNullOrEmpty(pageSettings.Tags))
            {
                tab.Terms.Clear();
                var termController = new TermController();
                var vocabularyController = Util.GetVocabularyController();
                var vocabulary = (vocabularyController.GetVocabularies()
                                    .Cast<Vocabulary>()
                                    .Where(v => v.Name == PageTagsVocabulary))
                                    .SingleOrDefault();

                int vocabularyId;
                if (vocabulary == null)
                {
                    var scopeType = Util.GetScopeTypeController().GetScopeTypes().SingleOrDefault(s => s.ScopeType == "Portal");
                    if (scopeType == null)
                    {
                        throw new Exception("Can't create default vocabulary as scope type 'Portal' can't finded.");
                    }

                    vocabularyId = vocabularyController.AddVocabulary(
                        new Vocabulary(PageTagsVocabulary, string.Empty, VocabularyType.Simple)
                        {
                            ScopeTypeId = scopeType.ScopeTypeId,
                            ScopeId = tab.PortalID
                        });
                }
                else
                {
                    vocabularyId = vocabulary.VocabularyId;
                }

                //get all terms info
                var allTerms = new List<Term>();
                var vocabularies = from v in vocabularyController.GetVocabularies()
                                   where (v.ScopeType.ScopeType == "Portal" && v.ScopeId == tab.PortalID && !v.Name.Equals("Tags", StringComparison.OrdinalIgnoreCase))
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

        private string GetContainerSrc(PageSettings pageSettings)
        {
            var defaultContainer = _defaultPortalThemeController.GetDefaultPortalContainer();
            if (pageSettings.ContainerSrc != null &&
                pageSettings.ContainerSrc.Equals(defaultContainer,
                StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            return pageSettings.ContainerSrc;
        }

        private string GetSkinSrc(PageSettings pageSettings)
        {
            var defaultSkin = _defaultPortalThemeController.GetDefaultPortalLayout();
            if (pageSettings.SkinSrc != null &&
                pageSettings.SkinSrc.Equals(defaultSkin,
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

        private static string GetExternalUrlRedirection(string url)
        {
            if (url == null)
            {
                return null;
            }

            return url.ToLower() == "http://" ? "" : Globals.AddHTTP(url);
        }

        /// <summary>
        /// If the tab description is equal to the portal description
        /// we store null so the system will serve the portal description instead
        /// </summary>
        /// <param name="pageSettings"></param>
        /// <returns>Tab Description value to be stored</returns>
        private string GetTabDescription(PageSettings pageSettings)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return pageSettings.Description != portalSettings.Description
                ? pageSettings.Description : null;
        }

        /// <summary>
        /// If the tab keywords is equal to the portal keywords
        /// we store null so the system will serve the portal keywords instead
        /// </summary>
        /// <param name="pageSettings"></param>
        /// <returns>Tab Keywords value to be stored</returns>
        private string GetKeyWords(PageSettings pageSettings)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return pageSettings.Keywords != portalSettings.KeyWords
                ? pageSettings.Keywords : null;
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
            if (!String.IsNullOrEmpty(url) && url != "/")
            {
                url = CleanTabUrl(url);

                string currentUrl = String.Empty;
                var friendlyUrlSettings = new FriendlyUrlSettings(portalSettings.PortalId);
                if (tab.TabID > -1)
                {
                    var baseUrl = Globals.AddHTTP(portalSettings.PortalAlias.HTTPAlias) + "/Default.aspx?TabId=" + tab.TabID;
                    var path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(tab,
                        baseUrl,
                        Globals.glbDefaultPage,
                        portalSettings.PortalAlias.HTTPAlias,
                        false,
                        friendlyUrlSettings,
                        Guid.Empty);

                    currentUrl = path.Replace(Globals.AddHTTP(portalSettings.PortalAlias.HTTPAlias), "");
                }

                if (url == currentUrl)
                {
                    return;
                }

                if (tabUrl == null)
                {
                    //Add new custom url
                    tabUrl = new TabUrlInfo
                    {
                        TabId = tab.TabID,
                        SeqNum = 0,
                        PortalAliasId = -1,
                        PortalAliasUsage = PortalAliasUsageType.Default,
                        QueryString = String.Empty,
                        Url = url,
                        HttpStatus = "200",
                        CultureCode = String.Empty,
                        IsSystem = true
                    };
                    //Save url
                    _tabController.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }
                else
                {
                    //Change the original 200 url to a redirect
                    tabUrl.HttpStatus = "301";
                    tabUrl.SeqNum = tab.TabUrls.Max(t => t.SeqNum) + 1;
                    _tabController.SaveTabUrl(tabUrl, portalSettings.PortalId, true);

                    //Add new custom url
                    tabUrl.Url = url;
                    tabUrl.HttpStatus = "200";
                    tabUrl.SeqNum = 0;
                    _tabController.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }


                //Delete any redirects to the same url
                foreach (var redirecturl in _tabController.GetTabUrls(tab.TabID, tab.PortalID))
                {
                    if (redirecturl.Url == url && redirecturl.HttpStatus != "200")
                    {
                        _tabController.DeleteTabUrl(redirecturl, tab.PortalID, true);
                    }
                }
            }
            else
            {
                if (url == "/" && tabUrl != null)
                {
                    _tabController.DeleteTabUrl(tabUrl, portalSettings.PortalId, true);
                }
            }
        }

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

            //Clean Url
            var options = UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(friendlyUrlSettings));
            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);

            return '/' + urlPath;
        }

        public void CopyThemeToDescendantPages(int pageId, Theme theme)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var portalId = portalSettings.PortalId;
            var tab = _tabController.GetTab(pageId, portalId, false);
            if (tab == null)
            {
                throw new PageNotFoundException();
            }

            TabController.CopyDesignToChildren(tab, theme.SkinSrc, theme.ContainerSrc);
        }

        public void CopyPermissionsToDescendantPages(int pageId)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var portalId = portalSettings.PortalId;
            var tab = _tabController.GetTab(pageId, portalId, false);
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

        public IEnumerable<Url> GetPageUrls(int tabId)
        {
            var tab = GetPageDetails(tabId);
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var portalId = portalSettings.PortalId;
            return _pageUrlsController.GetPageUrls(tab, portalId);
        }

        public PageSettings GetPageSettings(int pageId, PortalSettings requestPortalSettings = null)
        {
            var tab = GetPageDetails(pageId);

            var portalSettings = requestPortalSettings ?? _portalController.GetCurrentPortalSettings();

            if (!_contentVerifier.IsContentExistsForRequestedPortal(tab.PortalID, portalSettings))
            {
                throw new PageNotFoundException();
            }
           
            var page = Converters.ConvertToPageSettings<PageSettings>(tab);
            page.Modules = GetModules(page.TabId).Select(Converters.ConvertToModuleItem);
            page.PageUrls = GetPageUrls(page.TabId);
            page.Permissions = GetPermissionsData(pageId);
            page.SiteAliases = GetSiteAliases(portalSettings.PortalId);
            page.PrimaryAliasId = GetPrimaryAliasId(portalSettings.PortalId, portalSettings.CultureCode);
            page.Locales = GetLocales(portalSettings.PortalId);
            page.HasParent = tab.ParentId > -1;

            // icons
            var iconFile = string.IsNullOrEmpty(tab.IconFile) ? null : FileManager.Instance.GetFile(tab.PortalID, tab.IconFileRaw);
            if (iconFile != null) {
                page.IconFile = new FileDto
                {
                    fileId = iconFile.FileId,
                    fileName = iconFile.FileName,
                    folderId = iconFile.FolderId,
                    folderPath = iconFile.Folder
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
                    folderPath = iconFileLarge.Folder
                };
            }

            return page;
        }

        public PageUrlResult CreateCustomUrl(SeoUrl dto)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return _pageUrlsController.CreateCustomUrl(dto.SaveUrl, _tabController.GetTab(dto.TabId, portalSettings.PortalId, false));
        }

        public PageUrlResult UpdateCustomUrl(SeoUrl dto)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return _pageUrlsController.UpdateCustomUrl(dto.SaveUrl, _tabController.GetTab(dto.TabId, portalSettings.PortalId, false));
        }

        public PageUrlResult DeleteCustomUrl(UrlIdDto dto)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return _pageUrlsController.DeleteCustomUrl(dto.Id, _tabController.GetTab(dto.TabId, portalSettings.PortalId, false));
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
                                && (a.CultureCode == cultureCode || String.IsNullOrEmpty(a.CultureCode)))
                            .OrderByDescending(a => a.CultureCode)
                            .FirstOrDefault();
            return primary == null ? (int?)null : primary.KeyID;
        }

        public void CreateOrUpdateContentItem(TabInfo tab)
        {
            var contentController = Util.GetContentController();
            tab.Content = String.IsNullOrEmpty(tab.Title) ? tab.TabName : tab.Title;
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
            UpdateTabInfoFromPageSettings(tab, pageSettings);
            SavePagePermissions(tab, pageSettings.Permissions);

            _tabController.UpdateTab(tab);

            CreateOrUpdateContentItem(tab);

            SaveTabUrl(tab, pageSettings);

            MovePageIfNeeded(pageSettings, tab);

            return tab.TabID;
        }

        public void SavePagePermissions(TabInfo tab, PagePermissions permissions)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

            tab.TabPermissions.Clear();

            //add default permissions for administrators if needed
            if (!HasAdminPermissions(permissions))
            {
                //add default permissions
                var permissionsList = PermissionController.GetPermissionsByTab();
                foreach (var permissionInfo in permissionsList)
                {
                    var editPermisison = (PermissionInfo)permissionInfo;
                    var permission = new TabPermissionInfo(editPermisison)
                    {
                        RoleID = portalSettings.AdministratorRoleId,
                        AllowAccess = true,
                        RoleName = portalSettings.AdministratorRoleName
                    };
                    tab.TabPermissions.Add(permission);
                }
            }

            //add role permissions
            if (permissions.RolePermissions != null)
            {
                foreach (var rolePermission in permissions.RolePermissions.Where(NoLocked()))
                {
                    if (rolePermission.RoleId.ToString() == Globals.glbRoleAllUsers ||
                        RoleController.Instance.GetRoleById(portalSettings.PortalId, rolePermission.RoleId) != null)
                    {
                        foreach (var permission in rolePermission.Permissions)
                        {
                            tab.TabPermissions.Add(new TabPermissionInfo
                            {
                                PermissionID = permission.PermissionId,
                                RoleID = rolePermission.RoleId,
                                UserID = Null.NullInteger,
                                AllowAccess = permission.AllowAccess
                            });
                        }
                    }
                }
            }


            //add user permissions
            if (permissions.UserPermissions != null)
            {
                foreach (var userPermission in permissions.UserPermissions)
                {
                    var user = UserController.Instance.GetUserById(portalSettings.PortalId, userPermission.UserId);
                    if (user != null)
                    {
                        int roleId;
                        int.TryParse(Globals.glbRoleNothing, out roleId);
                        foreach (var permission in userPermission.Permissions)
                        {
                            tab.TabPermissions.Add(new TabPermissionInfo
                            {
                                PermissionID = permission.PermissionId,
                                RoleID = roleId,
                                UserID = userPermission.UserId,
                                AllowAccess = permission.AllowAccess
                            });
                        }
                    }
                }
            }
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

        public virtual PageSettings GetDefaultSettings(int pageId = 0)
        {
            var pageSettings = new PageSettings
            {
                Templates = _templateController.GetTemplates(),
                Permissions = GetPermissionsData(pageId)
            };

            pageSettings.TemplateId = _templateController.GetDefaultTemplateId(pageSettings.Templates);

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (PortalController.GetPortalSettingAsBoolean("SSLEnabled", portalSettings.PortalId, false) &&
                PortalController.GetPortalSettingAsBoolean("SSLEnforced", portalSettings.PortalId, false))
            {
                pageSettings.IsSecure = true;
            }

            return pageSettings;
        }

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
                        permissions.RolePermissions.OrderByDescending(p => p.Locked)
                            .ThenByDescending(p => p.IsDefault)
                            .ThenBy(p => p.RoleName)
                            .ToList();
                    permissions.UserPermissions = permissions.UserPermissions.OrderBy(p => p.DisplayName).ToList();
                }
            }

            return permissions;
        }

        public void DeleteTabModule(int pageId, int moduleId)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var tab = _tabController.GetTab(pageId, portalSettings.PortalId);
            if (tab == null)
            {
                throw new PageModuleNotFoundException();
            }

            var tabModule = _moduleController.GetModule(moduleId, pageId, false);
            if (tabModule == null)
            {
                throw new PageModuleNotFoundException();
            }

            if (!TabPermissionController.CanAddContentToPage(tab))
            {
                throw new SecurityException("You do not have permission to delete module on this page");
            }

            _moduleController.DeleteTabModule(pageId, moduleId, true);
            _moduleController.ClearCache(pageId);
        }


        public void CopyContentFromSourceTab(TabInfo tab, int sourceTabId, IEnumerable<ModuleItem> includedModules)
        {
            var sourceTab = _tabController.GetTab(sourceTabId, tab.PortalID);
            if (sourceTab == null || sourceTab.IsDeleted)
            {
                return;
            }
            //Copy Properties
            CopySourceTabProperties(tab, sourceTab);

            //Copy Modules
            CopyModulesFromSourceTab(tab, sourceTab, includedModules);
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
            tab.SkinSrc = sourceTab.SkinSrc.Equals(portalSettings.DefaultPortalSkin, StringComparison.OrdinalIgnoreCase) ? string.Empty : sourceTab.SkinSrc;
            tab.ContainerSrc = sourceTab.ContainerSrc;
            tab.IconFile = sourceTab.IconFile;
            tab.IconFileLarge = sourceTab.IconFileLarge;
            tab.PageHeadText = sourceTab.PageHeadText;
            tab.RefreshInterval = sourceTab.RefreshInterval;
            tab.IsSecure = sourceTab.IsSecure;
            _tabController.UpdateTab(tab);

            //update need tab settings.
            foreach (var key in TabSettingKeys)
            {
                if (sourceTab.TabSettings.ContainsKey(key))
                {
                    _tabController.UpdateTabSetting(tab.TabID, key, Convert.ToString(sourceTab.TabSettings[key]));
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
                    //Clone module as it exists in the cache and changes we make will update the cached object
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

                    //copy permissions from source module
                    foreach (ModulePermissionInfo permission in objModule.ModulePermissions)
                    {
                        newModule.ModulePermissions.Add(new ModulePermissionInfo
                        {
                            ModuleID = newModule.ModuleID,
                            PermissionID = permission.PermissionID,
                            RoleID = permission.RoleID,
                            UserID = permission.UserID,
                            PermissionKey = permission.PermissionKey,
                            AllowAccess = permission.AllowAccess
                        }, true);
                    }

                    ModulePermissionController.SaveModulePermissions(newModule);

                    if (copyType == ModuleCopyType.Copy)
                    {
                        if (!string.IsNullOrEmpty(newModule.DesktopModule.BusinessControllerClass))
                        {
                            var objObject = Reflection.CreateObject(newModule.DesktopModule.BusinessControllerClass, newModule.DesktopModule.BusinessControllerClass);
                            var o = objObject as IPortable;
                            if (o != null)
                            {
                                try
                                {
                                    _cloneModuleExecutionContext.SetCloneModuleContext(true);
                                    var content = Convert.ToString(o.ExportModule(module.Id));
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        o.ImportModule(newModule.ModuleID, content, newModule.DesktopModule.Version, UserController.Instance.GetCurrentUserInfo().UserID);
                                    }
                                }
                                finally
                                {
                                    _cloneModuleExecutionContext.SetCloneModuleContext(false);
                                }
                            }
                        }
                    }
                }

                if (copyType != ModuleCopyType.Reference && objModule != null)
                {
                    //Make reference copies on secondary language
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

        private static bool HasTags(string tags, IEnumerable<Term> terms)
        {
            return tags.Split(',').All(tag => terms.Any(t => string.Compare(t.Name, tag, StringComparison.CurrentCultureIgnoreCase) == 0));
        }
    }
}
