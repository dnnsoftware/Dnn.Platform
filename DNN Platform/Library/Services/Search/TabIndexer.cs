// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.Index
    /// Class:      TabIndexer
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The TabIndexer is an implementation of the abstract IndexingProvider
    /// class.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class TabIndexer : IndexingProviderBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabIndexer));
        private static readonly int TabSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("tab").SearchTypeId;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns the number of SearchDocuments indexed with Tab MetaData for the given portal.
        /// </summary>
        /// <remarks>This replaces "GetSearchIndexItems" as a newer implementation of search.</remarks>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public override int IndexSearchDocuments(
            int portalId,
            ScheduleHistoryItem schedule, DateTime startDateLocal, Action<IEnumerable<SearchDocument>> indexer)
        {
            Requires.NotNull("indexer", indexer);
            const int saveThreshold = 1024;
            var totalIndexed = 0;
            startDateLocal = this.GetLocalTimeOfLastIndexedItem(portalId, schedule.ScheduleID, startDateLocal);
            var searchDocuments = new List<SearchDocument>();
            var tabs = (
                from t in TabController.Instance.GetTabsByPortal(portalId).AsList()
                where t.LastModifiedOnDate > startDateLocal && (t.TabSettings["AllowIndex"] == null ||
                                                                "true".Equals(
                                                                    t.TabSettings["AllowIndex"].ToString(),
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
                            totalIndexed += this.IndexCollectedDocs(indexer, searchDocuments, portalId, schedule.ScheduleID);
                        }
                    }
                    catch (Exception ex)
                    {
                        Exceptions.Exceptions.LogException(ex);
                    }
                }

                if (searchDocuments.Count > 0)
                {
                    totalIndexed += this.IndexCollectedDocs(indexer, searchDocuments, portalId, schedule.ScheduleID);
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
                Description = tab.Description,
            };

            searchDoc.Keywords.Add("keywords", tab.KeyWords);

            // Using TabName for searchDoc.Title due to higher prevalence and relavency || TabTitle will be stored as a keyword
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

        private int IndexCollectedDocs(
            Action<IEnumerable<SearchDocument>> indexer,
            ICollection<SearchDocument> searchDocuments, int portalId, int scheduleId)
        {
            indexer.Invoke(searchDocuments);
            var total = searchDocuments.Count;
            this.SetLocalTimeOfLastIndexedItem(portalId, scheduleId, searchDocuments.Last().ModifiedTimeUtc);
            searchDocuments.Clear();
            return total;
        }
    }
}
