#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Helper;
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
using DotNetNuke.Services.Personalization;

namespace Dnn.PersonaBar.Pages.Components
{
    public class PagesController : ServiceLocator<IPagesController, PagesController>, IPagesController
    {
        private readonly ITabController _tabController;
        private readonly IModuleController _moduleController;
        private static readonly IList<string> TabSettingKeys = new List<string> { "CustomStylesheet" };

        public PagesController()
        {
            _tabController = TabController.Instance;
            _moduleController = ModuleController.Instance;
        }

        private static PortalSettings PortalSettings => PortalSettings.Current;

        public bool IsValidTabPath(TabInfo tab, string newTabPath, out string errorMessage)
        {
            var valid = true;
            errorMessage = string.Empty;

            //get default culture if the tab's culture is null
            var cultureCode = tab != null ? tab.CultureCode : string.Empty;
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = PortalSettings.DefaultLanguage;
            }

            //Validate Tab Path
            var tabId = TabController.GetTabByTabPath(PortalSettings.PortalId, newTabPath, cultureCode);
            if (tabId != Null.NullInteger && (tab == null || tabId != tab.TabID))
            {
                var existingTab = _tabController.GetTab(tabId, PortalSettings.PortalId, false);
                if (existingTab != null && existingTab.IsDeleted)
                {
                    errorMessage = "TabRecycled";
                }
                else
                {
                    errorMessage = "TabExists";
                }

                valid = false;
            }

            //check whether have conflict between tab path and portal alias.
            if (TabController.IsDuplicateWithPortalAlias(PortalSettings.PortalId, newTabPath))
            {
                errorMessage = "PathDuplicateWithAlias";
                valid = false;
            }

            return valid;
        }
        
        public List<int> GetPageHierarchy(int pageId)
        {
            var tab = TabController.Instance.GetTab(pageId, PortalSettings.PortalId);
            if (tab == null)
            {
                throw new PageNotFoundException();
            }

            var paths = new List<int> { tab.TabID };
            while (tab.ParentId != Null.NullInteger)
            {
                tab = TabController.Instance.GetTab(tab.ParentId, PortalSettings.PortalId);
                if (tab != null)
                {
                    paths.Insert(0, tab.TabID);
                }
            }

            return paths;
        }


        public TabInfo MovePage(PageMoveRequest request)
        {
            var tab = TabController.Instance.GetTab(request.PageId, PortalSettings.PortalId);
            if (tab == null)
            {                
                throw new PageNotFoundException();
            }

            if (tab.ParentId != request.ParentId)
            {
                string errorMessage;

                if (!IsValidTabPath(tab, Globals.GenerateTabPath(request.ParentId, tab.TabName), out errorMessage))
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
                    if (IsChild(PortalSettings.PortalId, tab.TabID, request.ParentId))
                    {
                        throw new PageException("DragInvalid");
                    }

                    TabController.Instance.MoveTabToParent(tab, request.ParentId);
                    break;
            }

            //as tab's parent may changed, url need refresh.
            return TabController.Instance.GetTab(request.PageId, PortalSettings.PortalId);            
        }

        public void DeletePage(PageItem page)
        {
            var tab = TabController.Instance.GetTab(page.Id, PortalSettings.PortalId);
            if (tab == null)
            {
                throw new PageNotFoundException();
            }

            if (TabPermissionController.CanDeletePage(tab))
            {
                TabController.Instance.SoftDeleteTab(tab.TabID, PortalSettings);
            }

        }

        public void EditModeForPage(int pageId, int userId)
        {
            var newCookie = new HttpCookie("LastPageId", $"{PortalSettings.PortalId}:{pageId}")
            {
                Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/")
            };
            HttpContext.Current.Response.Cookies.Add(newCookie);

            if (PortalSettings.UserMode != PortalSettings.Mode.Edit)
            {
                var personalizationController = new PersonalizationController();
                var personalization = personalizationController.LoadProfile(userId, PortalSettings.PortalId);
                personalization.Profile["Usability:UserMode" + PortalSettings.PortalId] = "EDIT";
                personalization.IsModified = true;
                personalizationController.SaveProfile(personalization);
            }
        }

