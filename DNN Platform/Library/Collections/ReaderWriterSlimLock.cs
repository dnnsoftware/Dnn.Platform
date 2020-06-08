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
            this._lock = @lock;
        }

        #region ISharedCollectionLock Members

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        private void EnsureNotDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("ReaderWriterSlimLock");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    //free managed resources here
                }

                //free unmanaged resrources here
                if (this._lock.IsReadLockHeld)
                {
                    this._lock.ExitReadLock();
                }
                else if (this._lock.IsWriteLockHeld)
                {
                    this._lock.ExitWriteLock();
                }
                else if (this._lock.IsUpgradeableReadLockHeld)
                {
                    this._lock.ExitUpgradeableReadLock();
                }

                this._lock = null;
                this._disposed = true;
            }
        }

        ~ReaderWriterSlimLock()
        {
            this.Dispose(false);
        }
    }
}
