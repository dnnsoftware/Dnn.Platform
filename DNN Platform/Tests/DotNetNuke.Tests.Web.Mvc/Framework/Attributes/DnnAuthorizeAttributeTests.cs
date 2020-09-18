// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework.Attributes
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Web.Mvc.Framework.ActionFilters;
    using Moq;
    using Moq.Protected;
    using NUnit.Framework;

    [TestFixture]
    internal class DnnAuthorizeAttributeTests
    {
        private MockRepository _mockRepository;
        private Mock<ActionDescriptor> _mockActionDescriptor;
        private Mock<ControllerDescriptor> _mockControllerDescriptor;
        private Mock<IRoleController> _mockRoleController;
        private Mock<DnnAuthorizeAttribute> _mockDnnAuthorizeAttribute;

        [SetUp]
        public void Setup()
        {
            this._mockRepository = new MockRepository(MockBehavior.Default);

            this._mockActionDescriptor = this._mockRepository.Create<ActionDescriptor>();
            this._mockControllerDescriptor = this._mockRepository.Create<ControllerDescriptor>();
            this._mockRoleController = this._mockRepository.Create<IRoleController>();

            this._mockDnnAuthorizeAttribute = this._mockRepository.Create<DnnAuthorizeAttribute>();
            this._mockDnnAuthorizeAttribute.CallBase = true;
        }

        [Test]
        public void AnonymousUser_IsNotAllowed_If_AllowAnonymousAtribute_IsNotPresent()
        {
            // Arrange
            this._mockDnnAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            var sut = this._mockDnnAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtControllerLevel()
        {
            // Arrange
            var sut = this._mockDnnAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtActionLevel()
        {
            // Arrange
            var sut = this._mockDnnAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsAllowed_ByDefault()
        {
            // Arrange
            this._mockDnnAuthorizeAttribute.Protected().Setup<bool>("IsAuthenticated").Returns(true);
            this._mockDnnAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            var sut = this._mockDnnAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsDenied_If_IncludedIn_DeniedRoles()
        {
            // Arrange
            const string roleName = "MyRole";

            var user = this.SetUpUserWithRole(roleName);

            this._mockDnnAuthorizeAttribute.Protected().Setup<bool>("IsAuthenticated").Returns(true);
            this._mockDnnAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            this._mockDnnAuthorizeAttribute.Protected().Setup<UserInfo>("GetCurrentUser").Returns(user);
            var sut = this._mockDnnAuthorizeAttribute.Object;
            sut.DenyRoles = roleName;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsAllowed_If_IncludedIn_DeniedRoles_But_IsSuperUser()
        {
            // Arrange
            const string roleName = "MyRole";

            var user = new UserInfo { IsSuperUser = true };

            this._mockDnnAuthorizeAttribute.Protected().Setup<bool>("IsAuthenticated").Returns(true);
            this._mockDnnAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            this._mockDnnAuthorizeAttribute.Protected().Setup<UserInfo>("GetCurrentUser").Returns(user);
            var sut = this._mockDnnAuthorizeAttribute.Object;
            sut.DenyRoles = roleName;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsDenied_If_IncludedIn_StaticRoles()
        {
            // Arrange
            const string roleName = "MyRole";

            var user = this.SetUpUserWithRole(roleName);

            this._mockDnnAuthorizeAttribute.Protected().Setup<bool>("IsAuthenticated").Returns(true);
            this._mockDnnAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            this._mockDnnAuthorizeAttribute.Protected().Setup<UserInfo>("GetCurrentUser").Returns(user);
            var sut = this._mockDnnAuthorizeAttribute.Object;
            sut.StaticRoles = roleName;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
        }

        private UserInfo SetUpUserWithRole(string roleName)
        {
            var user = new UserInfo
            {
                UserID = 1,
                PortalID = 1,
            };

            var roles = new List<UserRoleInfo>
            {
                new UserRoleInfo
                {
                    RoleName = roleName,
                    Status = RoleStatus.Approved
                },
            };

            this._mockRoleController.Setup(x => x.GetUserRoles(user, true)).Returns(roles);
            RoleController.SetTestableInstance(this._mockRoleController.Object);

            return user;
        }
    }
}
