#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System.Collections.Generic;
using System.Web.Mvc;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Framework.Attributes
{
    [TestFixture]
    class DnnAuthorizeAttributeTests
    {
        private MockRepository _mockRepository;
        private Mock<ActionDescriptor> _mockActionDescriptor;
        private Mock<ControllerDescriptor> _mockControllerDescriptor;
        private Mock<IRoleController> _mockRoleController;
        private Mock<DnnAuthorizeAttribute> _mockDnnAuthorizeAttribute;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Default);

            _mockActionDescriptor = _mockRepository.Create<ActionDescriptor>();
            _mockControllerDescriptor = _mockRepository.Create<ControllerDescriptor>();
            _mockRoleController = _mockRepository.Create<IRoleController>();

            _mockDnnAuthorizeAttribute = _mockRepository.Create<DnnAuthorizeAttribute>();
            _mockDnnAuthorizeAttribute.CallBase = true;
        }

        [Test]
        public void AnonymousUser_IsNotAllowed_If_AllowAnonymousAtribute_IsNotPresent()
        {
            // Arrange
            _mockDnnAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());            
            var sut = _mockDnnAuthorizeAttribute.Object;
            
            _mockActionDescriptor.Setup(x => x.IsDefined(typeof (AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            _mockRepository.VerifyAll();
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtControllerLevel()
        {
            // Arrange
            var sut = _mockDnnAuthorizeAttribute.Object;

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            _mockRepository.VerifyAll();
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtActionLevel()
        {
            // Arrange
            
            var sut = _mockDnnAuthorizeAttribute.Object;

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            _mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsAllowed_ByDefault()
        {
            // Arrange
            
            _mockDnnAuthorizeAttribute.Protected().Setup<bool>("IsAuthenticated").Returns(true);
            _mockDnnAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            var sut = _mockDnnAuthorizeAttribute.Object;

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();            
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);
            
            // Act
            sut.OnAuthorization(context);

            // Assert
            _mockRepository.VerifyAll();
        }

        private UserInfo SetUpUserWithRole(string roleName)
        {
            var user = new UserInfo
            {
                UserID = 1,
                PortalID = 1
            };

            var roles = new List<UserRoleInfo>
            {
                new UserRoleInfo
                {
                    RoleName = roleName,
                    Status = RoleStatus.Approved
                }
            };

            _mockRoleController.Setup(x => x.GetUserRoles(user, true)).Returns(roles);
            RoleController.SetTestableInstance(_mockRoleController.Object);

            return user;
        }

        [Test]
        public void RegisteredUser_IsDenied_If_IncludedIn_DeniedRoles()
        {
            // Arrange
            const string roleName = "MyRole";

            var user = SetUpUserWithRole(roleName);
            
            _mockDnnAuthorizeAttribute.Protected().Setup<bool>("IsAuthenticated").Returns(true);
            _mockDnnAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            _mockDnnAuthorizeAttribute.Protected().Setup<UserInfo>("GetCurrentUser").Returns(user);
            var sut = _mockDnnAuthorizeAttribute.Object;
            sut.DenyRoles = roleName;

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            _mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsAllowed_If_IncludedIn_DeniedRoles_But_IsSuperUser()
        {
            // Arrange
            const string roleName = "MyRole";

            var user = new UserInfo { IsSuperUser = true };
            
            _mockDnnAuthorizeAttribute.Protected().Setup<bool>("IsAuthenticated").Returns(true);
            _mockDnnAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            _mockDnnAuthorizeAttribute.Protected().Setup<UserInfo>("GetCurrentUser").Returns(user);
            var sut = _mockDnnAuthorizeAttribute.Object;
            sut.DenyRoles = roleName;

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            _mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsDenied_If_IncludedIn_StaticRoles()
        {
            // Arrange
            const string roleName = "MyRole";

            var user = SetUpUserWithRole(roleName);

            _mockDnnAuthorizeAttribute.Protected().Setup<bool>("IsAuthenticated").Returns(true);
            _mockDnnAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            _mockDnnAuthorizeAttribute.Protected().Setup<UserInfo>("GetCurrentUser").Returns(user);
            var sut = _mockDnnAuthorizeAttribute.Object;
            sut.StaticRoles = roleName;

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            _mockRepository.VerifyAll();
        }
    }
}
