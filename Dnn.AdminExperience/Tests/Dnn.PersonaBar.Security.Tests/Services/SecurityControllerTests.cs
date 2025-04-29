// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Tests.Services;

using Dnn.PersonaBar.Security.Services;
using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

[TestFixture]
public class SecurityControllerTests
{
    private const string SomeAlias = "some.domain.com:8080";
    private const string SslUrl = "https://" + SomeAlias + "/";
    private const int PortalId = 0;

    [Test]
    public void Services_Controller_AddPortalAlias_TrimsProtocolAndSlash()
    {
        // arrange
        var controllerMock = new Mock<Components.SecurityController>();
        var portalAliasControllerMock = new Mock<IPortalAliasController>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(Mock.Of<INavigationManager>());
        serviceCollection.AddSingleton(Mock.Of<IApplicationStatusInfo>());
        serviceCollection.AddSingleton(portalAliasControllerMock.As<IPortalAliasService>().Object);
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();

        var sut = new SecurityController(
            Mock.Of<Components.SecurityController>());

        // act
        var alias = sut.AddPortalAlias(SslUrl, PortalId);

        // assert
        Assert.That(alias, Is.EqualTo(SomeAlias));
    }

    [Test]

    public void Services_Controller_AddPortalAlias_WhenAliasExists_AddIsNotInvoked()
    {
        // arrange
        var controllerMock = new Mock<Components.SecurityController>();
        var portalAliasControllerMock = new Mock<IPortalAliasController>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(Mock.Of<INavigationManager>());
        serviceCollection.AddSingleton(Mock.Of<IApplicationStatusInfo>());
        serviceCollection.AddSingleton(portalAliasControllerMock.As<IPortalAliasService>().Object);
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
        var portalAliasInfo = new PortalAliasInfo
        {
            HTTPAlias = SomeAlias,
            PortalID = PortalId,
        };
        portalAliasControllerMock
            .Setup(c => c.GetPortalAlias(SomeAlias, PortalId))
            .Returns(() => portalAliasInfo);
        var sut = new SecurityController(
            Mock.Of<Components.SecurityController>());

        // act
        sut.AddPortalAlias(SslUrl, PortalId);

        // assert
        portalAliasControllerMock.Verify(
            c =>
                c.AddPortalAlias(It.Is<PortalAliasInfo>(match =>
                    match.HTTPAlias == SomeAlias && match.PortalID == PortalId)), Times.Never);
    }

    [Test]

    public void Services_Controller_AddPortalAlias_WhenAliasDoesNotExist_AddIsInvoked()
    {
        // arrange
        var controllerMock = new Mock<Components.SecurityController>();
        var portalAliasControllerMock = new Mock<IPortalAliasController>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(Mock.Of<INavigationManager>());
        serviceCollection.AddSingleton(Mock.Of<IApplicationStatusInfo>());
        serviceCollection.AddSingleton(portalAliasControllerMock.As<IPortalAliasService>().Object);
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
        portalAliasControllerMock
            .Setup(c => c.GetPortalAlias(SomeAlias, PortalId))
            .Returns(() => null);
        var sut = new SecurityController(
            Mock.Of<Components.SecurityController>());

        // act
        sut.AddPortalAlias(SslUrl, PortalId);

        // assert
        portalAliasControllerMock.Verify(
            c =>
                c.AddPortalAlias(It.Is<PortalAliasInfo>(match =>
                    match.HTTPAlias == SomeAlias && match.PortalID == PortalId)), Times.Once);
    }
}
