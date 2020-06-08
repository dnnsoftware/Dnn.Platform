// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Framework.Attributes
{
    [TestFixture]
    class DnnModuleAuthorizeAttributeTests
    {
        private MockRepository _mockRepository;
        private Mock<ActionDescriptor> _mockActionDescriptor;
        private Mock<ControllerDescriptor> _mockControllerDescriptor;
        private Mock<DnnModuleAuthorizeAttribute> _mockDnnModuleAuthorizeAttribute;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Default);

            _mockActionDescriptor = _mockRepository.Create<ActionDescriptor>();
            _mockControllerDescriptor = _mockRepository.Create<ControllerDescriptor>();

            _mockDnnModuleAuthorizeAttribute = _mockRepository.Create<DnnModuleAuthorizeAttribute>();
            _mockDnnModuleAuthorizeAttribute.CallBase = true;
        }

        [Test]
        public void AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent()
        {
            // Arrange
            _mockDnnModuleAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            var sut = _mockDnnModuleAuthorizeAttribute.Object;

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof (AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };            
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            _mockRepository.VerifyAll();
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel()
        {
            // Arrange
            var sut = _mockDnnModuleAuthorizeAttribute.Object;

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            _mockRepository.VerifyAll();
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel()
        {
            // Arrange
            var sut = _mockDnnModuleAuthorizeAttribute.Object;

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);
            
            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            //Assert.IsTrue(a.IsAuthorized);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsAllowed_IfHasModuleAccess()
        {
            // Arrange
            var sut = _mockDnnModuleAuthorizeAttribute.Object;
            _mockDnnModuleAuthorizeAttribute.Protected().Setup<bool>("HasModuleAccess").Returns(true);
            _mockDnnModuleAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var moduleContext = new ModuleInstanceContext();
            moduleContext.Configuration = new ModuleInfo();
            var fakeController = new Fakes.FakeDnnController { ModuleContext = moduleContext };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);
            
            // Act
            sut.OnAuthorization(context);

            // Assert
            _mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsDenied_IfHasNoModuleAccess()
        {
            // Arrange
            var sut = _mockDnnModuleAuthorizeAttribute.Object;
            _mockDnnModuleAuthorizeAttribute.Protected().Setup<bool>("HasModuleAccess").Returns(false);
            _mockDnnModuleAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var moduleContext = new ModuleInstanceContext();
            moduleContext.Configuration = new ModuleInfo();
            var fakeController = new Fakes.FakeDnnController { ModuleContext = moduleContext };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            sut.OnAuthorization(context);

            // Assert
            _mockRepository.VerifyAll();
        }
    }
}
