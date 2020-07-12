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

    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Tests.Instance.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    public class JavaScriptTests
    {
        private const string ScriptPrefix = "JSL.";

        private int libraryIdCounter = 20;

        private HttpContextBase _httpContext;

        [SetUp]
        public void Setup()
        {
            // fix Globals.Status
            var status = typeof(Globals).GetField("_status", BindingFlags.Static | BindingFlags.NonPublic);
            status.SetValue(null, Globals.UpgradeStatus.None);

            var httpContextMock = new Mock<HttpContextBase> { DefaultValue = DefaultValue.Mock, };
            httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object>());
            this._httpContext = httpContextMock.Object;
            HttpContextSource.RegisterInstance(this._httpContext);

            MockComponentProvider.CreateLocalizationProvider();
            var dataProviderMock = MockComponentProvider.CreateDataProvider();
            dataProviderMock.Setup(dp => dp.GetProviderPath()).Returns(string.Empty);
            dataProviderMock.Setup(dp => dp.GetVersion()).Returns(DotNetNukeContext.Current.Application.Version);
        }

        [TearDown]
        public void TearDown()
        {
            UnitTestHelper.ClearHttpContext();
            JavaScriptLibraryController.ClearInstance();
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
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
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
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
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
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
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
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
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
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
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
            Assert.AreNotEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
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
            Assert.AreNotEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
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
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
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
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
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

            // Assert
            Assert.AreNotEqual(true, this._httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId]);
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId]);
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

            // Assert
            Assert.AreNotEqual(true, this._httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId]);
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId]);
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
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
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
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
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

            // Assert
            Assert.AreNotEqual(true, this._httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId]);
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId]);
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

            // Assert
            Assert.AreNotEqual(true, this._httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId]);
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId]);
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