        public TabInfo SavePageDetails(PageSettings pageSettings)
        {
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
            if (!ValidatePageSettingsData(pageSettings, tab, out field, out errorMessage))
            {
                throw new PageValidationException(field, errorMessage);
            }

            var tabId = pageSettings.TabId <= 0
                ? AddTab(pageSettings)
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

        public IEnumerable<TabInfo> GetPageList(int parentId = -1, string searchKey = "")
        {
            var adminTabId = PortalSettings.AdminTabId;

            var tabs = TabController.GetPortalTabs(PortalSettings.PortalId, adminTabId, false, true, false, true);
            var pages = from t in tabs
                        where (t.ParentId != adminTabId) &&
                                !t.IsSystem &&
                                    ((string.IsNullOrEmpty(searchKey) && (t.ParentId == parentId))
                                        || (!string.IsNullOrEmpty(searchKey) &&
                                                (t.TabName.IndexOf(searchKey, StringComparison.InvariantCultureIgnoreCase) > Null.NullInteger
                                                    || t.LocalizedTabName.IndexOf(searchKey, StringComparison.InvariantCultureIgnoreCase) > Null.NullInteger)))
                        select t;

            return pages;
        }

        public TabInfo GetPageDetails(int pageId)
        {
            var tab = TabController.Instance.GetTab(pageId, PortalSettings.PortalId);
            if (tab == null)
            {
                throw new PageNotFoundException();
            }

            return tab;
        }

        public IEnumerable<ModuleInfo> GetModules(int pageId)
        {
            var dic = ModuleController.Instance.GetTabModules(pageId);
            var modules = dic.Values.Where(objModule => objModule.IsDeleted == false);
            return modules;
        }

        public bool ValidatePageSettingsData(PageSettings pageSettings, TabInfo tab, out string invalidField, out string errorMessage)
        {
            errorMessage = string.Empty;
            invalidField = string.Empty;

            var isValid = !string.IsNullOrEmpty(pageSettings.Name) && TabController.IsValidTabName(pageSettings.Name, out errorMessage);
            if (!isValid)
            {
                invalidField = pageSettings.PageType == "template" ? "templateName" : "name";
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "EmptyTabName";
                }
                return false;
            }

            var parentId = tab != null ? tab.ParentId : Null.NullInteger;
            if (pageSettings.PageType == "template")
            {
                parentId = GetTemplateParentId(tab?.PortalID ?? PortalSettings.PortalId);
            }

            isValid = IsValidTabPath(tab, Globals.GenerateTabPath(parentId, pageSettings.Name), out errorMessage);
            if (!isValid)
            {
                invalidField = pageSettings.PageType == "template" ? "templateName" : "name";
                errorMessage = (pageSettings.PageType == "template" ? "templates_" : "") + errorMessage;
                return false;
            }

            if (pageSettings.StartDate.HasValue && pageSettings.EndDate.HasValue && pageSettings.StartDate > pageSettings.EndDate)
            {
                errorMessage = "StartDateAfterEndDate";
                invalidField = "endDate";
                return false;
            }

            return ValidatePageUrlSettings(pageSettings, tab, ref invalidField, ref errorMessage);
        }

        protected int GetTemplateParentId(int tabId)
        {
            return Null.NullInteger;
        }

