// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework.Modules
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Tests.Web.Mvc.Fakes;
    using DotNetNuke.Tests.Web.Mvc.Fakes.Filters;
    using DotNetNuke.Web.Mvc.Framework.Modules;
    using NUnit.Framework;

    [TestFixture]
    public class ResultCapturingActionInvokerTests
    {
        private ResultCapturingActionInvoker _actionInvoker;

        [SetUp]
        public void Setup()
        {
            this._actionInvoker = new ResultCapturingActionInvoker();
        }

        [TearDown]
        public void TearDown()
        {
            this._actionInvoker = null;
        }

        [Test]
        public void InvokeActionResult_Sets_ResultOfLastInvoke()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();

            var controller = new FakeController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);

            // Act
            this._actionInvoker.InvokeAction(controller.ControllerContext, "Index");

            // Assert
            Assert.IsNotNull(this._actionInvoker.ResultOfLastInvoke);
            Assert.IsInstanceOf<ViewResult>(this._actionInvoker.ResultOfLastInvoke);
        }

        [Test]
        public void ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True()
        {
            // Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
            var controller = this.SetupController(httpContextBase);
            FakeHandleExceptionRedirectAttribute.IsExceptionHandled = true;
            var expectedResult = FakeHandleExceptionRedirectAttribute.Result;

            // Act
            this._actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithExceptionFilter));

            // Assert
            Assert.AreEqual(expectedResult, this._actionInvoker.ResultOfLastInvoke);
        }

        [Test]
        public void ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False()
        {
            // Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
            var controller = this.SetupController(httpContextBase);
            FakeHandleExceptionRedirectAttribute.IsExceptionHandled = false;
            var expectedResult = FakeRedirectAttribute.Result;

            // Act
            Assert.Throws<Exception>(() => this._actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithExceptionFilter)));

            // Assert
            Assert.AreEqual(expectedResult, this._actionInvoker.ResultOfLastInvoke);
        }

        [Test]
        public void ActionInvoker_InvokeOnExecutingFilters()
        {
            // Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
            var controller = this.SetupController(httpContextBase);
            var expectedResult = FakeRedirectAttribute.Result;

            // Act
            this._actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithOnExecutingFilter));

            // Assert
            Assert.AreEqual(expectedResult, this._actionInvoker.ResultOfLastInvoke);
        }

        [Test]
        public void ActionInvoker_InvokeOnExecutedFilters()
        {
            // Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
            var controller = this.SetupController(httpContextBase);
            var expectedResult = FakeRedirectAttribute.Result;

            // Act
            this._actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithOnExecutedFilter));

            // Assert
            Assert.AreEqual(expectedResult, this._actionInvoker.ResultOfLastInvoke);
        }

        private FakeDnnController SetupController(HttpContextBase context)
        {
            var controller = new FakeDnnController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
            return controller;
        }
    }
}
