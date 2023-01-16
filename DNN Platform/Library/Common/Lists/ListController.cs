﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Lists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Threading;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Provides access to Dnn Lists.</summary>
    public class ListController
    {
        /// <summary>The list of list types that are not localized.</summary>
        [Obsolete("Deprecated in v9.8.1, use UnLocalizedLists instead, schedule removal in v11.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.MaintainabilityRules",
            "SA1401:Fields should be private",
            Justification = "Make private in v11.")]
        public readonly string[] NonLocalizedLists = UnLocalizableLists;

        private static readonly string[] UnLocalizableLists = { "ContentTypes", "Processor", "DataType", "ProfanityFilter", "BannedPasswords" };
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="ListController"/> class.</summary>
        public ListController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ListController"/> class.</summary>
        /// <param name="eventLogger">An event logger.</param>
        public ListController(IEventLogger eventLogger)
        {
            this.eventLogger = eventLogger ?? Globals.DependencyProvider.GetRequiredService<IEventLogger>();
        }

        /// <summary>Gets the lists that do not support localization.</summary>
        public IReadOnlyCollection<string> UnLocalizedLists => UnLocalizableLists;

        /// <summary>
        /// Adds a new list entry to the database. If the current thread locale is not "en-US" then the text value will also be
        /// persisted to a resource file under App_GlobalResources using the list's name and the value as key.
        /// </summary>
        /// <param name="listEntry">The list entry.</param>
        /// <returns>The entry id.</returns>
        public int AddListEntry(ListEntryInfo listEntry)
        {
            bool enableSortOrder = listEntry.SortOrder > 0;
            this.ClearListCache(listEntry.PortalID);
            int entryId = DataProvider.Instance().AddListEntry(
                listEntry.ListName,
                listEntry.Value,
                listEntry.TextNonLocalized,
                listEntry.ParentID,
                listEntry.Level,
                enableSortOrder,
                listEntry.DefinitionID,
                listEntry.Description,
                listEntry.PortalID,
                listEntry.SystemList,
                UserController.Instance.GetCurrentUserInfo().UserID);

            if (entryId != Null.NullInteger)
            {
                this.eventLogger.AddLog(
                    listEntry,
                    PortalController.Instance.GetCurrentSettings(),
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    string.Empty,
                    EventLogType.LISTENTRY_CREATED);
            }

            if (Thread.CurrentThread.CurrentCulture.Name != Localization.SystemLocale && !UnLocalizableLists.Contains(listEntry.ListName))
            {
                if (string.IsNullOrEmpty(listEntry.ParentKey))
                {
                    LocalizationProvider.Instance.SaveString(
                        listEntry.Value + ".Text",
                        listEntry.TextNonLocalized,
                        listEntry.ResourceFileRoot,
                        Thread.CurrentThread.CurrentCulture.Name,
                        (PortalSettings)PortalController.Instance.GetCurrentSettings(),
                        LocalizationProvider.CustomizedLocale.None,
                        true,
                        true);
                }
                else
                {
                    LocalizationProvider.Instance.SaveString(
                        listEntry.ParentKey + "." + listEntry.Value + ".Text",
                        listEntry.TextNonLocalized,
                        listEntry.ResourceFileRoot,
                        Thread.CurrentThread.CurrentCulture.Name,
                        (PortalSettings)PortalController.Instance.GetCurrentSettings(),
                        LocalizationProvider.CustomizedLocale.None,
                        true,
                        true);
                }
            }

            this.ClearEntriesCache(listEntry.ListName, listEntry.PortalID);
            return entryId;
        }

        /// <summary>Deletes a list.</summary>
        /// <param name="listName">The name of the list to delete.</param>
        /// <param name="parentKey">The parent key for the list to delete.</param>
        public void DeleteList(string listName, string parentKey)
        {
            this.DeleteList(listName, parentKey, Null.NullInteger);
        }

        /// <summary>Deletes a list.</summary>
        /// <param name="listName">The name of the list to delete.</param>
        /// <param name="parentKey">The parent key of the list to delete.</param>
        /// <param name="portalId">The id of the site (portal) on which to delete the list.</param>
        public void DeleteList(string listName, string parentKey, int portalId)
        {
            ListInfo list = this.GetListInfo(listName, parentKey, portalId);

            this.eventLogger.AddLog(
                "ListName",
                listName,
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.LISTENTRY_DELETED);

            DataProvider.Instance().DeleteList(listName, parentKey);
            if (list != null)
            {
                this.ClearListCache(list.PortalID);
                this.ClearEntriesCache(list.Name, list.PortalID);
            }
        }

        /// <summary>Deletes a list.</summary>
        /// <param name="list">The <see cref="ListInfo"/> reference for the list to delete.</param>
        /// <param name="includeChildren">A value indicating wheter to also delete the children items for this list.</param>
        public void DeleteList(ListInfo list, bool includeChildren)
        {
            if (list == null)
            {
                return;
            }

            var lists = new SortedList<string, ListInfo>();
            lists.Add(list.Key, list);

            // add Children
            if (includeChildren)
            {
                foreach (KeyValuePair<string, ListInfo> listPair in this.GetListInfoDictionary(list.PortalID))
                {
                    if (listPair.Value.ParentList.StartsWith(list.Key))
                    {
                        lists.Add(listPair.Value.Key.Replace(":", "."), listPair.Value);
                    }
                }
            }

            // Delete items in reverse order so deeper descendants are removed before their parents
            for (int i = lists.Count - 1; i >= 0; i += -1)
            {
                this.DeleteList(lists.Values[i].Name, lists.Values[i].ParentKey, lists.Values[i].PortalID);
            }
        }

        /// <summary>Deletes a list entry.</summary>
        /// <param name="entryId">the id of the entry to delete.</param>
        /// <param name="deleteChild">A value indicating whether to also delete the children of that item.</param>
        public void DeleteListEntryByID(int entryId, bool deleteChild)
        {
            ListEntryInfo entry = this.GetListEntryInfo(entryId);
            DataProvider.Instance().DeleteListEntryByID(entryId, deleteChild);
            this.ClearListCache(entry.PortalID);
            this.ClearEntriesCache(entry.ListName, entry.PortalID);
        }

        /// <summary>Deletes a list entry by its name.</summary>
        /// <param name="listName">The name of the list entry.</param>
        /// <param name="listValue">The value of the list entry.</param>
        /// <param name="deleteChild">A value indicating whether to also delete the children of that item.</param>
        public void DeleteListEntryByListName(string listName, string listValue, bool deleteChild)
        {
            ListEntryInfo entry = this.GetListEntryInfo(listName, listValue);
            DataProvider.Instance().DeleteListEntryByListName(listName, listValue, deleteChild);
            this.ClearListCache(entry.PortalID);
            this.ClearEntriesCache(listName, entry.PortalID);
        }

        /// <summary>Gets a list entry information.</summary>
        /// <param name="entryId">The id of the list entry.</param>
        /// <returns><see cref="ListEntryInfo"/>.</returns>
        public ListEntryInfo GetListEntryInfo(int entryId)
        {
            return CBO.FillObject<ListEntryInfo>(DataProvider.Instance().GetListEntry(entryId));
        }

        /// <summary>Gets a list entry information.</summary>
        /// <param name="listName">The name of the list.</param>
        /// <param name="entryId">The id of the list entry.</param>
        /// <returns><see cref="ListEntryInfo"/>.</returns>
        public ListEntryInfo GetListEntryInfo(string listName, int entryId)
        {
            return this.GetListEntries(listName, Null.NullInteger).SingleOrDefault(l => l.EntryID == entryId);
        }

        /// <summary>Gets a list entry information.</summary>
        /// <param name="listName">The name of the list.</param>
        /// <param name="listValue">The value of the list entry.</param>
        /// <returns><see cref="ListEntryInfo"/>.</returns>
        public ListEntryInfo GetListEntryInfo(string listName, string listValue)
        {
            return this.GetListEntries(listName, Null.NullInteger).SingleOrDefault(l => l.Value == listValue);
        }

        /// <summary>Gets the entries in the list with the given <paramref name="listName"/>.</summary>
        /// <param name="listName">The name of the list.</param>
        /// <returns>An enumeration of list entries.</returns>
        public IEnumerable<ListEntryInfo> GetListEntryInfoItems(string listName)
        {
            return this.GetListEntries(listName, Null.NullInteger);
        }

        /// <summary>Gets the entries in a child list.</summary>
        /// <param name="listName">The list name.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <returns>An enumeration of list entries.</returns>
        public IEnumerable<ListEntryInfo> GetListEntryInfoItems(string listName, string parentKey)
        {
            return this.GetListEntries(listName, Null.NullInteger).Where(l => l.ParentKey == parentKey);
        }

        /// <summary>Gets the entries in a child list.</summary>
        /// <param name="listName">The list name.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <param name="portalId">The id of the site (portal) from which to get the list from.</param>
        /// <returns>An enumeration of list entries.</returns>
        public IEnumerable<ListEntryInfo> GetListEntryInfoItems(string listName, string parentKey, int portalId)
        {
            return this.GetListEntries(listName, portalId).Where(l => l.ParentKey == parentKey);
        }

        /// <summary>Gets all list entries for a given list name.</summary>
        /// <param name="listName">The name of the list to get.</param>
        /// <returns>A dictionary where the index is <see cref="ListEntryInfo.Key"/> and the value is the actual <see cref="ListEntryInfo"/>.</returns>
        public Dictionary<string, ListEntryInfo> GetListEntryInfoDictionary(string listName)
        {
            return this.GetListEntryInfoDictionary(listName, string.Empty, Null.NullInteger);
        }

        /// <summary>Gets all list entries for a given list name.</summary>
        /// <param name="listName">The name of the list to get.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <returns>A dictionary where the index is <see cref="ListEntryInfo.Key"/> and the value is the actual <see cref="ListEntryInfo"/>.</returns>
        public Dictionary<string, ListEntryInfo> GetListEntryInfoDictionary(string listName, string parentKey)
        {
            return this.GetListEntryInfoDictionary(listName, parentKey, Null.NullInteger);
        }

        /// <summary>Gets all list entries for a given list name.</summary>
        /// <param name="listName">The name of the list to get.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <param name="portalId">The id of the site (portal) from which to get the list entries from.</param>
        /// <returns>A dictionary where the index is <see cref="ListEntryInfo.Key"/> and the value is the actual <see cref="ListEntryInfo"/>.</returns>
        public Dictionary<string, ListEntryInfo> GetListEntryInfoDictionary(string listName, string parentKey, int portalId)
        {
            return ListEntryInfoItemsToDictionary(this.GetListEntryInfoItems(listName, parentKey, portalId));
        }

        /// <summary>Gets a single list.</summary>
        /// <param name="listName">The name of the list.</param>
        /// <returns><see cref="ListInfo"/>.</returns>
        public ListInfo GetListInfo(string listName)
        {
            return this.GetListInfo(listName, string.Empty);
        }

        /// <summary>Gets a single list.</summary>
        /// <param name="listName">The name of the list.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <returns><see cref="ListInfo"/>.</returns>
        public ListInfo GetListInfo(string listName, string parentKey)
        {
            return this.GetListInfo(listName, parentKey, -1);
        }

        /// <summary>Gets a single list.</summary>
        /// <param name="listName">The name of the list.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <param name="portalId">The id of the site (portal) to get the list from.</param>
        /// <returns><see cref="ListInfo"/>.</returns>
        public ListInfo GetListInfo(string listName, string parentKey, int portalId)
        {
            ListInfo list = null;
            string key = Null.NullString;
            if (!string.IsNullOrEmpty(parentKey))
            {
                key = parentKey + ":";
            }

            key += listName;
            Dictionary<string, ListInfo> dicLists = this.GetListInfoDictionary(portalId);
            if (!dicLists.TryGetValue(key, out list))
            {
                IDataReader dr = DataProvider.Instance().GetList(listName, parentKey, portalId);
                try
                {
                    list = this.FillListInfo(dr, true);
                }
                finally
                {
                    CBO.CloseDataReader(dr, true);
                }
            }

            return list;
        }

        /// <summary>Gets a collection of lists.</summary>
        /// <returns><see cref="ListInfoCollection"/>.</returns>
        public ListInfoCollection GetListInfoCollection()
        {
            return this.GetListInfoCollection(string.Empty);
        }

        /// <summary>Gets a collection of lists.</summary>
        /// <param name="listName">The list name.</param>
        /// <returns><see cref="ListInfoCollection"/>.</returns>
        public ListInfoCollection GetListInfoCollection(string listName)
        {
            return this.GetListInfoCollection(listName, string.Empty);
        }

        /// <summary>Gets a collection of lists.</summary>
        /// <param name="listName">The list name.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <returns><see cref="ListInfoCollection"/>.</returns>
        public ListInfoCollection GetListInfoCollection(string listName, string parentKey)
        {
            return this.GetListInfoCollection(listName, parentKey, -1);
        }

        /// <summary>Gets a collection of lists.</summary>
        /// <param name="listName">The list name.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <param name="portalId">The id of the site (portal) to get the list from.</param>
        /// <returns><see cref="ListInfoCollection"/>.</returns>
        public ListInfoCollection GetListInfoCollection(string listName, string parentKey, int portalId)
        {
            IList lists = new ListInfoCollection();
            foreach (KeyValuePair<string, ListInfo> listPair in this.GetListInfoDictionary(portalId).OrderBy(l => l.Value.DisplayName))
            {
                ListInfo list = listPair.Value;
                if ((list.Name == listName || string.IsNullOrEmpty(listName)) && (list.ParentKey == parentKey || string.IsNullOrEmpty(parentKey)) &&
                    (list.PortalID == portalId || portalId == Null.NullInteger))
                {
                    lists.Add(list);
                }
            }

            return (ListInfoCollection)lists;
        }

        /// <summary>
        /// Updates the list entry in the database using the values set on the listEntry. Note that if the current thread locale is not "en-US" then the
        /// text value will be persisted to a resource file under App_GlobalResources using the list's name and the value as key. Also the supplied text value
        /// will *not* be written to the database in this case (i.e. we expect the text value in the database to be the en-US text value).
        /// </summary>
        /// <param name="listEntry">The list entry info item to update.</param>
        public void UpdateListEntry(ListEntryInfo listEntry)
        {
            if (Thread.CurrentThread.CurrentCulture.Name == Localization.SystemLocale || UnLocalizableLists.Contains(listEntry.ListName))
            {
                DataProvider.Instance().UpdateListEntry(listEntry.EntryID, listEntry.Value, listEntry.TextNonLocalized, listEntry.Description, UserController.Instance.GetCurrentUserInfo().UserID);
            }
            else
            {
                var oldItem = this.GetListEntryInfo(listEntry.EntryID); // look up existing db record to be able to just update the value or description and not touch the en-US text value
                DataProvider.Instance().UpdateListEntry(listEntry.EntryID, listEntry.Value, oldItem.TextNonLocalized, listEntry.Description, UserController.Instance.GetCurrentUserInfo().UserID);

                var key = string.IsNullOrEmpty(listEntry.ParentKey)
                    ? listEntry.Value + ".Text"
                    : listEntry.ParentKey + "." + listEntry.Value + ".Text";

                LocalizationProvider.Instance.SaveString(
                    key,
                    listEntry.TextNonLocalized,
                    listEntry.ResourceFileRoot,
                    Thread.CurrentThread.CurrentCulture.Name,
                    (PortalSettings)PortalController.Instance.GetCurrentSettings(),
                    LocalizationProvider.CustomizedLocale.None,
                    true,
                    true);
            }

            this.eventLogger.AddLog(
                listEntry,
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                string.Empty,
                EventLogType.LISTENTRY_UPDATED);

            this.ClearListCache(listEntry.PortalID);
            this.ClearEntriesCache(listEntry.ListName, listEntry.PortalID);
        }

        /// <summary>Updates a list sort order.</summary>
        /// <param name="entryID">The id of the entry to move.</param>
        /// <param name="moveUp">The entry is moved up if <c>true</c> or moved down if <c>false</c>.</param>
        public void UpdateListSortOrder(int entryID, bool moveUp)
        {
            DataProvider.Instance().UpdateListSortOrder(entryID, moveUp);
            ListEntryInfo entry = this.GetListEntryInfo(entryID);
            this.ClearListCache(entry.PortalID);
            this.ClearEntriesCache(entry.ListName, entry.PortalID);
        }

        /// <summary>Gets a collection of list entries.</summary>
        /// <param name="listName">The name of the list to get.</param>
        /// <returns>A collection of list entries.</returns>
        [Obsolete("Obsoleted in 6.0.1 use IEnumerable<ListEntryInfo> GetListEntryInfoXXX(string) instead. Scheduled removal in v10.0.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ListEntryInfoCollection GetListEntryInfoCollection(string listName)
        {
            return this.GetListEntryInfoCollection(listName, string.Empty, Null.NullInteger);
        }

        /// <summary>Gets a collection of list entries.</summary>
        /// <param name="listName">The name of the list to get.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <returns>A collection of list entries.</returns>
        [Obsolete("Obsoleted in 6.0.1 use IEnumerable<ListEntryInfo> GetListEntryInfoXXX(string, string, int) instead. Scheduled removal in v10.0.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ListEntryInfoCollection GetListEntryInfoCollection(string listName, string parentKey)
        {
            return this.GetListEntryInfoCollection(listName, parentKey, Null.NullInteger);
        }

        /// <summary>Gets a collection of list entries.</summary>
        /// <param name="listName">The name of the list to get.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <param name="portalId">The id of the site (portal) to get the list from.</param>
        /// <returns>A collection of list entries.</returns>
        [Obsolete("Obsoleted in 6.0.1 use IEnumerable<ListEntryInfo> GetListEntryInfoXXX(string, string, int) instead. Scheduled removal in v10.0.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ListEntryInfoCollection GetListEntryInfoCollection(string listName, string parentKey, int portalId)
        {
            var items = this.GetListEntryInfoItems(listName, parentKey, portalId);

            var collection = new ListEntryInfoCollection();
            if (items != null)
            {
                items.ToList().ForEach(x => collection.Add(x.Key, x));
            }

            return collection;
        }

        private static Dictionary<string, ListEntryInfo> ListEntryInfoItemsToDictionary(IEnumerable<ListEntryInfo> items)
        {
            var dict = new Dictionary<string, ListEntryInfo>();
            items.ToList().ForEach(x => dict.Add(x.Key, x));

            return dict;
        }

        private void ClearListCache(int portalId)
        {
            DataCache.ClearListsCache(portalId);
        }

        private void ClearEntriesCache(string listName, int portalId)
        {
            string cacheKey = string.Format(DataCache.ListEntriesCacheKey, portalId, listName);
            DataCache.RemoveCache(cacheKey);
        }

        private ListInfo FillListInfo(IDataReader dr, bool checkForOpenDataReader)
        {
            ListInfo list = null;

            // read datareader
            bool canContinue = true;
            if (checkForOpenDataReader)
            {
                canContinue = false;
                if (dr.Read())
                {
                    canContinue = true;
                }
            }

            if (canContinue)
            {
                list = new ListInfo(Convert.ToString(dr["ListName"]));
                {
                    list.Level = Convert.ToInt32(dr["Level"]);
                    list.PortalID = Convert.ToInt32(dr["PortalID"]);
                    list.DefinitionID = Convert.ToInt32(dr["DefinitionID"]);
                    list.EntryCount = Convert.ToInt32(dr["EntryCount"]);
                    list.ParentID = Convert.ToInt32(dr["ParentID"]);
                    list.ParentKey = Convert.ToString(dr["ParentKey"]);
                    list.Parent = Convert.ToString(dr["Parent"]);
                    list.ParentList = Convert.ToString(dr["ParentList"]);
                    list.EnableSortOrder = Convert.ToInt32(dr["MaxSortOrder"]) > 0;
                    list.SystemList = Convert.ToInt32(dr["SystemList"]) > 0;
                }
            }

            return list;
        }

        private Dictionary<string, ListInfo> FillListInfoDictionary(IDataReader dr)
        {
            var dic = new Dictionary<string, ListInfo>();
            try
            {
                while (dr.Read())
                {
                    // fill business object
                    ListInfo list = this.FillListInfo(dr, false);
                    if (!dic.ContainsKey(list.Key))
                    {
                        dic.Add(list.Key, list);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                CBO.CloseDataReader(dr, true);
            }

            return dic;
        }

        private Dictionary<string, ListInfo> GetListInfoDictionary(int portalId)
        {
            string cacheKey = string.Format(DataCache.ListsCacheKey, portalId);
            return CBO.GetCachedObject<Dictionary<string, ListInfo>>(
                new CacheItemArgs(
                cacheKey,
                DataCache.ListsCacheTimeOut,
                DataCache.ListsCachePriority),
                c => this.FillListInfoDictionary(DataProvider.Instance().GetLists(portalId)));
        }

        private IEnumerable<ListEntryInfo> GetListEntries(string listName, int portalId)
        {
            string cacheKey = string.Format(DataCache.ListEntriesCacheKey, portalId, listName);
            return CBO.GetCachedObject<IEnumerable<ListEntryInfo>>(
                new CacheItemArgs(
                cacheKey,
                DataCache.ListsCacheTimeOut,
                DataCache.ListsCachePriority),
                c => CBO.FillCollection<ListEntryInfo>(DataProvider.Instance().GetListEntriesByListName(listName, string.Empty, portalId)));
        }
    }
}
