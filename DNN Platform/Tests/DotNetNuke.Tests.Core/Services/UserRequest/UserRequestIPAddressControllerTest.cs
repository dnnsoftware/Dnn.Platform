// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.UserRequest
{
    using System.Collections.Specialized;
    using System.Web;

    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class UserRequestIPAddressControllerTest
    {
        private Mock<IPortalController> _mockPortalController;
        private Mock<IHostController> _mockHostController;
        private Mock<HttpContextBase> _mockhttpContext;
        private Mock<HttpRequestBase> _mockRequest;

        private UserRequestIPAddressController _userRequestIPAddressController;

        [SetUp]
        public void Setup()
        {
            NameValueCollection serverVariables = new NameValueCollection();

            // Setup Mock
            this._mockhttpContext = HttpContextHelper.RegisterMockHttpContext();
            this._mockRequest = Mock.Get(this._mockhttpContext.Object.Request);
            this._mockRequest.Setup(x => x.ServerVariables).Returns(serverVariables);
            this._mockHostController = MockComponentProvider.CreateNew<IHostController>();
            this._mockPortalController = MockComponentProvider.CreateNew<IPortalController>();
            PortalController.SetTestableInstance(this._mockPortalController.Object);

            // System under test
            this._userRequestIPAddressController = new UserRequestIPAddressController();
        }

        [TearDown]
        public void TearDown()
        {
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
            this._mockHostController.Setup(hc => hc.GetString(It.IsAny<string>(), It.IsAny<string>())).Returns(headerName);
            this._mockRequest.Setup(x => x.Headers).Returns(headersWithXForwardedHeaders);

            // Act
            string userRequestIPAddress = this._userRequestIPAddressController.GetUserRequestIPAddress(this._mockhttpContext.Object.Request);

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
            this._mockRequest.Setup(x => x.ServerVariables).Returns(serverVariables);

            // Act
            var userRequestIPAddress = this._userRequestIPAddressController.GetUserRequestIPAddress(this._mockhttpContext.Object.Request);

            // Assert
            Assert.AreSame(expectedIp, userRequestIPAddress);
            this._mockRequest.VerifyGet(r => r.ServerVariables);
            this._mockRequest.VerifyGet(r => r.Headers);
            this._mockHostController.Verify(hc => hc.GetString(It.IsAny<string>(), It.IsAny<string>()));
        }

        [Test]
        public void UserRequestIPAddress_ShouldReturnIP_IfUserHostAddress()
        {
            // Arrange
            var expectedIp = "111.111.111.111";
            this._mockRequest.Setup(x => x.UserHostAddress).Returns(expectedIp);

            // Act
            var userRequestIPAddress = this._userRequestIPAddressController.GetUserRequestIPAddress(this._mockhttpContext.Object.Request);

            // Assert
            Assert.AreSame(expectedIp, userRequestIPAddress);
            this._mockRequest.VerifyGet(r => r.UserHostAddress);
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
            this._mockRequest.Setup(x => x.Headers).Returns(headersWithXForwardedHeaders);
            this._mockHostController.Setup(hc => hc.GetString(It.IsAny<string>(), It.IsAny<string>())).Returns(headerName);

            // Act
            var userRequestIPAddress = this._userRequestIPAddressController.GetUserRequestIPAddress(this._mockhttpContext.Object.Request);

            // Assert
            Assert.AreSame(string.Empty, userRequestIPAddress);
        }
    }
}
