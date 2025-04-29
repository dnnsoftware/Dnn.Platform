// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Folder;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Logging;
using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Tests.Utilities.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

[TestFixture]
public class FileContentTypeManagerTests
{
    [SetUp]

    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        var mockApplicationStatusInfo = new Mock<IApplicationStatusInfo>();
        mockApplicationStatusInfo.Setup(info => info.Status).Returns(UpgradeStatus.Install);
        serviceCollection.AddTransient<IApplicationStatusInfo>(container => mockApplicationStatusInfo.Object);
        serviceCollection.AddTransient<IEventLogger>(container => Mock.Of<IEventLogger>());
        serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
        serviceCollection.AddTransient<IHostSettingsService, HostController>();
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();

        var _mockData = MockComponentProvider.CreateDataProvider();
        var _mockCache = MockComponentProvider.CreateDataCacheProvider();
        var _globals = new Mock<IGlobals>();
        var _cbo = new Mock<ICBO>();

        _mockData.Setup(m => m.GetProviderPath()).Returns(string.Empty);

        TestableGlobals.SetTestableInstance(_globals.Object);
        CBO.SetTestableInstance(_cbo.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Globals.DependencyProvider = null;
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
