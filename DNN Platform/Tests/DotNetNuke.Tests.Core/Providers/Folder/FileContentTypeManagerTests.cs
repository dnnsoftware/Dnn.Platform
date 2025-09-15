// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FileContentTypeManagerTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            var mockData = MockComponentProvider.CreateDataProvider();
            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            var mockGlobals = new Mock<IGlobals>();
            var mockCbo = new Mock<ICBO>();

            mockData.Setup(m => m.GetProviderPath()).Returns(string.Empty);

            TestableGlobals.SetTestableInstance(mockGlobals.Object);
            CBO.SetTestableInstance(mockCbo.Object);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(mockCache.Object);
                    services.AddSingleton(mockData.Object);
                    services.AddSingleton(mockGlobals.Object);
                    services.AddSingleton(mockCbo.Object);
                });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
            TestableGlobals.ClearInstance();
            CBO.ClearInstance();
        }

        [Test]
        public void GetContentType_Returns_Known_Value_When_Extension_Is_Not_Managed()
        {
            const string notManagedExtension = "asdf609vas21AS:F,l/&%/(%$";

            var contentType = FileContentTypeManager.Instance.GetContentType(notManagedExtension);

            Assert.That(contentType, Is.EqualTo("application/octet-stream"));
        }

        [Test]
        public void GetContentType_Returns_Correct_Value_For_Extension()
        {
            const string notManagedExtension = "htm";

            var contentType = FileContentTypeManager.Instance.GetContentType(notManagedExtension);

            Assert.That(contentType, Is.EqualTo("text/html"));
        }
    }
}
