// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Mail;

using Dnn.ExchangeOnlineAuthProvider.Components;
using Dnn.GoogleMailAuthProvider.Components;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Services.Mail.OAuth;
using Moq;
using NUnit.Framework;

[TestFixture]
public class SmtpOAuthControllerTest
{
    [Test]
    public void Google_OAuth_Provider_Should_Exists()
    {
        var provider = CreateSmtpOAuthController().GetOAuthProvider("GoogleMail");

        Assert.That(provider, Is.Not.Null);
    }

    [Test]
    public void Exchange_OAuth_Provider_Should_Exists()
    {
        var provider = CreateSmtpOAuthController().GetOAuthProvider("ExchangeOnline");

        Assert.That(provider, Is.Not.Null);
    }

    [Test]
    public void NonExistent_OAuth_Provider_Should_Be_Null()
    {
        var provider = CreateSmtpOAuthController().GetOAuthProvider("AnotherProvider");

        Assert.That(provider, Is.Null);
    }

    private static SmtpOAuthController CreateSmtpOAuthController()
    {
        return new SmtpOAuthController(
            new ISmtpOAuthProvider[]
            {
                new GoogleMailOAuthProvider(Mock.Of<IHostSettingsService>(), Mock.Of<IPortalAliasService>()),
                new ExchangeOnlineOAuthProvider(Mock.Of<IHostSettingsService>(), Mock.Of<IPortalAliasService>()),
            });
    }
}
