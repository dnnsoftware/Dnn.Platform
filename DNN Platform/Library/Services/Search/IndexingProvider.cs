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
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;

#endregion

namespace DotNetNuke.Services.Search
{
    public abstract class IndexingProvider
    {
        /// <summary>
        /// This method must save search docuents in batches to minimize memory usage instead of returning all documents ollection at once.
        /// </summary>
        /// <param name="portalId">Portal ID to index</param>
        /// <param name="startDateLocal">Minimum modification date of items that need to be indexed</param>
        /// <param name="indexer">A delegate function to send the collection of documents to for saving/indexing</param>
        /// <returns></returns>
        public virtual int IndexSearchDocuments(int portalId,
            ScheduleHistoryItem schedule, DateTime startDateLocal, Action<IEnumerable<SearchDocument>> indexer)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Depricated in DNN 7.4.2 Use 'IndexSearchDocuments' instead for lower memory footprint during search.")]
        public virtual IEnumerable<SearchDocument> GetSearchDocuments(int portalId, DateTime startDateLocal)
        {
            return Enumerable.Empty<SearchDocument>();
        }

        [Obsolete("Legacy Search (ISearchable) -- Depricated in DNN 7.1. Use 'IndexSearchDocuments' instead.")]
        public abstract SearchItemInfoCollection GetSearchIndexItems(int portalId);

        private const string TimePostfix = "UtcTime";
        private const string DataPostfix = "Data";

        protected DateTime GetLocalTimeOfLastIndexedItem(int portalId, int scheduleId, DateTime localTime)
        {
            var lastTime = SearchHelper.Instance.GetIndexerCheckpointUtcTime(
                scheduleId, ScheduleItemSettingKey(portalId, TimePostfix)).ToLocalTime();
            return lastTime < localTime ? lastTime : localTime;
        }

        protected void SetLocalTimeOfLastIndexedItem(int portalId, int scheduleId, DateTime localTime)
        {
            SearchHelper.Instance.SetIndexerCheckpointUtcTime(
                scheduleId, ScheduleItemSettingKey(portalId, TimePostfix), localTime.ToUniversalTime());
        }

        /// <summary>
        /// This can be any free format data that can help the indexer to perform its job
        /// </summary>
        protected string GetLastCheckpointData(int portalId, int scheduleId)
        {
            return SearchHelper.Instance.GetIndexerCheckpointData(scheduleId, ScheduleItemSettingKey(portalId, DataPostfix));
        }

        /// <summary>
        /// This can be any free format data that can help the indexer to perform its job
        /// </summary>
        protected void SetLastCheckpointData(int portalId, int scheduleId, string data)
        {
            SearchHelper.Instance.SetIndexerCheckpointData(scheduleId, ScheduleItemSettingKey(portalId, DataPostfix), data);
        }

        /// <summary>
        /// Creates a unique name for the IndexingProvider implementation that can be used
        /// to save/retrieve scheduler item {key,name} setting pairs per portal and feature.
        /// </summary>
        /// <remarks>
        /// Note that changing the class name in derived classes will cause this key to differ
        /// from the names stored in the database; therefore, don't change the derived class
        /// [full] names once these are deployed to market in an actual release version.
        /// <para>The format of the key is as follows:
        /// <ol>
        /// <li>"Search" literal</li>
        /// <li>Name of the indexer class</li>
        /// <li>Hash of the full class name of the indexer class (this and the previous will keep the key short and unique)</li>
        /// <li>Portal ID the setting is related to</li>
        /// <li>An additional property identifier set by the caller (this allows more items to be saved per indexer per portal)</li>
        /// </ol>
        /// </para>
        /// </remarks>
        private string ScheduleItemSettingKey(int portalId, string propertyId)
        {
            Requires.NotNullOrEmpty("propertyId", propertyId);
            var t = GetType();
            return string.Join("_", "Search", t.Name, t.FullName.GetHashCode().ToString("x8"), portalId.ToString(), propertyId);
        }
    }
}