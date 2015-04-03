#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
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
    /// <history>
    ///		[cnurse]	11/15/2004	documented
    ///     [vnguyen]   04/16/2013  updated with methods for an Updated Search
    /// </history>
    /// -----------------------------------------------------------------------------
    public class SearchEngine
    {
        #region Properties

        public int IndexedSearchDocumentCount { get; private set; }
        
        public Dictionary<string, int> Results { get; private set; }

        public int DeletedCount { get; private set; }

        #endregion

        #region internal
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Indexes content within the given time farame
        /// </summary>
        /// <param name="startDate"></param>
        /// <history>
        ///     [vnguyen]   04/17/2013  created
        /// </history>
        /// -----------------------------------------------------------------------------
        internal void IndexContent(DateTime startDate)
        {
            var tabIndexer = new TabIndexer();
            var moduleIndexer = new ModuleIndexer(true);
            var userIndexer = new UserIndexer();
            IndexedSearchDocumentCount = 0;
            Results = new Dictionary<string, int>();

            //Index TAB META-DATA
            var searchDocs = GetSearchDocuments(tabIndexer, startDate);
            var searchDocuments = searchDocs as IList<SearchDocument> ?? searchDocs.ToList();
            StoreSearchDocuments(searchDocuments);
            IndexedSearchDocumentCount += searchDocuments.Count();
            Results.Add("Tabs", searchDocuments.Count());

            //Index MODULE META-DATA from modules that inherit from ModuleSearchBase
			searchDocs = GetModuleMetaData(moduleIndexer, startDate);
            searchDocuments = searchDocs as IList<SearchDocument> ?? searchDocs.ToList();
            StoreSearchDocuments(searchDocuments);
            IndexedSearchDocumentCount += searchDocuments.Count();
            Results.Add("Modules (Metadata)", searchDocuments.Count());

            //Index MODULE CONTENT from modules that inherit from ModuleSearchBase
            searchDocs = GetSearchDocuments(moduleIndexer, startDate);
            searchDocuments = searchDocs as IList<SearchDocument> ?? searchDocs.ToList();
            StoreSearchDocuments(searchDocuments);
            IndexedSearchDocumentCount += searchDocuments.Count();
            
            #pragma warning disable 0618
            //Index all Defunct ISearchable module content
            var searchItems = GetContent(moduleIndexer);
            SearchDataStoreProvider.Instance().StoreSearchItems(searchItems);
            #pragma warning restore 0618
            IndexedSearchDocumentCount += searchItems.Count;

            //Both ModuleSearchBase and ISearchable module content count
            Results.Add("Modules (Content)", searchDocuments.Count() + searchItems.Count);

            //Index User data
            if (HostController.Instance.GetBoolean("DisableUserCrawling", false)) return;
            searchDocs = GetSearchDocuments(userIndexer, startDate);
            searchDocuments = searchDocs as IList<SearchDocument> ?? searchDocs.ToList();
            StoreSearchDocuments(searchDocuments);
            var userIndexed =
                searchDocuments.Select(
                    d => d.UniqueKey.Substring(0, d.UniqueKey.IndexOf("_", StringComparison.Ordinal)))
                    .Distinct()
                    .Count();
            IndexedSearchDocumentCount += userIndexed;
            Results.Add("Users", userIndexed);
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
        /// <param name="startDate"></param>
        internal void DeleteOldDocsBeforeReindex(DateTime startDate)
        {
            var portal2Reindex = SearchHelper.Instance.GetPortalsToReindex(startDate);
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
        /// <param name="cutoffTime">UTC time for items to tprocess that are created before this time</param>
        internal void DeleteRemovedObjects(DateTime cutoffTime)
        {
            DeletedCount = 0;
            var searchController = InternalSearchController.Instance;
            var dataProvider = DataProvider.Instance();
            var reader = dataProvider.GetSearchDeletedItems(cutoffTime);
            while (reader.Read())
            {
                // Note: we saved this in the DB as SearchDocumentToDelete but retrieve as the descendant SearchDocument class
                var document = JsonConvert.DeserializeObject<SearchDocument>(reader["document"] as string);
                searchController.DeleteSearchDocument(document);
                DeletedCount += 1;
            }

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
        /// <param name="startDateLocal"></param>
        /// <returns></returns>
        /// <history>
        ///     [vnguyen]   04/17/2013  created
        /// </history>
        /// -----------------------------------------------------------------------------
        private IEnumerable<SearchDocument> GetSearchDocuments(IndexingProvider indexer, DateTime startDateLocal)
        {
            var searchDocs = new List<SearchDocument>();
            var portals = PortalController.Instance.GetPortals();
            DateTime indexSince;

            foreach (var portal in portals.Cast<PortalInfo>())
            {
                indexSince = FixedIndexingStartDate(portal.PortalID, startDateLocal);
                searchDocs.AddRange(indexer.GetSearchDocuments(portal.PortalID, indexSince));
            }

            // Include Host Level Items
            indexSince = FixedIndexingStartDate(-1, startDateLocal);
            searchDocs.AddRange(indexer.GetSearchDocuments(-1, indexSince));
            
            return searchDocs;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets all the Searchable Module MetaData SearchDocuments within the timeframe for all portals
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        /// <history>
        ///     [vnguyen]   04/17/2013  created
        /// </history>
        /// -----------------------------------------------------------------------------
		private IEnumerable<SearchDocument> GetModuleMetaData(ModuleIndexer indexer, DateTime startDate)
        {
            var searchDocs = new List<SearchDocument>();
            var portals = PortalController.Instance.GetPortals();
            DateTime indexSince;

            foreach (var portal in portals.Cast<PortalInfo>())
            {
                indexSince = FixedIndexingStartDate(portal.PortalID, startDate);
                searchDocs.AddRange(indexer.GetModuleMetaData(portal.PortalID, indexSince));
            }

            // Include Host Level Items
			indexSince = FixedIndexingStartDate(Null.NullInteger, startDate);
            searchDocs.AddRange(indexer.GetModuleMetaData(Null.NullInteger, indexSince));

            return searchDocs;
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
        /// <param name="portalId"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        private static DateTime FixedIndexingStartDate(int portalId, DateTime startDate)
        {
            if (startDate < SqlDateTime.MinValue.Value ||
                SearchHelper.Instance.IsReindexRequested(portalId, startDate))
            {
                return SqlDateTime.MinValue.Value.AddDays(1);
            }
            return startDate;
        }

        #endregion

        #region Obsolete Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LEGACY: Depricated in DNN 7.1. Use 'GetSearchDocuments' instead.
        /// Used for Legacy Search (ISearchable) 
        /// 
        /// GetContent gets all the content and passes it to the Indexer
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="indexer">The Index Provider that will index the content of the portal</param>
        /// <history>
        ///		[cnurse]	11/15/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [Obsolete("Legacy Search (ISearchable) -- Depricated in DNN 7.1. Use 'GetSearchDocuments' instead.")]
        protected SearchItemInfoCollection GetContent(IndexingProvider indexer)
        {
            var searchItems = new SearchItemInfoCollection();
            var portals = PortalController.Instance.GetPortals();
            for (var index = 0; index <= portals.Count - 1; index++)
            {
                var portal = (PortalInfo) portals[index];
                searchItems.AddRange(indexer.GetSearchIndexItems(portal.PortalID));
            }
            return searchItems;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LEGACY: Depricated in DNN 7.1. Use 'GetSearchDocuments' instead.
        /// Used for Legacy Search (ISearchable) 
        /// 
        /// GetContent gets the Portal's content and passes it to the Indexer
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="indexer">The Index Provider that will index the content of the portal</param>
        /// <history>
        ///		[cnurse]	11/15/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [Obsolete("Legacy Search (ISearchable) -- Depricated in DNN 7.1. Use 'GetSearchDocuments' instead.")]
        protected SearchItemInfoCollection GetContent(int portalId, IndexingProvider indexer)
        {
            var searchItems = new SearchItemInfoCollection();
            searchItems.AddRange(indexer.GetSearchIndexItems(portalId));
            return searchItems;
        }

        #endregion
        
    }
}
