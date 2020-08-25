// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Lists
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    using DotNetNuke.Instrumentation;

    [Serializable]
    [Obsolete("Obsoleted in 6.0.1.  Replaced by using generic collections of ListEntryInfo objects. Scheduled removal in v10.0.0.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ListEntryInfoCollection : CollectionBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListEntryInfoCollection));
        private readonly Hashtable _keyIndexLookup = new Hashtable();

        public ListEntryInfo Item(int index)
        {
            try
            {
                return (ListEntryInfo)this.List[index];
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

            // <tam:note key to be lowercase for appropiated seeking>
            try
            {
                if (this._keyIndexLookup[key.ToLowerInvariant()] == null)
                {
                    return null;
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return null;
            }

            index = Convert.ToInt32(this._keyIndexLookup[key.ToLowerInvariant()]);
            return (ListEntryInfo)this.List[index];
        }

        public ListEntryInfo GetChildren(string parentName)
        {
            return this.Item(parentName);
        }

        public void Add(string key, ListEntryInfo value)
        {
            int index;
            try // Do validation first
            {
                index = this.List.Add(value);
                this._keyIndexLookup.Add(key.ToLowerInvariant(), index);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }

        internal new void Clear()
        {
            this._keyIndexLookup.Clear();
            base.Clear();
        }
    }
}
