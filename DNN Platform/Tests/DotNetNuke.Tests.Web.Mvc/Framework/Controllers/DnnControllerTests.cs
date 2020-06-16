// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework.Controllers
{
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Tests.Web.Mvc.Fakes;
    using DotNetNuke.Web.Mvc.Framework.ActionResults;
    using NUnit.Framework;

    [TestFixture]
    public class DnnControllerTests
    {
        private const string TestViewName = "Foo";

        [Test]
        public void ActivePage_Property_Is_Null_If_PortalSettings_Not_Set_In_Context()
        {
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

            // Act
            var controller = this.SetupController(httpContextBase);

            // Assert
            Assert.IsNull(controller.ActivePage);
        }

        [Test]
        public void PortalSettings_Property_Is_Null_If_Not_Set_In_Context()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();

            // Act
            var controller = this.SetupController(context);

            // Assert
            Assert.IsNull(controller.PortalSettings);
        }

        [Test]
        public void ResultOfLastExecute_Returns_ViewResult()
        {
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

            // Act
            var controller = this.SetupController(httpContextBase);
            controller.ActionInvoker.InvokeAction(controller.ControllerContext, "Action1");

            // Assert
            Assert.NotNull(controller.ResultOfLastExecute);
            Assert.IsInstanceOf<DnnViewResult>(controller.ResultOfLastExecute);
        }

        [Test]
        public void User_Property_Is_Null_If_PortalSettings_Not_Set_In_Context()
        {
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

            // Act
            var controller = this.SetupController(httpContextBase);

            // Assert
            Assert.IsNull(controller.User);
        }

        [Test]
        public void View_Returns_DnnViewResult()
        {
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

            // Act
            var controller = this.SetupController(httpContextBase);
            var viewResult = controller.Action1();

            // Assert
            Assert.IsInstanceOf<DnnViewResult>(viewResult);
        }

        [Test]
        public void View_Returns_DnnViewResult_With_Correct_ViewName()
        {
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

            // Act
            var controller = this.SetupController(httpContextBase);
            var viewResult = controller.Action1();

            // Assert
            var dnnViewResult = viewResult as DnnViewResult;
            Assert.NotNull(dnnViewResult);
            Assert.AreEqual("Action1", dnnViewResult.ViewName);
        }

        [Test]
        public void View_Returns_DnnViewResult_With_Correct_MasterName()
        {
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

            // Act
            var controller = this.SetupController(httpContextBase);
            var viewResult = controller.Action2();

            // Assert
            var dnnViewResult = viewResult as DnnViewResult;
            Assert.NotNull(dnnViewResult);
            Assert.AreEqual("Action2", dnnViewResult.ViewName);
            Assert.AreEqual("Master2", dnnViewResult.MasterName);
        }

        [Test]
        public void View_Returns_DnnViewResult_With_Correct_ViewData()
        {
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

            // Act
            var controller = this.SetupController(httpContextBase);
            controller.ViewData.Add("key", "value");
            var viewResult = controller.Action2();

            // Assert
            var dnnViewResult = viewResult as DnnViewResult;
            Assert.NotNull(dnnViewResult);
            Assert.AreEqual("value", dnnViewResult.ViewData["key"]);
        }

        [Test]
        public void View_Returns_DnnViewResult_With_Correct_Model()
        {
            // Arrange
            var dog = new Dog() { Name = "Fluffy" };
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

            // Act
            var controller = this.SetupController(httpContextBase);
            var viewResult = controller.Action3(dog);

            // Assert
            var dnnViewResult = viewResult as DnnViewResult;
            Assert.NotNull(dnnViewResult);
            Assert.AreEqual(dog, dnnViewResult.ViewData.Model);
        }

        [Test]
        public void View_Returns_DnnViewResult_With_Correct_ViewEngines()
        {
            // Arrange
            var dog = new Dog() { Name = "Fluffy" };
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

            // Act
            var controller = this.SetupController(httpContextBase);
            var viewResult = controller.Action3(dog);

            // Assert
            var dnnViewResult = viewResult as DnnViewResult;
            Assert.NotNull(dnnViewResult);
            Assert.AreEqual(controller.ViewEngineCollection, dnnViewResult.ViewEngineCollection);
        }

        [Test]
        public void Initialize_CreatesInstance_Of_DnnUrlHelper()
        {
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

            // Act
            var controller = this.SetupController(httpContextBase);
            controller.MockInitialize(httpContextBase.Request.RequestContext);

            // Assert
            Assert.NotNull(controller.Url);
        }

        private FakeDnnController SetupController(HttpContextBase context)
        {
            var controller = new FakeDnnController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
            return controller;
        }
    }
}
