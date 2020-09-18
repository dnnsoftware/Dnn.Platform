// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework
{
    using System.Web.Mvc;

    using DotNetNuke.Tests.Web.Mvc.Fakes;
    using DotNetNuke.Web.Mvc.Framework;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Helpers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DnnWebViewPageTests
    {
        [Test]
        public void InitHelpers_Sets_Dnn_Property()
        {
            // Arrange
            var mockViewPage = new Mock<DnnWebViewPage>() { CallBase = true };
            var mockController = new Mock<ControllerBase>();
            var mockDnnController = mockController.As<IDnnController>();
            var viewContext = new ViewContext
            {
                Controller = mockController.Object,
            };
            mockViewPage.Object.ViewContext = viewContext;

            // Act
            mockViewPage.Object.InitHelpers();

            // Assert
            Assert.NotNull(mockViewPage.Object.Dnn);
            Assert.IsInstanceOf<DnnHelper>(mockViewPage.Object.Dnn);
        }

        [Test]
        public void InitHelpers_Sets_Html_Property()
        {
            // Arrange
            var mockViewPage = new Mock<DnnWebViewPage>() { CallBase = true };
            var mockController = new Mock<ControllerBase>();
            var mockDnnController = mockController.As<IDnnController>();
            var viewContext = new ViewContext
            {
                Controller = mockController.Object,
            };
            mockViewPage.Object.ViewContext = viewContext;

            // Act
            mockViewPage.Object.InitHelpers();

            // Assert
            Assert.NotNull(mockViewPage.Object.Html);
            Assert.IsInstanceOf<DnnHtmlHelper>(mockViewPage.Object.Html);
        }

        [Test]
        public void InitHelpers_Sets_Dnn_Property_For_Strongly_Typed_Helper()
        {
            // Arrange
            var mockViewPage = new Mock<DnnWebViewPage<Dog>>() { CallBase = true };
            var mockController = new Mock<ControllerBase>();
            var mockDnnController = mockController.As<IDnnController>();
            var viewContext = new ViewContext
            {
                Controller = mockController.Object,
            };
            mockViewPage.Object.ViewContext = viewContext;

            // Act
            mockViewPage.Object.InitHelpers();

            // Assert
            Assert.NotNull(mockViewPage.Object.Html);
            Assert.IsInstanceOf<DnnHtmlHelper<Dog>>(mockViewPage.Object.Html);
        }

        [Test]
        public void InitHelpers_Sets_Url_Property()
        {
            // Arrange
            var mockViewPage = new Mock<DnnWebViewPage>() { CallBase = true };
            var mockController = new Mock<ControllerBase>();
            var mockDnnController = mockController.As<IDnnController>();
            var viewContext = new ViewContext
            {
                Controller = mockController.Object,
            };
            mockViewPage.Object.ViewContext = viewContext;

            // Act
            mockViewPage.Object.InitHelpers();

            // Assert
            Assert.NotNull(mockViewPage.Object.Url);
            Assert.IsInstanceOf<DnnUrlHelper>(mockViewPage.Object.Url);
        }
    }
}
