// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Threading;

#endregion

namespace DotNetNuke.Collections.Internal
{
    internal class ReaderWriterSlimLock : ISharedCollectionLock
    {
        private bool _disposed;
        private ReaderWriterLockSlim _lock;

        public ReaderWriterSlimLock(ReaderWriterLockSlim @lock)
        {
            _lock = @lock;
        }

        #region ISharedCollectionLock Members

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ReaderWriterSlimLock");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //free managed resources here
                }

                //free unmanaged resrources here
                if (_lock.IsReadLockHeld)
                {
                    _lock.ExitReadLock();
                }
                else if (_lock.IsWriteLockHeld)
                {
                    _lock.ExitWriteLock();
                }
                else if (_lock.IsUpgradeableReadLockHeld)
                {
                    _lock.ExitUpgradeableReadLock();
                }

                _lock = null;
                _disposed = true;
            }
        }

        ~ReaderWriterSlimLock()
        {
            Dispose(false);
        }
    }
}
