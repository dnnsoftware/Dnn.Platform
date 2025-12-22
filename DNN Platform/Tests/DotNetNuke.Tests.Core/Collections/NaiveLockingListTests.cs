// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using DotNetNuke.Collections.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class NaiveLockingListTests
    {
        // Most of the tests here are very simple, as the real functionality is provided by List<T>
        // these tests only ensure that the methods are actually wired up
        // Complicated threading tests would be nice, but are not practical as the
        // NaiveLockList releases all locks immediately making it impossible to ensure
        // a lock is held while launching another thread in the test
        [Test]
        public void Add()
        {
            var list = new NaiveLockingList<string>();

            list.Add("sumthin");

            Assert.That(list, Is.EqualTo(new[] { "sumthin" }).AsCollection);
        }

        [Test]
        public void Clear()
        {
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };
            list.Clear();

            Assert.That(list, Is.EqualTo(new int[] { }).AsCollection);
        }

        [Test]
        public void Contains()
        {
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            Assert.Multiple(() =>
            {
                Assert.That(list, Has.Some.EqualTo(2));
                Assert.That(list, Has.None.EqualTo(9999));
            });
        }

        [Test]
        public void CopyTo()
        {
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            var dest = new int[4];
            list.CopyTo(dest, 0);

            Assert.That(dest, Is.EqualTo(list).AsCollection);
        }

        [Test]
        public void Remove()
        {
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            list.Remove(2);

            Assert.That(list, Is.EqualTo(new[] { 0, 1, 3 }).AsCollection);
        }

        [Test]
        public void Count()
        {
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            Assert.That(list, Has.Count.EqualTo(4));
        }

        [Test]
        public void IndexOf()
        {
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            Assert.That(list.IndexOf(2), Is.EqualTo(2));
        }

        [Test]
        public void Insert()
        {
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            list.Insert(2, 999);

            Assert.That(list, Is.EqualTo(new[] { 0, 1, 999, 2, 3 }).AsCollection);
        }

        [Test]
        public void RemoveAt()
        {
            var list = new NaiveLockingList<int> { 0, 1, 999, 2, 3 };

            list.RemoveAt(2);

            Assert.That(list, Is.EqualTo(new[] { 0, 1, 2, 3 }).AsCollection);
        }

        [Test]
        public void IndexOperator()
        {
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            list[2] = 999;

            Assert.That(list[2], Is.EqualTo(999));
        }

        [Test]
        public void NoWritesWhileEnumerating()
        {
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            using (list.GetEnumerator())
            {
                Assert.Throws<LockRecursionException>(() => list.Add(4));
            }
        }

        [Test]
        public void CanWriteAfterEnumerating()
        {
            var list = new NaiveLockingList<int> { 0, 1, 2, 3 };

            using (IEnumerator<int> e = list.GetEnumerator())
            {
                e.MoveNext();
                Debug.WriteLine(e.Current);
                e.MoveNext();
            }

            list.Add(4);
        }
    }
}
