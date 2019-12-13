// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Collections.Internal
{
    internal class MonitorLock : IDisposable, ISharedCollectionLock
    {
        private ExclusiveLockStrategy _lockStrategy;

        public MonitorLock(ExclusiveLockStrategy lockStrategy)
        {
            _lockStrategy = lockStrategy;
        }

        #region "IDisposable Support"

        // To detect redundant calls
        private bool _isDisposed;


        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _lockStrategy.Exit();
                    _lockStrategy = null;
                }
            }
            _isDisposed = true;
        }

        #endregion
    }
}
