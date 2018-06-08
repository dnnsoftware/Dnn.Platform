using DotNetNuke.Collections.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNuke.Collections.Internal
{
    class FakeLockStrategy : ILockStrategy
    {
        bool ILockStrategy.ThreadCanRead => throw new NotImplementedException();

        bool ILockStrategy.ThreadCanWrite => throw new NotImplementedException();

        bool ILockStrategy.SupportsConcurrentReads => throw new NotImplementedException();

        ISharedCollectionLock ILockStrategy.GetReadLock()
        {
            return new FakeDisposable();
        }

        ISharedCollectionLock ILockStrategy.GetReadLock(TimeSpan timeout)
        {
            return new FakeDisposable();
        }

        ISharedCollectionLock ILockStrategy.GetWriteLock()
        {
            return new FakeDisposable();
        }

        ISharedCollectionLock ILockStrategy.GetWriteLock(TimeSpan timeout)
        {
            return new FakeDisposable();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        ~FakeLockStrategy() {
            Dispose(false);
        }
        
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
