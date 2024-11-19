// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Common;

using System;

using DotNetNuke.Common;

using NUnit.Framework;

[TestFixture]
public class GlobalsTests
{
    [Test]
    public void FormatVersion_WhenTwoDigitVersionFormattedWithThreeParts_UsesZeroForThirdPart()
    {
        Assert.That(Globals.FormatVersion(new Version("4.8"), "0", 3, "."), Is.EqualTo("4.8.0"));
    }

    [Test]
    public void FormatVersion_WhenThreeDigitVersionFormattedWithThreeParts_DisplaysThirdPart()
    {
        Assert.That(Globals.FormatVersion(new Version("4.8.1"), "0", 3, "."), Is.EqualTo("4.8.1"));
    }

    [Test]
    public void FormatVersion_WhenFourDigitVersionFormattedWithThreeParts_DoesNotDisplayFourthPart()
    {
        Assert.That(Globals.FormatVersion(new Version("4.8.1.7"), "0", 3, "."), Is.EqualTo("4.8.1"));
    }

    [Test]
    public void FormatVersion_WhenTwoDigitVersion_DisplaysThreePartsWithLeadingZeroes_InABrokenWay()
    {
        Assert.That(Globals.FormatVersion(new Version("4.8")), Is.EqualTo("04.08.-01"));
    }

    [Test]
    public void FormatVersion_WhenThreeDigitVersion_DisplaysThirdPartWithLeadingZeroes()
    {
        Assert.That(Globals.FormatVersion(new Version("4.8.1")), Is.EqualTo("04.08.01"));
    }

    [Test]
    public void FormatVersion_WhenFourDigitVersion_DisplaysThreePartsWithLeadingZeroes()
    {
        Assert.That(Globals.FormatVersion(new Version("4.8.1.7")), Is.EqualTo("04.08.01"));
    }

    [Test]
    public void FormatVersion_WhenFourDigitVersionWithRevision_DisplaysThreePartsWithLeadingZeroes()
    {
        Assert.That(Globals.FormatVersion(new Version("4.8.1.7"), true), Is.EqualTo("04.08.01 (7)"));
    }
}
