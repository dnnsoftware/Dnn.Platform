// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Tests.Services
{
    using Dnn.PersonaBar.Pages.Components;
    using Dnn.PersonaBar.Security.Services;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web.Api.Auth.ApiTokens;

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
            var sut = new SecurityController(
                Mock.Of<Components.SecurityController>(),
                Mock.Of<IPagesController>(),
                Mock.Of<IPortalAliasService>(),
                Mock.Of<IApiTokenController>(),
                Mock.Of<IHostSettingsService>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IHostSettings>());

            // act
            var alias = sut.AddPortalAlias(SslUrl, PortalId);

            // assert
            Assert.That(alias, Is.EqualTo(SomeAlias));
        }

        [Test]
        public void Services_Controller_AddPortalAlias_WhenAliasExists_AddIsNotInvoked()
        {
            // arrange
            var portalAliasControllerMock = new Mock<IPortalAliasService>();
            var portalAliasInfo = new PortalAliasInfo
            {
                HTTPAlias = SomeAlias,
                PortalID = PortalId,
            };
            portalAliasControllerMock
                .Setup(c => c.GetPortalAlias(SomeAlias, PortalId))
                .Returns(() => portalAliasInfo);
            var sut = new SecurityController(
                Mock.Of<Components.SecurityController>(),
                Mock.Of<IPagesController>(),
                portalAliasControllerMock.Object,
                Mock.Of<IApiTokenController>(),
                Mock.Of<IHostSettingsService>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IHostSettings>());

            // act
            sut.AddPortalAlias(SslUrl, PortalId);

            // assert
            portalAliasControllerMock.Verify(
                c =>
                c.AddPortalAlias(It.Is<IPortalAliasInfo>(match =>
                    match.HttpAlias == SomeAlias && match.PortalId == PortalId)), Times.Never);
        }

        [Test]
        public void Services_Controller_AddPortalAlias_WhenAliasDoesNotExist_AddIsInvoked()
        {
            // arrange
            var portalAliasControllerMock = new Mock<IPortalAliasService>();
            portalAliasControllerMock
                .Setup(c => c.GetPortalAlias(SomeAlias, PortalId))
                .Returns(() => null);
            var sut = new SecurityController(
                Mock.Of<Components.SecurityController>(),
                Mock.Of<IPagesController>(),
                portalAliasControllerMock.Object,
                Mock.Of<IApiTokenController>(),
                Mock.Of<IHostSettingsService>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IHostSettings>());

            // act
            sut.AddPortalAlias(SslUrl, PortalId);

            // assert
            portalAliasControllerMock.Verify(
                c =>
                c.AddPortalAlias(It.Is<IPortalAliasInfo>(match =>
                    match.HttpAlias == SomeAlias && match.PortalId == PortalId)), Times.Once);
        }
    }
}
