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

    public abstract class SharedListTests
    {
        internal abstract LockingStrategy LockingStrategy { get; }

        [Test]
        public void TryAdd()
        {
            const string value = "value";

            var sharedList = new SharedList<string>(this.LockingStrategy);

            bool doInsert = false;
            using (ISharedCollectionLock l = sharedList.GetReadLock())
            {
                if (!sharedList.Contains(value))
                {
                    doInsert = true;
                }
            }

            if (doInsert)
            {
                using (ISharedCollectionLock l = sharedList.GetWriteLock())
                {
                    if (!sharedList.Contains(value))
                    {
                        sharedList.Add(value);
                    }
                }
            }

            CollectionAssert.AreEqual(new List<string> { value }, sharedList.BackingList);
        }

        [Test]
        [ExpectedException(typeof(WriteLockRequiredException))]
        [TestCaseSource("GetWriteMethods")]
        public void WriteRequiresLock(Action<SharedList<string>> writeAction)
        {
            writeAction.Invoke(this.InitSharedList("value"));
        }

        [Test]
        [ExpectedException(typeof(ReadLockRequiredException))]
        [TestCaseSource("GetReadMethods")]
        public void ReadRequiresLock(Action<SharedList<string>> readAction)
        {
            readAction.Invoke(this.InitSharedList("value"));
        }

        [Test]
        [ExpectedException(typeof(ReadLockRequiredException))]
        public void DisposedReadLockDeniesRead()
        {
            var d = new SharedList<string>(this.LockingStrategy);

            ISharedCollectionLock l = d.GetReadLock();
            l.Dispose();

            d.Contains("foo");
        }

        [Test]
        [ExpectedException(typeof(ReadLockRequiredException))]
        public void DisposedWriteLockDeniesRead()
        {
            var d = new SharedList<string>(this.LockingStrategy);

            ISharedCollectionLock l = d.GetWriteLock();
            l.Dispose();

            d.Contains("foo");
        }

        [Test]
        [ExpectedException(typeof(WriteLockRequiredException))]
        public void DisposedWriteLockDeniesWrite()
        {
            var sharedList = new SharedList<string>(this.LockingStrategy);

            ISharedCollectionLock l = sharedList.GetWriteLock();
            l.Dispose();

            sharedList[0] = "foo";
        }

        [Test]
        public void WriteLockEnablesRead()
        {
            var sharedList = this.InitSharedList("value");

            string actualValue = null;
            using (ISharedCollectionLock l = sharedList.GetWriteLock())
            {
                actualValue = sharedList[0];
            }

            Assert.AreEqual("value", actualValue);
        }

        [Test]
        public void CanGetAnotherLockAfterDisposingLock()
        {
            var d = new SharedList<string>(this.LockingStrategy);
            ISharedCollectionLock l = d.GetReadLock();
            l.Dispose();

            l = d.GetReadLock();
            l.Dispose();
        }

        [Test]
        public void DoubleDispose()
        {
            var d = new SharedList<string>(this.LockingStrategy);

            d.Dispose();
            d.Dispose();
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        [TestCaseSource("GetObjectDisposedExceptionMethods")]
        public void MethodsThrowAfterDisposed(Action<SharedList<string>> methodCall)
        {
            var d = new SharedList<string>(this.LockingStrategy);

            d.Dispose();
            methodCall.Invoke(d);
        }

        [Test]
        [ExpectedException(typeof(LockRecursionException))]
        public void TwoDictsShareALockWriteTest()
        {
            ILockStrategy ls = new ReaderWriterLockStrategy();
            var d1 = new SharedList<string>(ls);
            var d2 = new SharedList<string>(ls);

            using (ISharedCollectionLock readLock = d1.GetReadLock())
            {
                using (ISharedCollectionLock writeLock = d2.GetWriteLock())
                {
                    // do nothing
                }
            }
        }

        protected IEnumerable<Action<SharedList<string>>> GetObjectDisposedExceptionMethods()
        {
            var list = new List<Action<SharedList<string>>> { (SharedList<string> l) => l.GetReadLock(), (SharedList<string> l) => l.GetWriteLock() };

            list.AddRange(this.GetReadMethods());
            list.AddRange(this.GetWriteMethods());

            return list;
        }

        protected IEnumerable<Action<SharedList<string>>> GetReadMethods()
        {
            var list = new List<Action<SharedList<string>>>();

            list.Add(l => l.Contains("value"));
            list.Add(l => Console.WriteLine(l.Count));
            list.Add(l => Console.WriteLine(l.IsReadOnly));
            list.Add(l => l.GetEnumerator());
            list.Add(l => l.IndexOf("value"));
            list.Add(l => Console.WriteLine(l[0]));

            list.Add(l =>
                         {
                             var arr = new string[2];
                             l.CopyTo(arr, 0);
                         });

            return list;
        }

        protected IEnumerable<Action<SharedList<string>>> GetWriteMethods()
        {
            var list = new List<Action<SharedList<string>>> { l => l.Add("more"), l => l.Clear(), l => l.Remove("value"), l => l.Insert(0, "more"), l => l[0] = "more", l => l.RemoveAt(0) };

            return list;
        }

        private SharedList<T> InitSharedList<T>(T value)
        {
            var list = new SharedList<T>(this.LockingStrategy);
            list.BackingList.Add(value);
            return list;
        }
    }
}
