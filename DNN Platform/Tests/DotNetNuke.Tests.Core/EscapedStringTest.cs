// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class EscapedStringTest
    {
        [Test]
        public void SimpleCase()
        {
            DoTest(new[] { "first", "second" }, "first,second");
        }

        [Test]
        public void CombinesWithSpecifiedSeparator()
        {
            string result = EscapedString.Combine(new[] { "first", "second" }, ';');

            Assert.That(result, Is.EqualTo("first;second"));
        }

        [Test]
        public void SeparatesWithSpecifiedSeparator()
        {
            IEnumerable<string> result = EscapedString.Seperate("first]second", ']');

            Assert.That(result, Is.EqualTo(new[] { "first", "second" }).AsCollection);
        }

        [Test]
        public void EmbeddedSeparator()
        {
            DoTest(new[] { "fi,rst", "second" }, @"fi\,rst,second");
        }

        [Test]
        public void DoubleSeparator()
        {
            DoTest(new[] { "fi,,rst", "second" }, @"fi\,\,rst,second");
        }

        [Test]
        public void MultipleSeparators()
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
            DoTest(new[] { "first", string.Empty, "third" }, "first,,third");
        }

        [Test]
        public void MultipleEmptyElements()
        {
            DoTest(new[] { string.Empty, string.Empty, string.Empty }, ",,");
        }

        [Test]
        public void EmptyEnumerable()
        {
            DoTest(new object[] { }, string.Empty);
        }

        [Test]
        public void SingleElement()
        {
            DoTest(new[] { "only item here" }, "only item here");
        }

        [Test]
        public void AllEscapeChars()
        {
            DoTest(new[] { @"\", @"\\", @"\\\" }, @"\\,\\\\,\\\\\\");
        }

        [Test]
        public void AllSeparatorChars()
        {
            DoTest(new[] { ",", ",,", ",,," }, @"\,,\,\,,\,\,\,");
        }

        [Test]
        public void AllEscapedSeparators()
        {
            DoTest(new[] { @"\,", @"\,\,", @"\,\,\," }, @"\\\,,\\\,\\\,,\\\,\\\,\\\,");
        }

        [Test]
        public void TrimWhitespaces()
        {
            SeparateTest(
                ["item1", "ite\nm2", "item3", "item4", "item5", "item6", "item7"], "item1,ite\nm2,\nitem3, item4,item5\t,\r\nitem6,\vitem7", true);
        }

        [Test]
        public void KeepWhitespaces()
        {
            SeparateTest(
                ["item1", "ite\nm2", "\nitem3", " item4", "item5\t", "\r\nitem6", "\vitem7"], "item1,ite\nm2,\nitem3, item4,item5\t,\r\nitem6,\vitem7", false);
        }

        private static void DoTest(IEnumerable enumerable, string s)
        {
            CombineTest(enumerable, s);
            SeparateTest(enumerable.Cast<string>(), s);
        }

        private static void SeparateTest(IEnumerable<string> expected, string data, bool trimWhitespaces = false)
        {
            var result = EscapedString.Seperate(data, trimWhitespaces);

            Assert.That(result, Is.EqualTo(expected).AsCollection);
        }

        private static void CombineTest(IEnumerable data, string expected)
        {
            string result = EscapedString.Combine(data);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
