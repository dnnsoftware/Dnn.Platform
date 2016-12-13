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
using System.ComponentModel;

using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Common.Lists
{
    [Serializable]
    [Obsolete("Obsoleted in 6.0.1.  Replaced by using generic collections of ListEntryInfo objects"), EditorBrowsable(EditorBrowsableState.Never)]
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
                if (_keyIndexLookup[key.ToLower()] == null)
                {
                    return null;
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return null;
            }
            index = Convert.ToInt32(_keyIndexLookup[key.ToLower()]);
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
                _keyIndexLookup.Add(key.ToLower(), index);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }
    }
}