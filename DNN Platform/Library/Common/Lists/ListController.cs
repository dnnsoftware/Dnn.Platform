// Licensed to the .NET Foundation under one or more agreements.
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

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    public class ListController
    {
        public readonly string[] NonLocalizedLists = { "ContentTypes", "Processor", "DataType", "ProfanityFilter", "BannedPasswords" };

        /// <summary>
        /// Adds a new list entry to the database. If the current thread locale is not "en-US" then the text value will also be
        /// persisted to a resource file under App_GlobalResources using the list's name and the value as key.
        /// </summary>
        /// <param name="listEntry">The list entry.</param>
        /// <returns></returns>
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
                EventLogController.Instance.AddLog(listEntry, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.LISTENTRY_CREATED);
            }

            if (Thread.CurrentThread.CurrentCulture.Name != Localization.SystemLocale && !this.NonLocalizedLists.Contains(listEntry.ListName))
            {
                if (string.IsNullOrEmpty(listEntry.ParentKey))
                {
                    LocalizationProvider.Instance.SaveString(listEntry.Value + ".Text", listEntry.TextNonLocalized, listEntry.ResourceFileRoot, Thread.CurrentThread.CurrentCulture.Name, PortalController.Instance.GetCurrentPortalSettings(), LocalizationProvider.CustomizedLocale.None, true, true);
                }
                else
                {
                    LocalizationProvider.Instance.SaveString(listEntry.ParentKey + "." + listEntry.Value + ".Text", listEntry.TextNonLocalized, listEntry.ResourceFileRoot, Thread.CurrentThread.CurrentCulture.Name, PortalController.Instance.GetCurrentPortalSettings(), LocalizationProvider.CustomizedLocale.None, true, true);
                }
            }

            this.ClearEntriesCache(listEntry.ListName, listEntry.PortalID);
            return entryId;
        }

        public void DeleteList(string listName, string parentKey)
        {
            this.DeleteList(listName, parentKey, Null.NullInteger);
        }

        public void DeleteList(string listName, string parentKey, int portalId)
        {
            ListInfo list = this.GetListInfo(listName, parentKey, portalId);
            EventLogController.Instance.AddLog("ListName", listName, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.LISTENTRY_DELETED);
            DataProvider.Instance().DeleteList(listName, parentKey);
            if (list != null)
            {
                this.ClearListCache(list.PortalID);
                this.ClearEntriesCache(list.Name, list.PortalID);
            }
        }

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

        public void DeleteListEntryByID(int entryId, bool deleteChild)
        {
            ListEntryInfo entry = this.GetListEntryInfo(entryId);
            DataProvider.Instance().DeleteListEntryByID(entryId, deleteChild);
            this.ClearListCache(entry.PortalID);
            this.ClearEntriesCache(entry.ListName, entry.PortalID);
        }

        public void DeleteListEntryByListName(string listName, string listValue, bool deleteChild)
        {
            ListEntryInfo entry = this.GetListEntryInfo(listName, listValue);
            DataProvider.Instance().DeleteListEntryByListName(listName, listValue, deleteChild);
            this.ClearListCache(entry.PortalID);
            this.ClearEntriesCache(listName, entry.PortalID);
        }

        public ListEntryInfo GetListEntryInfo(int entryId)
        {
            return CBO.FillObject<ListEntryInfo>(DataProvider.Instance().GetListEntry(entryId));
        }

        public ListEntryInfo GetListEntryInfo(string listName, int entryId)
        {
            return this.GetListEntries(listName, Null.NullInteger).SingleOrDefault(l => l.EntryID == entryId);
        }

        public ListEntryInfo GetListEntryInfo(string listName, string listValue)
        {
            return this.GetListEntries(listName, Null.NullInteger).SingleOrDefault(l => l.Value == listValue);
        }

        public IEnumerable<ListEntryInfo> GetListEntryInfoItems(string listName)
        {
            return this.GetListEntries(listName, Null.NullInteger);
        }

        public IEnumerable<ListEntryInfo> GetListEntryInfoItems(string listName, string parentKey)
        {
            return this.GetListEntries(listName, Null.NullInteger).Where(l => l.ParentKey == parentKey);
        }

        public IEnumerable<ListEntryInfo> GetListEntryInfoItems(string listName, string parentKey, int portalId)
        {
            return this.GetListEntries(listName, portalId).Where(l => l.ParentKey == parentKey);
        }

        public Dictionary<string, ListEntryInfo> GetListEntryInfoDictionary(string listName)
        {
            return this.GetListEntryInfoDictionary(listName, string.Empty, Null.NullInteger);
        }

        public Dictionary<string, ListEntryInfo> GetListEntryInfoDictionary(string listName, string parentKey)
        {
            return this.GetListEntryInfoDictionary(listName, parentKey, Null.NullInteger);
        }

        public Dictionary<string, ListEntryInfo> GetListEntryInfoDictionary(string listName, string parentKey, int portalId)
        {
            return ListEntryInfoItemsToDictionary(this.GetListEntryInfoItems(listName, parentKey, portalId));
        }

        public ListInfo GetListInfo(string listName)
        {
            return this.GetListInfo(listName, string.Empty);
        }

        public ListInfo GetListInfo(string listName, string parentKey)
        {
            return this.GetListInfo(listName, parentKey, -1);
        }

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

        public ListInfoCollection GetListInfoCollection()
        {
            return this.GetListInfoCollection(string.Empty);
        }

        public ListInfoCollection GetListInfoCollection(string listName)
        {
            return this.GetListInfoCollection(listName, string.Empty);
        }

        public ListInfoCollection GetListInfoCollection(string listName, string parentKey)
        {
            return this.GetListInfoCollection(listName, parentKey, -1);
        }

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
            if (Thread.CurrentThread.CurrentCulture.Name == Localization.SystemLocale || this.NonLocalizedLists.Contains(listEntry.ListName))
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

                LocalizationProvider.Instance.SaveString(key, listEntry.TextNonLocalized, listEntry.ResourceFileRoot,
                    Thread.CurrentThread.CurrentCulture.Name, PortalController.Instance.GetCurrentPortalSettings(), LocalizationProvider.CustomizedLocale.None, true, true);
            }

            EventLogController.Instance.AddLog(listEntry, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.LISTENTRY_UPDATED);
            this.ClearListCache(listEntry.PortalID);
            this.ClearEntriesCache(listEntry.ListName, listEntry.PortalID);
        }

        public void UpdateListSortOrder(int EntryID, bool MoveUp)
        {
            DataProvider.Instance().UpdateListSortOrder(EntryID, MoveUp);
            ListEntryInfo entry = this.GetListEntryInfo(EntryID);
            this.ClearListCache(entry.PortalID);
            this.ClearEntriesCache(entry.ListName, entry.PortalID);
        }

        [Obsolete("Obsoleted in 6.0.1 use IEnumerable<ListEntryInfo> GetListEntryInfoXXX(string) instead. Scheduled removal in v10.0.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ListEntryInfoCollection GetListEntryInfoCollection(string listName)
        {
            return this.GetListEntryInfoCollection(listName, string.Empty, Null.NullInteger);
        }

        [Obsolete("Obsoleted in 6.0.1 use IEnumerable<ListEntryInfo> GetListEntryInfoXXX(string, string, int) instead. Scheduled removal in v10.0.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ListEntryInfoCollection GetListEntryInfoCollection(string listName, string parentKey)
        {
            return this.GetListEntryInfoCollection(listName, parentKey, Null.NullInteger);
        }

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

        private ListInfo FillListInfo(IDataReader dr, bool CheckForOpenDataReader)
        {
            ListInfo list = null;

            // read datareader
            bool canContinue = true;
            if (CheckForOpenDataReader)
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
