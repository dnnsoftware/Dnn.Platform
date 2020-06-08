// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Threading;

#endregion

namespace DotNetNuke.Collections.Internal
{
    public class ExclusiveLockStrategy : ILockStrategy
    {
        private readonly object _lock = new object();

        private bool _isDisposed;
        private Thread _lockedThread;

        #region ILockStrategy Members

        public ISharedCollectionLock GetReadLock()
        {
            return GetLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetReadLock(TimeSpan timeout)
        {
            return GetLock(timeout);
        }

        public ISharedCollectionLock GetWriteLock()
        {
            return GetLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetWriteLock(TimeSpan timeout)
        {
            return GetLock(timeout);
        }

        public bool ThreadCanRead
        {
            get
            {
                EnsureNotDisposed();
                return IsThreadLocked();
            }
        }

        public bool ThreadCanWrite
        {
            get
            {
                EnsureNotDisposed();
                return IsThreadLocked();
            }
        }

        public bool SupportsConcurrentReads
        {
            get
            {
                return false;
            }
        }

        public void Dispose()
        {
            _isDisposed = true;
            //todo remove disposable from interface?
        }

        #endregion

        private ISharedCollectionLock GetLock(TimeSpan timeout)
        {
            EnsureNotDisposed();
            if (IsThreadLocked())
            {
                throw new LockRecursionException();
            }

            if (Monitor.TryEnter(_lock, timeout))
            {
                _lockedThread = Thread.CurrentThread;
                return new MonitorLock(this);
            }
            else
            {
                throw new ApplicationException("ExclusiveLockStrategy.GetLock timed out");
            }
        }

        private ISharedCollectionLock GetLock()
        {
            EnsureNotDisposed();
            if (IsThreadLocked())
            {
                throw new LockRecursionException();
            }

            Monitor.Enter(_lock);
            _lockedThread = Thread.CurrentThread;
            return new MonitorLock(this);
        }

        private bool IsThreadLocked()
        {
            return Thread.CurrentThread.Equals(_lockedThread);
        }

        public void Exit()
        {
            EnsureNotDisposed();
            Monitor.Exit(_lock);
            _lockedThread = null;
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("ExclusiveLockStrategy");
            }
        }
    }
}
