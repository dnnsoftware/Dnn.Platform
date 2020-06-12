// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this.DoTest(new[] { "first", "second" }, "first,second");
        }

        [Test]
        public void CombinesWithSpecifiedSeperator()
        {
            string result = EscapedString.Combine(new[] { "first", "second"}, ';');

            Assert.AreEqual("first;second", result);
        }

        [Test]
        public void SeperatesWithSpecifiedSeperator()
        {
            IEnumerable<string> result = EscapedString.Seperate("first]second", ']');

            CollectionAssert.AreEqual(new[] { "first", "second"}, result);
        }

        [Test]
        public void EmbeddedSeperator()
        {
            this.DoTest(new[] { "fi,rst", "second" }, @"fi\,rst,second");
        }

        [Test]
        public void DoubleSeperator()
        {
            this.DoTest(new[] { "fi,,rst", "second" }, @"fi\,\,rst,second");
        }

        [Test]
        public void MultipleSeperators()
        {
            this.DoTest(new[] { "fi,rst", ",second," }, @"fi\,rst,\,second\,");
        }

        [Test]
        public void EscapeCharacter()
        {
            this.DoTest(new[] { @"fi\rst", "second" }, @"fi\\rst,second");
        }

        [Test]
        public void EmbeddedEscapeSequence()
        {
            this.DoTest(new[] { @"fi\,rst", "second" }, @"fi\\\,rst,second");
        }

        [Test]
        public void CrazyContrivedStuff()
        {
            this.DoTest(new[] { @"\\\,,fi\,rst,,\,\\", "second" }, @"\\\\\\\,\,fi\\\,rst\,\,\\\,\\\\,second");
        }

        [Test]
        public void EmptyElement()
        {
            this.DoTest(new[] { "first", "", "third" }, "first,,third");
        }

        [Test]
        public void MultipleEmptyElements()
        {
            this.DoTest(new[] { "", "", ""}, ",,");
        }

        [Test]
        public void EmptyEnumerable()
        {
            this.DoTest(new object[] { }, "");
        }

        [Test]
        public void SingleElement()
        {
            this.DoTest(new [] { "only item here"}, "only item here");
        }

        [Test]
        public void AllEscapeChars()
        {
            this.DoTest(new [] { @"\", @"\\", @"\\\"}, @"\\,\\\\,\\\\\\");
        }

        [Test]
        public void AllSeperatorChars()
        {
            this.DoTest(new [] { ",", ",,", ",,,"}, @"\,,\,\,,\,\,\,");
        }

        [Test]
        public void AllEscapedSeperators()
        {
            this.DoTest(new [] { @"\,", @"\,\,", @"\,\,\,"}, @"\\\,,\\\,\\\,,\\\,\\\,\\\,");
        }

        private void DoTest(IEnumerable enumerable, string s)
        {
            this.CombineTest(enumerable, s);
            this.SeperateTest(enumerable.Cast<String>(), s);
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
