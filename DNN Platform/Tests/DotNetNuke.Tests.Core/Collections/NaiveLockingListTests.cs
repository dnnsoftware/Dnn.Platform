﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using DotNetNuke.Collections.Internal;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Collections
{
    [TestFixture]
    public class NaiveLockingListTests
    {
        //Most of the tests here are very simple, as the real functionality is provided by List<T>
        //these tests only ensure that the methods are actually wired up
        //Complicated threading tests would be nice, but are not practical as the
        //NaiveLockList releases all locks immediately making it impossible to ensure
        //a lock is held while launching another thread in the test

        [Test]
        public void Add()
        {
            var list = new NaiveLockingList<string>();

            list.Add("sumthin");

            CollectionAssert.AreEqual(new[] {"sumthin"}, list);
        }

        [Test]
        public void Clear()
        {
            var list = new NaiveLockingList<int> {0, 1, 2, 3};
            list.Clear();

            CollectionAssert.AreEqual(new int[] {}, list);
        }

        [Test]
        public void Contains()
        {
            var list = new NaiveLockingList<int> {0, 1, 2, 3};

            Assert.IsTrue(list.Contains(2));
            Assert.IsFalse(list.Contains(9999));
        }

        [Test]
        public void CopyTo()
        {
            var list = new NaiveLockingList<int> {0, 1, 2, 3};

            var dest = new int[4];
            list.CopyTo(dest, 0);

            CollectionAssert.AreEqual(list, dest);
        }

        [Test]
        public void Remove()
        {
            var list = new NaiveLockingList<int> {0, 1, 2, 3};

            list.Remove(2);

            CollectionAssert.AreEqual(new[] {0, 1, 3}, list);
        }

        [Test]
        public void Count()
        {
            var list = new NaiveLockingList<int> {0, 1, 2, 3};

            Assert.AreEqual(4, list.Count);
        }

        [Test]
        public void IndexOf()
        {
            var list = new NaiveLockingList<int> {0, 1, 2, 3};

            Assert.AreEqual(2, list.IndexOf(2));
        }

        [Test]
        public void Insert()
        {
            var list = new NaiveLockingList<int> {0, 1, 2, 3};

            list.Insert(2, 999);

            CollectionAssert.AreEqual(new[] {0, 1, 999, 2, 3}, list);
        }

        [Test]
        public void RemoveAt()
        {
            var list = new NaiveLockingList<int> {0, 1, 999, 2, 3};

            list.RemoveAt(2);

            CollectionAssert.AreEqual(new[] {0, 1, 2, 3}, list);
        }

        [Test]
        public void IndexOperator()
        {
            var list = new NaiveLockingList<int> {0, 1, 2, 3};

            list[2] = 999;

            Assert.AreEqual(999, list[2]);
        }

        [Test, ExpectedException(typeof (LockRecursionException))]
        public void NoWritesWhileEnumerating()
        {
            var list = new NaiveLockingList<int> {0, 1, 2, 3};

            using (list.GetEnumerator())
            {
                list.Add(4);
            }
        }

        [Test]
        public void CanWriteAfterEnumerating()
        {
            var list = new NaiveLockingList<int> {0, 1, 2, 3};

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
