// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Http;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Search.Controllers;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.InternalServices.Views.Search;

    [DnnAuthorize(StaticRoles = "Administrators")]
    public class SearchServiceController : DnnApiController
    {
        private const string ModuleInfosCacheKey = "ModuleInfos{0}";
        private const CacheItemPriority ModuleInfosCachePriority = CacheItemPriority.AboveNormal;
        private const int ModuleInfosCacheTimeOut = 20;
        private const string ModuleTitleCacheKey = "SearchModuleTabTitle_{0}";
        private const CacheItemPriority ModuleTitleCachePriority = CacheItemPriority.Normal;
        private const int ModuleTitleCacheTimeOut = 20;

        private static readonly Regex GroupedBasicViewRegex = new Regex("userid(/|\\|=)(\\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private int HtmlModuleDefitionId;

        public SearchServiceController()
        {
            var modDef = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Text/HTML");
            this.HtmlModuleDefitionId = modDef != null ? modDef.ModuleDefID : -1;
        }

        // this constructor is for unit tests
        internal SearchServiceController(int htmlModuleDefitionId) // , TabController newtabController, ModuleController newmoduleController)
        {
            this.HtmlModuleDefitionId = htmlModuleDefitionId;

            // _tabController = newtabController;
            // _moduleController = newmoduleController;
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Preview(string keywords, string culture, int forceWild = 1, int portal = -1)
        {
            string cleanedKeywords;
            keywords = (keywords ?? string.Empty).Trim();
            var tags = SearchQueryStringParser.Instance.GetTags(keywords, out cleanedKeywords);
            var beginModifiedTimeUtc = SearchQueryStringParser.Instance.GetLastModifiedDate(cleanedKeywords, out cleanedKeywords);
            var searchTypes = SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out cleanedKeywords);

            var contentSources = this.GetSearchContentSources(searchTypes);
            var settings = this.GetSearchModuleSettings();
            var searchTypeIds = GetSearchTypeIds(settings, contentSources);
            var moduleDefids = GetSearchModuleDefIds(settings, contentSources);
            var portalIds = this.GetSearchPortalIds(settings, portal);

            var userSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId;
            var userSearchSource = contentSources.FirstOrDefault(s => s.SearchTypeId == userSearchTypeId);

            var results = new List<GroupedBasicView>();
            if (portalIds.Any() && searchTypeIds.Any() &&
                (!string.IsNullOrEmpty(cleanedKeywords) || tags.Any()))
            {
                var query = new SearchQuery
                {
                    KeyWords = cleanedKeywords,
                    Tags = tags,
                    PortalIds = portalIds,
                    SearchTypeIds = searchTypeIds,
                    ModuleDefIds = moduleDefids,
                    BeginModifiedTimeUtc = beginModifiedTimeUtc,
                    PageIndex = 1,
                    PageSize = 5,
                    TitleSnippetLength = 40,
                    BodySnippetLength = 100,
                    CultureCode = culture,
                    WildCardSearch = forceWild > 0,
                };

                try
                {
                    results = this.GetGroupedBasicViews(query, userSearchSource, this.PortalSettings.PortalId);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, results);
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Search(string search, string culture, int pageIndex, int pageSize, int sortOption)
        {
            string cleanedKeywords;
            search = (search ?? string.Empty).Trim();
            var tags = SearchQueryStringParser.Instance.GetTags(search, out cleanedKeywords);
            var beginModifiedTimeUtc = SearchQueryStringParser.Instance.GetLastModifiedDate(cleanedKeywords, out cleanedKeywords);
            var searchTypes = SearchQueryStringParser.Instance.GetSearchTypeList(cleanedKeywords, out cleanedKeywords);

            var contentSources = this.GetSearchContentSources(searchTypes);
            var settings = this.GetSearchModuleSettings();
            var searchTypeIds = GetSearchTypeIds(settings, contentSources);
            var moduleDefids = GetSearchModuleDefIds(settings, contentSources);
            var portalIds = this.GetSearchPortalIds(settings, -1);
            var userSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId;

            var more = false;
            var totalHits = 0;
            var results = new List<GroupedDetailView>();
            if (portalIds.Any() && searchTypeIds.Any() &&
                (!string.IsNullOrEmpty(cleanedKeywords) || tags.Any()))
            {
                var query = new SearchQuery
                {
                    KeyWords = cleanedKeywords,
                    Tags = tags,
                    PortalIds = portalIds,
                    SearchTypeIds = searchTypeIds,
                    ModuleDefIds = moduleDefids,
                    BeginModifiedTimeUtc = beginModifiedTimeUtc,
                    EndModifiedTimeUtc = beginModifiedTimeUtc > DateTime.MinValue ? DateTime.MaxValue : DateTime.MinValue,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    SortField = (SortFields)sortOption,
                    TitleSnippetLength = 120,
                    BodySnippetLength = 300,
                    CultureCode = culture,
                    WildCardSearch = this.IsWildCardEnabledForModule(),
                };

                try
                {
                    results = this.GetGroupedDetailViews(query, userSearchTypeId, out totalHits, out more).ToList();
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { results, totalHits, more });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedModules("SearchAdmin")]
        public HttpResponseMessage AddSynonymsGroup(SynonymsGroupDto synonymsGroup)
        {
            string duplicateWord;
            var synonymsGroupId = SearchHelper.Instance.AddSynonymsGroup(synonymsGroup.Tags, synonymsGroup.PortalId, synonymsGroup.Culture, out duplicateWord);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Id = synonymsGroupId, DuplicateWord = duplicateWord });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedModules("SearchAdmin")]
        public HttpResponseMessage UpdateSynonymsGroup(SynonymsGroupDto synonymsGroup)
        {
            string duplicateWord;
            var synonymsGroupId = SearchHelper.Instance.UpdateSynonymsGroup(synonymsGroup.Id, synonymsGroup.Tags, synonymsGroup.PortalId, synonymsGroup.Culture, out duplicateWord);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Id = synonymsGroupId, DuplicateWord = duplicateWord });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedModules("SearchAdmin")]
        public HttpResponseMessage DeleteSynonymsGroup(SynonymsGroupDto synonymsGroup)
        {
            SearchHelper.Instance.DeleteSynonymsGroup(synonymsGroup.Id, synonymsGroup.PortalId, synonymsGroup.Culture);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedModules("SearchAdmin")]
        public HttpResponseMessage AddStopWords(StopWordsDto stopWords)
        {
            var stopWordsId = SearchHelper.Instance.AddSearchStopWords(stopWords.Words, stopWords.PortalId, stopWords.Culture);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Id = stopWordsId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedModules("SearchAdmin")]
        public HttpResponseMessage UpdateStopWords(StopWordsDto stopWords)
        {
            var stopWordsId = SearchHelper.Instance.UpdateSearchStopWords(stopWords.Id, stopWords.Words, stopWords.PortalId, stopWords.Culture);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Id = stopWordsId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedModules("SearchAdmin")]
        public HttpResponseMessage DeleteStopWords(StopWordsDto stopWords)
        {
            SearchHelper.Instance.DeleteSearchStopWords(stopWords.Id, stopWords.PortalId, stopWords.Culture);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        internal IEnumerable<GroupedDetailView> GetGroupedDetailViews(SearchQuery searchQuery, int userSearchTypeId, out int totalHits, out bool more)
        {
            var searchResults = SearchController.Instance.SiteSearch(searchQuery);
            totalHits = searchResults.TotalHits;
            more = totalHits > searchQuery.PageSize * searchQuery.PageIndex;

            var groups = new List<GroupedDetailView>();
            var tabGroups = new Dictionary<string, IList<SearchResult>>();

            foreach (var result in searchResults.Results)
            {
                // var key = result.TabId + result.Url;
                var key = result.Url;
                if (!tabGroups.ContainsKey(key))
                {
                    tabGroups.Add(key, new List<SearchResult> { result });
                }
                else
                {
                    // when the result is a user search type, we should only show one result
                    // and if duplicate, we should also reduce the totalHit number.
                    if (result.SearchTypeId != userSearchTypeId ||
                        tabGroups[key].All(r => r.Url != result.Url))
                    {
                        tabGroups[key].Add(result);
                    }
                    else
                    {
                        totalHits--;
                    }
                }
            }

            var showFriendlyTitle = this.ActiveModule == null
                                    || !this.ActiveModule.ModuleSettings.ContainsKey("ShowFriendlyTitle")
                                    || Convert.ToBoolean(this.ActiveModule.ModuleSettings["ShowFriendlyTitle"]);
            foreach (var results in tabGroups.Values)
            {
                var group = new GroupedDetailView();

                // first entry
                var first = results[0];
                @group.Title = showFriendlyTitle ? this.GetFriendlyTitle(first) : first.Title;
                @group.DocumentUrl = first.Url;

                // Find a different title for multiple entries with same url
                if (results.Count > 1)
                {
                    if (first.TabId > 0)
                    {
                        var tab = TabController.Instance.GetTab(first.TabId, first.PortalId, false);
                        if (tab != null)
                        {
                            @group.Title = showFriendlyTitle && !string.IsNullOrEmpty(tab.Title) ? tab.Title : tab.TabName;
                        }
                    }
                    else if (first.ModuleId > 0)
                    {
                        var tabTitle = this.GetTabTitleFromModuleId(first.ModuleId);
                        if (!string.IsNullOrEmpty(tabTitle))
                        {
                            @group.Title = tabTitle;
                        }
                    }
                }
                else if (first.ModuleDefId > 0 && first.ModuleDefId == this.HtmlModuleDefitionId) // special handling for Html module
                {
                    var tabTitle = this.GetTabTitleFromModuleId(first.ModuleId);
                    if (!string.IsNullOrEmpty(tabTitle))
                    {
                        @group.Title = tabTitle;
                        if (first.Title != "Enter Title" && first.Title != "Text/HTML")
                        {
                            @group.Title += " > " + first.Title;
                        }

                        first.Title = @group.Title;
                    }
                }

                foreach (var result in results)
                {
                    var title = showFriendlyTitle ? this.GetFriendlyTitle(result) : result.Title;
                    var detail = new DetailedView
                    {
                        Title = title != null && title.Contains("<") ? HttpUtility.HtmlEncode(title) : title,
                        DocumentTypeName = InternalSearchController.Instance.GetSearchDocumentTypeDisplayName(result),
                        DocumentUrl = result.Url,
                        Snippet = result.Snippet,
                        Description = result.Description,
                        DisplayModifiedTime = result.DisplayModifiedTime,
                        Tags = result.Tags.ToList(),
                        AuthorProfileUrl = result.AuthorUserId > 0 ? Globals.UserProfileURL(result.AuthorUserId) : string.Empty,
                        AuthorName = result.AuthorName,
                    };
                    @group.Results.Add(detail);
                }

                groups.Add(@group);
            }

            return groups;
        }

        internal List<GroupedBasicView> GetGroupedBasicViews(SearchQuery query, SearchContentSource userSearchSource, int portalId)
        {
            int totalHists;
            var results = new List<GroupedBasicView>();
            var previews = this.GetBasicViews(query, out totalHists);

            foreach (var preview in previews)
            {
                // if the document type is user, then try to add user pic into preview's custom attributes.
                if (userSearchSource != null && preview.DocumentTypeName == userSearchSource.LocalizedName)
                {
                    var match = GroupedBasicViewRegex.Match(preview.DocumentUrl);
                    if (match.Success)
                    {
                        var userid = Convert.ToInt32(match.Groups[2].Value);
                        var user = UserController.Instance.GetUserById(portalId, userid);
                        if (user != null)
                        {
                            preview.Attributes.Add("Avatar", user.Profile.PhotoURL);
                        }
                    }
                }

                var groupedResult = results.SingleOrDefault(g => g.DocumentTypeName == preview.DocumentTypeName);
                if (groupedResult != null)
                {
                    if (!groupedResult.Results.Any(r => string.Equals(r.DocumentUrl, preview.DocumentUrl)))
                    {
                        groupedResult.Results.Add(new BasicView
                        {
                            Title = preview.Title.Contains("<") ? HttpUtility.HtmlEncode(preview.Title) : preview.Title,
                            Snippet = preview.Snippet,
                            Description = preview.Description,
                            DocumentUrl = preview.DocumentUrl,
                            Attributes = preview.Attributes,
                        });
                    }
                }
                else
                {
                    results.Add(new GroupedBasicView(preview));
                }
            }

            return results;
        }

        internal IEnumerable<BasicView> GetBasicViews(SearchQuery searchQuery, out int totalHits)
        {
            var sResult = SearchController.Instance.SiteSearch(searchQuery);
            totalHits = sResult.TotalHits;
            var showFriendlyTitle = this.GetBooleanSetting("ShowFriendlyTitle", true);
            var showDescription = this.GetBooleanSetting("ShowDescription", true);
            var showSnippet = this.GetBooleanSetting("ShowSnippet", true);
            var maxDescriptionLength = this.GetIntegerSetting("MaxDescriptionLength", 100);

            return sResult.Results.Select(result =>
            {
                var description = result.Description;
                if (!string.IsNullOrEmpty(description) && description.Length > maxDescriptionLength)
                {
                    description = description.Substring(0, maxDescriptionLength) + "...";
                }

                return new BasicView
                {
                    Title = this.GetTitle(result, showFriendlyTitle),
                    DocumentTypeName = InternalSearchController.Instance.GetSearchDocumentTypeDisplayName(result),
                    DocumentUrl = result.Url,
                    Snippet = showSnippet ? result.Snippet : string.Empty,
                    Description = showDescription ? description : string.Empty,
                };
            });
        }

        private static ArrayList GetModulesByDefinition(int portalID, string friendlyName)
        {
            var cacheKey = string.Format(ModuleInfosCacheKey, portalID);
            return CBO.GetCachedObject<ArrayList>(
                new CacheItemArgs(cacheKey, ModuleInfosCacheTimeOut, ModuleInfosCachePriority),
                args => CBO.FillCollection(DataProvider.Instance().GetModuleByDefinition(portalID, friendlyName), typeof(ModuleInfo)));
        }

        private static List<int> GetSearchTypeIds(IDictionary settings, IEnumerable<SearchContentSource> searchContentSources)
        {
            var list = new List<int>();
            var configuredList = new List<string>();
            if (settings != null && !string.IsNullOrEmpty(Convert.ToString(settings["ScopeForFilters"])))
            {
                configuredList = Convert.ToString(settings["ScopeForFilters"]).Split('|').ToList();
            }

            // check content source in configured list or not
            foreach (var contentSource in searchContentSources)
            {
                if (contentSource.IsPrivate)
                {
                    continue;
                }

                if (configuredList.Count > 0)
                {
                    if (configuredList.Any(l => l.Contains(contentSource.LocalizedName))) // in configured list
                    {
                        list.Add(contentSource.SearchTypeId);
                    }
                }
                else
                {
                    list.Add(contentSource.SearchTypeId);
                }
            }

            return list.Distinct().ToList();
        }

        private static IEnumerable<int> GetSearchModuleDefIds(IDictionary settings, IEnumerable<SearchContentSource> searchContentSources)
        {
            var list = new List<int>();
            var configuredList = new List<string>();
            if (settings != null && !string.IsNullOrEmpty(Convert.ToString(settings["ScopeForFilters"])))
            {
                configuredList = Convert.ToString(settings["ScopeForFilters"]).Split('|').ToList();
            }

            // check content source in configured list or not
            foreach (var contentSource in searchContentSources)
            {
                if (contentSource.IsPrivate)
                {
                    continue;
                }

                if (configuredList.Count > 0)
                {
                    if (configuredList.Any(l => l.Contains(contentSource.LocalizedName)) && contentSource.ModuleDefinitionId > 0) // in configured list
                    {
                        list.Add(contentSource.ModuleDefinitionId);
                    }
                }
                else
                {
                    if (contentSource.ModuleDefinitionId > 0)
                    {
                        list.Add(contentSource.ModuleDefinitionId);
                    }
                }
            }

            return list;
        }

        private bool IsWildCardEnabledForModule()
        {
            var searchModuleSettings = this.GetSearchModuleSettings();
            var enableWildSearch = true;
            if (!string.IsNullOrEmpty(Convert.ToString(searchModuleSettings["EnableWildSearch"])))
            {
                enableWildSearch = Convert.ToBoolean(searchModuleSettings["EnableWildSearch"]);
            }

            return enableWildSearch;
        }

        private ModuleInfo GetSearchModule()
        {
            var arrModules = GetModulesByDefinition(this.PortalSettings.PortalId, "Search Results");
            ModuleInfo findModule = null;
            if (arrModules.Count > 1)
            {
                findModule = arrModules.Cast<ModuleInfo>().FirstOrDefault(searchModule => searchModule.CultureCode == this.PortalSettings.CultureCode);
            }

            return findModule ?? (arrModules.Count > 0 ? (ModuleInfo)arrModules[0] : null);
        }

        private Hashtable GetSearchModuleSettings()
        {
            if (this.ActiveModule != null && this.ActiveModule.ModuleDefinition.FriendlyName == "Search Results")
            {
                return this.ActiveModule.ModuleSettings;
            }

            var searchModule = this.GetSearchModule();
            return searchModule != null ? searchModule.ModuleSettings : null;
        }

        private bool GetBooleanSetting(string settingName, bool defaultValue)
        {
            if (this.PortalSettings == null)
            {
                return defaultValue;
            }

            var settings = this.GetSearchModuleSettings();
            if (settings == null || !settings.ContainsKey(settingName))
            {
                return defaultValue;
            }

            return Convert.ToBoolean(settings[settingName]);
        }

        private int GetIntegerSetting(string settingName, int defaultValue)
        {
            if (this.PortalSettings == null)
            {
                return defaultValue;
            }

            var settings = this.GetSearchModuleSettings();
            if (settings == null || !settings.ContainsKey(settingName))
            {
                return defaultValue;
            }

            var settingValue = Convert.ToString(settings[settingName]);
            if (!string.IsNullOrEmpty(settingValue) && Regex.IsMatch(settingValue, "^\\d+$"))
            {
                return Convert.ToInt32(settingValue);
            }

            return defaultValue;
        }

        private List<int> GetSearchPortalIds(IDictionary settings, int portalId)
        {
            var list = new List<int>();
            if (settings != null && !string.IsNullOrEmpty(Convert.ToString(settings["ScopeForPortals"])))
            {
                list = Convert.ToString(settings["ScopeForPortals"]).Split('|').Select(s => Convert.ToInt32(s)).ToList();
            }

            if (portalId == -1)
            {
                portalId = this.PortalSettings.ActiveTab.PortalID;
            }

            if (portalId > -1 && !list.Contains(portalId))
            {
                list.Add(portalId);
            }

            // Add Host
            var userInfo = this.UserInfo;
            if (userInfo.IsSuperUser)
            {
                list.Add(-1);
            }

            return list;
        }

        private IList<SearchContentSource> GetSearchContentSources(IList<string> typesList)
        {
            var sources = new List<SearchContentSource>();
            var list = InternalSearchController.Instance.GetSearchContentSourceList(this.PortalSettings.PortalId);

            if (typesList.Any())
            {
                foreach (var contentSources in typesList.Select(t1 => list.Where(src => string.Equals(src.LocalizedName, t1, StringComparison.OrdinalIgnoreCase))))
                {
                    sources.AddRange(contentSources);
                }
            }
            else
            {
                // no types fitler specified, add all available content sources
                sources.AddRange(list);
            }

            return sources;
        }

        private string GetFriendlyTitle(SearchResult result)
        {
            if (result.Keywords.ContainsKey("title") && !string.IsNullOrEmpty(result.Keywords["title"]))
            {
                return result.Keywords["title"];
            }

            return result.Title;
        }

        private string GetTitle(SearchResult result, bool showFriendlyTitle = false)
        {
            if (result.ModuleDefId > 0 && result.ModuleDefId == this.HtmlModuleDefitionId) // special handling for Html module
            {
                var tabTitle = this.GetTabTitleFromModuleId(result.ModuleId);
                if (!string.IsNullOrEmpty(tabTitle))
                {
                    if (result.Title != "Enter Title" && result.Title != "Text/HTML")
                    {
                        return tabTitle + " > " + result.Title;
                    }

                    return tabTitle;
                }
            }

            return showFriendlyTitle ? this.GetFriendlyTitle(result) : result.Title;
        }

        private string GetTabTitleFromModuleId(int moduleId)
        {
            // no manual clearing of the cache exists; let is just expire
            var cacheKey = string.Format(ModuleTitleCacheKey, moduleId);

            return CBO.GetCachedObject<string>(new CacheItemArgs(cacheKey, ModuleTitleCacheTimeOut, ModuleTitleCachePriority, moduleId), this.GetTabTitleCallBack);
        }

        private object GetTabTitleCallBack(CacheItemArgs cacheItemArgs)
        {
            var moduleId = (int)cacheItemArgs.ParamList[0];
            var moduleInfo = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
            if (moduleInfo != null)
            {
                var tab = moduleInfo.ParentTab;

                return !string.IsNullOrEmpty(tab.Title) ? tab.Title : tab.TabName;
            }

            return string.Empty;
        }

        public class SynonymsGroupDto
        {
            public int Id { get; set; }

            public string Tags { get; set; }

            public int PortalId { get; set; }

            public string Culture { get; set; }
        }

        public class StopWordsDto
        {
            public int Id { get; set; }

            public string Words { get; set; }

            public int PortalId { get; set; }

            public string Culture { get; set; }
        }
    }
}
