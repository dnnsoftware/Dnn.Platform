// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Threading;

    public class ExclusiveLockStrategy : ILockStrategy
    {
        private readonly object _lock = new object();

        private bool _isDisposed;
        private Thread _lockedThread;

        public bool ThreadCanRead
        {
            get
            {
                this.EnsureNotDisposed();
                return this.IsThreadLocked();
            }
        }

        public bool ThreadCanWrite
        {
            get
            {
                this.EnsureNotDisposed();
                return this.IsThreadLocked();
            }
        }

        public bool SupportsConcurrentReads
        {
            get
            {
                return false;
            }
        }

        public ISharedCollectionLock GetReadLock()
        {
            return this.GetLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetReadLock(TimeSpan timeout)
        {
            return this.GetLock(timeout);
        }

        public ISharedCollectionLock GetWriteLock()
        {
            return this.GetLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetWriteLock(TimeSpan timeout)
        {
            return this.GetLock(timeout);
        }

        public void Dispose()
        {
            this._isDisposed = true;

            // todo remove disposable from interface?
        }

        public void Exit()
        {
            this.EnsureNotDisposed();
            Monitor.Exit(this._lock);
            this._lockedThread = null;
        }

        private ISharedCollectionLock GetLock(TimeSpan timeout)
        {
            this.EnsureNotDisposed();
            if (this.IsThreadLocked())
            {
                throw new LockRecursionException();
            }

            if (Monitor.TryEnter(this._lock, timeout))
            {
                this._lockedThread = Thread.CurrentThread;
                return new MonitorLock(this);
            }
            else
            {
                throw new ApplicationException("ExclusiveLockStrategy.GetLock timed out");
            }
        }

        private ISharedCollectionLock GetLock()
        {
            this.EnsureNotDisposed();
            if (this.IsThreadLocked())
            {
                throw new LockRecursionException();
            }

            Monitor.Enter(this._lock);
            this._lockedThread = Thread.CurrentThread;
            return new MonitorLock(this);
        }

        private bool IsThreadLocked()
        {
            return Thread.CurrentThread.Equals(this._lockedThread);
        }

        private void EnsureNotDisposed()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException("ExclusiveLockStrategy");
            }
        }
    }
}
