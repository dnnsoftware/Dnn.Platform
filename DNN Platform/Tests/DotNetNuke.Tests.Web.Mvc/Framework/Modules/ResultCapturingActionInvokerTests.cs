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

using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Tests.Web.Mvc.Fakes;
using DotNetNuke.Web.Mvc.Framework.Modules;
using NUnit.Framework;
using DotNetNuke.Tests.Web.Mvc.Fakes.Filters;
using System;

namespace DotNetNuke.Tests.Web.Mvc.Framework.Modules
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
        public void InvokeActionResult_Sets_ResultOfLastInvoke()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();

            var controller = new FakeController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);

            //Act
            _actionInvoker.InvokeAction(controller.ControllerContext, "Index");

            //Assert
            Assert.IsNotNull(_actionInvoker.ResultOfLastInvoke);
            Assert.IsInstanceOf<ViewResult>(_actionInvoker.ResultOfLastInvoke);
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
