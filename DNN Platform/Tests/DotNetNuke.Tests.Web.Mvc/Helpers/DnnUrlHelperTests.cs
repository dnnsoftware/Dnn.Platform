﻿#region Copyright

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

using System;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.ComponentModel;
using DotNetNuke.Tests.Utilities.Mocks;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Helpers;
using DotNetNuke.Web.Mvc.Routing;
using Moq;
using NUnit.Framework;
// ReSharper disable ObjectCreationAsStatement

namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
    [TestFixture]
    public class DnnUrlHelperTests
    {
        [SetUp]
        public void SetUp()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void Constructor_Throws_On_Null_ViewContext()
        {
            //Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnUrlHelper(null));
        }

        [Test]
        public void Constructor_Throws_On_Invalid_Controller_Property()
        {
            //Arrange
            var mockController = new Mock<ControllerBase>();
            var viewContext = new ViewContext {Controller = mockController.Object};

            //Act,Assert
            Assert.Throws<InvalidOperationException>(() => new DnnUrlHelper(viewContext));
        }

        [Test]
        public void Constructor_Sets_ModuleContext_Property()
        {
            //Arrange
            var mockController = new Mock<ControllerBase>();
            var mockDnnController = mockController.As<IDnnController>();
            var expectedContext = new ModuleInstanceContext();
            mockDnnController.Setup(c => c.ModuleContext).Returns(expectedContext);
            var viewContext = new ViewContext {Controller = mockController.Object};

            //Act
            var helper = new DnnUrlHelper(viewContext);

            //Assert
            Assert.AreEqual(expectedContext, helper.ModuleContext);
        }

        [Test]
        public void Action_Method_Calls_ModuleRouteProvider()
        {
            //Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            //Act
            helper.Action("foo");

            //Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
        }

        [Test]
        public void Action_Overload_1__Method_Calls_ModuleRouteProvider()
        {
            //Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            //Act
            helper.Action("foo", new RouteValueDictionary());

            //Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
        }

        [Test]
        public void Action_Overload_2_Method_Calls_ModuleRouteProvider()
        {
            //Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            //Act
            helper.Action("foo", "bar");

            //Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
        }

        [Test]
        public void Action_Overload_3_Method_Calls_ModuleRouteProvider()
        {
            //Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            //Act
            helper.Action("foo", "bar", new RouteValueDictionary());

            //Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
        }

        [Test]
        public void Action_Overload_4_Method_Calls_ModuleRouteProvider()
        {
            //Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            //Act
            helper.Action("foo", new { id = 5 });

            //Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
        }

        [Test]
        public void Action_Overload_5_Method_Calls_ModuleRouteProvider()
        {
            //Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            //Act
            helper.Action("foo", "bar", new { id = 5 });

            //Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
        }

        [Test]
        public void Content_Method_Calls_Returns_Correct_Url()
        {
            //Arrange
            var context = new ModuleInstanceContext();
            var helper = ArrangeHelper(context, "http://foo.com/foo");
            string expectedResult = "/foo/test.css";

            //Act
            var url = helper.Content("~/test.css");

            //Assert
            Assert.IsNotNull(url);
            Assert.True(expectedResult.Equals(url));
        }

        [Test]
        public void IsLocalUrl_Method_Calls_Returns_Correct_Result()
        {
            //Arrange
            var context = new ModuleInstanceContext();
            var helper = ArrangeHelper(context, "http://foo.com");

            //Act
            var withOuterUrl = helper.IsLocalUrl("http://dnnsoftware.com");
            var withLocalUrl = helper.IsLocalUrl("~/foo/foo.html");
            
            //Assert
            Assert.IsFalse(withOuterUrl);
            Assert.IsTrue(withLocalUrl);
        }

        [Test]
        public void GenerateUrl_Method_Passes_Correct_RouteValueCollection_To_ModuleRouteProvider()
        {
            //Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);


            RouteValueDictionary routeValues = null;
            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            mockRouteProvider.Setup(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext))
                                .Callback<RouteValueDictionary, ModuleInstanceContext>((r, c) => routeValues = r);

            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            //Act
            helper.Action("foo", "bar", new { id = 5 });

            //Assert
            Assert.AreEqual(3, routeValues.Values.Count);
            Assert.IsTrue(routeValues.ContainsKey("action"));
            Assert.IsTrue(routeValues.ContainsKey("controller"));
            Assert.IsTrue(routeValues.ContainsKey("id"));
            Assert.AreEqual("foo", (string)routeValues["action"]);
            Assert.AreEqual("bar", (string)routeValues["controller"]);
            Assert.AreEqual(5, (int)routeValues["id"]);
        }

        private static DnnUrlHelper ArrangeHelper(ModuleInstanceContext expectedContext, string url = null)
        {
            var mockController = new Mock<ControllerBase>();
            var mockDnnController = mockController.As<IDnnController>();

            var routeData = new RouteData();
            routeData.Values["controller"] = "bar";
            routeData.Values["action"] = "foo";
            var context = MockHelper.CreateMockControllerContext(url!=null? MockHelper.CreateMockHttpContext(url):null, routeData);
            
            mockDnnController.Setup(c => c.ModuleContext).Returns(expectedContext);
            mockDnnController.Setup(c => c.ControllerContext).Returns(context);

            var viewContext = new ViewContext { Controller = mockController.Object };

            if (!string.IsNullOrEmpty(url))
                viewContext.RequestContext = new RequestContext(MockHelper.CreateMockHttpContext(url), routeData);

            return new DnnUrlHelper(viewContext);
        }
    }
}
