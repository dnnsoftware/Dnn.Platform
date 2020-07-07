// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework.Modules
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Framework.Modules;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ModuleApplicationTests
    {
        private const string ActionName = "Action";
        private const string ControllerName = "Controller";

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IControllerFactory, DefaultControllerFactory>();
            Globals.DependencyProvider = services.BuildServiceProvider();
        }

        [Test]
        public void Init_Is_Called_In_First_ExecuteRequest_And_Not_In_Subsequent_Requests()
        {
            // Arrange
            var app = new Mock<ModuleApplication> { CallBase = true };

            int initCounter = 0;
            app.Setup(a => a.Init())
                .Callback(() => initCounter++);

            var controller = new Mock<IDnnController>();

            var controllerFactory = SetupControllerFactory(controller.Object);
            app.SetupGet(a => a.ControllerFactory).Returns(controllerFactory.Object);

            // Act
            app.Object.ExecuteRequest(CreateModuleContext(ControllerName, ActionName));
            app.Object.ExecuteRequest(CreateModuleContext(ControllerName, ActionName));

            // Assert
            Assert.AreEqual(1, initCounter);
        }

        [Test]
        public void ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller()
        {
            // Arrange
            var app = new ModuleApplication();

            var controllerFactory = new Mock<IControllerFactory>();
            RequestContext actualRequestContext = null;
            controllerFactory.Setup(f => f.CreateController(It.IsAny<RequestContext>(), ControllerName))
                                 .Callback<RequestContext, string>((c, n) => actualRequestContext = c)
                                 .Returns(new Mock<IDnnController>().Object);

            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

            // Act
            app.ExecuteRequest(moduleRequestContext);

            // Assert
            controllerFactory.Verify(f => f.CreateController(It.IsAny<RequestContext>(), ControllerName));
            Assert.AreSame(moduleRequestContext.HttpContext, actualRequestContext.HttpContext);
        }

        [Test]
        public void ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController()
        {
            // Arrange
            var app = new ModuleApplication();

            var controller = new Mock<IController>();
            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => app.ExecuteRequest(moduleRequestContext));
        }

        [Test]
        public void ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker()
        {
            // Arrange
            var app = new ModuleApplication();

            var controller = new Mock<Controller>();
            var invoker = new Mock<IActionInvoker>();
            controller.Object.ActionInvoker = invoker.Object;

            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => app.ExecuteRequest(moduleRequestContext));
        }

        [Test]
        public void ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController()
        {
            // Arrange
            var app = new ModuleApplication();

            var controller = new Mock<IController>();
            controller.As<IDnnController>();

            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

            // Act and Assert
            app.ExecuteRequest(moduleRequestContext);
        }

        [Test]
        public void ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller()
        {
            // Arrange
            var app = new ModuleApplication();

            ControllerContext controllerContext = MockHelper.CreateMockControllerContext();
            ActionResult actionResult = new Mock<ActionResult>().Object;

            var controller = SetupMockController(actionResult, controllerContext);

            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

            // Act
            ModuleRequestResult result = app.ExecuteRequest(moduleRequestContext);

            // Assert
            Assert.AreSame(actionResult, result.ActionResult);
            Assert.AreSame(controllerContext, result.ControllerContext);
        }

        [Test]
        public void ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext()
        {
            // Arrange
            var app = new ModuleApplication();

            var controller = new Mock<IController>();
            controller.As<IDnnController>();

            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

            // Act
            ModuleRequestResult result = app.ExecuteRequest(moduleRequestContext);

            // Assert
            controller.Verify(c => c.Execute(It.Is<RequestContext>(rc =>
                    rc.HttpContext == moduleRequestContext.HttpContext &&
                    rc.RouteData.GetRequiredString("controller") == ControllerName)));
        }

        [Test]
        public void ExecuteRequest_ReleasesController_After_Executing()
        {
            // Arrange
            var app = new ModuleApplication();

            var controller = new Mock<IDnnController>();

            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

            // Act
            ModuleRequestResult result = app.ExecuteRequest(moduleRequestContext);

            // Assert
            controllerFactory.Verify(cf => cf.ReleaseController(controller.Object));
        }

        [Test]
        public void ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception()
        {
            // Arrange
            var app = new ModuleApplication();

            var controller = new Mock<IDnnController>();
            controller.Setup(c => c.Execute(It.IsAny<RequestContext>()))
                .Throws(new Exception("Uh Oh!"));

            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

            // Act (and verify the exception is thrown; also supresses the exception so it doesn't fail the test)
            Assert.Throws<Exception>(() => app.ExecuteRequest(moduleRequestContext));

            // Assert
            controllerFactory.Verify(f => f.ReleaseController(controller.Object));
        }

        private static Mock<IDnnController> SetupMockController(ActionResult actionResult, ControllerContext controllerContext)
        {
            var mockController = new Mock<IDnnController>();
            mockController.Setup(c => c.ResultOfLastExecute)
                .Returns(actionResult);
            mockController.Setup(c => c.ControllerContext)
                .Returns(controllerContext);
            return mockController;
        }

        private static Mock<IControllerFactory> SetupControllerFactory(IController controller)
        {
            var controllerFactory = new Mock<IControllerFactory>();
            controllerFactory.Setup(cf => cf.CreateController(It.IsAny<RequestContext>(), It.IsAny<string>()))
                            .Returns(controller);
            return controllerFactory;
        }

        private static ModuleRequestContext CreateModuleContext(string controllerName, string actionName)
        {
            var routeData = new RouteData();
            routeData.Values.Add("controller", controllerName);
            routeData.Values.Add("action", actionName);

            var moduleContext = new ModuleInstanceContext { Configuration = new ModuleInfo { ModuleID = 42 } };
            return new ModuleRequestContext
            {
                HttpContext = MockHelper.CreateMockHttpContext("http://localhost/Portal/Page/ModuleRoute"),
                RouteData = routeData,
                ModuleContext = moduleContext,
            };
        }
    }
}
