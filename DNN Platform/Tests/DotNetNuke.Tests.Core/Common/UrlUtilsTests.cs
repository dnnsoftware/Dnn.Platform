// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Common;

using System;
using System.Collections.Generic;
using System.Web.Caching;
using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Settings;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Services.Cryptography;
using DotNetNuke.Tests.Utilities.Fakes;
using DotNetNuke.Tests.Utilities.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

[TestFixture]
public class UrlUtilsTests
{
    private static readonly Dictionary<string, IConfigurationSetting> HostSettings = new Dictionary<string, IConfigurationSetting>();

    [OneTimeSetUp]
    public static void OneTimeSetUp()
    {
        ComponentFactory.RegisterComponent<CryptographyProvider, CoreCryptographyProvider>();
        MockComponentProvider.CreateDataCacheProvider();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(Mock.Of<IApplicationStatusInfo>());
        serviceCollection.AddSingleton(Mock.Of<INavigationManager>());
        serviceCollection.AddSingleton<IHostSettingsService>(new FakeHostController(HostSettings));
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

    [Test]
    public void GetParameterNameReturnsName()
    {
        var result = UrlUtils.GetParameterName("key=value");
        Assert.AreEqual("key", result);
    }

    [Test]
    public void GetParameterNameReturnsEntireStringIfNoEqualsSign()
    {
        var result = UrlUtils.GetParameterName("just-a-key");
        Assert.AreEqual("just-a-key", result);
    }

    [Test]
    public void GetParameterNameReturnsEmptyIfStartsWithEqualsSign()
    {
        var result = UrlUtils.GetParameterName("=just-a-value");
        Assert.AreEqual(string.Empty, result);
    }

    [Test]
    public void GetParameterValueReturnsName()
    {
        var result = UrlUtils.GetParameterValue("key=value");
        Assert.AreEqual("value", result);
    }

    [Test]
    public void GetParameterValueReturnsEmptyStringIfNoEqualsSign()
    {
        var result = UrlUtils.GetParameterValue("just-a-key");
        Assert.AreEqual(string.Empty, result);
    }

    [Test]
    public void GetParameterValueReturnsEntireStringIfStartsWithEqualsSign()
    {
        var result = UrlUtils.GetParameterValue("=just-a-value");
        Assert.AreEqual("just-a-value", result);
    }

    [Test]
    public void ClosePopUpGeneratesAJavaScriptUrlWithValues()
    {
        var result = UrlUtils.ClosePopUp(false, "/hello",  false);
        Assert.AreEqual("""javascript:dnnModal.closePopUp(false, "/hello")""", result);

        result = UrlUtils.ClosePopUp(true, "blah", false);
        Assert.AreEqual("""javascript:dnnModal.closePopUp(true, "blah")""", result);
    }

    [Test]
    public void ClosePopUpGeneratesAScriptWhenOnClickEventIsTrue()
    {
        var result = UrlUtils.ClosePopUp(false, "/somewhere",  true);
        Assert.AreEqual("""dnnModal.closePopUp(false, "/somewhere")""", result);
    }

    [Test]
    public void ClosePopUpEncodesUrlParameter()
    {
        var result = UrlUtils.ClosePopUp(false, "/somewhere?value=%20hi&two='hey'",  true);
        Assert.AreEqual("""dnnModal.closePopUp(false, "/somewhere?value=%20hi\u0026two=\u0027hey\u0027")""", result);
    }

    [Test]
    public void ReplaceQSParamReplacesUnfriendlyParam()
    {
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.ReplaceQSParam("/somewhere?value=hi&two=hey", "two", "what");
        Assert.AreEqual("/somewhere?value=hi&two=what", result);
    }

    [Test]
    public void ReplaceQSParamReplacesFriendlyParam()
    {
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "true", };

        var result = UrlUtils.ReplaceQSParam("/somewhere/value/hi/two/hey/", "two", "what");
        Assert.AreEqual("/somewhere/value/hi/two/what/", result);
    }

    [Test]
    public void ReplaceQSParamHandlesSpecialCharacters()
    {
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.ReplaceQSParam("/somewhere?one.two=three$four&one_two=123", "one.two", "four$3");
        Assert.AreEqual("/somewhere?one.two=four$3&one_two=123", result);
    }

    [Test]
    public void StripQSParamRemovesUnfriendlyParam()
    {
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.StripQSParam("/somewhere?value=hi&two=hey&three=x", "two");
        Assert.AreEqual("/somewhere?value=hi&three=x", result);
    }

    [Test]
    public void StripQSParamRemovesFriendlyParam()
    {
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "true", };

        var result = UrlUtils.StripQSParam("/somewhere/value/hi/two/hey/", "two");
        Assert.AreEqual("/somewhere/value/hi/", result);
    }

    [Test]
    public void StripQSParamHandlesSpecialCharacters()
    {
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.StripQSParam("/somewhere?one.two=three$four&one_two=123", "one.two");
        Assert.AreEqual("/somewhere?one_two=123", result);
    }
}
