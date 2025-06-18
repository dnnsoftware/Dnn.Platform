// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.InternalServices
{
    using System.Data;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;
    using DotNetNuke.Web.InternalServices;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    /// <summary>Tests FileUploadController methods.</summary>
    [TestFixture]
    public class FileUploadControllerTests
    {
        private Mock<DataProvider> mockDataProvider;
        private FileUploadController testInstance;
        private Mock<CachingProvider> mockCachingProvider;
        private Mock<IPortalController> mockPortalController;
        private TestSynchronizationContext synchronizationContext = new TestSynchronizationContext();
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
            this.SetupDataProvider();
            this.SetupCachingProvider();
            this.SetupPortalSettings();
            this.SetupServiceProvider();
            this.SetupSynchronizationContext();
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
            this.testInstance?.Dispose();
        }

        [Test]
        [SetCulture("tr-TR")]
        public async Task UploadFromLocal_ShouldUploadFile_WithTrCultureAsync()
        {
            var formDataContent = new MultipartFormDataContent();
            formDataContent.Add(new StringContent("Hello World!"), "\"postfile\"", "\"testing\"");

            var request = new HttpRequestMessage();
            request.Content = formDataContent;
            this.testInstance.Request = request;
            await this.testInstance.UploadFromLocal(-1);

            Assert.That(this.synchronizationContext.IsUploadFileCalled(), Is.True);
        }

        private void SetupPortalSettings()
        {
            this.mockPortalController = MockComponentProvider.CreatePortalController();
            this.mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(new PortalSettings() { PortalId = 0 });
            PortalController.SetTestableInstance(this.mockPortalController.Object);
        }

        private void SetupDataProvider()
        {
            this.mockDataProvider = MockComponentProvider.CreateDataProvider();
            this.mockDataProvider.Setup(x => x.GetListEntriesByListName("ImageTypes", string.Empty, It.IsAny<int>()))
                .Returns(Mock.Of<IDataReader>());
        }

        private void SetupCachingProvider()
        {
            this.mockCachingProvider = MockComponentProvider.CreateDataCacheProvider();
            this.mockCachingProvider.Setup(x => x.GetItem(It.IsAny<string>()))
                .Returns(Enumerable.Empty<ListEntryInfo>());
        }

        private void SetupServiceProvider()
        {
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.mockPortalController.Object);
                    services.AddSingleton(this.mockCachingProvider.Object);
                    services.AddSingleton(this.mockDataProvider.Object);
                });
        }

        private void SetupSynchronizationContext()
        {
            this.synchronizationContext = new TestSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);
            this.testInstance = new FileUploadController();
        }

        private class TestSynchronizationContext : SynchronizationContext
        {
            private bool isUploadFileCalled;

            public override void Post(SendOrPostCallback d, object state)
            {
                d(state);
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                isUploadFileCalled = true;
            }

            public bool IsUploadFileCalled()
            {
                return isUploadFileCalled;
            }
        }
    }
}
