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
using System.Collections.Concurrent;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Collections.Internal
{
    [Serializable]
    public class SharedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {
        private ConcurrentDictionary<TKey, TValue> _dict;

        private bool _isDisposed;
        private ILockStrategy _lockController;
        private ILockStrategy _fakeLockController = new FakeLockStrategy();

        public SharedDictionary() : this(LockingStrategy.ReaderWriter)
        {
        }

        public SharedDictionary(ILockStrategy lockStrategy)
        {
            _dict = new ConcurrentDictionary<TKey, TValue>();
            _lockController = lockStrategy;
        }

        public SharedDictionary(LockingStrategy strategy) : this(LockingStrategyFactory.Create(strategy))
        {
        }

        internal ConcurrentDictionary<TKey, TValue> BackingDictionary
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
            _dict.TryAdd(item.Key, item.Value);
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            EnsureNotDisposed();
            return _dict.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            EnsureNotDisposed();
            _dict.ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            EnsureNotDisposed();
            TValue value;
            return _dict.TryRemove(item.Key, out value);
        }

        public int Count
        {
            get
            {
                EnsureNotDisposed();
                return _dict.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                /**
                 * Kept only for compatibility
                 * even if it was uselss.
                 * Actually we don't have control over 
                 * readability of the _dict instance, since
                 * the Dictionary class is instatiated in the 
                 * constructor as ReadWrite.
                 * Actually it is alwyas false
                 */
                return false; // _dict.IsReadOnly;
            }
        }

        public bool ContainsKey(TKey key)
        {
            EnsureNotDisposed();
            return _dict.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            EnsureNotDisposed();
            EnsureWriteAccess();
            _dict.TryAdd(key, value);
        }

        public bool Remove(TKey key)
        {
            EnsureNotDisposed();
            TValue value;
            return _dict.TryRemove(key, out value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            EnsureNotDisposed();
            return _dict.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                EnsureNotDisposed();
                TValue value;
                _dict.TryGetValue(key, out value);
                return value;
            }
            set
            {
                EnsureNotDisposed();
                _dict.TryAdd(key, value);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                EnsureNotDisposed();
                return _dict.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                EnsureNotDisposed();
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
            return _fakeLockController.GetReadLock(timeOut);
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