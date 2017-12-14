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