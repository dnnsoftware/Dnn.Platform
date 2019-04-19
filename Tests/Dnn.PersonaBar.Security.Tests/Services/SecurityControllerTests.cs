using Dnn.PersonaBar.Security.Services;
using DotNetNuke.Entities.Portals;
using Moq;
using NUnit.Framework;

namespace Dnn.PersonaBar.Security.Tests.Services
{
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
            var sut = new SecurityController(controllerMock.Object, portalAliasControllerMock.Object);

            // act
            var alias = sut.AddPortalAlias(SslUrl, PortalId);

            // assert
            Assert.AreEqual(SomeAlias, alias);
        }

        [Test]
        public void Services_Controller_AddPortalAlias_WhenAliasExists_AddIsNotInvoked()
        {
            // arrange
            var controllerMock = new Mock<Components.SecurityController>();
            var portalAliasControllerMock = new Mock<IPortalAliasController>();
            var portalAliasInfo = new PortalAliasInfo
            {
                HTTPAlias = SomeAlias,
                PortalID = PortalId,
            };
            portalAliasControllerMock
                .Setup(c => c.GetPortalAlias(SomeAlias, PortalId))
                .Returns(() => portalAliasInfo);
            var sut = new SecurityController(controllerMock.Object, portalAliasControllerMock.Object);

            // act
            sut.AddPortalAlias(SslUrl, PortalId);

            // assert
            portalAliasControllerMock.Verify(c =>
                c.AddPortalAlias(It.Is<PortalAliasInfo>(match =>
                    match.HTTPAlias == SomeAlias && match.PortalID == PortalId)), Times.Never);
        }

        [Test]
        public void Services_Controller_AddPortalAlias_WhenAliasDoesNotExist_AddIsInvoked()
        {
            // arrange
            var controllerMock = new Mock<Components.SecurityController>();
            var portalAliasControllerMock = new Mock<IPortalAliasController>();
            portalAliasControllerMock
                .Setup(c => c.GetPortalAlias(SomeAlias, PortalId))
                .Returns(() => null);
            var sut = new SecurityController(controllerMock.Object, portalAliasControllerMock.Object);

            // act
            sut.AddPortalAlias(SslUrl, PortalId);

            // assert
            portalAliasControllerMock.Verify(c =>
                c.AddPortalAlias(It.Is<PortalAliasInfo>(match =>
                    match.HTTPAlias == SomeAlias && match.PortalID == PortalId)), Times.Once);
        }
    }
}
