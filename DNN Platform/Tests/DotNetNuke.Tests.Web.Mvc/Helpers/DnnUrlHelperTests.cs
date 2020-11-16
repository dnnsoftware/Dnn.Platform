// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ObjectCreationAsStatement
namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
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
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnUrlHelper(null));
        }

        [Test]
        public void Constructor_Throws_On_Null_RequestContext()
        {
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnUrlHelper(null, new Mock<IDnnController>().Object));
        }

        [Test]
        public void Constructor_Throws_On_Null_Controller()
        {
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnUrlHelper(new Mock<RequestContext>().Object, null));
        }

        [Test]
        public void Constructor_Throws_On_Invalid_Controller_Property()
        {
            // Arrange
            var mockController = new Mock<ControllerBase>();
            var viewContext = new ViewContext { Controller = mockController.Object };

            // Act,Assert
            Assert.Throws<InvalidOperationException>(() => new DnnUrlHelper(viewContext));
        }

        [Test]
        public void ViewContext_Constructor_Sets_ModuleContext_Property()
        {
            // Arrange
            var mockController = new Mock<ControllerBase>();
            var mockDnnController = mockController.As<IDnnController>();
            var expectedContext = new ModuleInstanceContext();
            mockDnnController.Setup(c => c.ModuleContext).Returns(expectedContext);
            var viewContext = new ViewContext { Controller = mockController.Object };

            // Act
            var helper = new DnnUrlHelper(viewContext);

            // Assert
            Assert.AreEqual(expectedContext, helper.ModuleContext);
        }

        [Test]
        public void RequestContext_Constructor_Sets_ModuleContext_Property()
        {
            // Arrange
            var expectedContext = new ModuleInstanceContext();

            var mockController = new Mock<IDnnController>();
            mockController.SetupGet(c => c.ModuleContext)
                          .Returns(expectedContext);

            var requestContext = new RequestContext();

            // Act
            var helper = new DnnUrlHelper(requestContext, mockController.Object);

            // Assert
            Assert.NotNull(helper);
            Assert.AreEqual(expectedContext, helper.ModuleContext);
        }

        [Test]
        public void Action_Method_ViewContext_RetrievesRawUrl()
        {
            // Arrange
            var expectedContext = new ModuleInstanceContext();
            var rawUrl = "http://base.url/";
            var helper = ArrangeHelper(expectedContext, rawUrl);

            // Act
            var result = helper.Action();

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(rawUrl, result);
        }

        [Test]
        public void Action_Method_RequestContext_RetrievesRawUrl()
        {
            // Arrange
            var expectedContext = new ModuleInstanceContext();
            var rawUrl = "http://base.url/";
            var helper = ArrangeHelper(expectedContext, rawUrl, false);

            // Act
            var result = helper.Action();

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(rawUrl, result);
        }

        [Test]
        public void Action_Method_Calls_ModuleRouteProvider()
        {
            // Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo");

            // Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
        }

        [Test]
        public void Action_Overload_1__Method_Calls_ModuleRouteProvider()
        {
            // Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo", new RouteValueDictionary());

            // Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
        }

        [Test]
        public void Action_Overload_2_Method_Calls_ModuleRouteProvider()
        {
            // Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo", "bar");

            // Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
        }

        [Test]
        public void Action_Overload_3_Method_Calls_ModuleRouteProvider()
        {
            // Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo", "bar", new RouteValueDictionary());

            // Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
        }

        [Test]
        public void Action_Overload_4_Method_Calls_ModuleRouteProvider()
        {
            // Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo", new { id = 5 });

            // Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
        }

        [Test]
        public void Action_Overload_5_Method_Calls_ModuleRouteProvider()
        {
            // Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo", "bar", new { id = 5 });

            // Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
        }

        [Test]
        public void Content_Method_Calls_Returns_Correct_Url()
        {
            // Arrange
            var context = new ModuleInstanceContext();
            var helper = ArrangeHelper(context, "http://foo.com/foo");
            string expectedResult = "/foo/test.css";

            // Act
            var url = helper.Content("~/test.css");

            // Assert
            Assert.IsNotNull(url);
            Assert.True(expectedResult.Equals(url));
        }

        [Test]
        public void IsLocalUrl_Method_Calls_Returns_Correct_Result()
        {
            // Arrange
            var context = new ModuleInstanceContext();
            var helper = ArrangeHelper(context, "http://foo.com");

            // Act
            var withOuterUrl = helper.IsLocalUrl("http://dnnsoftware.com");
            var withLocalUrl = helper.IsLocalUrl("~/foo/foo.html");

            // Assert
            Assert.IsFalse(withOuterUrl);
            Assert.IsTrue(withLocalUrl);
        }

        [Test]
        public void GenerateUrl_Method_Passes_Correct_RouteValueCollection_To_ModuleRouteProvider()
        {
            // Arrange
            var expectedContext = new ModuleInstanceContext();
            var helper = ArrangeHelper(expectedContext);

            RouteValueDictionary routeValues = null;
            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
            mockRouteProvider.Setup(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext))
                                .Callback<RouteValueDictionary, ModuleInstanceContext>((r, c) => routeValues = r);

            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo", "bar", new { id = 5 });

            // Assert
            Assert.AreEqual(3, routeValues.Values.Count);
            Assert.IsTrue(routeValues.ContainsKey("action"));
            Assert.IsTrue(routeValues.ContainsKey("controller"));
            Assert.IsTrue(routeValues.ContainsKey("id"));
            Assert.AreEqual("foo", (string)routeValues["action"]);
            Assert.AreEqual("bar", (string)routeValues["controller"]);
            Assert.AreEqual(5, (int)routeValues["id"]);
        }

        private static DnnUrlHelper ArrangeHelper(ModuleInstanceContext expectedContext, string url = null, bool isViewContext = true)
        {
            var mockController = new Mock<ControllerBase>();
            var mockDnnController = mockController.As<IDnnController>();

            var routeData = new RouteData();
            routeData.Values["controller"] = "bar";
            routeData.Values["action"] = "foo";
            var context = MockHelper.CreateMockControllerContext(url != null ? MockHelper.CreateMockHttpContext(url) : null, routeData);

            mockDnnController.Setup(c => c.ModuleContext).Returns(expectedContext);
            mockDnnController.Setup(c => c.ControllerContext).Returns(context);

            if (isViewContext)
            {
                var viewContext = new ViewContext { Controller = mockController.Object };

                if (!string.IsNullOrEmpty(url))
                {
                    viewContext.RequestContext = new RequestContext(MockHelper.CreateMockHttpContext(url), routeData);
                }

                return new DnnUrlHelper(viewContext);
            }

            var requestContext = new RequestContext(MockHelper.CreateMockHttpContext(url ?? "http://base/"), routeData);
            return new DnnUrlHelper(requestContext, mockDnnController.Object);
        }
    }
}
