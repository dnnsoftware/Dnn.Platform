// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    [Serializable]
    public class SharedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {
        private IDictionary<TKey, TValue> _dict;

        private bool _isDisposed;
        private ILockStrategy _lockController;

        public SharedDictionary()
            : this(LockingStrategy.ReaderWriter)
        {
        }

        public SharedDictionary(ILockStrategy lockStrategy)
        {
            this._dict = new Dictionary<TKey, TValue>();
            this._lockController = lockStrategy;
        }

        public SharedDictionary(LockingStrategy strategy)
            : this(LockingStrategyFactory.Create(strategy))
        {
        }

        ~SharedDictionary()
        {
            this.Dispose(false);
        }

        public int Count
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this._dict.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this._dict.IsReadOnly;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this._dict.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this._dict.Values;
            }
        }

        internal IDictionary<TKey, TValue> BackingDictionary
        {
            get
            {
                return this._dict;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this._dict[key];
            }

            set
            {
                this.EnsureNotDisposed();
                this.EnsureWriteAccess();
                this._dict[key] = value;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this.IEnumerable_GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this._dict.Add(item);
        }

        public void Clear()
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this._dict.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            return this._dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            this._dict.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            return this._dict.Remove(item);
        }

        public bool ContainsKey(TKey key)
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            return this._dict.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this._dict.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            return this._dict.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            return this._dict.TryGetValue(key, out value);
        }

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        public ISharedCollectionLock GetReadLock()
        {
            return this.GetReadLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetReadLock(TimeSpan timeOut)
        {
            this.EnsureNotDisposed();
            return this._lockController.GetReadLock(timeOut);
        }

        public ISharedCollectionLock GetReadLock(int millisecondTimeout)
        {
            return this.GetReadLock(TimeSpan.FromMilliseconds(millisecondTimeout));
        }

        public ISharedCollectionLock GetWriteLock()
        {
            return this.GetWriteLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetWriteLock(TimeSpan timeOut)
        {
            this.EnsureNotDisposed();
            return this._lockController.GetWriteLock(timeOut);
        }

        public ISharedCollectionLock GetWriteLock(int millisecondTimeout)
        {
            return this.GetWriteLock(TimeSpan.FromMilliseconds(millisecondTimeout));
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable_GetEnumerator()
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();

            // todo nothing ensures read lock is held for life of enumerator
            return this._dict.GetEnumerator();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._isDisposed)
            {
                if (disposing)
                {
                    // dispose managed resrources here
                    this._dict = null;
                }

                // dispose unmanaged resrources here
                this._lockController.Dispose();
                this._lockController = null;
                this._isDisposed = true;
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return this.IEnumerable_GetEnumerator();
        }

        private void EnsureReadAccess()
        {
            if (!this._lockController.ThreadCanRead)
            {
                throw new ReadLockRequiredException();
            }
        }

        private void EnsureWriteAccess()
        {
            if (!this._lockController.ThreadCanWrite)
            {
                throw new WriteLockRequiredException();
            }
        }

        private void EnsureNotDisposed()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException("SharedDictionary");
            }
        }
    }
}
