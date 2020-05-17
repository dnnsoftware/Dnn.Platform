﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetNuke.Collections.Internal
{
    public class NaiveLockingList<T> : IList<T>
    {
        private readonly SharedList<T> _list = new SharedList<T>();
        //TODO is no recursion the correct policy
        
        void DoInReadLock(Action action)
        {
            DoInReadLock(() =>{ action.Invoke(); return true; });
        }

        TRet DoInReadLock<TRet>(Func<TRet> func)
        {
            using (_list.GetReadLock())
            {
                return func.Invoke();
            }
        }

        void DoInWriteLock(Action action)
        {
            DoInWriteLock(() => { action.Invoke(); return true; });
        }

        private TRet DoInWriteLock<TRet>(Func<TRet> func)
        {
            using (_list.GetWriteLock())
            {
                return func.Invoke();
            }
        }

        /// <summary>
        /// Access to the underlying SharedList
        /// <remarks>
        /// Allows locking to be explicitly managed for the sake of effeciency
        /// </remarks>
        /// </summary>
        public SharedList<T> SharedList
        {
            get
            {
                return _list;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            //disposal of enumerator will release read lock
            //TODO is there a need for some sort of timed release?  the timmer must release from the correct thread
            //if using RWLS
            var readLock = _list.GetReadLock();
            return new NaiveLockingEnumerator(_list.GetEnumerator(), readLock);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            DoInWriteLock(() => _list.Add(item));
        }

        public void Clear()
        {
            DoInWriteLock(() => _list.Clear());
        }

        public bool Contains(T item)
        {
            return DoInReadLock(() => _list.Contains(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            DoInReadLock(() => _list.CopyTo(array, arrayIndex));
        }

        public bool Remove(T item)
        {
            return DoInWriteLock(() => _list.Remove(item));
        }

        public int Count
        {
            get
            {
                return DoInReadLock(() => _list.Count);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public int IndexOf(T item)
        {
            return DoInReadLock(() => _list.IndexOf(item));
        }

        public void Insert(int index, T item)
        {
            DoInWriteLock(() => _list.Insert(index, item));
        }

        public void RemoveAt(int index)
        {
            DoInWriteLock(() => _list.RemoveAt(index));
        }

        public T this[int index]
        {
            get
            {
                return DoInReadLock(() => _list[index]);
            }
            set
            {
                DoInWriteLock(() => _list[index] = value);
            }
        }

        public class NaiveLockingEnumerator : IEnumerator<T>
        {
            private readonly IEnumerator<T> _enumerator;
            private bool _isDisposed;
            private readonly ISharedCollectionLock _readLock;

            public NaiveLockingEnumerator(IEnumerator<T> enumerator, ISharedCollectionLock readLock)
            {
                _enumerator = enumerator;
                _readLock = readLock;
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            public T Current
            {
                get
                {
                    return _enumerator.Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
                Dispose(true);

                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                        //dispose managed resrources here
                        _enumerator.Dispose();
                        _readLock.Dispose();
                    }

                    //dispose unmanaged resrources here
                    _isDisposed = true;
                }
            }

            ~NaiveLockingEnumerator()
            {
                Dispose(false);
            }
        }
    }
}
