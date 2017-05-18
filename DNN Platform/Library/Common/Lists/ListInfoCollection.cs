#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
                mKeyIndexLookup.Add(key.ToLower(), index);
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
                if (mKeyIndexLookup[key.ToLower()] == null)
                {
                    return null;
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return null;
            }
            index = Convert.ToInt32(mKeyIndexLookup[key.ToLower()]);
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
                if (mKeyIndexLookup[key.ToLower()] != null)
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
                index = Convert.ToInt32(mKeyIndexLookup[key.ToLower()]);
                obj = base.List[index];
            }
            return obj;
        }

        public ArrayList GetChild(string ParentKey)
        {
            var childList = new ArrayList();
            foreach (object child in List)
            {
                if (((ListInfo) child).Key.IndexOf(ParentKey.ToLower()) > -1)
                {
                    childList.Add(child);
                }
            }
            return childList;
        }
    }
}