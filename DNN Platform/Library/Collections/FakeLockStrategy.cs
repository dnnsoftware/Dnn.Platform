using System;

namespace DotNetNuke.Collections.Internal
{
    class FakeLockStrategy : ILockStrategy
    {
        bool ILockStrategy.ThreadCanRead => true;

        bool ILockStrategy.ThreadCanWrite => true;

        bool ILockStrategy.SupportsConcurrentReads => true;

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

        #region ILockStrategy Members

        public ISharedCollectionLock GetReadLock()
        {
            return GetReadLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetReadLock(TimeSpan timeout)
        {
            return new FakeDisposable();
        }

        public ISharedCollectionLock GetWriteLock()
        {
            return GetWriteLock(TimeSpan.FromMilliseconds(-1));
        }

        public ISharedCollectionLock GetWriteLock(TimeSpan timeout)
        {
            return new FakeDisposable();
        }

        public bool ThreadCanRead
        {
            get
            {
                return true;
            }
        }

        public bool ThreadCanWrite
        {
            get
            {
                return true;
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
