// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using Newtonsoft.Json;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke
    /// Class:      SearchEngine
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchEngine  manages the Indexing of the Portal content.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    internal class SearchEngine
    {
        internal SearchEngine(ScheduleHistoryItem scheduler, DateTime startTime)
        {
            this.SchedulerItem = scheduler;
            this.IndexingStartTime = startTime;
        }

        public ScheduleHistoryItem SchedulerItem { get; private set; }

        // the time from where to start indexing items
        public DateTime IndexingStartTime { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Indexes content within the given time farame.
        /// </summary>
        /// -----------------------------------------------------------------------------
        internal void IndexContent()
        {
            // Index TAB META-DATA
            var tabIndexer = new TabIndexer();
            var searchDocsCount = this.GetAndStoreSearchDocuments(tabIndexer);
            var indexedSearchDocumentCount = searchDocsCount;
            this.AddIdexingResults("Tabs Indexed", searchDocsCount);

            // Index MODULE META-DATA from modules that inherit from ModuleSearchBase
            var moduleIndexer = new ModuleIndexer(true);
            searchDocsCount = this.GetAndStoreModuleMetaData(moduleIndexer);
            indexedSearchDocumentCount += searchDocsCount;
            this.AddIdexingResults("Modules (Metadata) Indexed", searchDocsCount);

            // Index MODULE CONTENT from modules that inherit from ModuleSearchBase
            searchDocsCount = this.GetAndStoreSearchDocuments(moduleIndexer);
            indexedSearchDocumentCount += searchDocsCount;

            // Index all Defunct ISearchable module content
#pragma warning disable 0618
            var searchItems = this.GetContent(moduleIndexer);
            SearchDataStoreProvider.Instance().StoreSearchItems(searchItems);
#pragma warning restore 0618
            indexedSearchDocumentCount += searchItems.Count;

            // Both ModuleSearchBase and ISearchable module content count
            this.AddIdexingResults("Modules (Content) Indexed", searchDocsCount + searchItems.Count);

            if (!HostController.Instance.GetBoolean("DisableUserCrawling", false))
            {
                // Index User data
                var userIndexer = new UserIndexer();
                var userIndexed = this.GetAndStoreSearchDocuments(userIndexer);
                indexedSearchDocumentCount += userIndexed;
                this.AddIdexingResults("Users", userIndexed);
            }

            this.SchedulerItem.AddLogNote("<br/><b>Total Items Indexed: " + indexedSearchDocumentCount + "</b>");
        }

        internal bool CompactSearchIndexIfNeeded(ScheduleHistoryItem scheduleItem)
        {
            var shelper = SearchHelper.Instance;
            if (shelper.GetSearchCompactFlag())
            {
                shelper.SetSearchReindexRequestTime(false);
                var stopWatch = System.Diagnostics.Stopwatch.StartNew();
                if (InternalSearchController.Instance.OptimizeSearchIndex())
                {
                    stopWatch.Stop();
                    scheduleItem.AddLogNote(string.Format("<br/><b>Compacted Index, total time {0}</b>", stopWatch.Elapsed));
                }
            }

            return false;
        }

        /// <summary>
        /// Deletes all old documents when re-index was requested, so we start a fresh search.
        /// </summary>
        internal void DeleteOldDocsBeforeReindex()
        {
            var portal2Reindex = SearchHelper.Instance.GetPortalsToReindex(this.IndexingStartTime);
            var controller = InternalSearchController.Instance;

            foreach (var portalId in portal2Reindex)
            {
                controller.DeleteAllDocuments(portalId, SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId);
                controller.DeleteAllDocuments(portalId, SearchHelper.Instance.GetSearchTypeByName("tab").SearchTypeId);
            }
        }

        /// <summary>
        /// Deletes all deleted items from the system that are added to deletions table.
        /// </summary>
        internal void DeleteRemovedObjects()
        {
            var deletedCount = 0;
            var cutoffTime = this.SchedulerItem.StartDate.ToUniversalTime();
            var searchController = InternalSearchController.Instance;
            var dataProvider = DataProvider.Instance();
            using (var reader = dataProvider.GetSearchDeletedItems(cutoffTime))
            {
                while (reader.Read())
                {
                    // Note: we saved this in the DB as SearchDocumentToDelete but retrieve as the descendant SearchDocument class
                    var document = JsonConvert.DeserializeObject<SearchDocument>(reader["document"] as string);
                    searchController.DeleteSearchDocument(document);
                    deletedCount += 1;
                }

                reader.Close();
            }

            this.AddIdexingResults("Deleted Objects", deletedCount);
            dataProvider.DeleteProcessedSearchDeletedItems(cutoffTime);
        }

        /// <summary>
        /// Commits (flushes) all added and deleted content to search engine's disk file.
        /// </summary>
        internal void Commit()
        {
            InternalSearchController.Instance.Commit();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LEGACY: Deprecated in DNN 7.1. Use 'IndexSearchDocuments' instead.
        /// Used for Legacy Search (ISearchable)
        ///
        /// GetContent gets all the content and passes it to the Indexer.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="indexer">The Index Provider that will index the content of the portal.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        [Obsolete("Legacy Search (ISearchable) -- Deprecated in DNN 7.1. Use 'IndexSearchDocuments' instead.. Scheduled removal in v10.0.0.")]
        protected SearchItemInfoCollection GetContent(IndexingProviderBase indexer)
        {
            var searchItems = new SearchItemInfoCollection();
            var portals = PortalController.Instance.GetPortals();
            for (var index = 0; index <= portals.Count - 1; index++)
            {
                var portal = (PortalInfo)portals[index];
                searchItems.AddRange(indexer.GetSearchIndexItems(portal.PortalID));
            }

            return searchItems;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LEGACY: Deprecated in DNN 7.1. Use 'IndexSearchDocuments' instead.
        /// Used for Legacy Search (ISearchable)
        ///
        /// GetContent gets the Portal's content and passes it to the Indexer.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal.</param>
        /// <param name="indexer">The Index Provider that will index the content of the portal.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        [Obsolete("Legacy Search (ISearchable) -- Deprecated in DNN 7.1. Use 'IndexSearchDocuments' instead.. Scheduled removal in v10.0.0.")]
        protected SearchItemInfoCollection GetContent(int portalId, IndexingProvider indexer)
        {
            var searchItems = new SearchItemInfoCollection();
            searchItems.AddRange(indexer.GetSearchIndexItems(portalId));
            return searchItems;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Ensures all SearchDocuments have a SearchTypeId.
        /// </summary>
        /// <param name="searchDocs"></param>
        /// -----------------------------------------------------------------------------
        private static void StoreSearchDocuments(IEnumerable<SearchDocument> searchDocs)
        {
            var defaultSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId;

            var searchDocumentList = searchDocs as IList<SearchDocument> ?? searchDocs.ToList();
            foreach (var searchDocument in searchDocumentList.Where(searchDocument => searchDocument.SearchTypeId <= 0))
            {
                searchDocument.SearchTypeId = defaultSearchTypeId;
            }

            InternalSearchController.Instance.AddSearchDocuments(searchDocumentList);
        }

        private void AddIdexingResults(string description, int count)
        {
            this.SchedulerItem.AddLogNote(string.Format("<br/>&nbsp;&nbsp;{0}: {1}", description, count));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets all the Search Documents for the given timeframe.
        /// </summary>
        /// <param name="indexer"></param>
        /// -----------------------------------------------------------------------------
        private int GetAndStoreSearchDocuments(IndexingProviderBase indexer)
        {
            IList<SearchDocument> searchDocs;
            var portals = PortalController.Instance.GetPortals();
            DateTime indexSince;
            var indexedCount = 0;

            foreach (var portal in portals.Cast<PortalInfo>())
            {
                indexSince = this.FixedIndexingStartDate(portal.PortalID);
                try
                {
                    indexedCount += indexer.IndexSearchDocuments(
                        portal.PortalID, this.SchedulerItem, indexSince, StoreSearchDocuments);
                }
                catch (NotImplementedException)
                {
#pragma warning disable 618
                    searchDocs = indexer.GetSearchDocuments(portal.PortalID, indexSince).ToList();
#pragma warning restore 618
                    StoreSearchDocuments(searchDocs);
                    indexedCount += searchDocs.Count();
                }
            }

            // Include Host Level Items
            indexSince = this.FixedIndexingStartDate(-1);
            try
            {
                indexedCount += indexer.IndexSearchDocuments(
                    Null.NullInteger, this.SchedulerItem, indexSince, StoreSearchDocuments);
            }
            catch (NotImplementedException)
            {
#pragma warning disable 618
                searchDocs = indexer.GetSearchDocuments(-1, indexSince).ToList();
#pragma warning restore 618
                StoreSearchDocuments(searchDocs);
                indexedCount += searchDocs.Count();
            }

            return indexedCount;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets all the Searchable Module MetaData SearchDocuments within the timeframe for all portals.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private int GetAndStoreModuleMetaData(ModuleIndexer indexer)
        {
            IEnumerable<SearchDocument> searchDocs;
            var portals = PortalController.Instance.GetPortals();
            DateTime indexSince;
            var indexedCount = 0;

            // DateTime startDate
            foreach (var portal in portals.Cast<PortalInfo>())
            {
                indexSince = this.FixedIndexingStartDate(portal.PortalID);
                searchDocs = indexer.GetModuleMetaData(portal.PortalID, indexSince);
                StoreSearchDocuments(searchDocs);
                indexedCount += searchDocs.Count();
            }

            // Include Host Level Items
            indexSince = this.FixedIndexingStartDate(Null.NullInteger);
            searchDocs = indexer.GetModuleMetaData(Null.NullInteger, indexSince);
            StoreSearchDocuments(searchDocs);
            indexedCount += searchDocs.Count();

            return indexedCount;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adjusts the re-index date/time to account for the portal reindex value.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private DateTime FixedIndexingStartDate(int portalId)
        {
            var startDate = this.IndexingStartTime;
            if (startDate < SqlDateTime.MinValue.Value ||
                SearchHelper.Instance.IsReindexRequested(portalId, startDate))
            {
                return SqlDateTime.MinValue.Value.AddDays(1);
            }

            return startDate;
        }
    }
}
