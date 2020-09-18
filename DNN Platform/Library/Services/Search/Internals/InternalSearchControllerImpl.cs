// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
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
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Search.Controllers;
    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Search;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The Impl Controller class for Lucene.
    /// </summary>
    /// -----------------------------------------------------------------------------
    internal class InternalSearchControllerImpl : IInternalSearchController
    {
        private const string SearchableModuleDefsKey = "{0}-{1}";
        private const string SearchableModuleDefsCacheKey = "SearchableModuleDefs";
        private const string LocalizedResxFile = "~/DesktopModules/Admin/SearchResults/App_LocalResources/SearchableModules.resx";

        private const string HtmlTagsWithAttrs = "<[a-z_:][\\w:.-]*(\\s+(?<attr>\\w+\\s*?=\\s*?[\"'].*?[\"']))+\\s*/?>";

        private const string AttrText = "[\"'](?<text>.*?)[\"']";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InternalSearchControllerImpl));

        private static readonly string[] HtmlAttributesToRetain = { "alt", "title" };
        private static readonly DataProvider DataProvider = DataProvider.Instance();

        private static readonly Regex StripOpeningTagsRegex = new Regex(@"<\w*\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex StripClosingTagsRegex = new Regex(@"</\w*\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HtmlTagsRegex = new Regex(HtmlTagsWithAttrs, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex AttrTextRegex = new Regex(AttrText, RegexOptions.Compiled);

        private readonly int _titleBoost;
        private readonly int _tagBoost;
        private readonly int _contentBoost;
        private readonly int _descriptionBoost;
        private readonly int _authorBoost;
        private readonly int _moduleSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId;

        public InternalSearchControllerImpl()
        {
            var hostController = HostController.Instance;
            this._titleBoost = hostController.GetInteger(Constants.SearchTitleBoostSetting, Constants.DefaultSearchTitleBoost);
            this._tagBoost = hostController.GetInteger(Constants.SearchTagBoostSetting, Constants.DefaultSearchTagBoost);
            this._contentBoost = hostController.GetInteger(Constants.SearchContentBoostSetting, Constants.DefaultSearchKeywordBoost);
            this._descriptionBoost = hostController.GetInteger(Constants.SearchDescriptionBoostSetting, Constants.DefaultSearchDescriptionBoost);
            this._authorBoost = hostController.GetInteger(Constants.SearchAuthorBoostSetting, Constants.DefaultSearchAuthorBoost);
        }

        public IEnumerable<SearchContentSource> GetSearchContentSourceList(int portalId)
        {
            var searchableModuleDefsCacheArgs = new CacheItemArgs(
                    string.Format(SearchableModuleDefsKey, SearchableModuleDefsCacheKey, portalId),
                    120, CacheItemPriority.Default);

            var list = CBO.GetCachedObject<IList<SearchContentSource>>(
                searchableModuleDefsCacheArgs, this.SearchContentSourceCallback);

            return list;
        }

        public string GetSearchDocumentTypeDisplayName(SearchResult searchResult)
        {
            // ModuleDefId will be zero for non-module
            var key = string.Format("{0}-{1}", searchResult.SearchTypeId, searchResult.ModuleDefId);
            var keys = CBO.Instance.GetCachedObject<IDictionary<string, string>>(
                            new CacheItemArgs(key, 120, CacheItemPriority.Default), this.SearchDocumentTypeDisplayNameCallBack, false);

            return keys.ContainsKey(key) ? keys[key] : string.Empty;
        }

        public void AddSearchDocument(SearchDocument searchDocument)
        {
            this.AddSearchDocumentInternal(searchDocument, false);
        }

        public void AddSearchDocuments(IEnumerable<SearchDocument> searchDocuments)
        {
            var searchDocs = searchDocuments as IList<SearchDocument> ?? searchDocuments.ToList();
            if (searchDocs.Any())
            {
                const int commitBatchSize = 1024 * 16;
                var idx = 0;

                // var added = false;
                foreach (var searchDoc in searchDocs)
                {
                    try
                    {
                        this.AddSearchDocumentInternal(searchDoc, (++idx % commitBatchSize) == 0);

                        // added = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("Search Document error: {0}{1}{2}", searchDoc, Environment.NewLine, ex);
                    }
                }

                // Note: modified to do commit only once at the end of scheduler job
                // check so we don't commit again
                // if (added && (idx % commitBatchSize) != 0)
                // {
                //    Commit();
                // }
            }
        }

        public void DeleteSearchDocument(SearchDocument searchDocument)
        {
            this.DeleteSearchDocumentInternal(searchDocument, false);
        }

        public void DeleteSearchDocumentsByModule(int portalId, int moduleId, int moduleDefId)
        {
            Requires.NotNegative("PortalId", portalId);

            this.DeleteSearchDocument(new SearchDocument
            {
                PortalId = portalId,
                ModuleId = moduleId,
                ModuleDefId = moduleDefId,
                SearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId,
            });
        }

        public void DeleteAllDocuments(int portalId, int searchTypeId)
        {
            Requires.NotNegative("SearchTypeId", searchTypeId);

            this.DeleteSearchDocument(new SearchDocument
            {
                PortalId = portalId,
                SearchTypeId = searchTypeId,
            });
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
                            if (!modDefIds.Contains(module.ModuleDefID))
                            {
                                modDefIds.Add(module.ModuleDefID);
                            }
                        }

                        var list = modDefIds.Select(ModuleDefinitionController.GetModuleDefinitionByID).ToList();

                        foreach (var def in list)
                        {
                            var text = Localization.GetSafeJSString("Module_" + def.DefinitionName, LocalizedResxFile);
                            if (string.IsNullOrEmpty(text))
                            {
                                text = def.FriendlyName;
                            }

                            var result = new SearchContentSource
                            {
                                SearchTypeId = crawler.SearchTypeId,
                                SearchTypeName = crawler.SearchTypeName,
                                IsPrivate = crawler.IsPrivate,
                                ModuleDefinitionId = def.ModuleDefID,
                                LocalizedName = text,
                            };

                            results.Add(result);
                        }

                        break;

                    default:

                        var resultControllerType = Reflection.CreateType(crawler.SearchResultClass);
                        var resultController = (BaseResultController)Reflection.CreateObject(resultControllerType);
                        var localizedName = Localization.GetSafeJSString(resultController.LocalizedSearchTypeName);

                        results.Add(new SearchContentSource
                        {
                            SearchTypeId = crawler.SearchTypeId,
                            SearchTypeName = crawler.SearchTypeName,
                            IsPrivate = crawler.IsPrivate,
                            ModuleDefinitionId = 0,
                            LocalizedName = localizedName,
                        });
                        break;
                }
            }

            return results;
        }

        private static Query NumericValueQuery(string numericName, int numericVal)
        {
            return NumericRangeQuery.NewIntRange(numericName, numericVal, numericVal, true, true);
        }

        /// <summary>
        /// Add Field to Doc when supplied fieldValue > 0.
        /// </summary>
        private static void AddIntField(Document doc, int fieldValue, string fieldTag)
        {
            if (fieldValue > 0)
            {
                doc.Add(new NumericField(fieldTag, Field.Store.YES, true).SetIntValue(fieldValue));
            }
        }

        private static string StripTagsRetainAttributes(string html, IEnumerable<string> attributes, bool decoded, bool retainSpace)
        {
            var attributesList = attributes as IList<string> ?? attributes.ToList();
            var strippedString = html;
            var emptySpace = retainSpace ? " " : string.Empty;

            if (!string.IsNullOrEmpty(strippedString))
            {
                // Remove all opening HTML Tags with no attributes
                strippedString = StripOpeningTagsRegex.Replace(strippedString, emptySpace);

                // Remove all closing HTML Tags
                strippedString = StripClosingTagsRegex.Replace(strippedString, emptySpace);
            }

            if (!string.IsNullOrEmpty(strippedString))
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
            if (!decoded)
            {
                strippedString = StripTagsRetainAttributes(HttpUtility.HtmlDecode(strippedString), attributesList, true, retainSpace);
            }

            return strippedString;
        }

        private object SearchDocumentTypeDisplayNameCallBack(CacheItemArgs cacheItem)
        {
            var data = new Dictionary<string, string>();
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                var searchContentSources = this.GetSearchContentSourceList(portal.PortalID);
                foreach (var searchContentSource in searchContentSources)
                {
                    var key = string.Format("{0}-{1}", searchContentSource.SearchTypeId, searchContentSource.ModuleDefinitionId);
                    if (!data.ContainsKey(key))
                    {
                        data.Add(key, searchContentSource.LocalizedName);
                    }
                }
            }

            return data;
        }

        private void AddSearchDocumentInternal(SearchDocument searchDocument, bool autoCommit)
        {
            Requires.NotNull("SearchDocument", searchDocument);
            Requires.NotNullOrEmpty("UniqueKey", searchDocument.UniqueKey);
            Requires.NotNegative("SearchTypeId", searchDocument.SearchTypeId);
            Requires.PropertyNotEqualTo("searchDocument", "SearchTypeId", searchDocument.SearchTypeId, 0);
            Requires.PropertyNotEqualTo("searchDocument", "ModifiedTimeUtc", searchDocument.ModifiedTimeUtc.ToString(CultureInfo.InvariantCulture), DateTime.MinValue.ToString(CultureInfo.InvariantCulture));

            if (searchDocument.SearchTypeId == this._moduleSearchTypeId)
            {
                if (searchDocument.ModuleDefId <= 0)
                {
                    throw new ArgumentException(Localization.GetExceptionMessage("ModuleDefIdMustBeGreaterThanZero", "ModuleDefId must be greater than zero when SearchTypeId is for a module"));
                }

                if (searchDocument.ModuleId <= 0)
                {
                    throw new ArgumentException(Localization.GetExceptionMessage("ModuleIdMustBeGreaterThanZero", "ModuleId must be greater than zero when SearchTypeId is for a module"));
                }
            }
            else
            {
                if (searchDocument.ModuleDefId > 0)
                {
                    throw new ArgumentException(Localization.GetExceptionMessage("ModuleDefIdWhenSearchTypeForModule", "ModuleDefId is needed only when SearchTypeId is for a module"));
                }

                if (searchDocument.ModuleId > 0)
                {
                    throw new ArgumentException(Localization.GetExceptionMessage("ModuleIdWhenSearchTypeForModule", "ModuleId is needed only when SearchTypeId is for a module"));
                }
            }

            var doc = new Document();
            var sb = new StringBuilder();

            // TODO - Set setOmitTermFreqAndPositions
            // TODO - Check if Numeric fields need to have Store.YES or Store.NO
            // TODO - Should ModifiedTime be mandatory
            // TODO - Is Locale needed for a single-language site
            // TODO - Sorting

            // Field.Store.YES    | Stores the value. When the value is stored, the original String in its entirety is recorded in the index
            // Field.Store.NO     | Doesn’t store the value. This option is often used along with Index.ANALYZED to index a large text field that doesn’t need to be retrieved
            //                   | in its original form, such as bodies of web pages, or any other type of text document.
            // Index.ANALYZED     | Use the analyzer to break the field’s value into a stream of separate tokens and make each token searchable
            // Index.NOT_ANALYZED | Do index the field, but don’t analyze the String value.Instead, treat the Field’s entire value as a single token and make that token searchable.

            // Generic and Additional SearchDocument Params
            this.AddSearchDocumentParamters(doc, searchDocument, sb);

            // Remove the existing document from Lucene
            this.DeleteSearchDocumentInternal(searchDocument, false);

            // Add only when Document is active. The previous call would have otherwise deleted the document if it existed earlier
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
                this.Commit();
            }
        }

        private void DeleteSearchDocumentInternal(SearchDocument searchDocument, bool autoCommit)
        {
            var query = new BooleanQuery();

            if (searchDocument.SearchTypeId > -1)
            {
                query.Add(NumericValueQuery(Constants.SearchTypeTag, searchDocument.SearchTypeId), Occur.MUST);
            }

            if (searchDocument.PortalId > -1)
            {
                query.Add(NumericValueQuery(Constants.PortalIdTag, searchDocument.PortalId), Occur.MUST);
            }

            if (searchDocument.RoleId > -1)
            {
                query.Add(NumericValueQuery(Constants.RoleIdTag, searchDocument.RoleId), Occur.MUST);
            }

            if (searchDocument.ModuleDefId > 0)
            {
                query.Add(NumericValueQuery(Constants.ModuleDefIdTag, searchDocument.ModuleDefId), Occur.MUST);
            }

            if (searchDocument.ModuleId > 0)
            {
                query.Add(NumericValueQuery(Constants.ModuleIdTag, searchDocument.ModuleId), Occur.MUST);
            }

            if (searchDocument.TabId > 0)
            {
                query.Add(NumericValueQuery(Constants.TabIdTag, searchDocument.TabId), Occur.MUST);
            }

            if (searchDocument.AuthorUserId > 0)
            {
                query.Add(NumericValueQuery(Constants.AuthorIdTag, searchDocument.AuthorUserId), Occur.MUST);
            }

            if (!string.IsNullOrEmpty(searchDocument.UniqueKey))
            {
                query.Add(new TermQuery(new Term(Constants.UniqueKeyTag, searchDocument.UniqueKey)), Occur.MUST);
            }

            if (!string.IsNullOrEmpty(searchDocument.QueryString))
            {
                query.Add(new TermQuery(new Term(Constants.QueryStringTag, searchDocument.QueryString)), Occur.MUST);
            }

            if (!string.IsNullOrEmpty(searchDocument.CultureCode))
            {
                query.Add(NumericValueQuery(Constants.LocaleTag, Localization.GetCultureLanguageID(searchDocument.CultureCode)), Occur.MUST);
            }

            LuceneController.Instance.Delete(query);

            if (autoCommit)
            {
                this.Commit();
            }
        }

        private void AddSearchDocumentParamters(Document doc, SearchDocument searchDocument, StringBuilder sb)
        {
            // mandatory fields
            doc.Add(new Field(Constants.UniqueKeyTag, SearchHelper.Instance.StripTagsNoAttributes(searchDocument.UniqueKey, true), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new NumericField(Constants.PortalIdTag, Field.Store.YES, true).SetIntValue(searchDocument.PortalId));
            doc.Add(new NumericField(Constants.SearchTypeTag, Field.Store.YES, true).SetIntValue(searchDocument.SearchTypeId));
            doc.Add(!string.IsNullOrEmpty(searchDocument.CultureCode)
                        ? new NumericField(Constants.LocaleTag, Field.Store.YES, true).SetIntValue(Localization.GetCultureLanguageID(searchDocument.CultureCode))
                        : new NumericField(Constants.LocaleTag, Field.Store.YES, true).SetIntValue(-1));

            if (!string.IsNullOrEmpty(searchDocument.Title))
            {
                var field = new Field(Constants.TitleTag, StripTagsRetainAttributes(searchDocument.Title, HtmlAttributesToRetain, false, true), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                if (this._titleBoost > 0 && this._titleBoost != Constants.StandardLuceneBoost)
                {
                    field.Boost = this._titleBoost / 10f;
                }

                doc.Add(field);
            }

            if (!string.IsNullOrEmpty(searchDocument.Description))
            {
                var field = new Field(Constants.DescriptionTag, StripTagsRetainAttributes(searchDocument.Description, HtmlAttributesToRetain, false, true), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                if (this._descriptionBoost > 0 && this._descriptionBoost != Constants.StandardLuceneBoost)
                {
                    field.Boost = this._descriptionBoost / 10f;
                }

                doc.Add(field);
            }

            if (!string.IsNullOrEmpty(searchDocument.Body))
            {
                doc.Add(new Field(Constants.BodyTag, StripTagsRetainAttributes(searchDocument.Body, HtmlAttributesToRetain, false, true), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            }

            if (!string.IsNullOrEmpty(searchDocument.Url))
            {
                doc.Add(new Field(Constants.UrlTag, SearchHelper.Instance.StripTagsNoAttributes(searchDocument.Url, true), Field.Store.YES, Field.Index.NOT_ANALYZED));
            }

            if (!string.IsNullOrEmpty(searchDocument.QueryString))
            {
                doc.Add(new Field(Constants.QueryStringTag, searchDocument.QueryString, Field.Store.YES, Field.Index.NOT_ANALYZED));
            }

            foreach (var kvp in searchDocument.Keywords)
            {
                var key = kvp.Key.ToLowerInvariant();
                var needAnalyzed = Constants.FieldsNeedAnalysis.Contains(key);
                var field = new Field(SearchHelper.Instance.StripTagsNoAttributes(Constants.KeywordsPrefixTag + kvp.Key, true), SearchHelper.Instance.StripTagsNoAttributes(kvp.Value, true), Field.Store.YES, needAnalyzed ? Field.Index.ANALYZED : Field.Index.NOT_ANALYZED);
                switch (key)
                {
                    case Constants.TitleTag:
                        if (this._titleBoost > 0 && this._titleBoost != Constants.StandardLuceneBoost)
                        {
                            field.Boost = this._titleBoost / 10f;
                        }

                        break;
                    case Constants.SubjectTag:
                        if (this._contentBoost > 0 && this._contentBoost != Constants.StandardLuceneBoost)
                        {
                            field.Boost = this._contentBoost / 10f;
                        }

                        break;
                    case Constants.CommentsTag:
                        if (this._descriptionBoost > 0 && this._descriptionBoost != Constants.StandardLuceneBoost)
                        {
                            field.Boost = this._descriptionBoost / 10f;
                        }

                        break;
                    case Constants.AuthorNameTag:
                        if (this._authorBoost > 0 && this._authorBoost != Constants.StandardLuceneBoost)
                        {
                            field.Boost = this._authorBoost / 10f;
                        }

                        break;
                }

                doc.Add(field);
                sb.Append(SearchHelper.Instance.StripTagsNoAttributes(kvp.Value, true)).Append(" ");
            }

            foreach (var kvp in searchDocument.NumericKeys)
            {
                doc.Add(new NumericField(SearchHelper.Instance.StripTagsNoAttributes(Constants.NumericKeyPrefixTag + kvp.Key, true), Field.Store.YES, true).SetIntValue(kvp.Value));
            }

            bool tagBoostApplied = false;
            foreach (var tag in searchDocument.Tags)
            {
                var field = new Field(Constants.Tag, SearchHelper.Instance.StripTagsNoAttributes(tag.ToLowerInvariant(), true), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                if (!tagBoostApplied)
                {
                    if (this._tagBoost > 0 && this._tagBoost != Constants.StandardLuceneBoost)
                    {
                        field.Boost = this._tagBoost / 10f;
                        tagBoostApplied = true;
                    }
                }

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
                    if (this._authorBoost > 0 && this._authorBoost != Constants.StandardLuceneBoost)
                    {
                        field.Boost = this._authorBoost / 10f;
                    }

                    doc.Add(field);
                }
            }

            if (!string.IsNullOrEmpty(searchDocument.Permissions))
            {
                doc.Add(new Field(Constants.PermissionsTag, SearchHelper.Instance.StripTagsNoAttributes(searchDocument.Permissions, true), Field.Store.YES, Field.Index.NOT_ANALYZED));
            }

            doc.Add(new NumericField(Constants.ModifiedTimeTag, Field.Store.YES, true).SetLongValue(long.Parse(searchDocument.ModifiedTimeUtc.ToString(Constants.DateTimeFormat))));

            if (sb.Length > 0)
            {
                var field = new Field(Constants.ContentTag, SearchHelper.Instance.StripTagsNoAttributes(sb.ToString(), true), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                doc.Add(field);
                if (this._contentBoost > 0 && this._contentBoost != Constants.StandardLuceneBoost)
                {
                    field.Boost = this._contentBoost / 10f;
                }
            }
        }
    }
}
