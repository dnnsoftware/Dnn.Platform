#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Common.Lists
{
    public class ListController
    {
        private void ClearCache(int PortalId)
        {
            DataCache.ClearListsCache(PortalId);
        }

        private ListInfo FillListInfo(IDataReader dr, bool CheckForOpenDataReader)
        {
            ListInfo objListInfo = null;
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
                objListInfo = new ListInfo(Convert.ToString(dr["ListName"]));
                {
                    objListInfo.Level = Convert.ToInt32(dr["Level"]);
                    objListInfo.PortalID = Convert.ToInt32(dr["PortalID"]);
                    objListInfo.DefinitionID = Convert.ToInt32(dr["DefinitionID"]);
                    objListInfo.EntryCount = Convert.ToInt32(dr["EntryCount"]);
                    objListInfo.ParentID = Convert.ToInt32(dr["ParentID"]);
                    objListInfo.ParentKey = Convert.ToString(dr["ParentKey"]);
                    objListInfo.Parent = Convert.ToString(dr["Parent"]);
                    objListInfo.ParentList = Convert.ToString(dr["ParentList"]);
                    objListInfo.EnableSortOrder = (Convert.ToInt32(dr["MaxSortOrder"]) > 0);
                    objListInfo.SystemList = Convert.ToInt32(dr["SystemList"]) > 0;
                }
            }
            return objListInfo;
        }

        private Dictionary<string, ListInfo> FillListInfoDictionary(IDataReader dr)
        {
            var dic = new Dictionary<string, ListInfo>();
            try
            {
                ListInfo obj;
                while (dr.Read())
				{
                    // fill business object
                    obj = FillListInfo(dr, false);
                    if (!dic.ContainsKey(obj.Key))
                    {
                        dic.Add(obj.Key, obj);
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

        private object GetListInfoDictionaryCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int) cacheItemArgs.ParamList[0];
            return FillListInfoDictionary(DataProvider.Instance().GetLists(portalId));
        }

        private Dictionary<string, ListInfo> GetListInfoDictionary(int PortalId)
        {
            string cacheKey = string.Format(DataCache.ListsCacheKey, PortalId);
            return CBO.GetCachedObject<Dictionary<string, ListInfo>>(new CacheItemArgs(cacheKey, DataCache.ListsCacheTimeOut, DataCache.ListsCachePriority, PortalId), GetListInfoDictionaryCallBack);
        }

        public int AddListEntry(ListEntryInfo ListEntry)
        {
            bool EnableSortOrder = (ListEntry.SortOrder > 0);
            ClearCache(ListEntry.PortalID);
            int entryId = DataProvider.Instance().AddListEntry(ListEntry.ListName,
                                                        ListEntry.Value,
                                                        ListEntry.Text,
                                                        ListEntry.ParentID,
                                                        ListEntry.Level,
                                                        EnableSortOrder,
                                                        ListEntry.DefinitionID,
                                                        ListEntry.Description,
                                                        ListEntry.PortalID,
                                                        ListEntry.SystemList,
                                                        UserController.GetCurrentUserInfo().UserID);

            if (entryId != Null.NullInteger)
            {
                var objEventLog = new EventLogController();
                objEventLog.AddLog(ListEntry, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.LISTENTRY_CREATED);
            }

            return entryId;
        }

        public void DeleteList(string ListName, string ParentKey)
        {
            DeleteList(ListName, ParentKey, Null.NullInteger);
        }

		public void DeleteList(string ListName, string ParentKey, int portalId)
		{
			ListInfo list = GetListInfo(ListName, ParentKey, portalId);
			var objEventLog = new EventLogController();
			objEventLog.AddLog("ListName", ListName, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, EventLogController.EventLogType.LISTENTRY_DELETED);
			DataProvider.Instance().DeleteList(ListName, ParentKey);
			if (list != null)
				ClearCache(list.PortalID);
		}

        public void DeleteList(ListInfo list, bool includeChildren)
        {
            if (list == null)
            {
                return;
            }

            var lists = new SortedList<string, ListInfo>();
            lists.Add(list.Key, list);
            //add Children
            if (includeChildren)
            {
                foreach (KeyValuePair<string, ListInfo> listPair in GetListInfoDictionary(list.PortalID))
                {
                    if ((listPair.Value.ParentList.StartsWith(list.Key)))
                    {
                        lists.Add(listPair.Value.Key.Replace(":", "."), listPair.Value);
                    }
                }
            }
            //Delete items in reverse order so deeper descendants are removed before their parents
            for (int i = lists.Count - 1; i >= 0; i += -1)
            {
				DeleteList(lists.Values[i].Name, lists.Values[i].ParentKey, lists.Values[i].PortalID);
            }
        }

        public void DeleteListEntryByID(int EntryID, bool DeleteChild)
        {
            ListEntryInfo entry = GetListEntryInfo(EntryID);
            DataProvider.Instance().DeleteListEntryByID(EntryID, DeleteChild);
            ClearCache(entry.PortalID);
        }

        public void DeleteListEntryByListName(string ListName, string Value, bool DeleteChild)
        {
            ListEntryInfo entry = GetListEntryInfo(ListName, Value);
            DataProvider.Instance().DeleteListEntryByListName(ListName, Value, DeleteChild);
            ClearCache(entry.PortalID);
        }

        public ListEntryInfo GetListEntryInfo(int EntryID)
        {
            return (ListEntryInfo) CBO.FillObject(DataProvider.Instance().GetListEntry(EntryID), typeof (ListEntryInfo));
        }

        public ListEntryInfo GetListEntryInfo(string ListName, string Value)
        {
            return (ListEntryInfo) CBO.FillObject(DataProvider.Instance().GetListEntry(ListName, Value), typeof (ListEntryInfo));
        }

        public IEnumerable<ListEntryInfo> GetListEntryInfoItems(string listName)
        {
            return GetListEntryInfoItems(listName, "", Null.NullInteger);
        }

        public IEnumerable<ListEntryInfo> GetListEntryInfoItems(string listName, string parentKey)
        {
            return GetListEntryInfoItems(listName, parentKey, Null.NullInteger);
        }

        public IEnumerable<ListEntryInfo> GetListEntryInfoItems(string listName, string parentKey, int portalId)
        {
            return CBO.FillCollection<ListEntryInfo>(DataProvider.Instance().GetListEntriesByListName(listName, parentKey, portalId));
        }

        public Dictionary<string, ListEntryInfo> GetListEntryInfoDictionary(string listName)
        {
            return GetListEntryInfoDictionary(listName, "", Null.NullInteger);
        }

        public Dictionary<string, ListEntryInfo> GetListEntryInfoDictionary(string listName, string parentKey)
        {
            return GetListEntryInfoDictionary(listName, parentKey, Null.NullInteger);
        }

        public Dictionary<string, ListEntryInfo> GetListEntryInfoDictionary(string listName, string parentKey, int portalId)
        {
            return ListEntryInfoItemsToDictionary(GetListEntryInfoItems(listName, parentKey, portalId));
        }
        
        private static Dictionary<string, ListEntryInfo> ListEntryInfoItemsToDictionary(IEnumerable<ListEntryInfo> items)
        {
            var dict = new Dictionary<string, ListEntryInfo>();
            items.ToList().ForEach(x => dict.Add(x.Key, x));

            return dict;
        }

        public ListInfo GetListInfo(string ListName)
        {
            return GetListInfo(ListName, "");
        }

        public ListInfo GetListInfo(string ListName, string ParentKey)
        {
            return GetListInfo(ListName, ParentKey, -1);
        }

        public ListInfo GetListInfo(string ListName, string ParentKey, int PortalID)
        {
            ListInfo list = null;
            string key = Null.NullString;
            if (!string.IsNullOrEmpty(ParentKey))
            {
                key = ParentKey + ":";
            }
            key += ListName;
            Dictionary<string, ListInfo> dicLists = GetListInfoDictionary(PortalID);
            if (!dicLists.TryGetValue(key, out list))
            {
                IDataReader dr = DataProvider.Instance().GetList(ListName, ParentKey, PortalID);
                try
                {
                    list = FillListInfo(dr, true);
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
            return GetListInfoCollection("");
        }

        public ListInfoCollection GetListInfoCollection(string ListName)
        {
            return GetListInfoCollection(ListName, "");
        }

        public ListInfoCollection GetListInfoCollection(string ListName, string ParentKey)
        {
            return GetListInfoCollection(ListName, ParentKey, -1);
        }

        public ListInfoCollection GetListInfoCollection(string ListName, string ParentKey, int PortalID)
        {
            IList lists = new ListInfoCollection();
            foreach (KeyValuePair<string, ListInfo> listPair in GetListInfoDictionary(PortalID))
            {
                ListInfo list = listPair.Value;
                if ((list.Name == ListName || string.IsNullOrEmpty(ListName)) && (list.ParentKey == ParentKey || string.IsNullOrEmpty(ParentKey)) &&
                    (list.PortalID == PortalID || PortalID == Null.NullInteger))
                {
                    lists.Add(list);
                }
            }
            return (ListInfoCollection) lists;
        }

        public void UpdateListEntry(ListEntryInfo ListEntry)
        {
            DataProvider.Instance().UpdateListEntry(ListEntry.EntryID, ListEntry.Value, ListEntry.Text, ListEntry.Description, UserController.GetCurrentUserInfo().UserID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog(ListEntry, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.LISTENTRY_UPDATED);
            ClearCache(ListEntry.PortalID);
        }

        public void UpdateListSortOrder(int EntryID, bool MoveUp)
        {
            DataProvider.Instance().UpdateListSortOrder(EntryID, MoveUp);
            ListEntryInfo entry = GetListEntryInfo(EntryID);
            ClearCache(entry.PortalID);
        }

        [Obsolete("Obsoleted in 6.0.1 use IEnumerable<ListEntryInfo> GetListEntryInfoXXX(string) instead"), EditorBrowsable(EditorBrowsableState.Never)]
        public ListEntryInfoCollection GetListEntryInfoCollection(string listName)
        {
            return GetListEntryInfoCollection(listName, "", Null.NullInteger);
        }

        [Obsolete("Obsoleted in 6.0.1 use IEnumerable<ListEntryInfo> GetListEntryInfoXXX(string, string, int) instead"), EditorBrowsable(EditorBrowsableState.Never)]
        public ListEntryInfoCollection GetListEntryInfoCollection(string listName, string parentKey)
        {
            return GetListEntryInfoCollection(listName, parentKey, Null.NullInteger);
        }

        [Obsolete("Obsoleted in 6.0.1 use IEnumerable<ListEntryInfo> GetListEntryInfoXXX(string, string, int) instead"), EditorBrowsable(EditorBrowsableState.Never)]
        public ListEntryInfoCollection GetListEntryInfoCollection(string listName, string parentKey, int portalId)
        {
            var items = GetListEntryInfoItems(listName, parentKey, portalId);

            var collection = new ListEntryInfoCollection();
            if (items != null)
            {
                items.ToList().ForEach(x => collection.Add(x.Key, x));
            }
            return collection;
        }
    }
}