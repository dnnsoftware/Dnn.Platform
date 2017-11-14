using DotNetNuke.Tests.Web.Mvc.Fakes;
using DotNetNuke.Tests.Web.Mvc.Fakes.Filters;
using DotNetNuke.Web.Mvc.Framework.Modules;
using NUnit.Framework;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DotNetNuke.Tests.Web.Mvc.Modules
{
    [TestFixture]
    public class ResultCapturingActionInvokerTests
    {
        private ResultCapturingActionInvoker _actionInvoker;

        [SetUp]
        public void Setup()
        {
            _actionInvoker = new ResultCapturingActionInvoker();
        }

        [TearDown]
        public void TearDown()
        {
            _actionInvoker = null;
        }

        [Test]
        public void ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True()
        {
            //Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
            var controller = SetupController(httpContextBase);
            FakeHandleExceptionRedirectAttribute.IsExceptionHandled = true;
            var expectedResult = FakeHandleExceptionRedirectAttribute.Result;

            //Act
            _actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithExceptionFilter));

            //Assert
            Assert.AreEqual(expectedResult, _actionInvoker.ResultOfLastInvoke);
        }

        [Test]
        public void ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False()
        {
            //Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
            var controller = SetupController(httpContextBase);
            FakeHandleExceptionRedirectAttribute.IsExceptionHandled = false;
            var expectedResult = FakeRedirectAttribute.Result;

            //Act
            Assert.Throws<Exception>(() => _actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithExceptionFilter)));

            //Assert
            Assert.AreEqual(expectedResult, _actionInvoker.ResultOfLastInvoke);
        }

        [Test]
        public void ActionInvoker_InvokeOnExecutingFilters()
        {
            //Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
            var controller = SetupController(httpContextBase);
            var expectedResult = FakeRedirectAttribute.Result;

            //Act
            _actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithOnExecutingFilter));

            //Assert
            Assert.AreEqual(expectedResult, _actionInvoker.ResultOfLastInvoke);
        }

        [Test]
        public void ActionInvoker_InvokeOnExecutedFilters()
        {
            //Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
            var controller = SetupController(httpContextBase);
            var expectedResult = FakeRedirectAttribute.Result;

            //Act
            _actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithOnExecutedFilter));

            //Assert
            Assert.AreEqual(expectedResult, _actionInvoker.ResultOfLastInvoke);
        }

        private FakeDnnController SetupController(HttpContextBase context)
        {
            var controller = new FakeDnnController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
            return controller;
        }
    }
}
