#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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

namespace DotNetNuke.Web.InternalServices
{
    [AllowAnonymous]
    public class SearchServiceController : DnnApiController
    {
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

        #region private methods

        private readonly ModuleController _moduleController = new ModuleController();
        private readonly TabController _tabController = new TabController();
        
        private static readonly int HtmlModuleDefitionId = GetHtmlModuleDefinitionId();

        private static int GetHtmlModuleDefinitionId()
        {
            var modDef = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Text/HTML");
            return modDef != null ? modDef.ModuleDefID : -1;
        }

        private bool IsWildCardEnabledForModule()
        {
            var searchModuleSettings = GetSearchModuleSettings();
            var enableWildSearch = true;
            if (!string.IsNullOrEmpty(Convert.ToString(searchModuleSettings["EnableWildSearch"])))
            {
                enableWildSearch = Convert.ToBoolean(searchModuleSettings["EnableWildSearch"]);
            }

            return enableWildSearch;
        }

        #region Loads Search portal ids, crawler ids and module def ids

        private const string ModuleInfosCacheKey = "ModuleInfos{0}";
        private const CacheItemPriority ModuleInfosCachePriority = CacheItemPriority.AboveNormal;
        private const int ModuleInfosCacheTimeOut = 20;

        private static ArrayList GetModulesByDefinition(int portalID, string friendlyName)
        {
            var cacheKey = string.Format(ModuleInfosCacheKey, portalID);
            return CBO.GetCachedObject<ArrayList>(
                new CacheItemArgs(cacheKey, ModuleInfosCacheTimeOut, ModuleInfosCachePriority),
                args => CBO.FillCollection(DataProvider.Instance().GetModuleByDefinition(portalID, friendlyName), typeof(ModuleInfo)));
        }

        private ModuleInfo GetSearchModule()
        {
            var arrModules = GetModulesByDefinition(PortalSettings.PortalId, "Search Results");
	        ModuleInfo findModule = null;
            if (arrModules.Count > 1)
            {
                findModule = arrModules.Cast<ModuleInfo>().FirstOrDefault(searchModule => searchModule.CultureCode == PortalSettings.CultureCode);
            }

	        return findModule ?? (ModuleInfo) arrModules[0];
        }

        private Hashtable GetSearchModuleSettings()
        {
			if (ActiveModule != null && ActiveModule.ModuleDefinition.FriendlyName == "Search Results")
			{
				return ActiveModule.ModuleSettings;
			}

            var searchModule = GetSearchModule();
            return searchModule != null ? searchModule.ModuleSettings : null;
        }

        private List<int> GetSearchPortalIds(IDictionary settings, int portalId)
        {
            var list = new List<int>();
            if (settings != null && !string.IsNullOrEmpty(Convert.ToString(settings["ScopeForPortals"])))
            {
                list = Convert.ToString(settings["ScopeForPortals"]).Split('|').Select(s => Convert.ToInt32(s)).ToList();
            }

            if (portalId == -1) portalId = PortalSettings.ActiveTab.PortalID;
            if (portalId > -1 && !list.Contains(portalId)) list.Add(portalId);

            //Add Host 
            var userInfo = UserInfo;
            if (userInfo.IsSuperUser)
                list.Add(-1);

            return list;
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
                if (contentSource.IsPrivate) continue;
                if (configuredList.Count > 0)
                {
                    if (configuredList.Any(l => l.Contains(contentSource.LocalizedName))) // in configured list
                        list.Add(contentSource.SearchTypeId);
                }
                else
                {
                    list.Add(contentSource.SearchTypeId);
                }
            }

            return list;
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
                if (contentSource.IsPrivate) continue; ;
                if (configuredList.Count > 0)
                {
                    if (configuredList.Any(l => l.Contains(contentSource.LocalizedName)) && contentSource.ModuleDefinitionId > 0) // in configured list
                        list.Add(contentSource.ModuleDefinitionId);
                }
                else
                {
                    if (contentSource.ModuleDefinitionId > 0)
                        list.Add(contentSource.ModuleDefinitionId);
                }
            }

