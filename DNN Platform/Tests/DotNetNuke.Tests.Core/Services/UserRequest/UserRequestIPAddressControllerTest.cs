﻿#region Copyright
// DotNetNuke® - https://www.dnnsoftware.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using DotNetNuke.Entities.Portals;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;
using DotNetNuke.Entities.Controllers;
using System.Web;
using DotNetNuke.Tests.Utilities;
using System.Collections.Specialized;
using DotNetNuke.Services.UserRequest;

namespace DotNetNuke.Tests.Core.Services.UserRequest
{
    [TestFixture]
    class UserRequestIPAddressControllerTest
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
            _mockhttpContext = HttpContextHelper.RegisterMockHttpContext();
            _mockRequest = Mock.Get(_mockhttpContext.Object.Request);
            _mockRequest.Setup(x => x.ServerVariables).Returns(serverVariables);
            _mockHostController = MockComponentProvider.CreateNew<IHostController>();
            _mockPortalController = MockComponentProvider.CreateNew<IPortalController>();
            PortalController.SetTestableInstance(_mockPortalController.Object);

            // System under test
            _userRequestIPAddressController = new UserRequestIPAddressController();
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }        

        [TestCase("111.111.111.111","X-Forwarded-For")]
        [TestCase("111.111.111.111,123.112.11.33", "X-Forwarded-For")]
        [TestCase("111.111.111.111", "X-ProxyUser-Ip")]
        [TestCase("111.111.111.111,23.112.11.33", "X-ProxyUser-Ip")]
        public void UserRequestIPAddress_ShouldReturnIP_IfAnyHeaderIsPresent(string requestIp, string headerName)
        {
            //Arrange
            var expectedIp = "111.111.111.111";                    

            NameValueCollection headersWithXForwardedHeaders = new NameValueCollection();
            headersWithXForwardedHeaders.Add(headerName, requestIp);
            _mockHostController.Setup(hc => hc.GetString(It.IsAny<string>(), It.IsAny<string>())).Returns(headerName);
            _mockRequest.Setup(x => x.Headers).Returns(headersWithXForwardedHeaders);

            //Act
            string userRequestIPAddress = _userRequestIPAddressController.GetUserRequestIPAddress(_mockhttpContext.Object.Request);

            //Assert            
            Assert.AreEqual(expectedIp, userRequestIPAddress);
        }

        [Test]
        public void UserRequestIPAddress_ShouldReturnIP_IfRemoteAddrServerVariablePresent()
        {
            //Arrange
            var expectedIp = "111.111.111.111";
            var remoteVariable = "REMOTE_ADDR";
            var requestIp = "111.111.111.111";

            NameValueCollection serverVariables = new NameValueCollection();
            serverVariables.Add(remoteVariable, requestIp);
            _mockRequest.Setup(x => x.ServerVariables).Returns(serverVariables);

            //Act
            var userRequestIPAddress = _userRequestIPAddressController.GetUserRequestIPAddress(_mockhttpContext.Object.Request);

            //Assert            
            Assert.AreSame(expectedIp, userRequestIPAddress);
            _mockRequest.VerifyGet(r => r.ServerVariables);
            _mockRequest.VerifyGet(r => r.Headers);
            _mockHostController.Verify(hc => hc.GetString(It.IsAny<string>(), It.IsAny<string>()));
        }

        [Test]
        public void UserRequestIPAddress_ShouldReturnIP_IfUserHostAddress()
        {
            //Arrange
            var expectedIp = "111.111.111.111";
            _mockRequest.Setup(x => x.UserHostAddress).Returns(expectedIp);

            //Act
            var userRequestIPAddress = _userRequestIPAddressController.GetUserRequestIPAddress(_mockhttpContext.Object.Request);

            //Assert            
            Assert.AreSame(expectedIp, userRequestIPAddress);
            _mockRequest.VerifyGet(r => r.UserHostAddress);
        }

        [TestCase("abc.111.eer")]
        [TestCase("somedomain.com")]
        [TestCase("244.275.111.111")]
        public void UserRequestIPAddress_ShouldReturnEmptyString_IfIPAddressIsNotValid(string requestIp)
        {
            //Arrange
            var headerName = "X-Forwarded-For";           

            NameValueCollection headersWithXForwardedHeaders = new NameValueCollection();
            headersWithXForwardedHeaders.Add(headerName, requestIp);
            _mockRequest.Setup(x => x.Headers).Returns(headersWithXForwardedHeaders);
            _mockHostController.Setup(hc => hc.GetString(It.IsAny<string>(), It.IsAny<string>())).Returns(headerName);
            
            //Act
            var userRequestIPAddress = _userRequestIPAddressController.GetUserRequestIPAddress(_mockhttpContext.Object.Request);

            //Assert            
            Assert.AreSame(string.Empty, userRequestIPAddress);
        }
    }
}
