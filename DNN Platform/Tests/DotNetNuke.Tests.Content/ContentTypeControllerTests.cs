// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Content;

using System;
using System.Linq;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Content.Mocks;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

/// <summary>  Summary description for ContentTypeTests.</summary>
[TestFixture]
public class ContentTypeControllerTests
{
    private Mock<CachingProvider> mockCache;

    [SetUp]

    public void SetUp()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
        serviceCollection.AddTransient<IApplicationStatusInfo>(container => new DotNetNuke.Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>()));
        serviceCollection.AddTransient<IHostSettingsService, HostController>();
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();

        // Register MockCachingProvider
        this.mockCache = MockComponentProvider.CreateNew<CachingProvider>();
        MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(string.Empty);
    }

    [TearDown]
    public void TearDown()
    {
        Globals.DependencyProvider = null;
        MockComponentProvider.ResetContainer();
    }

    [Test]
    public void ContentTypeController_AddContentType_Throws_On_Null_ContentType()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        // Act, Arrange
        Assert.Throws<ArgumentNullException>(() => contentTypeController.AddContentType(null));
    }

    [Test]
    public void ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        ContentType contentType = ContentTestHelper.CreateValidContentType();

        // Act
        int contentTypeId = contentTypeController.AddContentType(contentType);

        // Assert
        mockDataService.Verify(ds => ds.AddContentType(contentType));
    }

    [Test]
    public void ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.AddContentType(It.IsAny<ContentType>())).Returns(Constants.CONTENTTYPE_AddContentTypeId);
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        var contentType = ContentTestHelper.CreateValidContentType();

        // Act
        int contentTypeId = contentTypeController.AddContentType(contentType);

        // Assert
        Assert.That(contentTypeId, Is.EqualTo(Constants.CONTENTTYPE_AddContentTypeId));
    }

    [Test]
    public void ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.AddContentType(It.IsAny<ContentType>())).Returns(Constants.CONTENTTYPE_AddContentTypeId);
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        var contentType = ContentTestHelper.CreateValidContentType();

        // Act
        contentTypeController.AddContentType(contentType);

        // Assert
        Assert.That(contentType.ContentTypeId, Is.EqualTo(Constants.CONTENTTYPE_AddContentTypeId));
    }

    [Test]
    public void ContentTypeController_DeleteContentType_Throws_On_Null_ContentType()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        // Act, Arrange
        Assert.Throws<ArgumentNullException>(() => contentTypeController.DeleteContentType(null));
    }

    [Test]
    public void ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        ContentType contentType = ContentTestHelper.CreateValidContentType();
        contentType.ContentTypeId = Null.NullInteger;

        // Act, Arrange
        Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.DeleteContentType(contentType));
    }

    [Test]
    public void ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        var contentType = ContentTestHelper.CreateValidContentType();
        contentType.ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

        // Act
        contentTypeController.DeleteContentType(contentType);

        // Assert
        mockDataService.Verify(ds => ds.DeleteContentType(contentType));
    }

    [Test]

    public void ContentTypeController_GetContentTypes_Calls_DataService()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetContentTypes()).Returns(MockHelper.CreateValidContentTypesReader(Constants.CONTENTTYPE_ValidContentTypeCount));
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        // Act
        var contentTypes = contentTypeController.GetContentTypes();

        // Assert
        mockDataService.Verify(ds => ds.GetContentTypes());
    }

    [Test]

    public void ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetContentTypes()).Returns(MockHelper.CreateEmptyContentTypeReader());
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        // Act
        var contentTypes = contentTypeController.GetContentTypes();

        // Assert
        Assert.That(contentTypes, Is.Not.Null);
        Assert.That(contentTypes.Count(), Is.EqualTo(0));
    }

    [Test]

    public void ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetContentTypes()).Returns(MockHelper.CreateValidContentTypesReader(Constants.CONTENTTYPE_ValidContentTypeCount));
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        // Act
        var contentTypes = contentTypeController.GetContentTypes();

        // Assert
        Assert.That(contentTypes.Count(), Is.EqualTo(Constants.CONTENTTYPE_ValidContentTypeCount));
    }

    [Test]
    public void ContentTypeController_UpdateContentType_Throws_On_Null_ContentType()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        // Act, Arrange
        Assert.Throws<ArgumentNullException>(() => contentTypeController.UpdateContentType(null));
    }

    [Test]
    public void ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        var contentType = ContentTestHelper.CreateValidContentType();
        contentType.ContentType = Constants.CONTENTTYPE_InValidContentType;

        Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.UpdateContentType(contentType));
    }

    [Test]
    public void ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        var contentTypeController = new ContentTypeController(mockDataService.Object);

        var contentType = ContentTestHelper.CreateValidContentType();
        contentType.ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId;
        contentType.ContentType = Constants.CONTENTTYPE_UpdateContentType;

        // Act
        contentTypeController.UpdateContentType(contentType);

        // Assert
        mockDataService.Verify(ds => ds.UpdateContentType(contentType));
    }
}
