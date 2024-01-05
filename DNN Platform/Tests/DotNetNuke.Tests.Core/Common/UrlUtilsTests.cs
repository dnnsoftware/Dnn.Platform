// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Common;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

[TestFixture]
public class UrlUtilsTests
{
    [OneTimeSetUp]
    public static void OneTimeSetUp()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient(container => Mock.Of<IApplicationStatusInfo>());
        serviceCollection.AddTransient(container => Mock.Of<INavigationManager>());
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public void CombineEmptyBase()
    {
        var result = UrlUtils.Combine(string.Empty, "a/b/c");
        Assert.AreEqual("a/b/c", result);
    }

    [Test]
    public void CombineEmptyRelative()
    {
        var result = UrlUtils.Combine("/a/b/c", string.Empty);
        Assert.AreEqual("/a/b/c", result);
    }

    [Test]
    public void CombineRelativeWithBaseTrimsSlashes()
    {
        var result = UrlUtils.Combine("/a/b/c/", "/d/e/f/");
        Assert.AreEqual("/a/b/c/d/e/f/", result);
    }
}
