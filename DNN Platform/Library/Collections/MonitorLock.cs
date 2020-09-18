// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;

    internal class MonitorLock : IDisposable, ISharedCollectionLock
    {
        private ExclusiveLockStrategy _lockStrategy;

        // To detect redundant calls
        private bool _isDisposed;

        public MonitorLock(ExclusiveLockStrategy lockStrategy)
        {
            this._lockStrategy = lockStrategy;
        }

        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._isDisposed)
            {
                if (disposing)
                {
                    this._lockStrategy.Exit();
                    this._lockStrategy = null;
                }
            }

            this._isDisposed = true;
        }
    }
}
