// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Framework.JavaScriptLibraries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Tests.Instance.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    public class JavaScriptTests
    {
        private const string ScriptPrefix = "JSL.";

        private int libraryIdCounter = 20;

        private HttpContextBase httpContext;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            var mockApplicationStatusInfo = new Mock<IApplicationStatusInfo>();
            mockApplicationStatusInfo.Setup(info => info.Status).Returns(UpgradeStatus.None);

            var mockApplication = new Mock<IApplicationInfo>();
            mockApplication.Setup(app => app.Version).Returns(new Version("1.0.0.0"));

            var dnnContext = new DotNetNukeContext(mockApplication.Object);

            MockComponentProvider.CreateLocalizationProvider();
            var dataProviderMock = MockComponentProvider.CreateDataProvider();
            dataProviderMock.Setup(dp => dp.GetProviderPath()).Returns(string.Empty);
            dataProviderMock.Setup(dp => dp.GetVersion()).Returns(dnnContext.Application.Version);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(mockApplicationStatusInfo.Object);
                    services.AddSingleton(mockApplication.Object);
                    services.AddSingleton<IDnnContext>(dnnContext);
                    services.AddSingleton(dataProviderMock.Object);
                });

            this.httpContext = HttpContextSource.Current;
        }

        [TearDown]
        public void TearDown()
        {
            JavaScriptLibraryController.ClearInstance();
            this.serviceProvider.Dispose();
        }

        [Test]
        public void CanRegisterLibraryByName()
        {
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test");

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCase()
        {
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("test");

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
        }

        [Test]
        public void CanRegisterLibraryByNameAndVersion()
        {
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2));

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
        }

        [Test]
        public void CanRegisterLibraryByNameAndExactVersion()
        {
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2), SpecificVersion.Exact);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCaseAndExactVersion()
        {
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 2), SpecificVersion.Exact);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
        }

        [Test]
        public void FailToRegisterLibraryByNameAndMismatchedVersion()
        {
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 0));

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.Not.EqualTo(true));
        }

        [Test]
        public void FailToRegisterLibraryByNameAndMismatchedExactVersion()
        {
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 0), SpecificVersion.Exact);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.Not.EqualTo(true));
        }

        [Test]
        public void CanRegisterLibraryByNameAndSameMinorVersion()
        {
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 0), SpecificVersion.LatestMinor);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCaseAndSameMinorVersion()
        {
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 0), SpecificVersion.LatestMinor);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMinorVersion()
        {
            // Arrange
            int lowerVersionJavaScriptLibraryId = this.libraryIdCounter++;
            int higherVersionJavaScriptLibraryId = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(
                new JavaScriptLibrary
                {
                    JavaScriptLibraryID = lowerVersionJavaScriptLibraryId,
                    LibraryName = "Test",
                    Version = new Version(2, 1, 0),
                }, new JavaScriptLibrary
                {
                    JavaScriptLibraryID = higherVersionJavaScriptLibraryId,
                    LibraryName = "Test",
                    Version = new Version(3, 3, 3),
                });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2), SpecificVersion.LatestMinor);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(this.httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId], Is.Not.EqualTo(true));
                Assert.That(this.httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId], Is.EqualTo(true));
            });
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase()
        {
            // Arrange
            int lowerVersionJavaScriptLibraryId = this.libraryIdCounter++;
            int higherVersionJavaScriptLibraryId = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(
                new JavaScriptLibrary
                {
                    JavaScriptLibraryID = lowerVersionJavaScriptLibraryId,
                    LibraryName = "test",
                    Version = new Version(2, 1, 0),
                }, new JavaScriptLibrary
                {
                    JavaScriptLibraryID = higherVersionJavaScriptLibraryId,
                    LibraryName = "Test",
                    Version = new Version(3, 3, 3),
                });

            // Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 2), SpecificVersion.LatestMinor);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(this.httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId], Is.Not.EqualTo(true));
                Assert.That(this.httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId], Is.EqualTo(true));
            });
        }

        [Test]
        public void CanRegisterLibraryByNameAndSameMajorVersion()
        {
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 1, 1), SpecificVersion.LatestMajor);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCaseAndSameMajorVersion()
        {
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("test", new Version(2, 1, 1), SpecificVersion.LatestMajor);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMajorVersion()
        {
            // Arrange
            int lowerVersionJavaScriptLibraryId = this.libraryIdCounter++;
            int higherVersionJavaScriptLibraryId = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(
                new JavaScriptLibrary
                {
                    JavaScriptLibraryID = lowerVersionJavaScriptLibraryId,
                    LibraryName = "Test",
                    Version = new Version(1, 2, 2),
                }, new JavaScriptLibrary
                {
                    JavaScriptLibraryID = higherVersionJavaScriptLibraryId,
                    LibraryName = "Test",
                    Version = new Version(3, 3, 3),
                });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2), SpecificVersion.LatestMajor);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(this.httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId], Is.Not.EqualTo(true));
                Assert.That(this.httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId], Is.EqualTo(true));
            });
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase()
        {
            // Arrange
            int lowerVersionJavaScriptLibraryId = this.libraryIdCounter++;
            int higherVersionJavaScriptLibraryId = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(
                new JavaScriptLibrary
                {
                    JavaScriptLibraryID = lowerVersionJavaScriptLibraryId,
                    LibraryName = "test",
                    Version = new Version(1, 2, 2),
                }, new JavaScriptLibrary
                {
                    JavaScriptLibraryID = higherVersionJavaScriptLibraryId,
                    LibraryName = "Test",
                    Version = new Version(3, 3, 3),
                });

            // Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 2), SpecificVersion.LatestMajor);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(this.httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId], Is.Not.EqualTo(true));
                Assert.That(this.httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId], Is.EqualTo(true));
            });
        }

        private void SetupJavaScriptLibraryController(params JavaScriptLibrary[] libraries)
        {
            var libraryController = new Mock<IJavaScriptLibraryController>();
            libraryController.Setup(lc => lc.GetLibraries()).Returns(libraries);
            libraryController.Setup(lc => lc.GetLibrary(It.IsAny<Func<JavaScriptLibrary, bool>>())).Returns((Func<JavaScriptLibrary, bool> predicate) => libraries.SingleOrDefault(predicate));
            libraryController.Setup(lc => lc.GetLibraries(It.IsAny<Func<JavaScriptLibrary, bool>>())).Returns((Func<JavaScriptLibrary, bool> predicate) => libraries.Where(predicate));
            JavaScriptLibraryController.SetTestableInstance(libraryController.Object);
        }
    }
}
