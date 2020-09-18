// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class SharedList<T> : IList<T>, IDisposable
    {
        private readonly List<T> _list = new List<T>();
        private ILockStrategy _lockStrategy;

        private bool _isDisposed;

        public SharedList()
            : this(LockingStrategy.ReaderWriter)
        {
        }

        public SharedList(ILockStrategy lockStrategy)
        {
            this._lockStrategy = lockStrategy;
        }

        public SharedList(LockingStrategy strategy)
            : this(LockingStrategyFactory.Create(strategy))
        {
        }

        ~SharedList()
        {
            this.Dispose(false);
        }

        public int Count
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this._list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return ((ICollection<T>)this._list).IsReadOnly;
            }
        }

        internal IList<T> BackingList
        {
            get
            {
                return this._list;
            }
        }

        public T this[int index]
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this._list[index];
            }

            set
            {
                this.EnsureNotDisposed();
                this.EnsureWriteAccess();
                this._list[index] = value;
            }
        }

        public void Add(T item)
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this._list.Add(item);
        }

        public void Clear()
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this._list.Clear();
        }

        public bool Contains(T item)
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            return this._list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            this._list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            return this._list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            return this._list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            return this._list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this._list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this._list.RemoveAt(index);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        // To detect redundant calls
        public void EnsureNotDisposed()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException("SharedList");
            }
        }

        public ISharedCollectionLock GetReadLock()
        {
            return this.GetReadLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetReadLock(TimeSpan timeOut)
        {
            this.EnsureNotDisposed();
            return this._lockStrategy.GetReadLock(timeOut);
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
            return this._lockStrategy.GetWriteLock(timeOut);
        }

        public ISharedCollectionLock GetWriteLock(int millisecondTimeout)
        {
            return this.GetWriteLock(TimeSpan.FromMilliseconds(millisecondTimeout));
        }

        public IEnumerator GetEnumerator1()
        {
            return this.GetEnumerator();
        }

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this._isDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                }

                this._lockStrategy.Dispose();
                this._lockStrategy = null;
            }

            this._isDisposed = true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator1();
        }

        private void EnsureReadAccess()
        {
            if (!this._lockStrategy.ThreadCanRead)
            {
                throw new ReadLockRequiredException();
            }
        }

        private void EnsureWriteAccess()
        {
            if (!this._lockStrategy.ThreadCanWrite)
            {
                throw new WriteLockRequiredException();
            }
        }
    }
}
