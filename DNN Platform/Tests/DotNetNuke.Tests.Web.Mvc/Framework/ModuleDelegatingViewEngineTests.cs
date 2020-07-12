// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework
{
    using System.Linq;
    using System.Web.Mvc;

    using DotNetNuke.Common;
    using DotNetNuke.Web.Mvc.Framework;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Framework.Modules;
    using DotNetNuke.Web.Mvc.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ModuleDelegatingViewEngineTests
    {
        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IControllerFactory, DefaultControllerFactory>();
            Globals.DependencyProvider = services.BuildServiceProvider();
        }

        [Test]
        public void Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var result = new ViewEngineResult(new[] { "foo", "bar", "baz" });
            var controller = new Mock<DnnController>();
            controller.SetupAllProperties();
            var context = MockHelper.CreateMockControllerContext(controller.Object);

            // ReSharper disable ConvertToConstant.Local
            string viewName = "Foo";

            // ReSharper restore ConvertToConstant.Local
            mockEngines.Setup(e => e.FindPartialView(context, viewName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();

            // Act
            ViewEngineResult engineResult = viewEngine.FindPartialView(context, viewName, true);

            // Assert
            mockEngines.Verify(e => e.FindPartialView(context, viewName));
            Assert.AreEqual("foo", engineResult.SearchedLocations.ElementAt(0));
            Assert.AreEqual("bar", engineResult.SearchedLocations.ElementAt(1));
            Assert.AreEqual("baz", engineResult.SearchedLocations.ElementAt(2));
        }

        [Test]
        public void Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var result = new ViewEngineResult(new[] { "foo", "bar", "baz" });
            var controller = new Mock<DnnController>();
            controller.SetupAllProperties();
            var context = MockHelper.CreateMockControllerContext(controller.Object);

            // ReSharper disable ConvertToConstant.Local
            var viewName = "Foo";
            var masterName = "Bar";

            // ReSharper restore ConvertToConstant.Local
            mockEngines.Setup(e => e.FindView(context, viewName, masterName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();

            // Act
            var engineResult = viewEngine.FindView(context, viewName, masterName, true);

            // Assert
            mockEngines.Verify(e => e.FindView(context, viewName, masterName));
            Assert.AreEqual("foo", engineResult.SearchedLocations.ElementAt(0));
            Assert.AreEqual("bar", engineResult.SearchedLocations.ElementAt(1));
            Assert.AreEqual("baz", engineResult.SearchedLocations.ElementAt(2));
        }

        [Test]
        public void Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var mockEngine = new Mock<IViewEngine>();
            var mockView = new Mock<IView>();
            var result = new ViewEngineResult(mockView.Object, mockEngine.Object);
            var controller = new Mock<DnnController>();
            controller.SetupAllProperties();
            var context = MockHelper.CreateMockControllerContext(controller.Object);

            // ReSharper disable ConvertToConstant.Local
            string viewName = "Foo";
            string masterName = "Bar";

            // ReSharper restore ConvertToConstant.Local
            mockEngines.Setup(e => e.FindView(context, viewName, masterName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();

            // Act
            ViewEngineResult engineResult = viewEngine.FindView(context, viewName, masterName, true);
            viewEngine.ReleaseView(context, engineResult.View);

            // Assert
            mockEngine.Verify(e => e.ReleaseView(context, mockView.Object));
        }

        [Test]
        public void Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var mockEngine = new Mock<IViewEngine>();
            var mockView = new Mock<IView>();
            var result = new ViewEngineResult(mockView.Object, mockEngine.Object);
            var controller = new Mock<DnnController>();
            controller.SetupAllProperties();
            var context = MockHelper.CreateMockControllerContext(controller.Object);

            // ReSharper disable ConvertToConstant.Local
            string viewName = "Foo";

            // ReSharper restore ConvertToConstant.Local
            mockEngines.Setup(e => e.FindPartialView(context, viewName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();

            // Act
            ViewEngineResult engineResult = viewEngine.FindPartialView(context, viewName, true);
            viewEngine.ReleaseView(context, engineResult.View);

            // Assert
            mockEngine.Verify(e => e.ReleaseView(context, mockView.Object));
        }

        [Test]
        public void Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var viewEngine = new ModuleDelegatingViewEngine();
            var controller = new Mock<DnnController>();
            controller.SetupAllProperties();
            var context = MockHelper.CreateMockControllerContext(controller.Object);

            // Act
            var engineResult = viewEngine.FindView(context, "Foo", "Bar", true);

            // Assert
            Assert.IsNotNull(engineResult);
            Assert.IsNull(engineResult.View);
            Assert.AreEqual(0, engineResult.SearchedLocations.Count());
        }

        [Test]
        public void Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var viewEngine = new ModuleDelegatingViewEngine();
            var controller = new Mock<DnnController>();
            controller.SetupAllProperties();
            var context = MockHelper.CreateMockControllerContext(controller.Object);

            // Act
            var engineResult = viewEngine.FindPartialView(context, "Foo", true);

            // Assert
            Assert.IsNotNull(engineResult);
            Assert.IsNull(engineResult.View);
            Assert.AreEqual(0, engineResult.SearchedLocations.Count());
        }

        private static void SetupMockModuleApplication(ControllerContext context, ViewEngineCollection engines)
        {
            var mockApp = new Mock<ModuleApplication>();
            mockApp.Object.ViewEngines = engines;
            (context.Controller as IDnnController).ViewEngineCollectionEx = engines;

            var activeModuleRequest = new ModuleRequestResult
            {
                ModuleApplication = mockApp.Object,
            };

            context.HttpContext.SetModuleRequestResult(activeModuleRequest);
        }
    }
}
