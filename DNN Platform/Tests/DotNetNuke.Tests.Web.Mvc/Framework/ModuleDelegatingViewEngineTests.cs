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

using System.Linq;
using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Framework;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Routing;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Framework
{
    [TestFixture]
    public class ModuleDelegatingViewEngineTests
    {
        [Test]
        public void Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var result = new ViewEngineResult(new[] {"foo", "bar", "baz"});
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
            var result = new ViewEngineResult(new[] {"foo", "bar", "baz"});
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
                                                ModuleApplication = mockApp.Object
                                            };

            context.HttpContext.SetModuleRequestResult(activeModuleRequest);
        }
    }

}
