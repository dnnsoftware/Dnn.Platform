// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading;

    [Serializable]
    public class ReaderWriterLockStrategy : IDisposable, ILockStrategy
    {
        [NonSerialized]
        private ReaderWriterLockSlim _lock;

        private LockRecursionPolicy _lockRecursionPolicy;

        private bool _isDisposed;

        public ReaderWriterLockStrategy()
            : this(LockRecursionPolicy.NoRecursion)
        {
        }

        public ReaderWriterLockStrategy(LockRecursionPolicy recursionPolicy)
        {
            this._lockRecursionPolicy = recursionPolicy;
            this._lock = new ReaderWriterLockSlim(recursionPolicy);
        }

        // The special constructor is used to deserialize values.
        public ReaderWriterLockStrategy(SerializationInfo info, StreamingContext context)
        {
            this._lockRecursionPolicy = (LockRecursionPolicy)info.GetValue("_lockRecursionPolicy", typeof(LockRecursionPolicy));
            this._lock = new ReaderWriterLockSlim(this._lockRecursionPolicy);
        }

        ~ReaderWriterLockStrategy()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            this.Dispose(false);
        }

        public bool ThreadCanRead
        {
            get
            {
                this.EnsureNotDisposed();
                return this.Lock.IsReadLockHeld || this.Lock.IsWriteLockHeld;
            }
        }

        public bool ThreadCanWrite
        {
            get
            {
                this.EnsureNotDisposed();
                return this.Lock.IsWriteLockHeld;
            }
        }

        public bool SupportsConcurrentReads
        {
            get
            {
                return true;
            }
        }

        private ReaderWriterLockSlim Lock
        {
            get
            {
                return this._lock ?? (this._lock = new ReaderWriterLockSlim(this._lockRecursionPolicy));
            }
        }

        // Implement this method to serialize data. The method is called
        // on serialization.
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Use the AddValue method to specify serialized values.
            info.AddValue("_lockRecursionPolicy", this._lockRecursionPolicy, typeof(LockRecursionPolicy));
        }

        public ISharedCollectionLock GetReadLock()
        {
            return this.GetReadLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetReadLock(TimeSpan timeout)
        {
            this.EnsureNotDisposed();
            if ((this.Lock.RecursionPolicy == LockRecursionPolicy.NoRecursion && this.Lock.IsReadLockHeld) ||
                this.Lock.TryEnterReadLock(timeout))
            {
                return new ReaderWriterSlimLock(this.Lock);
            }
            else
            {
                throw new ApplicationException("ReaderWriterLockStrategy.GetReadLock timed out");
            }
        }

        public ISharedCollectionLock GetWriteLock()
        {
            return this.GetWriteLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetWriteLock(TimeSpan timeout)
        {
            this.EnsureNotDisposed();
            if ((this.Lock.RecursionPolicy == LockRecursionPolicy.NoRecursion && this.Lock.IsWriteLockHeld) ||
                this.Lock.TryEnterWriteLock(timeout))
            {
                return new ReaderWriterSlimLock(this.Lock);
            }
            else
            {
                throw new ApplicationException("ReaderWriterLockStrategy.GetWriteLock timed out");
            }
        }

        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this._isDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                }

                if (this._lock != null)
                {
                    this._lock.Dispose();
                    this._lock = null;
                }
            }

            this._isDisposed = true;
        }

        private void EnsureNotDisposed()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException("ReaderWriterLockStrategy");
            }
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
    }
}
