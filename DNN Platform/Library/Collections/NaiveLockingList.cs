// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class NaiveLockingList<T> : IList<T>
    {
        private readonly SharedList<T> _list = new SharedList<T>();

        /// <summary>
        /// Gets access to the underlying SharedList.
        /// <remarks>
        /// Allows locking to be explicitly managed for the sake of effeciency
        /// </remarks>
        /// </summary>
        public SharedList<T> SharedList
        {
            get
            {
                return this._list;
            }
        }

        public int Count
        {
            get
            {
                return this.DoInReadLock(() => this._list.Count);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public T this[int index]
        {
            get
            {
                return this.DoInReadLock(() => this._list[index]);
            }

            set
            {
                this.DoInWriteLock(() => this._list[index] = value);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            // disposal of enumerator will release read lock
            // TODO is there a need for some sort of timed release?  the timmer must release from the correct thread
            // if using RWLS
            var readLock = this._list.GetReadLock();
            return new NaiveLockingEnumerator(this._list.GetEnumerator(), readLock);
        }

        public void Add(T item)
        {
            this.DoInWriteLock(() => this._list.Add(item));
        }

        public void Clear()
        {
            this.DoInWriteLock(() => this._list.Clear());
        }

        public bool Contains(T item)
        {
            return this.DoInReadLock(() => this._list.Contains(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.DoInReadLock(() => this._list.CopyTo(array, arrayIndex));
        }

        public bool Remove(T item)
        {
            return this.DoInWriteLock(() => this._list.Remove(item));
        }

        public int IndexOf(T item)
        {
            return this.DoInReadLock(() => this._list.IndexOf(item));
        }

        public void Insert(int index, T item)
        {
            this.DoInWriteLock(() => this._list.Insert(index, item));
        }

        public void RemoveAt(int index)
        {
            this.DoInWriteLock(() => this._list.RemoveAt(index));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        // TODO is no recursion the correct policy
        private void DoInReadLock(Action action)
        {
            this.DoInReadLock(() =>
            {
                action.Invoke();
                return true;
            });
        }

        private TRet DoInReadLock<TRet>(Func<TRet> func)
        {
            using (this._list.GetReadLock())
            {
                return func.Invoke();
            }
        }

        private void DoInWriteLock(Action action)
        {
            this.DoInWriteLock(() =>
            {
                action.Invoke();
                return true;
            });
        }

        private TRet DoInWriteLock<TRet>(Func<TRet> func)
        {
            using (this._list.GetWriteLock())
            {
                return func.Invoke();
            }
        }

        public class NaiveLockingEnumerator : IEnumerator<T>
        {
            private readonly IEnumerator<T> _enumerator;
            private readonly ISharedCollectionLock _readLock;
            private bool _isDisposed;

            public NaiveLockingEnumerator(IEnumerator<T> enumerator, ISharedCollectionLock readLock)
            {
                this._enumerator = enumerator;
                this._readLock = readLock;
            }

            ~NaiveLockingEnumerator()
            {
                this.Dispose(false);
            }

            public T Current
            {
                get
                {
                    return this._enumerator.Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            public bool MoveNext()
            {
                return this._enumerator.MoveNext();
            }

            public void Reset()
            {
                this._enumerator.Reset();
            }

            public void Dispose()
            {
                this.Dispose(true);

                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!this._isDisposed)
                {
                    if (disposing)
                    {
                        // dispose managed resrources here
                        this._enumerator.Dispose();
                        this._readLock.Dispose();
                    }

                    // dispose unmanaged resrources here
                    this._isDisposed = true;
                }
            }
        }
    }
}
