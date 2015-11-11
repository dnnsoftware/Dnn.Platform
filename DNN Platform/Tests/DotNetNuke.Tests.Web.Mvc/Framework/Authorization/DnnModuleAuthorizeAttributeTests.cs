#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Modules;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Framework.Authorization
{
    [TestFixture]
    class DnnModuleAuthorizeAttributeTests
    {
        private MockRepository _mockRepository;
        private Mock<ActionDescriptor> _mockActionDescriptor;
        private Mock<ControllerDescriptor> _mockControllerDescriptor;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Default);

            _mockActionDescriptor = _mockRepository.Create<ActionDescriptor>();
            _mockControllerDescriptor = _mockRepository.Create<ControllerDescriptor>();
        }

        [Test]
        public void AnonymousUser_IsNotAllowed_If_AllowAnonymousAtribute_IsNotPresent()
        {
            // Arrange
            var a = new TestableDnnModuleAuthorizeAttribute();

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof (AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };            
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            a.OnAuthorization(context);

            // Assert
            Assert.IsFalse(a.IsAuthorized);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtControllerLevel()
        {
            // Arrange
            var a = new TestableDnnModuleAuthorizeAttribute();

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            a.OnAuthorization(context);

            // Assert
            Assert.IsTrue(a.IsAuthorized);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtActionLevel()
        {
            // Arrange
            var a = new TestableDnnModuleAuthorizeAttribute();

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);
            
            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            a.OnAuthorization(context);

            // Assert
            Assert.IsTrue(a.IsAuthorized);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsAllowed_IfHasModuleAccess()
        {
            // Arrange
            var a = new TestableDnnModuleAuthorizeAttribute(true);

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var moduleContext = new ModuleInstanceContext();
            moduleContext.Configuration = new ModuleInfo();
            var fakeController = new Fakes.FakeDnnController { ModuleContext = moduleContext };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);
            
            // Act
            a.OnAuthorization(context);

            // Assert
            Assert.IsTrue(a.IsAuthorized);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void RegisteredUser_IsDenied_IfHasNoModuleAccess()
        {
            // Arrange
            var a = new TestableDnnModuleAuthorizeAttribute(false);

            _mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            _mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);

            var moduleContext = new ModuleInstanceContext();
            moduleContext.Configuration = new ModuleInfo();
            var fakeController = new Fakes.FakeDnnController { ModuleContext = moduleContext };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, _mockActionDescriptor.Object);

            // Act
            a.OnAuthorization(context);

            // Assert
            Assert.IsFalse(a.IsAuthorized);
            _mockRepository.VerifyAll();
        }
    }
}
