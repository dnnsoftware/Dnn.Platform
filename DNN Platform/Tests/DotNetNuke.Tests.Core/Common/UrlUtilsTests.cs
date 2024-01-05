// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Common;

using System;
using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Services.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

[TestFixture]
public class UrlUtilsTests
{
    [OneTimeSetUp]
    public static void OneTimeSetUp()
    {
        ComponentFactory.RegisterComponent<CryptographyProvider, CoreCryptographyProvider>();

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

    [Test]
    public void DecodeParameterHandlesRoundTrip()
    {
        const string input = "DNN Platform!";
        var encodedValue = UrlUtils.EncodeParameter(input);
        var result = UrlUtils.DecodeParameter(encodedValue);
        Assert.AreEqual(input, result);
    }

    [Test]
    public void DecodeParameterHandlesSpecialCharacters()
    {
        var result = UrlUtils.DecodeParameter("RE5_O1-$");
        Assert.AreEqual("DN;_", result);
    }

    [Test]
    public void DecryptParameterHandlesRoundTrip()
    {
        const string input = "DNN Platform!";
        var key = Guid.NewGuid().ToString();
        var encodedValue = UrlUtils.EncryptParameter(input, key);
        var result = UrlUtils.DecryptParameter(encodedValue, key);
        Assert.AreEqual(input, result);
    }

    [Test]
    public void EncodeParameterReplacesPaddingSymbols()
    {
        var result = UrlUtils.EncodeParameter("D");
        Assert.AreEqual("RA$$", result);
    }

    [Test]
    public void EncryptParameterReplacesPaddingSymbols()
    {
        var result = UrlUtils.EncryptParameter("D", "key");
        Assert.IsTrue(result.EndsWith("%3d"));
    }
}
