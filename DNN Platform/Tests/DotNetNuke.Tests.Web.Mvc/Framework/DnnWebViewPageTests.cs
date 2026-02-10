// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework
{
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Web.Mvc.Fakes;
    using DotNetNuke.Web.Mvc.Framework;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Helpers;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DnnWebViewPageTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
            this.serviceProvider = FakeServiceProvider.Setup(services =>
            {
                services.AddSingleton(Mock.Of<IClientResourceController>());
                services.AddSingleton(Mock.Of<IApplicationStatusInfo>());
                services.AddSingleton(Mock.Of<IEventLogger>());
            });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
        }

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
            Assert.That(mockViewPage.Object.Dnn, Is.Not.Null);
            Assert.That(mockViewPage.Object.Dnn, Is.InstanceOf<DnnHelper>());
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
            Assert.That(mockViewPage.Object.Html, Is.Not.Null);
            Assert.That(mockViewPage.Object.Html, Is.InstanceOf<DnnHtmlHelper>());
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
            Assert.That(mockViewPage.Object.Html, Is.Not.Null);
            Assert.That(mockViewPage.Object.Html, Is.InstanceOf<DnnHtmlHelper<Dog>>());
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
            Assert.That(mockViewPage.Object.Url, Is.Not.Null);
            Assert.That(mockViewPage.Object.Url, Is.InstanceOf<DnnUrlHelper>());
        }
    }
}
