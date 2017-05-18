// // DotNetNuke® - http://www.dotnetnuke.com
// // Copyright (c) 2002-2017
// // by DotNetNuke Corporation
// // 
// // Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// // documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// // the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// // to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// // 
// // The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// // of the Software.
// // 
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// // TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// // THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// // CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// // DEALINGS IN THE SOFTWARE.

using System.Reflection;

using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Tests.Instance.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Tests.Core.Framework.JavaScriptLibraries
{
    public class JavaScriptTests
    {
        private const string ScriptPrefix = "JSL.";

        private int libraryIdCounter = 20;

        private HttpContextBase _httpContext;

        [SetUp]
        public void Setup()
        {
            //fix Globals.Status
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
            //Arrange
            int JavaScriptLibraryID = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            //Act
            JavaScript.RequestRegistration("Test");

            //Assert
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCase()
        {
            //Arrange
            int JavaScriptLibraryID = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            //Act
            JavaScript.RequestRegistration("test");

            //Assert
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
        }

        [Test]
        public void CanRegisterLibraryByNameAndVersion()
        {
            //Arrange
            int JavaScriptLibraryID = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            //Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2));

            //Assert
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
        }

        [Test]
        public void CanRegisterLibraryByNameAndExactVersion()
        {
            //Arrange
            int JavaScriptLibraryID = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            //Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2), SpecificVersion.Exact);

            //Assert
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCaseAndExactVersion()
        {
            //Arrange
            int JavaScriptLibraryID = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            //Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 2), SpecificVersion.Exact);

            //Assert
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
        }

        [Test]
        public void FailToRegisterLibraryByNameAndMismatchedVersion()
        {
            //Arrange
            int JavaScriptLibraryID = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            //Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 0));

            //Assert
            Assert.AreNotEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
        }

        [Test]
        public void FailToRegisterLibraryByNameAndMismatchedExactVersion()
        {
            //Arrange
            int JavaScriptLibraryID = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            //Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 0), SpecificVersion.Exact);

            //Assert
            Assert.AreNotEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
        }

        [Test]
        public void CanRegisterLibraryByNameAndSameMinorVersion()
        {
            //Arrange
            int JavaScriptLibraryID = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            //Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 0), SpecificVersion.LatestMinor);

            //Assert
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCaseAndSameMinorVersion()
        {
            //Arrange
            int JavaScriptLibraryID = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            //Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 0), SpecificVersion.LatestMinor);

            //Assert
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMinorVersion()
        {
            //Arrange
            int lowerVersionJavaScriptLibraryId = libraryIdCounter++;
            int higherVersionJavaScriptLibraryId = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
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

            //Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2), SpecificVersion.LatestMinor);

            //Assert
            Assert.AreNotEqual(true, this._httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId]);
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId]);
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase()
        {
            //Arrange
            int lowerVersionJavaScriptLibraryId = libraryIdCounter++;
            int higherVersionJavaScriptLibraryId = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
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

            //Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 2), SpecificVersion.LatestMinor);

            //Assert
            Assert.AreNotEqual(true, this._httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId]);
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId]);
        }

        [Test]
        public void CanRegisterLibraryByNameAndSameMajorVersion()
        {
            //Arrange
            int JavaScriptLibraryID = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            //Act
            JavaScript.RequestRegistration("Test", new Version(2, 1, 1), SpecificVersion.LatestMajor);

            //Assert
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCaseAndSameMajorVersion()
        {
            //Arrange
            int JavaScriptLibraryID = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            //Act
            JavaScript.RequestRegistration("test", new Version(2, 1, 1), SpecificVersion.LatestMajor);

            //Assert
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + JavaScriptLibraryID]);
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMajorVersion()
        {
            //Arrange
            int lowerVersionJavaScriptLibraryId = libraryIdCounter++;
            int higherVersionJavaScriptLibraryId = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
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

            //Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2), SpecificVersion.LatestMajor);

            //Assert
            Assert.AreNotEqual(true, this._httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId]);
            Assert.AreEqual(true, this._httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId]);
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase()
        {
            //Arrange
            int lowerVersionJavaScriptLibraryId = libraryIdCounter++;
            int higherVersionJavaScriptLibraryId = libraryIdCounter++;
            SetupJavaScriptLibraryController(new JavaScriptLibrary
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

            //Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 2), SpecificVersion.LatestMajor);

            //Assert
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