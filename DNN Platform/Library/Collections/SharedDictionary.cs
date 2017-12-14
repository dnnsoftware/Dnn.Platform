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
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Collections.Internal
{
    [Serializable]
    public class SharedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {
        private IDictionary<TKey, TValue> _dict;

        private bool _isDisposed;
        private ILockStrategy _lockController;

        public SharedDictionary() : this(LockingStrategy.ReaderWriter)
        {
        }

        public SharedDictionary(ILockStrategy lockStrategy)
        {
            _dict = new Dictionary<TKey, TValue>();
            _lockController = lockStrategy;
        }

        public SharedDictionary(LockingStrategy strategy) : this(LockingStrategyFactory.Create(strategy))
        {
        }

        internal IDictionary<TKey, TValue> BackingDictionary
        {
            get
            {
                return _dict;
            }
        }

        #region IDictionary<TKey,TValue> Members

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return IEnumerable_GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return IEnumerable_GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            EnsureNotDisposed();
            EnsureWriteAccess();
            _dict.Add(item);
        }

        public void Clear()
        {
            EnsureNotDisposed();
            EnsureWriteAccess();
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            EnsureNotDisposed();
            EnsureReadAccess();
            return _dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            EnsureNotDisposed();
            EnsureReadAccess();
            _dict.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            EnsureNotDisposed();
            EnsureWriteAccess();
            return _dict.Remove(item);
        }

        public int Count
        {
            get
            {
                EnsureNotDisposed();
                EnsureReadAccess();
                return _dict.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                EnsureNotDisposed();
                EnsureReadAccess();
                return _dict.IsReadOnly;
            }
        }

        public bool ContainsKey(TKey key)
        {
            EnsureNotDisposed();
            EnsureReadAccess();
            return _dict.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            EnsureNotDisposed();
            EnsureWriteAccess();
            _dict.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            EnsureNotDisposed();
            EnsureWriteAccess();
            return _dict.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            EnsureNotDisposed();
            EnsureReadAccess();
            return _dict.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                EnsureNotDisposed();
                EnsureReadAccess();
                return _dict[key];
            }
            set
            {
                EnsureNotDisposed();
                EnsureWriteAccess();
                _dict[key] = value;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                EnsureNotDisposed();
                EnsureReadAccess();
                return _dict.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                EnsureNotDisposed();
                EnsureReadAccess();
                return _dict.Values;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        public ISharedCollectionLock GetReadLock()
        {
            return GetReadLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetReadLock(TimeSpan timeOut)
        {
            EnsureNotDisposed();
            return _lockController.GetReadLock(timeOut);
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
            return _lockController.GetWriteLock(timeOut);
        }

        public ISharedCollectionLock GetWriteLock(int millisecondTimeout)
        {
            return GetWriteLock(TimeSpan.FromMilliseconds(millisecondTimeout));
        }


        private void EnsureReadAccess()
        {
            if (!(_lockController.ThreadCanRead))
            {
                throw new ReadLockRequiredException();
            }
        }

        private void EnsureWriteAccess()
        {
            if (!_lockController.ThreadCanWrite)
            {
                throw new WriteLockRequiredException();
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable_GetEnumerator()
        {
            EnsureNotDisposed();
            EnsureReadAccess();

            //todo nothing ensures read lock is held for life of enumerator
            return _dict.GetEnumerator();
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("SharedDictionary");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    //dispose managed resrources here
                    _dict = null;
                }

                //dispose unmanaged resrources here
                _lockController.Dispose();
                _lockController = null;
                _isDisposed = true;
            }
        }

        ~SharedDictionary()
        {
            Dispose(false);
        }
    }
}