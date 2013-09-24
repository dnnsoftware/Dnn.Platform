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
#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Caching;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

#endregion

namespace DotNetNuke.Services.Search.Controllers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The Impl Controller class for Search
    /// </summary>
    /// -----------------------------------------------------------------------------
    internal class SearchControllerImpl : ISearchController
    {
        #region Private Properties

        private const string SeacrchContollersCacheKey = "SearchControllers";
        private const int MaxLucenceRefetches = 10;
        private const int MaxLucenceLookBacks = 10;
        
        private readonly int _moduleSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId;

        #endregion
        
        #region Core Search APIs

        public SearchResults SiteSearch(SearchQuery searchQuery)
        {
            var results = GetResults(searchQuery);
            return new SearchResults{TotalHits = results.Item1, Results = results.Item2};
        }

        public SearchResults ModuleSearch(SearchQuery searchQuery)
        {
            searchQuery.SearchTypeIds = new List<int> { _moduleSearchTypeId };
            var results = GetResults(searchQuery);
            return new SearchResults { TotalHits = results.Item1, Results = results.Item2 };
        }

        #endregion

        #region Private Methods

        private Tuple<int, IList<SearchResult>> GetResults(SearchQuery searchQuery)
        {
            Requires.NotNull("Query", searchQuery);
            Requires.PropertyNotEqualTo("searchQuery", "SearchTypeIds", searchQuery.SearchTypeIds.Count(), 0);

            if((searchQuery.ModuleId > 0) && (searchQuery.SearchTypeIds.Count() > 1 || !searchQuery.SearchTypeIds.Contains(_moduleSearchTypeId)))
                throw new ArgumentException("ModuleId based search must have SearchTypeId for a module only");

            if(searchQuery.SortField == SortFields.CustomStringField || searchQuery.SortField == SortFields.CustomNumericField
                || searchQuery.SortField == SortFields.NumericKey || searchQuery.SortField == SortFields.Keyword)
                Requires.NotNullOrEmpty("CustomSortField", searchQuery.CustomSortField);

            //TODO - Explore Slop factor for Phrase query

            var query = new BooleanQuery();
            if (!string.IsNullOrEmpty(searchQuery.KeyWords))
            {
                try
                {
                    var keywords = SearchHelper.Instance.RephraseSearchText(searchQuery.KeyWords, searchQuery.WildCardSearch);
                    // don't use stemming analyzer for exact matches or non-analyzed fields (e.g. Tags)
                    var analyzer = new SearchQueryAnalyzer(true);
                    var nonStemmerAnalyzer = new SearchQueryAnalyzer(false);
                    var keywordQuery = new BooleanQuery();
                    foreach (var fieldToSearch in Constants.KeyWordSearchFields)
                    {
                        var parserContent = new QueryParser(Constants.LuceneVersion, fieldToSearch,
                            fieldToSearch == Constants.Tag ? nonStemmerAnalyzer : analyzer);
                        var parsedQueryContent = parserContent.Parse(keywords);
                        keywordQuery.Add(parsedQueryContent, Occur.SHOULD);
                    }
                    query.Add(keywordQuery, Occur.MUST);
                }
                catch (Exception)
                {
                    foreach (var word in searchQuery.KeyWords.Split(' '))
                    {
                        query.Add(new TermQuery(new Term(Constants.ContentTag, word.ToLower())), Occur.SHOULD);
                    }
                }
            }

            var portalIdQuery = new BooleanQuery();
            foreach (var portalId in searchQuery.PortalIds)
            {
                portalIdQuery.Add(NumericRangeQuery.NewIntRange(Constants.PortalIdTag, portalId, portalId, true, true), Occur.SHOULD);                
            }
            if (searchQuery.PortalIds.Any()) query.Add(portalIdQuery, Occur.MUST);

            ApplySearchTypeIdFilter(query, searchQuery);

            if (searchQuery.BeginModifiedTimeUtc > DateTime.MinValue && searchQuery.EndModifiedTimeUtc >= searchQuery.BeginModifiedTimeUtc)
            {
                query.Add(NumericRangeQuery.NewLongRange(Constants.ModifiedTimeTag, long.Parse(searchQuery.BeginModifiedTimeUtc.ToString(Constants.DateTimeFormat)), long.Parse(searchQuery.EndModifiedTimeUtc.ToString(Constants.DateTimeFormat)), true, true), Occur.MUST);
            }

            if(searchQuery.RoleId > 0)
                query.Add(NumericRangeQuery.NewIntRange(Constants.RoleIdTag, searchQuery.RoleId, searchQuery.RoleId, true, true), Occur.MUST);  

            foreach (var tag in searchQuery.Tags)
            {
                query.Add(new TermQuery(new Term(Constants.Tag, tag.ToLower())), Occur.MUST);
            }

            if (!string.IsNullOrEmpty(searchQuery.CultureCode))
            {
                var localeQuery = new BooleanQuery();

                var languageId = Localization.Localization.GetCultureLanguageID(searchQuery.CultureCode);
                localeQuery.Add(NumericRangeQuery.NewIntRange(Constants.LocaleTag, languageId, languageId, true, true), Occur.SHOULD);
                localeQuery.Add(NumericRangeQuery.NewIntRange(Constants.LocaleTag, Null.NullInteger, Null.NullInteger, true, true), Occur.SHOULD);
                query.Add(localeQuery, Occur.MUST);
            }



            var luceneQuery = new LuceneQuery
            {
                Query = query,
                Sort = GetSort(searchQuery),
                PageIndex = searchQuery.PageIndex,
                PageSize = searchQuery.PageSize,
                TitleSnippetLength = searchQuery.TitleSnippetLength,
                BodySnippetLength = searchQuery.BodySnippetLength
            };

            return GetSecurityTrimmedResults(searchQuery, luceneQuery);
        }

        private Sort GetSort(SearchQuery query)
        {
            var sort = Sort.RELEVANCE; //default sorting - relevance is always descending.
            if (query.SortField != SortFields.Relevance)
            {
                var reverse = query.SortDirection != SortDirections.Ascending;

                switch (query.SortField)
                {
                    case SortFields.LastModified:
                        sort = new Sort(new SortField(Constants.ModifiedTimeTag, SortField.LONG, reverse));
                        break;
                    case SortFields.Title:
                        sort = new Sort(new SortField(Constants.TitleTag, SortField.STRING, reverse));
                        break;
                    case SortFields.Tag:
                        sort = new Sort(new SortField(Constants.Tag, SortField.STRING, reverse));
                        break;
                    case SortFields.NumericKey:
                        sort = new Sort(new SortField(Constants.NumericKeyPrefixTag + query.CustomSortField, SortField.INT, reverse));
                        break;
                    case SortFields.Keyword:
                        sort = new Sort(new SortField(Constants.KeywordsPrefixTag + query.CustomSortField, SortField.STRING, reverse));
                        break;
                    case SortFields.CustomStringField:
                        sort = new Sort(new SortField(query.CustomSortField, SortField.STRING, reverse));
                        break;
                    case SortFields.CustomNumericField:
                        sort = new Sort(new SortField(query.CustomSortField, SortField.INT, reverse));
                        break;
                    default:
                        sort = Sort.RELEVANCE;
                        break;
                }
            }

            return sort;
        }

        private void ApplySearchTypeIdFilter(BooleanQuery query, SearchQuery searchQuery)
        {
            //Special handling for Module Search
            if (searchQuery.SearchTypeIds.Count() == 1 && searchQuery.SearchTypeIds.Contains(_moduleSearchTypeId))
            {
                //When moduleid is specified, we ignore other searchtypeid or moduledefinitionid. Major security check
                if (searchQuery.ModuleId > 0)
                {
                    //this is the main hook for module based search. Occur.MUST is a requirement for this condition or else results from other modules will be found
                    query.Add(NumericRangeQuery.NewIntRange(Constants.ModuleIdTag, searchQuery.ModuleId, searchQuery.ModuleId, true, true), Occur.MUST);
                }
                else
                {
                    var modDefQuery = new BooleanQuery();
                    foreach (var moduleDefId in searchQuery.ModuleDefIds)
                    {
                        modDefQuery.Add(NumericRangeQuery.NewIntRange(Constants.ModuleDefIdTag, moduleDefId, moduleDefId, true, true), Occur.SHOULD);
                    }
                    if (searchQuery.ModuleDefIds.Any())
                        query.Add(modDefQuery, Occur.MUST); //Note the MUST
                }

                query.Add(NumericRangeQuery.NewIntRange(Constants.SearchTypeTag, _moduleSearchTypeId, _moduleSearchTypeId, true, true), Occur.MUST);
            }
            else
            {
                var searchTypeIdQuery = new BooleanQuery();
                foreach (var searchTypeId in searchQuery.SearchTypeIds)
                {
                    if (searchTypeId == _moduleSearchTypeId)
                    {
			foreach (var moduleDefId in searchQuery.ModuleDefIds.OrderBy(id => id))
                        {
                            searchTypeIdQuery.Add(NumericRangeQuery.NewIntRange(Constants.ModuleDefIdTag, moduleDefId, moduleDefId, true, true), Occur.SHOULD);
                        }
                        if (!searchQuery.ModuleDefIds.Any())
                            searchTypeIdQuery.Add(NumericRangeQuery.NewIntRange(Constants.SearchTypeTag, searchTypeId, searchTypeId, true, true), Occur.SHOULD);  
                    }
                    else
                    {
                        searchTypeIdQuery.Add(NumericRangeQuery.NewIntRange(Constants.SearchTypeTag, searchTypeId, searchTypeId, true, true), Occur.SHOULD);       
                    }
                }
                query.Add(searchTypeIdQuery, Occur.MUST);
            }
        }

        private SearchResult GetSearchResultFromLuceneResult(LuceneResult luceneResult)
        {
            var result = new SearchResult();
            var doc = luceneResult.Document;
            result.DisplayScore = luceneResult.DisplayScore;
            result.Score = luceneResult.Score;
            
            // set culture code of result
            result.CultureCode = string.Empty;
            var localeField = luceneResult.Document.GetField(Constants.LocaleTag);
            if (localeField != null)
            {
                int id;
                if (int.TryParse(localeField.StringValue, out id) && id >= 0)
                {
                    result.CultureCode = LocaleController.Instance.GetLocale(id).Code;
                }
                else
                {
                    result.CultureCode = Null.NullString;
                }
            }

            FillTagsValues(doc, result);
            result.Snippet = GetSnippet(result, luceneResult);
            return result;
        }

        private static void FillTagsValues(Document doc, SearchResult result)
        {
            foreach (var field in doc.GetFields())
            {
                if (field.StringValue == null) continue;
                int intField;
                switch (field.Name)
                {
                    case Constants.UniqueKeyTag:
                        result.UniqueKey = field.StringValue;
                        break;
                    case Constants.TitleTag:
                        var title = field.StringValue;
                        //TODO - Need better highlighting logic for Title
                        //result.Title = string.IsNullOrEmpty(titleSnippet) ? title : string.Format("...{0}...", titleSnippet);
                        result.Title = title;
                        break;
                    case Constants.BodyTag:
                        result.Body = field.StringValue;
                        break;
                    case Constants.DescriptionTag:
                        result.Description = field.StringValue;
                        break;
                    case Constants.Tag:
                        result.Tags = result.Tags.Concat(new string[] { field.StringValue });
                        break;
                    case Constants.PermissionsTag:
                        result.Permissions = field.StringValue;
                        break;
                    case Constants.QueryStringTag:
                        result.QueryString = field.StringValue;
                        break;
                    case Constants.UrlTag:
                        result.Url = field.StringValue;
                        break;
                    case Constants.SearchTypeTag:
                        if(int.TryParse(field.StringValue, out intField)) result.SearchTypeId = intField;
                        break;
                    case Constants.ModuleIdTag:
                        if (int.TryParse(field.StringValue, out intField)) result.ModuleId = intField;
                        break;
                    case Constants.ModuleDefIdTag:
                        if (int.TryParse(field.StringValue, out intField)) result.ModuleDefId = intField;
                        break;
                    case Constants.PortalIdTag:
                        if (int.TryParse(field.StringValue, out intField)) result.PortalId = intField;
                        break;
                    case Constants.AuthorIdTag:
                        if (int.TryParse(field.StringValue, out intField)) result.AuthorUserId = intField;
                        break;
                    case Constants.RoleIdTag:
                        if (int.TryParse(field.StringValue, out intField)) result.RoleId = intField;
                        break;
                    case Constants.AuthorNameTag:
                        result.AuthorName = field.StringValue;
                        break;
                    case Constants.TabIdTag:
                        if (int.TryParse(field.StringValue, out intField)) result.TabId = intField;
                        break;
                    case Constants.ModifiedTimeTag:
                        DateTime modifiedTimeUtc;
                        DateTime.TryParseExact(field.StringValue, Constants.DateTimeFormat, null, DateTimeStyles.None, out modifiedTimeUtc);
                        result.ModifiedTimeUtc = modifiedTimeUtc;
                        break;
                    default:
                        if (field.Name.StartsWith(Constants.NumericKeyPrefixTag))
                        {
                            var key = field.Name.Substring(Constants.NumericKeyPrefixTag.Length);
                            if (int.TryParse(field.StringValue, out intField))
                            {
                                if (!result.NumericKeys.ContainsKey(key))
                                    result.NumericKeys.Add(key, intField);
                            }
                        }
                        else if (field.Name.StartsWith(Constants.KeywordsPrefixTag))
                        {
                            var key = field.Name.Substring(Constants.KeywordsPrefixTag.Length);
                            if (!result.Keywords.ContainsKey(key))
                                result.Keywords.Add(key, field.StringValue);
                        }
                        break;
                }
            }
        }

        private string GetSnippet(SearchResult searchResult, LuceneResult luceneResult)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(luceneResult.TitleSnippet)) sb.Append(luceneResult.TitleSnippet + "...");
            if (!string.IsNullOrEmpty(luceneResult.DescriptionSnippet)) sb.Append(luceneResult.DescriptionSnippet + "...");
            if (!string.IsNullOrEmpty(luceneResult.TagSnippet)) sb.Append(luceneResult.TagSnippet + "...");
            if (!string.IsNullOrEmpty(luceneResult.BodySnippet)) sb.Append(luceneResult.BodySnippet + "...");
            if (!string.IsNullOrEmpty(luceneResult.AuthorSnippet)) sb.Append(luceneResult.AuthorSnippet + "...");
            if (!string.IsNullOrEmpty(luceneResult.ContentSnippet)) sb.Append(luceneResult.ContentSnippet + "...");

            var snippet = sb.ToString();
           if (string.IsNullOrEmpty(snippet)) snippet = searchResult.Title;

            return snippet;
        }

        private Dictionary<int, BaseResultController> GetSearchResultControllers()
        {
            var cachArg = new CacheItemArgs(SeacrchContollersCacheKey, 120, CacheItemPriority.Default);
            return CBO.GetCachedObject<Dictionary<int, BaseResultController>>(cachArg, GetSearchResultsControllersCallBack);
        }

        private Dictionary<int, BaseResultController> GetSearchResultsControllersCallBack(CacheItemArgs cacheItem)
        {
            var searchTypes = SearchHelper.Instance.GetSearchTypes();
            var resultControllers = new Dictionary<int, BaseResultController>();

            foreach (var searchType in searchTypes)
            {
                try
                {
                    var searchControllerType = Reflection.CreateType(searchType.SearchResultClass);
                    var searchController = Reflection.CreateObject(searchControllerType);

                    resultControllers.Add(searchType.SearchTypeId, (BaseResultController)searchController);
                }
                catch (Exception ex)
                {
                    Exceptions.Exceptions.LogException(ex);
                }
            }

            return resultControllers;
        }
        
        private Tuple<int, IList<SearchResult>> GetSecurityTrimmedResults(SearchQuery searchQuery, LuceneQuery luceneQuery)
        {
            var results = new List<SearchResult>();
            var totalHits = 0;

            //****************************************************************************
            // First Fetch and determine starting item of current page
            //****************************************************************************
            if (searchQuery.PageSize > 0)
            {
                var luceneResults = LuceneController.Instance.Search(luceneQuery, out totalHits, HasPermissionToViewDoc);
                results = luceneResults.Select(GetSearchResultFromLuceneResult).ToList();

                //****************************************************************************
                //Adding URL Links to final trimmed results
                //****************************************************************************
                foreach (var result in results)
                {
                    if (string.IsNullOrEmpty(result.Url))
                    {
                        var resultController = GetSearchResultControllers().Single(sc => sc.Key == result.SearchTypeId).Value;
                        result.Url = resultController.GetDocUrl(result);
                    }
                }
            }

            return new Tuple<int, IList<SearchResult>>(totalHits, results);
        }
        
        private bool HasPermissionToViewDoc(Document document)
        {
            // others LuceneResult fields are not impotrant at this moment
            var result = GetPartialSearchResult(document);
            var resultController = GetSearchResultControllers().SingleOrDefault(sc => sc.Key == result.SearchTypeId).Value;
            return resultController != null && resultController.HasViewPermission(result);
        }

        private static SearchResult GetPartialSearchResult(Document doc)
        {
            var result = new SearchResult();
            var localeField = doc.GetField(Constants.LocaleTag);

            if (localeField != null)
            {
                int id;
                result.CultureCode = int.TryParse(localeField.StringValue, out id) && id >= 0
                    ? LocaleController.Instance.GetLocale(id).Code : Null.NullString;
            }

            FillTagsValues(doc, result);
            return result;
        }

        #endregion
    }
}
