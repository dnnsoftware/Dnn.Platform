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

#endregion

namespace DotNetNuke.Services.Log.EventLog
{
    [Serializable]
    [Obsolete("Deprecated in 6.0. Replaced by List<LogInfo>.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LogInfoArray : IEnumerable, IList
    {
        private readonly ArrayList _arrLogs = new ArrayList();

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return _arrLogs.GetEnumerator();
        }

        #endregion

        #region IList Members

        public int Count
        {
            get
            {
                return _arrLogs.Count;
            }
        }

        public int Add(object objLogInfo)
        {
            return _arrLogs.Add(objLogInfo);
        }

        public void Remove(object objLogInfo)
        {
            _arrLogs.Remove(objLogInfo);
        }

        public void CopyTo(Array array, int index)
        {
            _arrLogs.CopyTo(array, index);
        }

        public bool IsSynchronized
        {
            get
            {
                return _arrLogs.IsSynchronized;
            }
        }

        public object SyncRoot
        {
            get
            {
                return _arrLogs.SyncRoot;
            }
        }

        public void Clear()
        {
            _arrLogs.Clear();
        }

        public bool Contains(object value)
        {
            if (_arrLogs.Contains(value))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int IndexOf(object value)
        {
            return _arrLogs.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            _arrLogs.Insert(index, value);
        }

        public bool IsFixedSize
        {
            get
            {
                return _arrLogs.IsFixedSize;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _arrLogs.IsReadOnly;
            }
        }

        public object this[int index]
        {
            get
            {
                return _arrLogs[index];
            }
            set
            {
                _arrLogs[index] = value;
            }
        }

        public void RemoveAt(int index)
        {
            _arrLogs.RemoveAt(index);
        }

        #endregion

        public LogInfo GetItem(int Index)
        {
            return (LogInfo) _arrLogs[Index];
        }

        public IEnumerator GetEnumerator(int index, int count)
        {
            return _arrLogs.GetEnumerator(index, count);
        }
    }
}