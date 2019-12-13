﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Runtime.Serialization;
using System.Threading;

namespace DotNetNuke.Collections.Internal
{
    [Serializable]
    public class ReaderWriterLockStrategy : IDisposable, ILockStrategy
    {
        [NonSerialized]
        private ReaderWriterLockSlim _lock;

        private LockRecursionPolicy _lockRecursionPolicy;

        private ReaderWriterLockSlim Lock
        {
            get
            {
                return _lock ?? (_lock = new ReaderWriterLockSlim(_lockRecursionPolicy));
            }
        }

        // Implement this method to serialize data. The method is called 
        // on serialization.
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Use the AddValue method to specify serialized values.
            info.AddValue("_lockRecursionPolicy", _lockRecursionPolicy, typeof(LockRecursionPolicy));
        }

        public ReaderWriterLockStrategy()
            : this(LockRecursionPolicy.NoRecursion)
        {
        }

        public ReaderWriterLockStrategy(LockRecursionPolicy recursionPolicy)
        {
            _lockRecursionPolicy = recursionPolicy;
            _lock = new ReaderWriterLockSlim(recursionPolicy);
        }

        // The special constructor is used to deserialize values.
        public ReaderWriterLockStrategy(SerializationInfo info, StreamingContext context)
        {
            _lockRecursionPolicy = (LockRecursionPolicy)info.GetValue("_lockRecursionPolicy", typeof(LockRecursionPolicy));
            _lock = new ReaderWriterLockSlim(_lockRecursionPolicy);
        }

        #region ILockStrategy Members

        public ISharedCollectionLock GetReadLock()
        {
            return GetReadLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetReadLock(TimeSpan timeout)
        {
            EnsureNotDisposed();
            if (Lock.RecursionPolicy == LockRecursionPolicy.NoRecursion && Lock.IsReadLockHeld || 
                Lock.TryEnterReadLock(timeout))
            {
                return new ReaderWriterSlimLock(Lock);
            }
            else
            {
                throw new ApplicationException("ReaderWriterLockStrategy.GetReadLock timed out");
            }
        }

        public ISharedCollectionLock GetWriteLock()
        {
            return GetWriteLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetWriteLock(TimeSpan timeout)
        {
            EnsureNotDisposed();
            if (Lock.RecursionPolicy == LockRecursionPolicy.NoRecursion && Lock.IsWriteLockHeld || 
                Lock.TryEnterWriteLock(timeout))
            {
                return new ReaderWriterSlimLock(Lock);
            }
            else
            {
                throw new ApplicationException("ReaderWriterLockStrategy.GetWriteLock timed out");
            }
        }

        public bool ThreadCanRead
        {
            get
            {
                EnsureNotDisposed();
                return Lock.IsReadLockHeld || Lock.IsWriteLockHeld;
            }
        }

        public bool ThreadCanWrite
        {
            get
            {
                EnsureNotDisposed();
                return Lock.IsWriteLockHeld;
            }
        }

        public bool SupportsConcurrentReads
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region "IDisposable Support"

        private bool _isDisposed;

        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("ReaderWriterLockStrategy");
            }
        }

        // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    //dispose managed state (managed objects).
                }

                if (_lock != null)
                {
                    _lock.Dispose();
                    _lock = null;
                }
            }
            _isDisposed = true;
        }

        ~ReaderWriterLockStrategy()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(false);
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.

        #endregion
    }
}
