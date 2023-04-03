// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Threading;

    /// <summary>Represents an exclusive locking strategy.</summary>
    public class ExclusiveLockStrategy : ILockStrategy
    {
        private readonly object @lock = new object();

        private bool isDisposed;
        private Thread lockedThread;

        /// <inheritdoc/>
        public bool ThreadCanRead
        {
            get
            {
                this.EnsureNotDisposed();
                return this.IsThreadLocked();
            }
        }

        /// <inheritdoc/>
        public bool ThreadCanWrite
        {
            get
            {
                this.EnsureNotDisposed();
                return this.IsThreadLocked();
            }
        }

        /// <inheritdoc/>
        public bool SupportsConcurrentReads
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public ISharedCollectionLock GetReadLock()
        {
            return this.GetLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <inheritdoc/>
        public ISharedCollectionLock GetReadLock(TimeSpan timeout)
        {
            return this.GetLock(timeout);
        }

        /// <inheritdoc/>
        public ISharedCollectionLock GetWriteLock()
        {
            return this.GetLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <inheritdoc/>
        public ISharedCollectionLock GetWriteLock(TimeSpan timeout)
        {
            return this.GetLock(timeout);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.isDisposed = true;

            // todo remove disposable from interface?
        }

        /// <summary>Releases the exclusive lock.</summary>
        public void Exit()
        {
            this.EnsureNotDisposed();
            Monitor.Exit(this.@lock);
            this.lockedThread = null;
        }

        private ISharedCollectionLock GetLock(TimeSpan timeout)
        {
            this.EnsureNotDisposed();
            if (this.IsThreadLocked())
            {
                throw new LockRecursionException();
            }

            if (Monitor.TryEnter(this.@lock, timeout))
            {
                this.lockedThread = Thread.CurrentThread;
                return new MonitorLock(this);
            }
            else
            {
                throw new ApplicationException("ExclusiveLockStrategy.GetLock timed out");
            }
        }

        private bool IsThreadLocked()
        {
            return Thread.CurrentThread.Equals(this.lockedThread);
        }

        private void EnsureNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("ExclusiveLockStrategy");
            }
        }
    }
}
