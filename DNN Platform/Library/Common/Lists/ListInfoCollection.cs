﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;

using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Common.Lists
{
    [Serializable]
    public class ListInfoCollection : CollectionBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ListInfoCollection));
        private readonly Hashtable mKeyIndexLookup = new Hashtable();

        public ListInfo GetChildren(string ParentName)
        {
            return (ListInfo) Item(ParentName);
        }

        internal new void Clear()
        {
            mKeyIndexLookup.Clear();
            base.Clear();
        }

        public void Add(string key, object value)
        {
            int index;
            //<tam:note key to be lowercase for appropiated seeking>
            try
            {
                index = base.List.Add(value);
                mKeyIndexLookup.Add(key.ToLowerInvariant(), index);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }

        public object Item(int index)
        {
            try
            {
                object obj;
                obj = base.List[index];
                return obj;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return null;
            }
        }

        public object Item(string key)
        {
            int index;
            object obj;
            try //Do validation first
            {
                if (mKeyIndexLookup[key.ToLowerInvariant()] == null)
                {
                    return null;
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return null;
            }
            index = Convert.ToInt32(mKeyIndexLookup[key.ToLowerInvariant()]);
            obj = base.List[index];
            return obj;
        }

        // Another method, get Lists on demand
        public object Item(string key, bool Cache)
        {
            int index;
            object obj = null;
            bool itemExists = false;
            try //Do validation first
            {
                if (mKeyIndexLookup[key.ToLowerInvariant()] != null)
                {
                    itemExists = true;
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
            //key will be in format Country.US:Region
            if (!itemExists)
            {
                var ctlLists = new ListController();
                string listName = key.Substring(key.IndexOf(":") + 1);
                string parentKey = key.Replace(listName, "").TrimEnd(':');
                ListInfo listInfo = ctlLists.GetListInfo(listName, parentKey);
                //the collection has been cache, so add this entry list into it if specified
                if (Cache)
                {
                    Add(listInfo.Key, listInfo);
                    return listInfo;
                }
            }
            else
            {
                index = Convert.ToInt32(mKeyIndexLookup[key.ToLowerInvariant()]);
                obj = base.List[index];
            }
            return obj;
        }

        public ArrayList GetChild(string ParentKey)
        {
            var childList = new ArrayList();
            foreach (object child in List)
            {
                if (((ListInfo) child).Key.IndexOf(ParentKey.ToLowerInvariant()) > -1)
                {
                    childList.Add(child);
                }
            }
            return childList;
        }
    }
}
