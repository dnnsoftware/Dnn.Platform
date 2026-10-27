// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Common;

using System;

using DotNetNuke.Common.Utilities;

using NUnit.Framework;

[TestFixture]
internal class AssemblyVersionsTests
{
    [TestCase("1.0", "1.0.0")]
    [TestCase("1.2.3", "1.2.3")]
    [TestCase("1.2.3.4", "1.2.3")]
    public void ToThreePartString_PadsMissingBuildComponent(string input, string expected)
    {
        var result = new Version(input).ToThreePartString();

        Assert.That(result, Is.EqualTo(expected));
    }
}
