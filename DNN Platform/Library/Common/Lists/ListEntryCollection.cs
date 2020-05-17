// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;
using System.ComponentModel;

using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Common.Lists
{
    [Serializable]
    [Obsolete("Obsoleted in 6.0.1.  Replaced by using generic collections of ListEntryInfo objects. Scheduled removal in v10.0.0."), EditorBrowsable(EditorBrowsableState.Never)]
    public class ListEntryInfoCollection : CollectionBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ListEntryInfoCollection));
        private readonly Hashtable _keyIndexLookup = new Hashtable();

        public ListEntryInfo Item(int index)
        {
            try
            {
                return (ListEntryInfo) base.List[index];
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return null;
            }
        }

        public ListEntryInfo Item(string key)
        {
            int index;
            //<tam:note key to be lowercase for appropiated seeking>
            try
            {
                if (_keyIndexLookup[key.ToLowerInvariant()] == null)
                {
                    return null;
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return null;
            }
            index = Convert.ToInt32(_keyIndexLookup[key.ToLowerInvariant()]);
            return (ListEntryInfo) base.List[index];
        }

        public ListEntryInfo GetChildren(string parentName)
        {
            return Item(parentName);
        }

        internal new void Clear()
        {
            _keyIndexLookup.Clear();
            base.Clear();
        }

        public void Add(string key, ListEntryInfo value)
        {
            int index;
            try //Do validation first
            {
                index = base.List.Add(value);
                _keyIndexLookup.Add(key.ToLowerInvariant(), index);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }
    }
}
