// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Services.UserRequest
{
    using System.Collections.Specialized;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class UserRequestIPAddressControllerTest
    {
        private Mock<IPortalController> mockPortalController;
        private Mock<IHostSettingsService> hostSettingsService;
        private Mock<HttpRequestBase> mockRequest;

        private UserRequestIPAddressController userRequestIPAddressController;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            // Setup Mock
            this.hostSettingsService = new Mock<IHostSettingsService>();
            this.mockPortalController = MockComponentProvider.CreateNew<IPortalController>();
            PortalController.SetTestableInstance(this.mockPortalController.Object);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.mockPortalController.Object);
                    services.AddSingleton(this.hostSettingsService.Object);
                    services.AddSingleton((IHostSettingsService)this.hostSettingsService.Object);
                });
            var mockHttpContext = Mock.Get(HttpContextSource.Current);
            this.mockRequest = Mock.Get(mockHttpContext.Object.Request);
            this.mockRequest.Setup(x => x.ServerVariables).Returns(new NameValueCollection());

            // System under test
            this.userRequestIPAddressController = new UserRequestIPAddressController();
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
            MockComponentProvider.ResetContainer();
        }

        [TestCase("111.111.111.111", "X-Forwarded-For")]
        [TestCase("111.111.111.111,123.112.11.33", "X-Forwarded-For")]
        [TestCase("111.111.111.111", "X-ProxyUser-Ip")]
        [TestCase("111.111.111.111,23.112.11.33", "X-ProxyUser-Ip")]
        public void UserRequestIPAddress_ShouldReturnIP_IfAnyHeaderIsPresent(string requestIp, string headerName)
        {
            // Arrange
            var expectedIp = "111.111.111.111";

            NameValueCollection headersWithXForwardedHeaders = new NameValueCollection();
            headersWithXForwardedHeaders.Add(headerName, requestIp);
            this.hostSettingsService.As<IHostSettingsService>().Setup(hc => hc.GetString(It.IsAny<string>())).Returns(headerName);
            this.mockRequest.Setup(x => x.Headers).Returns(headersWithXForwardedHeaders);

            // Act
            string userRequestIPAddress = this.userRequestIPAddressController.GetUserRequestIPAddress(this.mockRequest.Object);

            // Assert
            Assert.That(userRequestIPAddress, Is.EqualTo(expectedIp));
        }

        [Test]
        public void UserRequestIPAddress_ShouldReturnIP_IfRemoteAddrServerVariablePresent()
        {
            // Arrange
            var expectedIp = "111.111.111.111";
            var remoteVariable = "REMOTE_ADDR";
            var requestIp = "111.111.111.111";

            NameValueCollection serverVariables = new NameValueCollection();
            serverVariables.Add(remoteVariable, requestIp);
            this.mockRequest.Setup(x => x.ServerVariables).Returns(serverVariables);

            // Act
            var userRequestIPAddress = this.userRequestIPAddressController.GetUserRequestIPAddress(this.mockRequest.Object);

            // Assert
            Assert.That(userRequestIPAddress, Is.SameAs(expectedIp));
            this.mockRequest.VerifyGet(r => r.ServerVariables);
        }

        [Test]
        public void UserRequestIPAddress_ShouldReturnIP_IfUserHostAddress()
        {
            // Arrange
            var expectedIp = "111.111.111.111";
            this.mockRequest.Setup(x => x.UserHostAddress).Returns(expectedIp);

            // Act
            var userRequestIPAddress = this.userRequestIPAddressController.GetUserRequestIPAddress(this.mockRequest.Object);

            // Assert
            Assert.That(userRequestIPAddress, Is.SameAs(expectedIp));
            this.mockRequest.VerifyGet(r => r.UserHostAddress);
        }

        [TestCase("abc.111.eer")]
        [TestCase("somedomain.com")]
        [TestCase("244.275.111.111")]
        public void UserRequestIPAddress_ShouldReturnEmptyString_IfIPAddressIsNotValid(string requestIp)
        {
            // Arrange
            var headerName = "X-Forwarded-For";

            NameValueCollection headersWithXForwardedHeaders = new NameValueCollection();
            headersWithXForwardedHeaders.Add(headerName, requestIp);
            this.mockRequest.Setup(x => x.Headers).Returns(headersWithXForwardedHeaders);
            this.hostSettingsService.Setup(hc => hc.GetString(It.IsAny<string>(), It.IsAny<string>())).Returns(headerName);

            // Act
            var userRequestIPAddress = this.userRequestIPAddressController.GetUserRequestIPAddress(this.mockRequest.Object);

            // Assert
            Assert.That(userRequestIPAddress, Is.SameAs(string.Empty));
        }
    }
}
