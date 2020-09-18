// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;

    /// <summary>A base class for search indexers.</summary>
    [Obsolete("Legacy Indexing base class -- Deprecated in DNN 7.1. Use 'IndexingProviderBase' instead.. Scheduled removal in v10.0.0.")]
    public abstract class IndexingProvider
    {
        private const string TimePostfix = "UtcTime";
        private const string DataPostfix = "Data";

        /// <summary>This method must save search documents in batches to minimize memory usage instead of returning all documents at once.</summary>
        /// <param name="portalId">ID of the portal for which to index items.</param>
        /// <param name="startDateLocal">Minimum modification date of items that need to be indexed.</param>
        /// <param name="indexer">A delegate function to send the collection of documents to for saving/indexing.</param>
        /// <returns>The number of documents indexed.</returns>
        public virtual int IndexSearchDocuments(
            int portalId,
            ScheduleHistoryItem schedule, DateTime startDateLocal, Action<IEnumerable<SearchDocument>> indexer)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Deprecated in DNN 7.4.2 Use 'IndexSearchDocuments' instead for lower memory footprint during search.. Scheduled removal in v10.0.0.")]
        public virtual IEnumerable<SearchDocument> GetSearchDocuments(int portalId, DateTime startDateLocal)
        {
            return Enumerable.Empty<SearchDocument>();
        }

        [Obsolete("Legacy Search (ISearchable) -- Deprecated in DNN 7.1. Use 'IndexSearchDocuments' instead.. Scheduled removal in v10.0.0.")]
        public abstract SearchItemInfoCollection GetSearchIndexItems(int portalId);

        /// <summary>Retrieves the date/time of the last item to be indexed.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="scheduleId">The schedule ID.</param>
        /// <param name="localTime">The local time passed to <see cref="IndexSearchDocuments" />.</param>
        /// <returns>Either <paramref name="localTime"/> or the stored index time, whichever is earlier.</returns>
        protected DateTime GetLocalTimeOfLastIndexedItem(int portalId, int scheduleId, DateTime localTime)
        {
            var lastTime = SearchHelper.Instance.GetIndexerCheckpointUtcTime(
                scheduleId, this.ScheduleItemSettingKey(portalId, TimePostfix)).ToLocalTime();
            return lastTime < localTime ? lastTime : localTime;
        }

        /// <summary>Stores the date/time of the last item to be indexed.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="scheduleId">The schedule ID.</param>
        /// <param name="localTime">The local time to store.</param>
        protected void SetLocalTimeOfLastIndexedItem(int portalId, int scheduleId, DateTime localTime)
        {
            SearchHelper.Instance.SetIndexerCheckpointUtcTime(
                scheduleId, this.ScheduleItemSettingKey(portalId, TimePostfix), localTime.ToUniversalTime());
        }

        /// <summary>Retrieves free format data to help the indexer to perform its job.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="scheduleId">The schedule ID.</param>
        /// <returns>The checkpoint data.</returns>
        protected string GetLastCheckpointData(int portalId, int scheduleId)
        {
            return SearchHelper.Instance.GetIndexerCheckpointData(scheduleId, this.ScheduleItemSettingKey(portalId, DataPostfix));
        }

        /// <summary>Stores free format data to help the indexer to perform its job.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="scheduleId">The schedule ID.</param>
        /// <param name="data">The data to store.</param>
        protected void SetLastCheckpointData(int portalId, int scheduleId, string data)
        {
            SearchHelper.Instance.SetIndexerCheckpointData(scheduleId, this.ScheduleItemSettingKey(portalId, DataPostfix), data);
        }

        /// <summary>
        /// Creates a unique name for the IndexingProvider implementation that can be used
        /// to save/retrieve scheduler item {key,name} setting pairs per portal and feature.
        /// </summary>
        /// <param name="portalId">The ID of the portal.</param>
        /// <param name="propertyId">The name of the property.</param>
        /// <remarks>
        /// Note that changing the class name in derived classes will cause this key to differ
        /// from the names stored in the database; therefore, don't change the derived class
        /// [full] names once these are deployed to market in an actual release version.
        /// <para>The format of the key is as follows:.
        /// <ol>
        /// <li>"Search" literal</li>
        /// <li>Name of the indexer class</li>
        /// <li>Hash of the full class name of the indexer class (this and the previous will keep the key short and unique)</li>
        /// <li>Portal ID the setting is related to</li>
        /// <li>An additional property identifier set by the caller (this allows more items to be saved per indexer per portal)</li>
        /// </ol>
        /// </para>
        /// </remarks>
        /// <returns>The setting key.</returns>
        private string ScheduleItemSettingKey(int portalId, string propertyId)
        {
            Requires.NotNullOrEmpty("propertyId", propertyId);
            var t = this.GetType();
            return string.Join("_", "Search", t.Name, t.FullName.GetHashCode().ToString("x8"), portalId.ToString(), propertyId);
        }
    }
}
