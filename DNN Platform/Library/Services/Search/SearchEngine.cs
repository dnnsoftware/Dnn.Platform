// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using Newtonsoft.Json;

    /// <summary>The SearchEngine manages the Indexing of the Portal content.</summary>
    internal partial class SearchEngine
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchEngine));
        private readonly IBusinessControllerProvider businessControllerProvider;

        /// <summary>Initializes a new instance of the <see cref="SearchEngine"/> class.</summary>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        internal SearchEngine(ScheduleHistoryItem scheduler, DateTime startTime, IBusinessControllerProvider businessControllerProvider)
        {
            this.businessControllerProvider = businessControllerProvider;
            this.SchedulerItem = scheduler;
            this.IndexingStartTime = startTime;
        }

        /// <summary>Gets the scheduler item.</summary>
        public ScheduleHistoryItem SchedulerItem { get; private set; }

        /// <summary>Gets the time from where to start indexing items.</summary>
        public DateTime IndexingStartTime { get; private set; }

        /// <summary>Indexes content within the given time frame.</summary>
        internal void IndexContent()
        {
            // Index TAB META-DATA
            var tabIndexer = new TabIndexer();
            var searchDocsCount = this.GetAndStoreSearchDocuments(tabIndexer);
            var indexedSearchDocumentCount = searchDocsCount;
            this.AddIndexingResults("Tabs Indexed", searchDocsCount);

            // Index MODULE META-DATA from modules that inherit from ModuleSearchBase
            var moduleIndexer = new ModuleIndexer(true, this.businessControllerProvider);
            searchDocsCount = this.GetAndStoreModuleMetaData(moduleIndexer);
            indexedSearchDocumentCount += searchDocsCount;
            this.AddIndexingResults("Modules (Metadata) Indexed", searchDocsCount);

            // Index MODULE CONTENT from modules that inherit from ModuleSearchBase
            searchDocsCount = this.GetAndStoreSearchDocuments(moduleIndexer);
            indexedSearchDocumentCount += searchDocsCount;

            this.AddIndexingResults("Modules (Content) Indexed", searchDocsCount);

            if (!HostController.Instance.GetBoolean("DisableUserCrawling", false))
            {
                // Index User data
                var userIndexer = new UserIndexer();
                var userIndexed = this.GetAndStoreSearchDocuments(userIndexer);
                indexedSearchDocumentCount += userIndexed;
                this.AddIndexingResults("Users", userIndexed);
            }

            this.SchedulerItem.AddLogNote("<br/><b>Total Items Indexed: " + indexedSearchDocumentCount + "</b>");
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal bool CompactSearchIndexIfNeeded(ScheduleHistoryItem scheduleItem)
        {
            var searchHelper = SearchHelper.Instance;
            if (!searchHelper.GetSearchCompactFlag())
            {
                return false;
            }

            searchHelper.SetSearchReindexRequestTime(false);
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            if (!InternalSearchController.Instance.OptimizeSearchIndex())
            {
                return false;
            }

            stopWatch.Stop();
            scheduleItem.AddLogNote($"<br/><b>Compacted Index, total time {stopWatch.Elapsed}</b>");

            return false;
        }

        /// <summary>Deletes all old documents when re-index was requested, so we start a fresh search.</summary>
        internal void DeleteOldDocsBeforeReindex()
        {
            var controller = InternalSearchController.Instance;
            var moduleSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId;
            var tabSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("tab").SearchTypeId;
            foreach (var portalId in SearchHelper.Instance.GetPortalsToReindex(this.IndexingStartTime))
            {
                controller.DeleteAllDocuments(portalId, moduleSearchTypeId);
                controller.DeleteAllDocuments(portalId, tabSearchTypeId);
            }
        }

        /// <summary>Deletes all deleted items from the system that are added to deletions table.</summary>
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

            this.AddIndexingResults("Deleted Objects", deletedCount);
            dataProvider.DeleteProcessedSearchDeletedItems(cutoffTime);
        }

        /// <summary>Commits (flushes) all added and deleted content to search engine's disk file.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal void Commit()
        {
            InternalSearchController.Instance.Commit();
        }

        /// <summary>Ensures all SearchDocuments have a SearchTypeId.</summary>
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

        private void AddIndexingResults(string description, int count)
        {
            this.SchedulerItem.AddLogNote($"<br/>&nbsp;&nbsp;{description}: {count}");
        }

        /// <summary>Gets all the Search Documents for the given timeframe.</summary>
        private int GetAndStoreSearchDocuments(IndexingProviderBase indexer)
        {
            var portals = PortalController.Instance.GetPortals();
            DateTime indexSince;
            var indexedCount = 0;

            foreach (var portal in portals.Cast<IPortalInfo>())
            {
                indexSince = this.FixedIndexingStartDate(portal.PortalId);
                try
                {
                    indexedCount += indexer.IndexSearchDocuments(
                        portal.PortalId, this.SchedulerItem, indexSince, StoreSearchDocuments);
                }
                catch (NotImplementedException exc)
                {
                    Logger.Warn("Indexer not implemented", exc);
                }
            }

            // Include Host Level Items
            indexSince = this.FixedIndexingStartDate(-1);
            try
            {
                indexedCount += indexer.IndexSearchDocuments(
                    Null.NullInteger, this.SchedulerItem, indexSince, StoreSearchDocuments);
            }
            catch (NotImplementedException exc)
            {
                Logger.Warn("Indexer not implemented", exc);
            }

            return indexedCount;
        }

        /// <summary>Gets all the Searchable Module MetaData SearchDocuments within the timeframe for all portals.</summary>
        private int GetAndStoreModuleMetaData(ModuleIndexer indexer)
        {
            IEnumerable<SearchDocument> searchDocs;
            var portals = PortalController.Instance.GetPortals();
            DateTime indexSince;
            var indexedCount = 0;

            // DateTime startDate
            foreach (var portal in portals.Cast<IPortalInfo>())
            {
                indexSince = this.FixedIndexingStartDate(portal.PortalId);
                searchDocs = indexer.GetModuleMetaData(portal.PortalId, indexSince);
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

        /// <summary>Adjusts the re-index date/time to account for the portal reindex value.</summary>
        private DateTime FixedIndexingStartDate(int portalId)
        {
            var startDate = this.IndexingStartTime;
            if (startDate < SqlDateTime.MinValue.Value || SearchHelper.Instance.IsReindexRequested(portalId, startDate))
            {
                return SqlDateTime.MinValue.Value.AddDays(1);
            }

            return startDate;
        }
    }
}
