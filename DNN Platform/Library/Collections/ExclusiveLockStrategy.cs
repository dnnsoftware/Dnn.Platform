#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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