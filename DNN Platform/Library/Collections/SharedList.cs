#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

#endregion

namespace DotNetNuke.Collections.Internal
{
    public class SharedList<T> : IList<T>, IDisposable
    {
        private readonly List<T> _list = new List<T>();
        private ILockStrategy _lockStrategy;

        public SharedList() : this(LockingStrategy.ReaderWriter)
        {
        }

        public SharedList(ILockStrategy lockStrategy)
        {
            _lockStrategy = lockStrategy;
        }

        public SharedList(LockingStrategy strategy) : this(LockingStrategyFactory.Create(strategy))
        {
        }

        internal IList<T> BackingList
        {
            get
            {
                return _list;
            }
        }

        #region IList<T> Members

        public void Add(T item)
        {
            EnsureNotDisposed();
            EnsureWriteAccess();
            _list.Add(item);
        }

        public void Clear()
        {
            EnsureNotDisposed();
            EnsureWriteAccess();
            _list.Clear();
        }

        public bool Contains(T item)
        {
            EnsureNotDisposed();
            EnsureReadAccess();
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            EnsureNotDisposed();
            EnsureReadAccess();
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                EnsureNotDisposed();
                EnsureReadAccess();
                return _list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                EnsureNotDisposed();
                EnsureReadAccess();
                return ((ICollection<T>) _list).IsReadOnly;
            }
        }

        public bool Remove(T item)
        {
            EnsureNotDisposed();
            EnsureWriteAccess();
            return _list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            EnsureNotDisposed();
            EnsureReadAccess();
            return _list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            EnsureNotDisposed();
            EnsureReadAccess();
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            EnsureNotDisposed();
            EnsureWriteAccess();
            _list.Insert(index, item);
        }

        public T this[int index]
        {
            get
            {
                EnsureNotDisposed();
                EnsureReadAccess();
                return _list[index];
            }
            set
            {
                EnsureNotDisposed();
                EnsureWriteAccess();
                _list[index] = value;
            }
        }

        public void RemoveAt(int index)
        {
            EnsureNotDisposed();
            EnsureWriteAccess();
            _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }

        #endregion

        #region "IDisposable Support"

        private bool _isDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // To detect redundant calls

        public void EnsureNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("SharedList");
            }
        }

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                }

                _lockStrategy.Dispose();
                _lockStrategy = null;
            }
            _isDisposed = true;
        }

        ~SharedList()
        {
            Dispose(false);
        }

        #endregion

        public ISharedCollectionLock GetReadLock()
        {
            return GetReadLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetReadLock(TimeSpan timeOut)
        {
            EnsureNotDisposed();
            return _lockStrategy.GetReadLock(timeOut);
        }

        public ISharedCollectionLock GetReadLock(int millisecondTimeout)
        {
            return GetReadLock(TimeSpan.FromMilliseconds(millisecondTimeout));
        }

        public ISharedCollectionLock GetWriteLock()
        {
            return GetWriteLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetWriteLock(TimeSpan timeOut)
        {
            EnsureNotDisposed();
            return _lockStrategy.GetWriteLock(timeOut);
        }

        public ISharedCollectionLock GetWriteLock(int millisecondTimeout)
        {
            return GetWriteLock(TimeSpan.FromMilliseconds(millisecondTimeout));
        }

        private void EnsureReadAccess()
        {
            if (!(_lockStrategy.ThreadCanRead))
            {
                throw new ReadLockRequiredException();
            }
        }

        private void EnsureWriteAccess()
        {
            if (!_lockStrategy.ThreadCanWrite)
            {
                throw new WriteLockRequiredException();
            }
        }

        public IEnumerator GetEnumerator1()
        {
            return GetEnumerator();
        }
    }
}