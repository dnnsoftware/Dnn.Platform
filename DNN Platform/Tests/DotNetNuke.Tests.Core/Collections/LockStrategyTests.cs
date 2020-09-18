// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using DotNetNuke.Collections.Internal;
    using NUnit.Framework;

    public abstract class LockStrategyTests
    {
        [Test]
        public void DoubleDisposeAllowed()
        {
            var strategy = this.GetLockStrategy();

            strategy.Dispose();
            strategy.Dispose();

            // no exception on 2nd dispose
        }

        [Test]
        [ExpectedException(typeof(LockRecursionException))]
        public virtual void DoubleReadLockThrows()
        {
            using (var strategy = this.GetLockStrategy())
            {
                using (var readLock1 = strategy.GetReadLock())
                {
                    using (var readLock2 = strategy.GetReadLock())
                    {
                        // do nothing
                    }
                }
            }
        }

        [Test]
        [ExpectedException(typeof(LockRecursionException))]
        public void ReadAndWriteLockOnSameThreadThrows()
        {
            using (var strategy = this.GetLockStrategy())
            {
                using (var readLock1 = strategy.GetReadLock())
                {
                    using (var readLock2 = strategy.GetWriteLock())
                    {
                        // do nothing
                    }
                }
            }
        }

        [Test]
        [ExpectedException(typeof(LockRecursionException))]
        public void WriteAndReadLockOnSameThreadThrows()
        {
            using (var strategy = this.GetLockStrategy())
            {
                using (var readLock1 = strategy.GetWriteLock())
                {
                    using (var readLock2 = strategy.GetReadLock())
                    {
                        // do nothing
                    }
                }
            }
        }

        [Test]
        public void DoubleReadLockOnDifferentThreads()
        {
            using (var strategy = this.GetLockStrategy())
            {
                if (strategy.SupportsConcurrentReads)
                {
                    using (var readLock1 = strategy.GetReadLock())
                    {
                        var t = new Thread(GetReadLock);
                        t.Start(strategy);

                        // sleep and let new thread run
                        t.Join(TimeSpan.FromMilliseconds(100));

                        // assert that read thread has terminated
                        Console.WriteLine(t.ThreadState.ToString());
                        Assert.IsTrue(t.ThreadState == ThreadState.Stopped);
                    }
                }
            }
        }

        [Test]
        public void DoubleWriteLockOnDifferentThreadsWaits()
        {
            using (ILockStrategy strategy = this.GetLockStrategy())
            {
                Thread t;
                using (var writeLock1 = strategy.GetWriteLock())
                {
                    t = new Thread(GetWriteLock);
                    t.Start(strategy);

                    // sleep and let new thread run and block
                    Thread.Sleep(50);

                    // assert that write thread has not terminated
                    Assert.IsTrue(t.IsAlive);
                } // release write lock

                Thread.Sleep(50);

                // assert that getwritelock did complete once first writelock was released it's call
                Assert.IsFalse(t.IsAlive);
            }
        }

        [Test]
        [ExpectedException(typeof(LockRecursionException))]
        public virtual void DoubleWriteLockThrows()
        {
            using (ILockStrategy strategy = this.GetLockStrategy())
            {
                using (var writeLock1 = strategy.GetWriteLock())
                {
                    using (var writeLock2 = strategy.GetWriteLock())
                    {
                        // do nothing
                    }
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        [TestCaseSource("GetObjectDisposedExceptionMethods")]
        public void MethodsThrowAfterDisposed(Action<ILockStrategy> methodCall)
        {
            var strategy = this.GetLockStrategy();

            strategy.Dispose();
            methodCall.Invoke(strategy);
        }

        [Test]
        public void ReadLockPreventsWriteLock()
        {
            Thread t = null;

            using (var strategy = this.GetLockStrategy())
            {
                using (var readLock = strategy.GetReadLock())
                {
                    t = new Thread(GetWriteLock);
                    t.Start(strategy);

                    // sleep and let new thread run
                    Thread.Sleep(100);

                    // assert that write thread is still waiting
                    Assert.IsTrue(t.IsAlive);
                }

                // release read lock

                // sleep and let write thread finish up
                Thread.Sleep(100);
                Assert.IsFalse(t.IsAlive);
            }

            // release controller
        }

        [Test]
        public void OnlyOneWriteLockAllowed()
        {
            Thread t = null;

            using (var strategy = this.GetLockStrategy())
            {
                using (var writeLock = strategy.GetWriteLock())
                {
                    t = new Thread(GetWriteLock);
                    t.Start(strategy);

                    // sleep and let new thread run
                    Thread.Sleep(100);

                    // assert that write thread is still waiting
                    Assert.IsTrue(t.IsAlive);
                }

                // release write lock
                // sleep and let write thread finish up
                Thread.Sleep(100);
                Assert.IsFalse(t.IsAlive);
            }
        }

        [Test]
        public void MultipleReadLocksAllowed()
        {
            Thread t = null;

            using (var strategy = this.GetLockStrategy())
            {
                if (strategy.SupportsConcurrentReads)
                {
                    using (var readLock = strategy.GetReadLock())
                    {
                        t = new Thread(GetReadLock);
                        t.Start(strategy);

                        // sleep and let new thread run
                        Thread.Sleep(100);

                        // assert that read thread has terminated
                        Assert.IsFalse(t.IsAlive);
                    }
                }
                else
                {
                    Assert.IsTrue(true);
                }
            }
        }

        internal abstract ILockStrategy GetLockStrategy();

        protected virtual IEnumerable<Action<ILockStrategy>> GetObjectDisposedExceptionMethods()
        {
            var l = new List<Action<ILockStrategy>>();

            l.Add((ILockStrategy strategy) => strategy.GetReadLock());
            l.Add((ILockStrategy strategy) => strategy.GetWriteLock());
            l.Add((ILockStrategy strategy) => Console.WriteLine(strategy.ThreadCanRead));
            l.Add((ILockStrategy strategy) => Console.WriteLine(strategy.ThreadCanWrite));

            return l;
        }

        private static void GetReadLock(object obj)
        {
            var strategy = (ILockStrategy)obj;
            using (var readLock = strategy.GetReadLock())
            {
                // do nothing
            }
        }

        private static void GetWriteLock(object obj)
        {
            var strategy = (ILockStrategy)obj;
            using (var writeLock = strategy.GetWriteLock())
            {
                // do nothing
            }
        }
    }
}
