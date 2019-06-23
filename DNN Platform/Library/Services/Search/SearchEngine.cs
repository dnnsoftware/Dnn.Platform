#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Data.SqlTypes;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Services.Scheduling;
using Newtonsoft.Json;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke
    /// Class:      SearchEngine
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchEngine  manages the Indexing of the Portal content
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    internal class SearchEngine
    {
        internal SearchEngine(ScheduleHistoryItem scheduler, DateTime startTime)
        {
            SchedulerItem = scheduler;
            IndexingStartTime = startTime;
        }

        #region Properties

        public ScheduleHistoryItem SchedulerItem { get; private set; }

        // the time from where to start indexing items
        public DateTime IndexingStartTime { get; private set; }

        #endregion

        #region internal
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Indexes content within the given time farame
        /// </summary>
        /// -----------------------------------------------------------------------------
        internal void IndexContent()
        {
            //Index TAB META-DATA
            var tabIndexer = new TabIndexer();
            var searchDocsCount = GetAndStoreSearchDocuments(tabIndexer);
            var indexedSearchDocumentCount = searchDocsCount;
            AddIdexingResults("Tabs Indexed", searchDocsCount);

            //Index MODULE META-DATA from modules that inherit from ModuleSearchBase
            var moduleIndexer = new ModuleIndexer(true);
            searchDocsCount = GetAndStoreModuleMetaData(moduleIndexer);
            indexedSearchDocumentCount += searchDocsCount;
            AddIdexingResults("Modules (Metadata) Indexed", searchDocsCount);

            //Index MODULE CONTENT from modules that inherit from ModuleSearchBase
            searchDocsCount = GetAndStoreSearchDocuments(moduleIndexer);
            indexedSearchDocumentCount += searchDocsCount;
            
            //Both ModuleSearchBase and ISearchable module content count
            AddIdexingResults("Modules (Content) Indexed", searchDocsCount);

            if (!HostController.Instance.GetBoolean("DisableUserCrawling", false))
            {
                //Index User data
                var userIndexer = new UserIndexer();
                var userIndexed = GetAndStoreSearchDocuments(userIndexer);
                indexedSearchDocumentCount += userIndexed;
                AddIdexingResults("Users", userIndexed);
            }

            SchedulerItem.AddLogNote("<br/><b>Total Items Indexed: " + indexedSearchDocumentCount + "</b>");
        }

        private void AddIdexingResults(string description, int count)
        {
            SchedulerItem.AddLogNote(string.Format("<br/>&nbsp;&nbsp;{0}: {1}", description, count));
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
            var portal2Reindex = SearchHelper.Instance.GetPortalsToReindex(IndexingStartTime);
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
            var cutoffTime = SchedulerItem.StartDate.ToUniversalTime();
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
            AddIdexingResults("Deleted Objects", deletedCount);
            dataProvider.DeleteProcessedSearchDeletedItems(cutoffTime);
        }

        /// <summary>
        /// Commits (flushes) all added and deleted content to search engine's disk file
        /// </summary>
        internal void Commit()
        {
            InternalSearchController.Instance.Commit();
        }
        #endregion

        #region Private

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets all the Search Documents for the given timeframe.
        /// </summary>
        /// <param name="indexer"></param>
        /// -----------------------------------------------------------------------------
        private int GetAndStoreSearchDocuments(IndexingProvider indexer)
        {
            var portals = PortalController.Instance.GetPortals();
            DateTime indexSince;
            var indexedCount = 0;

            foreach (var portal in portals.Cast<PortalInfo>())
            {
                indexSince = FixedIndexingStartDate(portal.PortalID);
                try
                {
                    indexedCount += indexer.IndexSearchDocuments(
                        portal.PortalID, SchedulerItem, indexSince, StoreSearchDocuments);
                }
                catch (NotImplementedException)
                {
                    //Nothing to do, no fallback
                }
            }

            // Include Host Level Items
            indexSince = FixedIndexingStartDate(-1);
            try
            {
                indexedCount += indexer.IndexSearchDocuments(
                    Null.NullInteger, SchedulerItem, indexSince, StoreSearchDocuments);
            }
            catch (NotImplementedException)
            {
                //Again no fallback
            }
            return indexedCount;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets all the Searchable Module MetaData SearchDocuments within the timeframe for all portals
        /// </summary>
        /// -----------------------------------------------------------------------------
        private int GetAndStoreModuleMetaData(ModuleIndexer indexer)
        {
            IEnumerable<SearchDocument> searchDocs;
            var portals = PortalController.Instance.GetPortals();
            DateTime indexSince;
            var indexedCount = 0;
            //DateTime startDate

            foreach (var portal in portals.Cast<PortalInfo>())
            {
                indexSince = FixedIndexingStartDate(portal.PortalID);
                searchDocs = indexer.GetModuleMetaData(portal.PortalID, indexSince);
                StoreSearchDocuments(searchDocs);
                indexedCount += searchDocs.Count();
            }

            // Include Host Level Items
            indexSince = FixedIndexingStartDate(Null.NullInteger);
            searchDocs = indexer.GetModuleMetaData(Null.NullInteger, indexSince);
            StoreSearchDocuments(searchDocs);
            indexedCount += searchDocs.Count();

            return indexedCount;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Ensures all SearchDocuments have a SearchTypeId
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adjusts the re-index date/time to account for the portal reindex value
        /// </summary>
        /// -----------------------------------------------------------------------------
        private DateTime FixedIndexingStartDate(int portalId)
        {
            var startDate = IndexingStartTime;
            if (startDate < SqlDateTime.MinValue.Value ||
                SearchHelper.Instance.IsReindexRequested(portalId, startDate))
            {
                return SqlDateTime.MinValue.Value.AddDays(1);
            }
            return startDate;
        }

        #endregion
        
    }
}
