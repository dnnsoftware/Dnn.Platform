namespace DotNetNuke.Tests.Web.InternalServices;

using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Abstractions;
using Abstractions.Application;
using Abstractions.Logging;

using Common;
using Common.Lists;

using Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Web.InternalServices;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Services.Cache;

using Utilities.Mocks;
/// <summary>Tests FileUploadController methods.</summary>
[TestFixture]
public class FileUploadControllerTests
{
    private Mock<DataProvider> _mockDataProvider;
    private FileUploadController _testInstance;
    private Mock<CachingProvider> _mockCachingProvider;
    private Mock<IPortalController> _mockPortalController;
    private TestSynchronizationContext _synchronizationContext = new TestSynchronizationContext();

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
        this._testInstance?.Dispose();
    }

    [Test]
    [SetCulture("tr-TR")]

    public async Task UploadFromLocal_ShouldUploadFile_WithTrCultureAsync()
    {
        var formDataContent = new MultipartFormDataContent();
        formDataContent.Add(new StringContent("Hello World!"), "\"postfile\"", "\"testing\"");

        var request = new HttpRequestMessage();
        request.Content = formDataContent;
        this._testInstance.Request = request;
        await this._testInstance.UploadFromLocal(-1);

        Assert.That(_synchronizationContext.IsUploadFileCalled(), Is.True);
    }

    private void SetupPortalSettings()
    {
        _mockPortalController = MockComponentProvider.CreatePortalController();
        _mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(new PortalSettings() { PortalId = 0 });
        PortalController.SetTestableInstance(_mockPortalController.Object);
    }

    private void SetupDataProvider()
    {
        this._mockDataProvider = MockComponentProvider.CreateDataProvider();
        this._mockDataProvider.Setup(x => x.GetListEntriesByListName("ImageTypes", string.Empty, It.IsAny<int>()))
            .Returns(Mock.Of<IDataReader>());
    }

    private void SetupCachingProvider()
    {
        this._mockCachingProvider = MockComponentProvider.CreateDataCacheProvider();
        this._mockCachingProvider.Setup(x => x.GetItem(It.IsAny<string>()))
            .Returns(Enumerable.Empty<ListEntryInfo>());
    }

    private void SetupServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IApplicationStatusInfo>(
            container => new Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>()));
        serviceCollection.AddTransient(container => Mock.Of<INavigationManager>());
        serviceCollection.AddTransient(container => Mock.Of<IEventLogger>());
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
    }

    private void SetupSynchronizationContext()
    {
        _synchronizationContext = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
        this._testInstance = new FileUploadController();
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
