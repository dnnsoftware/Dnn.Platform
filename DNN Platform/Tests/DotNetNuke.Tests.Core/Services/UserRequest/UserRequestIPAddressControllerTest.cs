// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.UserRequest
{
    using System.Collections.Specialized;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class UserRequestIPAddressControllerTest
    {
        private Mock<IPortalController> mockPortalController;
        private Mock<IHostController> mockHostController;
        private Mock<HttpContextBase> mockhttpContext;
        private Mock<HttpRequestBase> mockRequest;

        private UserRequestIPAddressController userRequestIPAddressController;

        [SetUp]
        public void Setup()
        {
            NameValueCollection serverVariables = new NameValueCollection();

            // Setup Mock
            this.mockhttpContext = HttpContextHelper.RegisterMockHttpContext();
            this.mockRequest = Mock.Get(this.mockhttpContext.Object.Request);
            this.mockRequest.Setup(x => x.ServerVariables).Returns(serverVariables);
            this.mockHostController = new Mock<IHostController>();
            this.mockHostController.As<IHostSettingsService>();
            this.mockPortalController = MockComponentProvider.CreateNew<IPortalController>();
            PortalController.SetTestableInstance(this.mockPortalController.Object);

            // System under test
            this.userRequestIPAddressController = new UserRequestIPAddressController();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
            serviceCollection.AddTransient<IApplicationStatusInfo>(container => Mock.Of<IApplicationStatusInfo>());
            serviceCollection.AddTransient<IHostSettingsService>(container => (IHostSettingsService)this.mockHostController.Object);
            Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            Globals.DependencyProvider = null;
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
            this.mockHostController.Setup(hc => hc.GetString(It.IsAny<string>(), It.IsAny<string>())).Returns(headerName);
            this.mockRequest.Setup(x => x.Headers).Returns(headersWithXForwardedHeaders);

            // Act
            string userRequestIPAddress = this.userRequestIPAddressController.GetUserRequestIPAddress(this.mockhttpContext.Object.Request);

            // Assert
            Assert.AreEqual(expectedIp, userRequestIPAddress);
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
            var userRequestIPAddress = this.userRequestIPAddressController.GetUserRequestIPAddress(this.mockhttpContext.Object.Request);

            // Assert
            Assert.AreSame(expectedIp, userRequestIPAddress);
            this.mockRequest.VerifyGet(r => r.ServerVariables);
            this.mockRequest.VerifyGet(r => r.Headers);
            this.mockHostController.Verify(hc => hc.GetString(It.IsAny<string>(), It.IsAny<string>()));
        }

        [Test]
        public void UserRequestIPAddress_ShouldReturnIP_IfUserHostAddress()
        {
            // Arrange
            var expectedIp = "111.111.111.111";
            this.mockRequest.Setup(x => x.UserHostAddress).Returns(expectedIp);

            // Act
            var userRequestIPAddress = this.userRequestIPAddressController.GetUserRequestIPAddress(this.mockhttpContext.Object.Request);

            // Assert
            Assert.AreSame(expectedIp, userRequestIPAddress);
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
            this.mockHostController.Setup(hc => hc.GetString(It.IsAny<string>(), It.IsAny<string>())).Returns(headerName);

            // Act
            var userRequestIPAddress = this.userRequestIPAddressController.GetUserRequestIPAddress(this.mockhttpContext.Object.Request);

            // Assert
            Assert.AreSame(string.Empty, userRequestIPAddress);
        }
    }
}
