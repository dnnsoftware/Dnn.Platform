// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework.Attributes
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Framework.ActionFilters;
    using Moq;
    using Moq.Protected;
    using NUnit.Framework;

    [TestFixture]
    internal class DnnModuleAuthorizeAttributeTests
    {
        private MockRepository _mockRepository;
        private Mock<ActionDescriptor> _mockActionDescriptor;
        private Mock<ControllerDescriptor> _mockControllerDescriptor;
        private Mock<DnnModuleAuthorizeAttribute> _mockDnnModuleAuthorizeAttribute;

        [SetUp]
        public void Setup()
        {
            this._mockRepository = new MockRepository(MockBehavior.Default);

            this._mockActionDescriptor = this._mockRepository.Create<ActionDescriptor>();
            this._mockControllerDescriptor = this._mockRepository.Create<ControllerDescriptor>();

            this._mockDnnModuleAuthorizeAttribute = this._mockRepository.Create<DnnModuleAuthorizeAttribute>();
            this._mockDnnModuleAuthorizeAttribute.CallBase = true;
        }

        [Test]
        public void AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent()
        {
            // Arrange
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel()
        {
            // Arrange
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel()
        {
            // Arrange
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            // Assert.IsTrue(a.IsAuthorized);
            this._mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsAllowed_IfHasModuleAccess()
        {
            // Arrange
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup<bool>("HasModuleAccess").Returns(true);
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var moduleContext = new ModuleInstanceContext();
            moduleContext.Configuration = new ModuleInfo();
            var fakeController = new Fakes.FakeDnnController { ModuleContext = moduleContext };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsDenied_IfHasNoModuleAccess()
        {
            // Arrange
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup<bool>("HasModuleAccess").Returns(false);
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var moduleContext = new ModuleInstanceContext();
            moduleContext.Configuration = new ModuleInfo();
            var fakeController = new Fakes.FakeDnnController { ModuleContext = moduleContext };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
        }
    }
}
