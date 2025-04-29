// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.GeneratedImage;

using System;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.GeneratedImage;
using Moq;
using NUnit.Framework;

[TestFixture]
public class UriValidatorTests
{
    [TestCase("https://mysite.com/page", true)]
    [TestCase("http://mysite.com/page", true)]
    [TestCase("https://mysite.com", true)]
    [TestCase("http://mysite.com", true)]
    [TestCase("https://mysite.com/siteB", true)]
    [TestCase("http://mysite.com/siteB", true)]
    [TestCase("https://badactor.com", false)]
    [TestCase("http://badactor.com", false)]
    [TestCase("https://badactor.com/siteB", false)]
    [TestCase("http://badactor.com/siteB", false)]
    [TestCase("https://mysite.com.badactor.com", false)]
    [TestCase("http://mysite.com.badactor.com", false)]
    [TestCase("https://mysite.com.badactor.com/siteB", false)]
    [TestCase("http://mysite.com.badactor.com/siteB", false)]
    [TestCase("https://mysite.com.badactor.com/siteB/page", false)]
    [TestCase("http://mysite.com.badactor.com/siteB/page", false)]
    public void UriBelongsToSite_MultipleScenarios(string uriString, bool expected)
    {
        // Arrange
        var mockPortalAliasController = new Mock<IPortalAliasController>();
        var portalAliases = new PortalAliasCollection();
        portalAliases.Add("mysite", new PortalAliasInfo { HTTPAlias = "mysite.com" });
        portalAliases.Add("siteB", new PortalAliasInfo { HTTPAlias = "mysite.com/siteB" });
        mockPortalAliasController
            .Setup(controller => controller.GetPortalAliases())
            .Returns(portalAliases);

        var validator = new UriValidator(mockPortalAliasController.Object);

        var testUri = new Uri(uriString);

        // Act
        var result = validator.UriBelongsToSite(testUri);

        Assert.That(result, Is.EqualTo(expected));
    }
}