        private bool ValidatePageUrlSettings(PageSettings pageSettings, TabInfo tab, ref string invalidField, ref string errorMessage)
        {
            var urlPath = !string.IsNullOrEmpty(pageSettings.Url) ? pageSettings.Url.TrimStart('/') : string.Empty;

            if (string.IsNullOrEmpty(urlPath))
            {
                return true;
            }

            bool modified;
            //Clean Url
            var options = UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(PortalSettings.PortalId)));
            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);
            if (modified)
            {
                errorMessage = "UrlPathCleaned";
                invalidField = "url";
                return false;
            }

            //Validate for uniqueness
            urlPath = FriendlyUrlController.ValidateUrl(urlPath, tab != null ? tab.TabID : Null.NullInteger, PortalSettings, out modified);
            if (modified)
            {
                errorMessage = "UrlPathNotUnique";
                invalidField = "url";
                return false;
            }

            return true;
        }

        public int AddTab(PageSettings pageSettings)
        {
            var portalId = PortalSettings.PortalId;
            var tab = new TabInfo { PortalID = portalId };
            UpdateTabInfoFromPageSettings(tab, pageSettings);

            if (PortalSettings.ContentLocalizationEnabled)
            {
                tab.CultureCode = PortalSettings.CultureCode;
            }

            SavePagePermissions(tab, pageSettings.Permissions);

            var tabId = _tabController.AddTab(tab);
            tab = _tabController.GetTab(tabId, portalId);
            
            AddTabExtension(tab, pageSettings);

            CreateOrUpdateContentItem(tab);

            if (pageSettings.TemplateId > 0)
            {
                CopyContentFromSourceTab(tab, pageSettings.TemplateId);
            }

            SaveTabUrl(tab, pageSettings);

            _tabController.ClearCache(portalId);
            return tab.TabID;
        }

        protected void AddTabExtension(TabInfo tab, PageSettings pageSettings)
        {
            
        }

        private void UpdateTabInfoFromPageSettings(TabInfo tab, PageSettings pageSettings)
        {
            tab.TabName = pageSettings.Name;
            tab.TabPath = Globals.GenerateTabPath(tab.ParentId, tab.TabName);
            tab.Title = pageSettings.Title;
            tab.Description = GetTabDescription(pageSettings);
            tab.KeyWords = GetKeyWords(pageSettings);
            tab.IsVisible = pageSettings.IncludeInMenu;

            tab.StartDate = pageSettings.StartDate ?? Null.NullDate;
            tab.EndDate = pageSettings.EndDate ?? Null.NullDate;

            tab.IsSecure = pageSettings.IsSecure;
            tab.TabSettings["AllowIndex"] = pageSettings.AllowIndex;

            tab.TabSettings["CacheProvider"] = pageSettings.CacheProvider;
            tab.TabSettings["CacheDuration"] = pageSettings.CacheDuration;
            tab.TabSettings["CacheIncludeExclude"] = pageSettings.CacheIncludeExclude;
            tab.TabSettings["IncludeVaryBy"] = pageSettings.CacheIncludeVaryBy;
            tab.TabSettings["ExcludeVaryBy"] = pageSettings.CacheExcludeVaryBy;
            tab.TabSettings["MaxVaryByCount"] = pageSettings.CacheMaxVaryByCount;

            if (pageSettings.PageType == "template")
            {
                tab.ParentId = GetTemplateParentId(tab.PortalID);
                tab.IsSystem = true;
            }

            tab.TabSettings["Evoq_TrackLinks"] = pageSettings.TrackLinks;

            tab.Terms.Clear();
            if (!string.IsNullOrEmpty(pageSettings.Tags))
            {
                tab.Terms.Clear();
                var termController = new TermController();
                var vocabularyController = Util.GetVocabularyController();
                var vocabulary = (vocabularyController.GetVocabularies()
                                    .Cast<Vocabulary>()
                                    .Where(v => v.Name == Constants.PageTagsVocabulary))
                                    .SingleOrDefault();

                var vocabularyId = Null.NullInteger;
                if (vocabulary == null)
                {
                    var scopeType = Util.GetScopeTypeController().GetScopeTypes().SingleOrDefault(s => s.ScopeType == "Portal");
                    if (scopeType == null)
                    {
                        throw new Exception("Can't create default vocabulary as scope type 'Portal' can't finded.");
                    }

                    vocabularyId = vocabularyController.AddVocabulary(
                        new Vocabulary(Constants.PageTagsVocabulary, string.Empty, VocabularyType.Simple)
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
                                   where (v.ScopeType.ScopeType == "Portal" && v.ScopeId == tab.PortalID && !v.Name.Equals("Tags", StringComparison.InvariantCultureIgnoreCase))
                                   select v;
                foreach (var v in vocabularies)
                {
                    allTerms.AddRange(termController.GetTermsByVocabulary(v.VocabularyId));
                }

                foreach (var tag in pageSettings.Tags.Trim().Split(','))
                {
                    if (!string.IsNullOrEmpty(tag))
                    {
                        var term = allTerms.FirstOrDefault(t => t.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
                        if (term == null)
                        {
                            var termId = termController.AddTerm(new Term(tag, string.Empty, vocabularyId));
                            term = termController.GetTerm(termId);
                        }

                        tab.Terms.Add(term);
                    }
                }
            }
        }

        /// <summary>
        /// If the tab description is equal to the portal description
        /// we store null so the system will serve the portal description instead
        /// </summary>
        /// <param name="pageSettings"></param>
        /// <returns>Tab Description value to be stored</returns>
        private string GetTabDescription(PageSettings pageSettings)
        {
            return pageSettings.Description != PortalSettings.Description
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
            return pageSettings.Keywords != PortalSettings.KeyWords
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

            if (!String.IsNullOrEmpty(url) && url != "/")
            {
                url = CleanTabUrl(url);

                string currentUrl = String.Empty;
                var friendlyUrlSettings = new FriendlyUrlSettings(PortalSettings.PortalId);
                if (tab.TabID > -1)
                {
                    var baseUrl = Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias) + "/Default.aspx?TabId=" + tab.TabID;
                    var path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(tab,
                        baseUrl,
                        Globals.glbDefaultPage,
                        PortalSettings.PortalAlias.HTTPAlias,
                        false,
                        friendlyUrlSettings,
                        Guid.Empty);

                    currentUrl = path.Replace(Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias), "");
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
                    _tabController.SaveTabUrl(tabUrl, PortalSettings.PortalId, true);
                }
                else
                {
                    //Change the original 200 url to a redirect
                    tabUrl.HttpStatus = "301";
                    tabUrl.SeqNum = tab.TabUrls.Max(t => t.SeqNum) + 1;
                    _tabController.SaveTabUrl(tabUrl, PortalSettings.PortalId, true);

                    //Add new custom url
                    tabUrl.Url = url;
                    tabUrl.HttpStatus = "200";
                    tabUrl.SeqNum = 0;
                    _tabController.SaveTabUrl(tabUrl, PortalSettings.PortalId, true);
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
                if (tabUrl != null)
                {
                    _tabController.DeleteTabUrl(tabUrl, PortalSettings.PortalId, true);
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

            var friendlyUrlSettings = new FriendlyUrlSettings(PortalSettings.PortalId);
            urlPath = UrlRewriterUtils.CleanExtension(urlPath, friendlyUrlSettings, string.Empty);

            //Clean Url
            var options = UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(friendlyUrlSettings));
            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);

            return '/' + urlPath;
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

        public void CopyContentFromSourceTab(TabInfo tab, int sourceTabId)
        {
            var sourceTab = _tabController.GetTab(sourceTabId, tab.PortalID);
            if (sourceTab == null || sourceTab.IsDeleted)
            {
                return;
            }
            //Copy Properties
            CopySourceTabProperties(tab, sourceTab);

            //Copy Modules
            CopyModulesFromSourceTab(tab, sourceTab);
        }

        private void CopySourceTabProperties(TabInfo tab, TabInfo sourceTab)
        {
            tab.SkinSrc = sourceTab.SkinSrc.Equals(PortalSettings.DefaultPortalSkin, StringComparison.InvariantCultureIgnoreCase) ? string.Empty : sourceTab.SkinSrc;
            tab.ContainerSrc = sourceTab.ContainerSrc;
            tab.IconFile = sourceTab.IconFile;
            tab.IconFileLarge = sourceTab.IconFileLarge;
            tab.PageHeadText = sourceTab.PageHeadText;
            tab.RefreshInterval = sourceTab.RefreshInterval;

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

        private void CopyModulesFromSourceTab(TabInfo tab, TabInfo sourceTab)
        {
            foreach (var module in sourceTab.ChildModules.Values)
            {
                if (module.IsDeleted || module.AllTabs)
                {
                    continue;
                }

                var newModule = module.Clone();

                newModule.TabID = tab.TabID;
                newModule.DefaultLanguageGuid = Null.NullGuid;
                newModule.CultureCode = tab.CultureCode;
                newModule.VersionGuid = Guid.NewGuid();
                newModule.LocalizedVersionGuid = Guid.NewGuid();

                newModule.ModuleID = Null.NullInteger;
                _moduleController.InitialModulePermission(newModule, newModule.TabID, 0);
                newModule.InheritViewPermissions = module.InheritViewPermissions;

                newModule.ModuleID = _moduleController.AddModule(newModule);

                //Copy each setting to the new TabModule instance
                foreach (DictionaryEntry setting in module.ModuleSettings)
                {
                    _moduleController.UpdateModuleSetting(newModule.ModuleID, Convert.ToString(setting.Key), Convert.ToString(setting.Value));
                }

                foreach (DictionaryEntry setting in module.TabModuleSettings)
                {
                    _moduleController.UpdateTabModuleSetting(newModule.TabModuleID, Convert.ToString(setting.Key), Convert.ToString(setting.Value));
                }

                //copy permissions from source module
                foreach (ModulePermissionInfo permission in module.ModulePermissions)
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

                if (!string.IsNullOrEmpty(newModule.DesktopModule.BusinessControllerClass))
                {
                    var moduleBizClass = Reflection.CreateObject(newModule.DesktopModule.BusinessControllerClass, newModule.DesktopModule.BusinessControllerClass) as IPortable;
                    if (moduleBizClass != null)
                    {
                        var content = Convert.ToString(moduleBizClass.ExportModule(module.ModuleID));
                        if (!string.IsNullOrEmpty(content))
                        {
                            content = XmlUtils.RemoveInvalidXmlCharacters(content);
                            moduleBizClass.ImportModule(newModule.ModuleID, content, newModule.DesktopModule.Version, UserController.Instance.GetCurrentUserInfo().UserID);
                        }
                    }
                }
            }
        }

        public int UpdateTab(TabInfo tab, PageSettings pageSettings)
        {
            UpdateTabInfoFromPageSettings(tab, pageSettings);
            UpdateTabExtension(tab, pageSettings);
            SavePagePermissions(tab, pageSettings.Permissions);

            _tabController.UpdateTab(tab);

            CreateOrUpdateContentItem(tab);

            SaveTabUrl(tab, pageSettings);
            
            return tab.TabID;
        }

        
        public void SavePagePermissions(TabInfo tab, PagePermissions permissions)
        {
            var hasAdmin = permissions.RolePermissions == null ? false : permissions.RolePermissions.Any(permission => permission.RoleId == PortalSettings.AdministratorRoleId);

            tab.TabPermissions.Clear();

            //add default permissions for administrators
            if (!hasAdmin || (permissions.RolePermissions.Count == 0 && permissions.UserPermissions.Count == 0))
            {
                //add default permissions
                var permissionController = new PermissionController();
                var permissionsList = permissionController.GetPermissionByCodeAndKey("SYSTEM_TAB", "VIEW");
                permissionsList.AddRange(permissionController.GetPermissionByCodeAndKey("SYSTEM_TAB", "EDIT"));
                foreach (var permissionInfo in permissionsList)
                {
                    var editPermisison = (PermissionInfo)permissionInfo;
                    var permission = new TabPermissionInfo(editPermisison)
                    {
                        RoleID = PortalSettings.AdministratorRoleId,
                        AllowAccess = true,
                        RoleName = PortalSettings.AdministratorRoleName
                    };
                    tab.TabPermissions.Add(permission);

                }
            }

            //add role permissions
            if (permissions.RolePermissions != null)
            {
                foreach (var rolePermission in permissions.RolePermissions)
                {
                    foreach (var permission in rolePermission.Permissions)
                    {
                        tab.TabPermissions.Add(new TabPermissionInfo()
                        {
                            PermissionID = permission.PermissionId,
                            RoleID = rolePermission.RoleId,
                            UserID = Null.NullInteger,
                            AllowAccess = permission.AllowAccess
                        });
                    }
                }
            }


            //add user permissions
            if (permissions.UserPermissions != null)
            {
                foreach (var userPermission in permissions.UserPermissions)
                {
                    foreach (var permission in userPermission.Permissions)
                    {
                        tab.TabPermissions.Add(new TabPermissionInfo()
                        {
                            PermissionID = permission.PermissionId,
                            RoleID = int.Parse(Globals.glbRoleNothing),
                            UserID = userPermission.UserId,
                            AllowAccess = permission.AllowAccess
                        });
                    }
                }
            }
        }


        protected void UpdateTabExtension(TabInfo tab, PageSettings pageSettings)
        {
            
        }

        public PagePermissions GetPermissionsData(int pageId)
        {
            var permissions = new PagePermissions(true);
            if (pageId > 0)
            {
                var tab = TabController.Instance.GetTab(pageId, PortalSettings.PortalId);
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

        protected override Func<IPagesController> GetFactory()
        {
            return () => new PagesController();
        }
    }
}