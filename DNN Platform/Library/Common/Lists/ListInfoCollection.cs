// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Lists
{
    using System;
    using System.Collections;

    using DotNetNuke.Instrumentation;

    [Serializable]
    public class ListInfoCollection : CollectionBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListInfoCollection));
        private readonly Hashtable mKeyIndexLookup = new Hashtable();

        public ListInfo GetChildren(string ParentName)
        {
            return (ListInfo)this.Item(ParentName);
        }

        public void Add(string key, object value)
        {
            int index;

            // <tam:note key to be lowercase for appropiated seeking>
            try
            {
                index = this.List.Add(value);
                this.mKeyIndexLookup.Add(key.ToLowerInvariant(), index);
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
                obj = this.List[index];
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
            try // Do validation first
            {
                if (this.mKeyIndexLookup[key.ToLowerInvariant()] == null)
                {
                    return null;
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return null;
            }

            index = Convert.ToInt32(this.mKeyIndexLookup[key.ToLowerInvariant()]);
            obj = this.List[index];
            return obj;
        }

        // Another method, get Lists on demand
        public object Item(string key, bool Cache)
        {
            int index;
            object obj = null;
            bool itemExists = false;
            try // Do validation first
            {
                if (this.mKeyIndexLookup[key.ToLowerInvariant()] != null)
                {
                    itemExists = true;
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            // key will be in format Country.US:Region
            if (!itemExists)
            {
                var ctlLists = new ListController();
                string listName = key.Substring(key.IndexOf(":") + 1);
                string parentKey = key.Replace(listName, string.Empty).TrimEnd(':');
                ListInfo listInfo = ctlLists.GetListInfo(listName, parentKey);

                // the collection has been cache, so add this entry list into it if specified
                if (Cache)
                {
                    this.Add(listInfo.Key, listInfo);
                    return listInfo;
                }
            }
            else
            {
                index = Convert.ToInt32(this.mKeyIndexLookup[key.ToLowerInvariant()]);
                obj = this.List[index];
            }

            return obj;
        }

        public ArrayList GetChild(string ParentKey)
        {
            var childList = new ArrayList();
            foreach (object child in this.List)
            {
                if (((ListInfo)child).Key.IndexOf(ParentKey.ToLowerInvariant()) > -1)
                {
                    childList.Add(child);
                }
            }

            return childList;
        }

        internal new void Clear()
        {
            this.mKeyIndexLookup.Clear();
            base.Clear();
        }
    }
}
