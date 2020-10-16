﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Provides naive locking for generic lists.
    /// </summary>
    /// <typeparam name="T">The type of value in the list.</typeparam>
    public class NaiveLockingList<T> : IList<T>
    {
        private readonly SharedList<T> list = new SharedList<T>();

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
                return this.list;
            }
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                return this.DoInReadLock(() => this.list.Count);
            }
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public T this[int index]
        {
            get
            {
                return this.DoInReadLock(() => this.list[index]);
            }

            set
            {
                this.DoInWriteLock(() => this.list[index] = value);
            }
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            // disposal of enumerator will release read lock
            // TODO is there a need for some sort of timed release?  the timmer must release from the correct thread
            // if using RWLS
            var readLock = this.list.GetReadLock();
            return new NaiveLockingEnumerator(this.list.GetEnumerator(), readLock);
        }

        /// <inheritdoc/>
        public void Add(T item)
        {
            this.DoInWriteLock(() => this.list.Add(item));
        }

        /// <inheritdoc/>
        public void Clear()
        {
            this.DoInWriteLock(() => this.list.Clear());
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            return this.DoInReadLock(() => this.list.Contains(item));
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.DoInReadLock(() => this.list.CopyTo(array, arrayIndex));
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            return this.DoInWriteLock(() => this.list.Remove(item));
        }

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            return this.DoInReadLock(() => this.list.IndexOf(item));
        }

        /// <inheritdoc/>
        public void Insert(int index, T item)
        {
            this.DoInWriteLock(() => this.list.Insert(index, item));
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            this.DoInWriteLock(() => this.list.RemoveAt(index));
        }

        /// <inheritdoc/>
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
            using (this.list.GetReadLock())
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
            using (this.list.GetWriteLock())
            {
                return func.Invoke();
            }
        }

        /// <summary>An <see cref="IEnumerator{T}"/> implementation for <see cref="NaiveLockingList{T}"/>.</summary>
        public class NaiveLockingEnumerator : IEnumerator<T>
        {
            private readonly IEnumerator<T> enumerator;
            private readonly ISharedCollectionLock readLock;
            private bool isDisposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="NaiveLockingEnumerator"/> class.
            /// </summary>
            /// <param name="enumerator">The enumerator to implement locking on.</param>
            /// <param name="readLock">An <see cref="ISharedCollectionLock"/> instance to implement the read lock.</param>
            public NaiveLockingEnumerator(IEnumerator<T> enumerator, ISharedCollectionLock readLock)
            {
                this.enumerator = enumerator;
                this.readLock = readLock;
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="NaiveLockingEnumerator"/> class.
            /// </summary>
            ~NaiveLockingEnumerator()
            {
                this.Dispose(false);
            }

            /// <inheritdoc/>
            public T Current
            {
                get
                {
                    return this.enumerator.Current;
                }
            }

            /// <inheritdoc/>
            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            /// <inheritdoc/>
            public bool MoveNext()
            {
                return this.enumerator.MoveNext();
            }

            /// <inheritdoc/>
            public void Reset()
            {
                this.enumerator.Reset();
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                this.Dispose(true);

                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Disposes this instance resources.
            /// </summary>
            /// <param name="disposing">A value indicating whether this instance is currently disposing.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (!this.isDisposed)
                {
                    if (disposing)
                    {
                        // dispose managed resrources here
                        this.enumerator.Dispose();
                        this.readLock.Dispose();
                    }

                    // dispose unmanaged resrources here
                    this.isDisposed = true;
                }
            }
        }
    }
}
