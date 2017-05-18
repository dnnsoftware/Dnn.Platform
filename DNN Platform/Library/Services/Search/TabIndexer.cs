#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Linq;
using System.Collections.Generic;
using DotNetNuke.Common;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.Index
    /// Class:      TabIndexer
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The TabIndexer is an implementation of the abstract IndexingProvider
    /// class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class TabIndexer : IndexingProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabIndexer));
        private static readonly int TabSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("tab").SearchTypeId;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns the number of SearchDocuments indexed with Tab MetaData for the given portal.
        /// </summary>
        /// <remarks>This replaces "GetSearchIndexItems" as a newer implementation of search.</remarks>
        /// -----------------------------------------------------------------------------
        public override int IndexSearchDocuments(int portalId,
            ScheduleHistoryItem schedule, DateTime startDateLocal, Action<IEnumerable<SearchDocument>> indexer)
        {
            Requires.NotNull("indexer", indexer);
            const int saveThreshold = 1024;
            var totalIndexed = 0;
            startDateLocal = GetLocalTimeOfLastIndexedItem(portalId, schedule.ScheduleID, startDateLocal);
            var searchDocuments = new List<SearchDocument>();
            var tabs = (
                from t in TabController.Instance.GetTabsByPortal(portalId).AsList()
                where t.LastModifiedOnDate > startDateLocal && (t.TabSettings["AllowIndex"] == null ||
                                                                "true".Equals(t.TabSettings["AllowIndex"].ToString(),
                                                                    StringComparison.CurrentCultureIgnoreCase))
                select t).OrderBy(t => t.LastModifiedOnDate).ThenBy(t => t.TabID).ToArray();

            if (tabs.Any())
            {
                foreach (var tab in tabs)
                {
                    try
                    {
                        var searchDoc = GetTabSearchDocument(tab);
                        searchDocuments.Add(searchDoc);

                        if (searchDocuments.Count >= saveThreshold)
                        {
                            totalIndexed += IndexCollectedDocs(indexer, searchDocuments, portalId, schedule.ScheduleID);
                        }
                    }
                    catch (Exception ex)
                    {
                        Exceptions.Exceptions.LogException(ex);
                    }
                }

                if (searchDocuments.Count > 0)
                {
                    totalIndexed += IndexCollectedDocs(indexer, searchDocuments, portalId, schedule.ScheduleID);
                }
            }

            return totalIndexed;
        }

        private static SearchDocument GetTabSearchDocument(TabInfo tab)
        {
            var searchDoc = new SearchDocument
            {
                SearchTypeId = TabSearchTypeId,
                UniqueKey = Constants.TabMetaDataPrefixTag + tab.TabID,
                TabId = tab.TabID,
                PortalId = tab.PortalID,
                CultureCode = tab.CultureCode,
                ModifiedTimeUtc = tab.LastModifiedOnDate.ToUniversalTime(),
                Body = string.Empty,
                Description = tab.Description
            };

            searchDoc.Keywords.Add("keywords", tab.KeyWords);

            //Using TabName for searchDoc.Title due to higher prevalence and relavency || TabTitle will be stored as a keyword
            searchDoc.Title = tab.TabName;
            searchDoc.Keywords.Add("title", tab.Title);

            if (tab.Terms != null && tab.Terms.Count > 0)
            {
                searchDoc.Tags = tab.Terms.Select(t => t.Name);
            }

            if (Logger.IsTraceEnabled)
            {
                Logger.Trace("TabIndexer: Search document for metaData added for page [" + tab.Title + " tid:" + tab.TabID + "]");
            }

            return searchDoc;
        }

        private int IndexCollectedDocs(Action<IEnumerable<SearchDocument>> indexer,
            ICollection<SearchDocument> searchDocuments, int portalId, int scheduleId)
        {
            indexer.Invoke(searchDocuments);
            var total = searchDocuments.Count;
            SetLocalTimeOfLastIndexedItem(portalId, scheduleId, searchDocuments.Last().ModifiedTimeUtc);
            searchDocuments.Clear();
            return total;
        }

        [Obsolete("Legacy Search (ISearchable) -- Deprecated in DNN 7.1. Use 'IndexSearchDocuments' instead.")]
        public override SearchItemInfoCollection GetSearchIndexItems(int portalId)
        {
            return null;
        }
    }
}