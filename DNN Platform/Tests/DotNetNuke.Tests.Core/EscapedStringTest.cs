#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common.Utilities;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core
{
    [TestFixture]
    public class EscapedStringTest
    {
        [Test]
        public void SimpleCase()
        {
            DoTest(new[] { "first", "second" }, "first,second");
        }

        [Test]
        public void CombinesWithSpecifiedSeperator()
        {
            string result = EscapedString.Combine(new[] {"first", "second"}, ';');

            Assert.AreEqual("first;second", result);
        }

        [Test]
        public void SeperatesWithSpecifiedSeperator()
        {
            IEnumerable<string> result = EscapedString.Seperate("first]second", ']');

            CollectionAssert.AreEqual(new[] {"first", "second"}, result);
        }

        [Test]
        public void EmbeddedSeperator()
        {
            DoTest(new[] { "fi,rst", "second" }, @"fi\,rst,second");
        }

        [Test]
        public void DoubleSeperator()
        {
            DoTest(new[] { "fi,,rst", "second" }, @"fi\,\,rst,second");
        }

        [Test]
        public void MultipleSeperators()
        {
            DoTest(new[] { "fi,rst", ",second," }, @"fi\,rst,\,second\,");
        }

        [Test]
        public void EscapeCharacter()
        {
            DoTest(new[] { @"fi\rst", "second" }, @"fi\\rst,second");
        }

        [Test]
        public void EmbeddedEscapeSequence()
        {
            DoTest(new[] { @"fi\,rst", "second" }, @"fi\\\,rst,second");
        }

        [Test]
        public void CrazyContrivedStuff()
        {
            DoTest(new[] { @"\\\,,fi\,rst,,\,\\", "second" }, @"\\\\\\\,\,fi\\\,rst\,\,\\\,\\\\,second");
        }

        [Test]
        public void EmptyElement()
        {
            DoTest(new[] { "first", "", "third" }, "first,,third");
        }

        [Test]
        public void MultipleEmptyElements()
        {
            DoTest(new[] {"", "", ""}, ",,");
        }

        [Test]
        public void EmptyEnumerable()
        {
            DoTest(new object[] {}, "");
        }

        [Test]
        public void SingleElement()
        {
            DoTest(new [] {"only item here"}, "only item here");
        }

        [Test]
        public void AllEscapeChars()
        {
            DoTest(new [] {@"\", @"\\", @"\\\"}, @"\\,\\\\,\\\\\\");
        }

        [Test]
        public void AllSeperatorChars()
        {
            DoTest(new [] {",", ",,", ",,,"}, @"\,,\,\,,\,\,\,");
        }

        [Test]
        public void AllEscapedSeperators()
        {
            DoTest(new [] {@"\,", @"\,\,", @"\,\,\,"}, @"\\\,,\\\,\\\,,\\\,\\\,\\\,");
        }

        private void DoTest(IEnumerable enumerable, string s)
        {
            CombineTest(enumerable, s);
            SeperateTest(enumerable.Cast<String>(), s);
        }

        private void SeperateTest(IEnumerable<string> expected, string data)
        {
            var result = EscapedString.Seperate(data);

            CollectionAssert.AreEqual(expected, result);
        }

        private void CombineTest(IEnumerable data, string expected)
        {
            string result = EscapedString.Combine(data);

            Assert.AreEqual(expected, result);
        }
    }
}