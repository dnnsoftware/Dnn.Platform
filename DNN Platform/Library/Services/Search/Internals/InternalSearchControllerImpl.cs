#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Caching;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Search.Entities;

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

#endregion

namespace DotNetNuke.Services.Search.Internals
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The Impl Controller class for Lucene
    /// </summary>
    /// -----------------------------------------------------------------------------
    internal class InternalSearchControllerImpl : IInternalSearchController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LuceneControllerImpl));
        private const string SearchableModuleDefsKey = "{0}-{1}";
        private const string SearchableModuleDefsCacheKey = "SearchableModuleDefs";
        private const string LocalizedResxFile = "~/DesktopModules/Admin/SearchResults/App_LocalResources/SearchableModules.resx";

        private static readonly string[] HtmlAttributesToRetain = new[] { "alt", "title" };
        private readonly int _titleBoost;
        private readonly int _tagBoost;
        private readonly int _contentBoost;
        private readonly int _descriptionBoost;
        private readonly int _authorBoost;
        private readonly int _moduleSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId;

        private static readonly DataProvider DataProvider = DataProvider.Instance();

        #region constructor
        public InternalSearchControllerImpl()
        {
            var hostController = HostController.Instance;
            _titleBoost = hostController.GetInteger(Constants.SearchTitleBoostSetting, Constants.DefaultSearchTitleBoost);
            _tagBoost = hostController.GetInteger(Constants.SearchTagBoostSetting, Constants.DefaultSearchTagBoost);
            _contentBoost = hostController.GetInteger(Constants.SearchContentBoostSetting, Constants.DefaultSearchKeywordBoost);
            _descriptionBoost = hostController.GetInteger(Constants.SearchDescriptionBoostSetting, Constants.DefaultSearchDescriptionBoost);
            _authorBoost = hostController.GetInteger(Constants.SearchAuthorBoostSetting, Constants.DefaultSearchAuthorBoost);
        }
        #endregion

        internal virtual object SearchContentSourceCallback(CacheItemArgs cacheItem)
        {
            var searchTypes = CBO.FillCollection<SearchType>(DataProvider.GetAllSearchTypes());

            var results = new List<SearchContentSource>();

            foreach (var crawler in searchTypes)
            {
                switch (crawler.SearchTypeName)
                {
                    case "module": // module crawler

                        // get searchable module definition list
                        var portalId = int.Parse(cacheItem.CacheKey.Split('-')[1]);
                        var modules = ModuleController.Instance.GetSearchModules(portalId);
                        var modDefIds = new HashSet<int>();

                        foreach (ModuleInfo module in modules)
                        {
                            if (!modDefIds.Contains(module.ModuleDefID)) modDefIds.Add(module.ModuleDefID);
                        }
                        
                        var list = modDefIds.Select(ModuleDefinitionController.GetModuleDefinitionByID).ToList();

                        foreach (var def in list)
                        {
                            var text = Localization.Localization.GetSafeJSString("Module_" + def.DefinitionName, LocalizedResxFile);
                            if (string.IsNullOrEmpty(text))
                            {
                                text = def.FriendlyName;
                            }
                            var result = new SearchContentSource
                            {
                                SearchTypeId = crawler.SearchTypeId,
                                SearchTypeName = crawler.SearchTypeName,
                                IsPrivate = crawler.IsPrivate,
                                ModuleDefinitionId =  def.ModuleDefID,
                                LocalizedName = text
                            };

                            results.Add(result);
                        }

                        break;

                    default:

                        var localizedName = Localization.Localization.GetSafeJSString("Crawler_" + crawler.SearchTypeName, LocalizedResxFile);
                        if (string.IsNullOrEmpty(localizedName))
                        {
                            localizedName = crawler.SearchTypeName;
                        }

                        results.Add(new SearchContentSource
                        {
                            SearchTypeId = crawler.SearchTypeId,
                            SearchTypeName = crawler.SearchTypeName,
                            IsPrivate = crawler.IsPrivate,
                            ModuleDefinitionId = 0,
                            LocalizedName = localizedName
                        });
                        break;
                }
            }

            return results;

        }

        public IEnumerable<SearchContentSource> GetSearchContentSourceList(int portalId)
        {
            var searchableModuleDefsCacheArgs = new CacheItemArgs(
                    string.Format(SearchableModuleDefsKey, SearchableModuleDefsCacheKey, portalId),
                    120, CacheItemPriority.Default);

            var list = CBO.GetCachedObject<IList<SearchContentSource>>(
                searchableModuleDefsCacheArgs, SearchContentSourceCallback);

            return list;
        }

        private object SearchDocumentTypeDisplayNameCallBack(CacheItemArgs cacheItem)
        {
            var data = new Dictionary<string, string>();
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                var searchContentSources = GetSearchContentSourceList(portal.PortalID);
                foreach (var searchContentSource in searchContentSources)
                {
                    var key = string.Format("{0}-{1}", searchContentSource.SearchTypeId, searchContentSource.ModuleDefinitionId);
                    if(!data.ContainsKey(key))
                        data.Add(key, searchContentSource.LocalizedName);
                }
            }

            return data;
        }

        public string GetSearchDocumentTypeDisplayName(SearchResult searchResult)
        {
            //ModuleDefId will be zero for non-module
            var key = string.Format("{0}-{1}", searchResult.SearchTypeId, searchResult.ModuleDefId);
            var keys = CBO.Instance.GetCachedObject<IDictionary<string, string>>(
                            new CacheItemArgs(key, 120, CacheItemPriority.Default), SearchDocumentTypeDisplayNameCallBack, false);

            return keys.ContainsKey(key) ? keys[key] : string.Empty;
        }

        #region Core Search APIs

        public void AddSearchDocument(SearchDocument searchDocument)
        {
            AddSearchDocumentInternal(searchDocument, false);
        }

        private void AddSearchDocumentInternal(SearchDocument searchDocument, bool autoCommit)
        {
            Requires.NotNull("SearchDocument", searchDocument);
            Requires.NotNullOrEmpty("UniqueKey", searchDocument.UniqueKey);
            Requires.NotNegative("SearchTypeId", searchDocument.SearchTypeId);
            Requires.PropertyNotEqualTo("searchDocument", "SearchTypeId", searchDocument.SearchTypeId, 0);
            Requires.PropertyNotEqualTo("searchDocument", "ModifiedTimeUtc", searchDocument.ModifiedTimeUtc.ToString(CultureInfo.InvariantCulture), DateTime.MinValue.ToString(CultureInfo.InvariantCulture));
            
            if (searchDocument.SearchTypeId == _moduleSearchTypeId)
            {
                if(searchDocument.ModuleDefId <= 0)
                    throw new ArgumentException( Localization.Localization.GetExceptionMessage("ModuleDefIdMustBeGreaterThanZero","ModuleDefId must be greater than zero when SearchTypeId is for a module"));

                if (searchDocument.ModuleId <= 0)
                    throw new ArgumentException(Localization.Localization.GetExceptionMessage("ModuleIdMustBeGreaterThanZero","ModuleId must be greater than zero when SearchTypeId is for a module"));
            }
            else
            {
                if (searchDocument.ModuleDefId > 0)
                    throw new ArgumentException(Localization.Localization.GetExceptionMessage("ModuleDefIdWhenSearchTypeForModule","ModuleDefId is needed only when SearchTypeId is for a module"));

                if (searchDocument.ModuleId > 0)
                    throw new ArgumentException(Localization.Localization.GetExceptionMessage("ModuleIdWhenSearchTypeForModule","ModuleId is needed only when SearchTypeId is for a module"));
            }

            var doc = new Document();
            var sb = new StringBuilder();

            //TODO Set setOmitTermFreqAndPositions
            //TODO Check if Numeric fields need to have Store.YES or Store.NO
            //TODO - Should ModifiedTime be mandatory
            //TODO - Is Locale needed for a single-language site
            //TODO - Sorting

            //Field.Store.YES    | Stores the value. When the value is stored, the original String in its entirety is recorded in the index
            //Field.Store.NO     | Doesn’t store the value. This option is often used along with Index.ANALYZED to index a large text field that doesn’t need to be retrieved
            //                   | in its original form, such as bodies of web pages, or any other type of text document.
            //Index.ANALYZED     | Use the analyzer to break the field’s value into a stream of separate tokens and make each token searchable
            //Index.NOT_ANALYZED | Do index the field, but don’t analyze the String value.Instead, treat the Field’s entire value as a single token and make that token searchable.

            // Generic and Additional SearchDocument Params
            AddSearchDocumentParamters(doc, searchDocument, sb);


            //Remove the existing document from Lucene
            DeleteSearchDocumentInternal(searchDocument, false);

            //Add only when Document is active. The previous call would have otherwise deleted the document if it existed earlier
            if (searchDocument.IsActive)
            {
                Thread.SetData(Thread.GetNamedDataSlot(Constants.TlsSearchInfo), searchDocument);
                try
                {
                    LuceneController.Instance.Add(doc);
                }
                finally
                {
                    Thread.SetData(Thread.GetNamedDataSlot(Constants.TlsSearchInfo), null);
                }
            }

            if (autoCommit)
            {
                Commit();
            }
        }

        public void AddSearchDocuments(IEnumerable<SearchDocument> searchDocuments)
        {
            var searchDocs = searchDocuments as IList<SearchDocument> ?? searchDocuments.ToList();
            if (searchDocs.Any())
            {
                const int commitBatchSize = 1024;
                var idx = 0;
                var added = false;

                foreach (var searchDoc in searchDocs)
                {
                    try
                    {
                        AddSearchDocumentInternal(searchDoc, (++idx%commitBatchSize) == 0);
                        added = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("Search Document error: {0}{1}{2}", searchDoc, Environment.NewLine, ex);
                    }
                }

                // check so we don't commit again
                if (added && (idx % commitBatchSize) != 0)
                {
                    Commit();
                }
            }
        }

        public void DeleteSearchDocument(SearchDocument searchDocument)
        {
            Requires.NotNullOrEmpty("UniqueKey", searchDocument.UniqueKey);
            Requires.NotNegative("SearchTypeId", searchDocument.SearchTypeId);
            Requires.PropertyNotEqualTo("searchDocument", "SearchTypeId", searchDocument.SearchTypeId, 0);

            DeleteSearchDocumentInternal(searchDocument, false);
        }

        private void DeleteSearchDocumentInternal(SearchDocument searchDocument, bool autoCommit)
        {
            var query = new BooleanQuery
                {
                    {new TermQuery(new Term(Constants.UniqueKeyTag, searchDocument.UniqueKey)), Occur.MUST},
                    {NumericRangeQuery.NewIntRange(Constants.PortalIdTag, searchDocument.PortalId, searchDocument.PortalId, true, true), Occur.MUST},
                    {NumericRangeQuery.NewIntRange(Constants.SearchTypeTag, searchDocument.SearchTypeId, searchDocument.SearchTypeId, true, true), Occur.MUST}
                };

            //ModuleCrawler Type
            if (searchDocument.SearchTypeId == _moduleSearchTypeId)
            {
                if (searchDocument.ModuleId > 0)
                    query.Add(NumericRangeQuery.NewIntRange(Constants.ModuleIdTag, searchDocument.ModuleId, searchDocument.ModuleId, true, true), Occur.MUST);
                query.Add(NumericRangeQuery.NewIntRange(Constants.ModuleDefIdTag, searchDocument.ModuleDefId, searchDocument.ModuleDefId, true, true), Occur.MUST);
            }

            LuceneController.Instance.Delete(query);

            if (autoCommit)
            {
                Commit();
            }
        }

        public void DeleteSearchDocumentsByModule(int portalId, int moduleId, int moduleDefId)
        {
            var query = new BooleanQuery
                {
                    {NumericRangeQuery.NewIntRange(Constants.SearchTypeTag, SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId, SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId, true, true), Occur.MUST},
                    {NumericRangeQuery.NewIntRange(Constants.PortalIdTag, portalId, portalId, true, true), Occur.MUST},
                    {NumericRangeQuery.NewIntRange(Constants.ModuleIdTag, moduleId, moduleId, true, true), Occur.MUST},
                    {NumericRangeQuery.NewIntRange(Constants.ModuleDefIdTag, moduleDefId, moduleDefId, true, true), Occur.MUST}
                };

            LuceneController.Instance.Delete(query);
        }

        public void DeleteAllDocuments(int portalId, int searchTypeId)
        {
            var query = new BooleanQuery
                {
                    {NumericRangeQuery.NewIntRange(Constants.PortalIdTag, portalId, portalId, true, true), Occur.MUST},
                    {NumericRangeQuery.NewIntRange(Constants.SearchTypeTag, searchTypeId, searchTypeId, true, true), Occur.MUST}
                };

            LuceneController.Instance.Delete(query);
        }

        public void Commit()
        {
            LuceneController.Instance.Commit();
        }

        public bool OptimizeSearchIndex()
        {
            // run optimization in background
            return LuceneController.Instance.OptimizeSearchIndex(true);
        }

        public SearchStatistics GetSearchStatistics()
        {
            return LuceneController.Instance.GetSearchStatistics();
        }

        #endregion

        #region Private methods

        private void AddSearchDocumentParamters(Document doc, SearchDocument searchDocument, StringBuilder sb)
        {
            //mandatory fields
            doc.Add(new Field(Constants.UniqueKeyTag, StripTagsNoAttributes(searchDocument.UniqueKey, true), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new NumericField(Constants.PortalIdTag, Field.Store.YES, true).SetIntValue(searchDocument.PortalId));
            doc.Add(new NumericField(Constants.SearchTypeTag, Field.Store.YES, true).SetIntValue(searchDocument.SearchTypeId));
            doc.Add(!string.IsNullOrEmpty(searchDocument.CultureCode)
                        ? new NumericField(Constants.LocaleTag, Field.Store.YES, true).SetIntValue(Localization.Localization.GetCultureLanguageID(searchDocument.CultureCode))
                        : new NumericField(Constants.LocaleTag, Field.Store.YES, true).SetIntValue(-1));

            if (!string.IsNullOrEmpty(searchDocument.Title))
            {
                var field = new Field(Constants.TitleTag, StripTagsRetainAttributes(searchDocument.Title, HtmlAttributesToRetain, false, true), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                if (_titleBoost >0 && _titleBoost != Constants.StandardLuceneBoost) field.Boost = _titleBoost / 10f;
                doc.Add(field);
            }

            if (!string.IsNullOrEmpty(searchDocument.Description))
            {
                var field = new Field(Constants.DescriptionTag, StripTagsRetainAttributes(searchDocument.Description, HtmlAttributesToRetain, false, true), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                if (_descriptionBoost > 0 && _descriptionBoost != Constants.StandardLuceneBoost) field.Boost = _descriptionBoost / 10f;
                doc.Add(field);
            }

            if (!string.IsNullOrEmpty(searchDocument.Body))
            {
                doc.Add(new Field(Constants.BodyTag, StripTagsRetainAttributes(searchDocument.Body, HtmlAttributesToRetain, false, true), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            }

            if (!string.IsNullOrEmpty(searchDocument.Url))
            {
                doc.Add(new Field(Constants.UrlTag, StripTagsNoAttributes(searchDocument.Url, true), Field.Store.YES, Field.Index.NOT_ANALYZED));
            }

            if (!string.IsNullOrEmpty(searchDocument.QueryString))
            {
                doc.Add(new Field(Constants.QueryStringTag, searchDocument.QueryString, Field.Store.YES, Field.Index.NOT_ANALYZED));
            }

            foreach (var kvp in searchDocument.Keywords)
            {
                doc.Add(new Field(StripTagsNoAttributes(Constants.KeywordsPrefixTag + kvp.Key, true), StripTagsNoAttributes(kvp.Value, true), Field.Store.YES, Field.Index.NOT_ANALYZED));
                sb.Append(StripTagsNoAttributes(kvp.Value, true)).Append(" ");
            }

            foreach (var kvp in searchDocument.NumericKeys)
            {
                doc.Add(new NumericField(StripTagsNoAttributes(Constants.NumericKeyPrefixTag + kvp.Key, true), Field.Store.YES, true).SetIntValue(kvp.Value));
            }

            foreach (var tag in searchDocument.Tags)
            {
                var field = new Field(Constants.Tag, StripTagsNoAttributes(tag.ToLower(), true), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                if (_tagBoost > 0 && _tagBoost != Constants.StandardLuceneBoost) field.Boost = _tagBoost / 10f;
                doc.Add(field);
            }

            AddIntField(doc, searchDocument.TabId, Constants.TabIdTag);
            AddIntField(doc, searchDocument.ModuleDefId, Constants.ModuleDefIdTag);
            AddIntField(doc, searchDocument.ModuleId, Constants.ModuleIdTag);
            AddIntField(doc, searchDocument.AuthorUserId, Constants.AuthorIdTag);
            AddIntField(doc, searchDocument.RoleId, Constants.RoleIdTag);

            if (searchDocument.AuthorUserId > 0)
            {
                var user = UserController.Instance.GetUserById(searchDocument.PortalId, searchDocument.AuthorUserId);
                if (user != null && !string.IsNullOrEmpty(user.DisplayName))
                {
                    var field = new Field(Constants.AuthorNameTag, user.DisplayName, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                    if (_authorBoost > 0 && _authorBoost != Constants.StandardLuceneBoost) field.Boost = _authorBoost / 10f;
                    doc.Add(field);
                }
            }

            if (!string.IsNullOrEmpty(searchDocument.Permissions))
            {
                doc.Add(new Field(Constants.PermissionsTag, StripTagsNoAttributes(searchDocument.Permissions, true), Field.Store.YES, Field.Index.NOT_ANALYZED));
            }

            doc.Add(new NumericField(Constants.ModifiedTimeTag, Field.Store.YES, true).SetLongValue(long.Parse(searchDocument.ModifiedTimeUtc.ToString(Constants.DateTimeFormat))));

            if (sb.Length > 0)
            {
                var field = new Field(Constants.ContentTag, StripTagsNoAttributes(sb.ToString(), true), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                doc.Add(field);
                if (_contentBoost > 0 && _contentBoost != Constants.StandardLuceneBoost) field.Boost = _contentBoost/10f;
            }

        }

        /// <summary>
        /// Add Field to Doc when supplied fieldValue > 0
        /// </summary>
        private void AddIntField(Document doc, int fieldValue, string fieldTag)
        {
            if (fieldValue > 0)
                doc.Add(new NumericField(fieldTag, Field.Store.YES, true).SetIntValue(fieldValue));
        }

        private static string StripTagsNoAttributes(string html, bool retainSpace)
        {
            var strippedString = !String.IsNullOrEmpty(html) ? HtmlUtils.StripTags(html, retainSpace) : html;

            // Encode and Strip again
            strippedString = !String.IsNullOrEmpty(strippedString) ? HtmlUtils.StripTags(html, retainSpace) : html;

            return strippedString;
        }

        private const string HtmlTagsWithAttrs = "<[A-Za-z_:][\\w:.-]*(\\s+(?<attr>\\w+\\s*?=\\s*?[\"'].*?[\"']))+\\s*/?>";
        private static readonly Regex HtmlTagsRegex = new Regex(HtmlTagsWithAttrs, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private const string AttrText = "[\"'](?<text>.*?)[\"']";
        private static readonly Regex AttrTextRegex = new Regex(AttrText, RegexOptions.Compiled);

        private static string StripTagsRetainAttributes(string html, IEnumerable<string> attributes, bool decoded, bool retainSpace)
        {
            var attributesList = attributes as IList<string> ?? attributes.ToList();
            var attributeStr = string.Join("|", attributesList);
            var strippedString = html;
            var emptySpace = retainSpace ? " " : "";

            if (!String.IsNullOrEmpty(strippedString))
            {
                // Remove all opening HTML Tags with no attributes
                strippedString = Regex.Replace(strippedString, @"<\w*\s*>", emptySpace, RegexOptions.IgnoreCase);
                // Remove all closing HTML Tags
                strippedString = Regex.Replace(strippedString, @"</\w*\s*>", emptySpace, RegexOptions.IgnoreCase);
            }

            if (!String.IsNullOrEmpty(strippedString))
            {
                var list = new List<string>();

                foreach (var match in HtmlTagsRegex.Matches(strippedString).Cast<Match>())
                {
                    var captures = match.Groups["attr"].Captures;
                    foreach (var capture in captures.Cast<Capture>())
                    {
                        var val = capture.Value.Trim();
                        var pos = val.IndexOf('=');
                        if (pos > 0)
                        {
                            var attr = val.Substring(0, pos).Trim();
                            if (attributesList.Contains(attr))
                            {
                                var text = AttrTextRegex.Match(val).Groups["text"].Value.Trim();
                                if (text.Length > 0 && !list.Contains(text))
                                {
                                    list.Add(text);
                                }
                            }
                        }
                    }

                    if (list.Count > 0)
                    {
                        strippedString = strippedString.Replace(match.ToString(), string.Join(" ", list));
                        list.Clear();
                    }
                }
            }

            // If not decoded, decode and strip again. Becareful with recursive
            if (!decoded) strippedString = StripTagsRetainAttributes(HttpUtility.HtmlDecode(strippedString), attributesList, true, retainSpace);

            return strippedString;
        }

        #endregion
    }
}
