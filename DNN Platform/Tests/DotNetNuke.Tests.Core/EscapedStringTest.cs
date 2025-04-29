// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core;

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
        this.DoTest(new[] { "first", "second" }, "first,second");
    }

    [Test]
    public void CombinesWithSpecifiedSeperator()
    {
        string result = EscapedString.Combine(new[] { "first", "second" }, ';');

        Assert.That(result, Is.EqualTo("first;second"));
    }

    [Test]
    public void SeperatesWithSpecifiedSeperator()
    {
        IEnumerable<string> result = EscapedString.Seperate("first]second", ']');

        Assert.That(result, Is.EqualTo(new[] { "first", "second" }).AsCollection);
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
        this.DoTest(new[] { "first", string.Empty, "third" }, "first,,third");
    }

    [Test]
    public void MultipleEmptyElements()
    {
        this.DoTest(new[] { string.Empty, string.Empty, string.Empty }, ",,");
    }

    [Test]
    public void EmptyEnumerable()
    {
        this.DoTest(new object[] { }, string.Empty);
    }

    [Test]
    public void SingleElement()
    {
        this.DoTest(new[] { "only item here" }, "only item here");
    }

    [Test]
    public void AllEscapeChars()
    {
        this.DoTest(new[] { @"\", @"\\", @"\\\" }, @"\\,\\\\,\\\\\\");
    }

    [Test]
    public void AllSeperatorChars()
    {
        this.DoTest(new[] { ",", ",,", ",,," }, @"\,,\,\,,\,\,\,");
    }

    [Test]
    public void AllEscapedSeperators()
    {
        this.DoTest(new[] { @"\,", @"\,\,", @"\,\,\," }, @"\\\,,\\\,\\\,,\\\,\\\,\\\,");
    }

    [Test]
    public void TrimWhitespaces()
    {
        this.SeperateTest(
            new[] { "item1", "ite\nm2", "item3", "item4", "item5", "item6", "item7" }, "item1,ite\nm2,\nitem3, item4,item5\t,\r\nitem6,\vitem7", true);
    }

    [Test]
    public void KeepWhitespaces()
    {
        this.SeperateTest(
            new[] { "item1", "ite\nm2", "\nitem3", " item4", "item5\t", "\r\nitem6", "\vitem7" }, "item1,ite\nm2,\nitem3, item4,item5\t,\r\nitem6,\vitem7", false);
    }

    private void DoTest(IEnumerable enumerable, string s)
    {
        this.CombineTest(enumerable, s);
        this.SeperateTest(enumerable.Cast<string>(), s);
    }

    private void SeperateTest(IEnumerable<string> expected, string data, bool trimWhitespaces = false)
    {
        var result = EscapedString.Seperate(data, trimWhitespaces);

        Assert.That(result, Is.EqualTo(expected).AsCollection);
    }

    private void CombineTest(IEnumerable data, string expected)
    {
        string result = EscapedString.Combine(data);

        Assert.That(result, Is.EqualTo(expected));
    }
}
