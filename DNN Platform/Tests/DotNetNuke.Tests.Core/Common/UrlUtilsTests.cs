// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Common;

using System;
using System.Collections.Generic;
using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Settings;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
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
        var hostController = new FakeHostController(HostSettings);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(Mock.Of<IApplicationStatusInfo>());
        serviceCollection.AddSingleton(Mock.Of<INavigationManager>());
        serviceCollection.AddSingleton(Mock.Of<IPortalSettingsController>());
        serviceCollection.AddSingleton<IHostSettingsService>(hostController);
        serviceCollection.AddSingleton<IHostSettings>(new HostSettings(hostController));

        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public void CombineEmptyBase()
    {
        var result = UrlUtils.Combine(string.Empty, "a/b/c");
        Assert.That(result, Is.EqualTo("a/b/c"));
    }

    [Test]
    public void CombineEmptyRelative()
    {
        var result = UrlUtils.Combine("/a/b/c", string.Empty);
        Assert.That(result, Is.EqualTo("/a/b/c"));
    }

    [Test]
    public void CombineRelativeWithBaseTrimsSlashes()
    {
        var result = UrlUtils.Combine("/a/b/c/", "/d/e/f/");
        Assert.That(result, Is.EqualTo("/a/b/c/d/e/f/"));
    }

    [Test]
    public void DecodeParameterHandlesRoundTrip()
    {
        const string input = "DNN Platform!";
        var encodedValue = UrlUtils.EncodeParameter(input);
        var result = UrlUtils.DecodeParameter(encodedValue);
        Assert.That(result, Is.EqualTo(input));
    }

    [Test]
    public void DecodeParameterHandlesSpecialCharacters()
    {
        var result = UrlUtils.DecodeParameter("RE5_O1-$");
        Assert.That(result, Is.EqualTo("DN;_"));
    }

    [Test]
    public void DecryptParameterHandlesRoundTrip()
    {
        const string input = "DNN Platform!";
        var key = Guid.NewGuid().ToString();
        var encodedValue = UrlUtils.EncryptParameter(input, key);
        var result = UrlUtils.DecryptParameter(encodedValue, key);
        Assert.That(result, Is.EqualTo(input));
    }

    [Test]
    public void EncodeParameterReplacesPaddingSymbols()
    {
        var result = UrlUtils.EncodeParameter("D");
        Assert.That(result, Is.EqualTo("RA$$"));
    }

    [Test]
    public void EncryptParameterReplacesPaddingSymbols()
    {
        var result = UrlUtils.EncryptParameter("D", "key");
        Assert.That(result.EndsWith("%3d"), Is.True);
    }

    [Test]
    public void GetParameterNameReturnsName()
    {
        var result = UrlUtils.GetParameterName("key=value");
        Assert.That(result, Is.EqualTo("key"));
    }

    [Test]
    public void GetParameterNameReturnsEntireStringIfNoEqualsSign()
    {
        var result = UrlUtils.GetParameterName("just-a-key");
        Assert.That(result, Is.EqualTo("just-a-key"));
    }

    [Test]
    public void GetParameterNameReturnsEmptyIfStartsWithEqualsSign()
    {
        var result = UrlUtils.GetParameterName("=just-a-value");
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetParameterValueReturnsName()
    {
        var result = UrlUtils.GetParameterValue("key=value");
        Assert.That(result, Is.EqualTo("value"));
    }

    [Test]
    public void GetParameterValueReturnsEmptyStringIfNoEqualsSign()
    {
        var result = UrlUtils.GetParameterValue("just-a-key");
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetParameterValueReturnsEntireStringIfStartsWithEqualsSign()
    {
        var result = UrlUtils.GetParameterValue("=just-a-value");
        Assert.That(result, Is.EqualTo("just-a-value"));
    }

    [Test]
    public void ClosePopUpGeneratesAJavaScriptUrlWithValues()
    {
        var result = UrlUtils.ClosePopUp(false, "/hello",  false);
        Assert.That(result, Is.EqualTo("javascript:dnnModal.closePopUp(false, '/hello')"));

        result = UrlUtils.ClosePopUp(true, "blah", false);
        Assert.That(result, Is.EqualTo("javascript:dnnModal.closePopUp(true, 'blah')"));
    }

    [Test]
    public void ClosePopUpGeneratesAScriptWhenOnClickEventIsTrue()
    {
        var result = UrlUtils.ClosePopUp(false, "/somewhere",  true);
        Assert.That(result, Is.EqualTo("dnnModal.closePopUp(false, '/somewhere')"));
    }

    [Test]
    public void ClosePopUpEncodesUrlParameter()
    {
        var result = UrlUtils.ClosePopUp(false, "/somewhere?value=%20hi&two='hey'",  true);
        Assert.That(result, Is.EqualTo("""dnnModal.closePopUp(false, '/somewhere?value=%20hi\u0026two=\u0027hey\u0027')"""));
    }

    [Test]
    public void ReplaceQSParamReplacesUnfriendlyParam()
    {
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.ReplaceQSParam("/somewhere?value=hi&two=hey", "two", "what");
        Assert.That(result, Is.EqualTo("/somewhere?value=hi&two=what"));
    }

    [Test]
    public void ReplaceQSParamReplacesFriendlyParam()
    {
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "true", };

        var result = UrlUtils.ReplaceQSParam("/somewhere/value/hi/two/hey/", "two", "what");
        Assert.That(result, Is.EqualTo("/somewhere/value/hi/two/what/"));
    }

    [Test]
    public void ReplaceQSParamHandlesSpecialCharacters()
    {
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.ReplaceQSParam("/somewhere?one.two=three$four&one_two=123", "one.two", "four$3");
        Assert.That(result, Is.EqualTo("/somewhere?one.two=four$3&one_two=123"));
    }

    [Test]
    public void StripQSParamRemovesUnfriendlyParam()
    {
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.StripQSParam("/somewhere?value=hi&two=hey&three=x", "two");
        Assert.That(result, Is.EqualTo("/somewhere?value=hi&three=x"));
    }

    [Test]
    public void StripQSParamRemovesFriendlyParam()
    {
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "true", };

        var result = UrlUtils.StripQSParam("/somewhere/value/hi/two/hey/", "two");
        Assert.That(result, Is.EqualTo("/somewhere/value/hi/"));
    }

    [Test]
    public void StripQSParamHandlesSpecialCharacters()
    {
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.StripQSParam("/somewhere?one.two=three$four&one_two=123", "one.two");
        Assert.That(result, Is.EqualTo("/somewhere?one_two=123"));
    }

    [Test]
    public void ValidateReturnUrlReturnsNullWhenInputIsNull()
    {
        var result = UrlUtils.ValidReturnUrl(null);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ValidateReturnUrlReturnsEmptyWhenInputIsEmpty()
    {
        var result = UrlUtils.ValidReturnUrl(string.Empty);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ValidateReturnUrlDoesNotAcceptDataUrl()
    {
        var result = UrlUtils.ValidReturnUrl("data:text/plain,I am text file");
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ValidateReturnUrlDoesNotAcceptXssAttack()
    {
        var result = UrlUtils.ValidReturnUrl("/return?onclick=alert()");
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain()
    {
        var portalAlias = new PortalAliasInfo { HTTPAlias = "dnncommunity.org", };
        var portalSettings = new PortalSettings(-1, portal: null) { PortalAlias = portalAlias, };
        var portalControllerMock = new Mock<IPortalController>();
        portalControllerMock.Setup(c => c.GetCurrentPortalSettings()).Returns(portalSettings);
        PortalController.SetTestableInstance(portalControllerMock.Object);

        var result = UrlUtils.ValidReturnUrl("https://another.evil/return");
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain()
    {
        var portalAlias = new PortalAliasInfo { HTTPAlias = "dnncommunity.org", };
        var portalSettings = new PortalSettings(-1, portal: null) { PortalAlias = portalAlias, };
        var portalControllerMock = new Mock<IPortalController>();
        portalControllerMock.Setup(c => c.GetCurrentPortalSettings()).Returns(portalSettings);
        PortalController.SetTestableInstance(portalControllerMock.Object);

        var result = UrlUtils.ValidReturnUrl("https://dnncommunity.org/return");
        Assert.That(result, Is.EqualTo("https://dnncommunity.org/return"));
    }

    [Test]
    public void ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch()
    {
        var portalAlias = new PortalAliasInfo { HTTPAlias = "dnncommunity.org", };
        var portalSettings = new PortalSettings(-1, portal: null) { PortalAlias = portalAlias, };
        var portalControllerMock = new Mock<IPortalController>();
        portalControllerMock.Setup(c => c.GetCurrentPortalSettings()).Returns(portalSettings);
        PortalController.SetTestableInstance(portalControllerMock.Object);

        var result = UrlUtils.ValidReturnUrl("/////dnncommunity.net/return");
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches()
    {
        var portalAlias = new PortalAliasInfo { HTTPAlias = "dnncommunity.org", };
        var portalSettings = new PortalSettings(-1, portal: null) { PortalAlias = portalAlias, };
        var portalControllerMock = new Mock<IPortalController>();
        portalControllerMock.Setup(c => c.GetCurrentPortalSettings()).Returns(portalSettings);
        PortalController.SetTestableInstance(portalControllerMock.Object);

        var result = UrlUtils.ValidReturnUrl("/////dnncommunity.org/return");
        Assert.That(result, Is.EqualTo("//dnncommunity.org/return"));
    }

    [Test]
    public void IsPopUpIsTrueWhenPopUpParameterIsOnUrl()
    {
        var result = UrlUtils.IsPopUp("/page?popUp=true");
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsPopUpIsFalseWhenPopUpParameterIsNotOnUrl()
    {
        var result = UrlUtils.IsPopUp("/page");
        Assert.That(result, Is.False);
    }
}
