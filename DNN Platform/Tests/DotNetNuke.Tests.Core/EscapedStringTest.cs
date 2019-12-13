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