            return list;
        }

        private IList<SearchContentSource> GetSearchContentSources(IList<string> typesList)
        {
            var sources = new List<SearchContentSource>();
            var list = InternalSearchController.Instance.GetSearchContentSourceList(PortalSettings.PortalId);

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

        #endregion

        #region view models for search results

        private IEnumerable<GroupedDetailView> GetGroupedDetailViews(SearchQuery searchQuery, out int totalHits, out bool more)
        {
            var searchResults = SearchController.Instance.SiteSearch(searchQuery);
            totalHits = searchResults.TotalHits;
            more = searchResults.Results.Count == searchQuery.PageSize;

            var groups = new List<GroupedDetailView>();
            var tabGroups = new Dictionary<string, IList<SearchResult>>();
            var userSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId;
           
            foreach (var result in searchResults.Results)
            {
                //var key = result.TabId + result.Url;
                var key = result.Url;
                if (!tabGroups.ContainsKey(key))
                {
                    tabGroups.Add(key, new List<SearchResult> { result });
                }
                else
                {
                    //when the result is a user search type, we should only show one result
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

            foreach (var results in tabGroups.Values)
            {
                var group = new GroupedDetailView();

                //first entry
                var first = results[0];
                group.Title = first.Title;
                group.DocumentUrl = first.Url;

                //Find a different title for multiple entries with same url
                if (results.Count > 1)
                {
                    if (first.TabId > 0)
                    {
                        var tab = _tabController.GetTab(first.TabId, first.PortalId, false);
                        if (tab != null)
                            group.Title = tab.TabName;
                    }
                    else if (first.ModuleId > 0)
                    {
                        var tabTitle = GetTabTitleFromModuleId(first.ModuleId);
                        if (!string.IsNullOrEmpty(tabTitle))
                        {
                            group.Title = tabTitle;
                        }
                    }
                }
                else if (first.ModuleDefId > 0 && first.ModuleDefId == HtmlModuleDefitionId) //special handling for Html module
                {
                    var tabTitle = GetTabTitleFromModuleId(first.ModuleId);
                    if (!string.IsNullOrEmpty(tabTitle))
                    {
                        group.Title = tabTitle + " > " + first.Title;
                        first.Title = group.Title;
                    }
                }

                foreach (var result in results)
                {
                    var detail = new DetailedView
                    {
                        Title = result.Title,
                        DocumentTypeName = InternalSearchController.Instance.GetSearchDocumentTypeDisplayName(result),
                        DocumentUrl = result.Url,
                        Snippet = result.Snippet,
                        DisplayModifiedTime = result.DisplayModifiedTime,
                        Tags = result.Tags.ToList(),
                        AuthorProfileUrl = result.AuthorUserId > 0 ? Globals.UserProfileURL(result.AuthorUserId) : string.Empty,
                        AuthorName = result.AuthorName
                    };
                    group.Results.Add(detail);
                }

                groups.Add(group);
            }

            return groups;
        }

        private IEnumerable<BasicView> GetBasicViews(SearchQuery searchQuery, out int totalHits)
        {
            var sResult = SearchController.Instance.SiteSearch(searchQuery);
            totalHits = sResult.TotalHits;

            return sResult.Results.Select(result => 
                new BasicView
                    {
                        Title = GetTitle(result),
                        DocumentTypeName = InternalSearchController.Instance.GetSearchDocumentTypeDisplayName(result),
                        DocumentUrl = result.Url,
                        Snippet = result.Snippet,
                    });
        }

        private string GetTitle(SearchResult result)
        {
            if (result.ModuleDefId > 0 && result.ModuleDefId == HtmlModuleDefitionId) //special handling for Html module
            {
                var tabTitle = GetTabTitleFromModuleId(result.ModuleId);
                if (!string.IsNullOrEmpty(tabTitle))
                {
                    return tabTitle + " > " + result.Title;
                }
            }

            return result.Title;
        }

        private const string ModuleTitleCacheKey = "SearchModuleTabTitle_{0}";
        private const CacheItemPriority ModuleTitleCachePriority = CacheItemPriority.Normal;
        private const int ModuleTitleCacheTimeOut = 20;

        private string GetTabTitleFromModuleId(int moduleId)
        {
            // no manual clearing of the cache exists; let is just expire
            var cacheKey = string.Format(ModuleTitleCacheKey, moduleId);

            return CBO.GetCachedObject<string>(new CacheItemArgs(cacheKey, ModuleTitleCacheTimeOut, ModuleTitleCachePriority, moduleId), GetTabTitleCallBack);
        }

        private object GetTabTitleCallBack(CacheItemArgs cacheItemArgs)
        {
            var moduleId = (int)cacheItemArgs.ParamList[0];
            var moduleInfo = _moduleController.GetModule(moduleId);
            if (moduleInfo != null)
            {
                return moduleInfo.ParentTab.TabName;
            }

            return string.Empty;
        }

        #endregion

        #endregion

        [HttpGet]
        [DnnExceptionFilter]
        public HttpResponseMessage Preview(string keywords, string culture, int forceWild = 1, int portal = -1)
        {
            string cleanedKeywords;
            keywords = (keywords ?? string.Empty).Trim();
            var tags = SearchQueryStringParser.Instance.GetTags(keywords, out cleanedKeywords);
            var beginModifiedTimeUtc = SearchQueryStringParser.Instance.GetLastModifiedDate(cleanedKeywords, out cleanedKeywords);
            var searchTypes = SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out cleanedKeywords);

            var contentSources = GetSearchContentSources(searchTypes);
            var settings = GetSearchModuleSettings();
            var searchTypeIds = GetSearchTypeIds(settings, contentSources);
            var moduleDefids = GetSearchModuleDefIds(settings, contentSources);
            var portalIds = GetSearchPortalIds(settings, portal);

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
                    WildCardSearch = forceWild > 0 
                };

                try
                {
                    int totalHists;
                    var previews = GetBasicViews(query, out totalHists);
                    var userSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId;
                    var userSearchSource = contentSources.FirstOrDefault(s => s.SearchTypeId == userSearchTypeId);
                    foreach (var preview in previews)
                    {
                        //if the document type is user, then try to add user pic into preview's custom attributes.
                        if (userSearchSource != null && preview.DocumentTypeName == userSearchSource.LocalizedName)
                        {
                            var match = Regex.Match(preview.DocumentUrl, "userid(/|\\|=)(\\d+)", RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                var userid = Convert.ToInt32(match.Groups[2].Value);
                                var user = UserController.GetUserById(PortalSettings.PortalId, userid);
                                if (user != null)
                                {
                                    preview.Attributes.Add("Avatar", user.Profile.PhotoURL);
                                }
                            }
                        }

                        var groupedResult = results.SingleOrDefault(g => g.DocumentTypeName == preview.DocumentTypeName);
                        if (groupedResult != null)
                        {
                            if(!groupedResult.Results.Any(r => string.Equals(r.DocumentUrl, preview.DocumentUrl)))
                            groupedResult.Results.Add(new BasicView
                            {
                                Title = preview.Title,
                                Snippet = preview.Snippet,
                                DocumentUrl = preview.DocumentUrl,
                                Attributes = preview.Attributes
                            });
                        }
                        else
                        {
                            results.Add(new GroupedBasicView(preview));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Services.Exceptions.Exceptions.LogException(ex);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, results);
        }

        [HttpGet]
        [DnnExceptionFilter]
        public HttpResponseMessage Search(string search, string culture, int pageIndex, int pageSize, int sortOption)
        {
            string cleanedKeywords;
            search = (search ?? string.Empty).Trim();
            var tags = SearchQueryStringParser.Instance.GetTags(search, out cleanedKeywords);
            var beginModifiedTimeUtc = SearchQueryStringParser.Instance.GetLastModifiedDate(cleanedKeywords, out cleanedKeywords);
            var searchTypes = SearchQueryStringParser.Instance.GetSearchTypeList(cleanedKeywords, out cleanedKeywords);

            var contentSources = GetSearchContentSources(searchTypes);
            var settings = GetSearchModuleSettings();
            var searchTypeIds = GetSearchTypeIds(settings, contentSources);
            var moduleDefids = GetSearchModuleDefIds(settings, contentSources);
            var portalIds = GetSearchPortalIds(settings, -1);

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
                        SortField = (SortFields) sortOption,
                        TitleSnippetLength = 120,
                        BodySnippetLength = 300,
                        CultureCode = culture,
                        WildCardSearch = IsWildCardEnabledForModule()
                    };

                try
                {
                    results = GetGroupedDetailViews(query, out totalHits, out more).ToList();
                }
                catch (Exception ex)
                {
                    Services.Exceptions.Exceptions.LogException(ex);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { results, totalHits, more });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        [SupportedModules("SearchAdmin")]
        [DnnExceptionFilter]
        public HttpResponseMessage AddSynonymsGroup(SynonymsGroupDto synonymsGroup)
        {
            string duplicateWord;
            var synonymsGroupId = SearchHelper.Instance.AddSynonymsGroup(synonymsGroup.Tags, synonymsGroup.PortalId, synonymsGroup.Culture, out duplicateWord);
            return Request.CreateResponse(HttpStatusCode.OK, new { Id = synonymsGroupId, DuplicateWord = duplicateWord });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        [SupportedModules("SearchAdmin")]
        [DnnExceptionFilter]
        public HttpResponseMessage UpdateSynonymsGroup(SynonymsGroupDto synonymsGroup)
        {
            string duplicateWord;
            var synonymsGroupId = SearchHelper.Instance.UpdateSynonymsGroup(synonymsGroup.Id, synonymsGroup.Tags, synonymsGroup.PortalId, synonymsGroup.Culture, out duplicateWord);
            return Request.CreateResponse(HttpStatusCode.OK, new { Id = synonymsGroupId, DuplicateWord = duplicateWord });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        [SupportedModules("SearchAdmin")]
        [DnnExceptionFilter]
        public HttpResponseMessage DeleteSynonymsGroup(SynonymsGroupDto synonymsGroup)
        {
            SearchHelper.Instance.DeleteSynonymsGroup(synonymsGroup.Id, synonymsGroup.PortalId, synonymsGroup.Culture);
            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        [SupportedModules("SearchAdmin")]
        [DnnExceptionFilter]
        public HttpResponseMessage AddStopWords(StopWordsDto stopWords)
        {
            var stopWordsId = SearchHelper.Instance.AddSearchStopWords(stopWords.Words, stopWords.PortalId, stopWords.Culture);
            return Request.CreateResponse(HttpStatusCode.OK, new { Id = stopWordsId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        [SupportedModules("SearchAdmin")]
        [DnnExceptionFilter]
        public HttpResponseMessage UpdateStopWords(StopWordsDto stopWords)
        {
            var stopWordsId = SearchHelper.Instance.UpdateSearchStopWords(stopWords.Id, stopWords.Words, stopWords.PortalId, stopWords.Culture);
            return Request.CreateResponse(HttpStatusCode.OK, new { Id = stopWordsId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        [SupportedModules("SearchAdmin")]
        [DnnExceptionFilter]
        public HttpResponseMessage DeleteStopWords(StopWordsDto stopWords)
        {
            SearchHelper.Instance.DeleteSearchStopWords(stopWords.Id, stopWords.PortalId, stopWords.Culture);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

    }
}