// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Content;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Tests.Content.Mocks;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

/// <summary>  Summary description for ContentItemTests.</summary>
[TestFixture]
public class ContentControllerTests
{
    private const int ModuleSearchTypeId = 1;

    private Mock<CachingProvider> mockCache;
    private Mock<DataProvider> mockDataProvider;
    private Mock<Services.Search.Internals.ISearchHelper> mockSearchHelper;

    [SetUp]

    public void SetUp()
    {
        this.mockCache = MockComponentProvider.CreateNew<CachingProvider>();
        this.mockDataProvider = MockComponentProvider.CreateDataProvider();
        this.mockSearchHelper = new Mock<Services.Search.Internals.ISearchHelper>();

        Services.Search.Internals.SearchHelper.SetTestableInstance(this.mockSearchHelper.Object);

        this.mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>())).Returns<string>(
            (string searchTypeName) => new SearchType { SearchTypeName = searchTypeName, SearchTypeId = ModuleSearchTypeId });

        var serviceCollection = new ServiceCollection();
        var mockApplicationStatusInfo = new Mock<IApplicationStatusInfo>();
        mockApplicationStatusInfo.Setup(info => info.Status).Returns(UpgradeStatus.Install);

        serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
        serviceCollection.AddTransient<IApplicationStatusInfo>(container => mockApplicationStatusInfo.Object);
        serviceCollection.AddTransient<IHostSettingsService, HostController>();

        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        Globals.DependencyProvider = null;
        MockComponentProvider.ResetContainer();
    }

    [Test]
    public void ContentController_AddContentItem_Throws_On_Null_ContentItem()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        // Act, Arrange
        Assert.Throws<ArgumentNullException>(() => controller.AddContentItem(null));
    }

    [Test]
    public void ContentController_AddContentItem_Calls_DataService_On_Valid_Arguments()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();

        ContentController controller = new ContentController(mockDataService.Object);

        ComponentFactory.RegisterComponentInstance<IContentController>(controller);

        ContentItem content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Constants.CONTENT_ValidContentItemId;

        // Act
        int contentId = controller.AddContentItem(content);

        // Assert
        mockDataService.Verify(ds => ds.AddContentItem(content, It.IsAny<int>()));
    }

    [Test]
    public void ContentController_AddContentItem_Returns_ValidId_On_Valid_ContentItem()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.AddContentItem(It.IsAny<ContentItem>(), It.IsAny<int>())).Returns(Constants.CONTENT_AddContentItemId);
        ContentController controller = new ContentController(mockDataService.Object);

        ComponentFactory.RegisterComponentInstance<IContentController>(controller);

        ContentItem content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Constants.CONTENT_ValidContentItemId;

        // Act
        int contentId = controller.AddContentItem(content);

        // Assert
        Assert.That(contentId, Is.EqualTo(Constants.CONTENT_AddContentItemId));
    }

    [Test]
    public void ContentController_AddContentItem_Sets_ContentItemId_Property_On_Valid_ContentItem()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.AddContentItem(It.IsAny<ContentItem>(), It.IsAny<int>())).Returns(Constants.CONTENT_AddContentItemId);
        ContentController controller = new ContentController(mockDataService.Object);

        ComponentFactory.RegisterComponentInstance<IContentController>(controller);

        ContentItem content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Constants.CONTENT_ValidContentItemId;

        // Act
        int contentId = controller.AddContentItem(content);

        // Assert
        Assert.That(content.ContentItemId, Is.EqualTo(Constants.CONTENT_AddContentItemId));
    }

    [Test]
    public void ContentController_AddContentItem_Sets_CreatedByUserId_And_LastModifiedUserId()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.AddContentItem(It.IsAny<ContentItem>(), It.IsAny<int>())).Returns(Constants.CONTENT_AddContentItemId);
        ContentController controller = new ContentController(mockDataService.Object);

        ComponentFactory.RegisterComponentInstance<IContentController>(controller);

        ContentItem content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Constants.CONTENT_ValidContentItemId;
        content.CreatedByUserID = 1;

        // Act
        int contentId = controller.AddContentItem(content);

        // Assert
        mockDataService.Verify(
            service =>
                service.AddContentItem(It.IsAny<ContentItem>(), content.CreatedByUserID), Times.Once);
    }

    [Test]
    public void ContentController_AddContentItem_Sets_Override_CreatedByUserId_And_LastModifiedUserId()
    {
        // Arrange
        int expectedUserId = 5;
        Mock<IUserController> mockUserController = new Mock<IUserController>();
        mockUserController.Setup(user => user.GetCurrentUserInfo()).Returns(new UserInfo { UserID = expectedUserId });
        UserController.SetTestableInstance(mockUserController.Object);

        Mock<ICBO> mockCBO = new Mock<ICBO>();
        CBO.SetTestableInstance(mockCBO.Object);

        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.AddContentItem(It.IsAny<ContentItem>(), It.IsAny<int>())).Returns(Constants.CONTENT_AddContentItemId);

        ContentController controller = new ContentController(mockDataService.Object);

        ComponentFactory.RegisterComponentInstance<IContentController>(controller);

        ContentItem content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Constants.CONTENT_ValidContentItemId;

        // Act
        int contentId = controller.AddContentItem(content);

        // Assert
        mockDataService.Verify(
            service =>
                service.AddContentItem(It.IsAny<ContentItem>(), expectedUserId), Times.Once);

        // Cleanup
        UserController.ClearInstance();
        CBO.ClearInstance();
    }

    [Test]
    public void ContentController_DeleteContentItem_Throws_On_Null_ContentItem()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        // Act, Arrange
        Assert.Throws<ArgumentNullException>(() => controller.DeleteContentItem(null));
    }

    [Test]
    public void ContentController_DeleteContentItem_Throws_On_Negative_ContentItemId()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        ContentItem content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Null.NullInteger;

        // Act, Arrange
        Assert.Throws<ArgumentOutOfRangeException>(() => controller.DeleteContentItem(content));
    }

    [Test]
    public void ContentController_DeleteContentItem_Calls_DataService_On_Valid_ContentItemId()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        ContentItem content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Constants.CONTENT_DeleteContentItemId;

        // Act
        controller.DeleteContentItem(content);

        // Assert
        mockDataService.Verify(ds => ds.DeleteContentItem(content.ContentItemId));
    }

    [Test]

    public void ContentController_GetContentItem_Throws_On_Negative_ContentItemId()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        // Act, Arrange
        Assert.Throws<ArgumentOutOfRangeException>(() => controller.GetContentItem(Null.NullInteger));
    }

    [Test]

    public void ContentController_GetContentItem_Returns_Null_On_InValid_ContentItemId()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetContentItem(Constants.CONTENT_InValidContentItemId)).Returns(MockHelper.CreateEmptyContentItemReader());
        ContentController controller = new ContentController(mockDataService.Object);

        // Act
        ContentItem content = controller.GetContentItem(Constants.CONTENT_InValidContentItemId);

        // Assert
        Assert.That(content, Is.Null);
    }

    [Test]

    public void ContentController_GetContentItem_Calls_DataService_On_Valid_ContentItemId()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetContentItem(Constants.CONTENT_ValidContentItemId)).Returns(MockHelper.CreateValidContentItemReader());
        ContentController controller = new ContentController(mockDataService.Object);

        // Act
        ContentItem content = controller.GetContentItem(Constants.CONTENT_ValidContentItemId);

        // Assert
        mockDataService.Verify(ds => ds.GetContentItem(Constants.CONTENT_ValidContentItemId));
    }

    [Test]

    public void ContentController_GetContentItem_Returns_ContentItem_On_Valid_ContentItemId()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetContentItem(Constants.CONTENT_ValidContentItemId)).Returns(MockHelper.CreateValidContentItemReader());
        ContentController controller = new ContentController(mockDataService.Object);

        // Act
        ContentItem content = controller.GetContentItem(Constants.CONTENT_ValidContentItemId);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(content.ContentItemId, Is.EqualTo(Constants.CONTENT_ValidContentItemId));
            Assert.That(content.Content, Is.EqualTo(ContentTestHelper.GetContent(Constants.CONTENT_ValidContentItemId)));
            Assert.That(content.ContentKey, Is.EqualTo(ContentTestHelper.GetContentKey(Constants.CONTENT_ValidContentItemId)));
        });
    }

    [Test]

    public void ContentController_GetContentItemsByTerm_Throws_On_Null_Term()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        // Act, Arrange
        Assert.Throws<ArgumentException>(() => controller.GetContentItemsByTerm(Null.NullString));
    }

    [Test]

    public void ContentController_GetContentItemsByTerm_Calls_DataService()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetContentItemsByTerm(Constants.TERM_ValidName)).Returns(MockHelper.CreateValidContentItemReader());
        ContentController controller = new ContentController(mockDataService.Object);

        // Act
        IQueryable<ContentItem> contentItems = controller.GetContentItemsByTerm(Constants.TERM_ValidName);

        // Assert
        mockDataService.Verify(ds => ds.GetContentItemsByTerm(Constants.TERM_ValidName));
    }

    [Test]

    public void ContentController_GetContentItemsByTerm_Returns_Empty_List_If_Term_Not_Used()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetContentItemsByTerm(Constants.TERM_UnusedName)).Returns(MockHelper.CreateEmptyContentItemReader());
        ContentController controller = new ContentController(mockDataService.Object);

        // Act
        IQueryable<ContentItem> contentItems = controller.GetContentItemsByTerm(Constants.TERM_UnusedName);

        // Assert
        Assert.That(contentItems.Count(), Is.EqualTo(0));
    }

    [Test]

    public void ContentController_GetContentItemsByTerm_Returns_List_Of_ContentItems()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetContentItemsByTerm(Constants.TERM_ValidName)).Returns(MockHelper.CreateValidContentItemsReader(
            Constants.CONTENT_TaggedItemCount,
            Constants.CONTENT_IndexedFalse,
            Null.NullInteger,
            Constants.TERM_ValidName));
        ContentController controller = new ContentController(mockDataService.Object);

        // Act
        IQueryable<ContentItem> contentItems = controller.GetContentItemsByTerm(Constants.TERM_ValidName);

        // Assert
        Assert.That(contentItems.Count(), Is.EqualTo(Constants.CONTENT_TaggedItemCount));
    }

    [Test]

    public void ContentController_GetContentItemsByContentType_Returns_Results()
    {
        var mock = new Mock<IDataService>();
        mock.Setup(ds => ds.GetContentItemsByContentType(It.IsAny<int>()))
            .Returns(MockHelper.CreateValidContentItemsReader(10, true, 0, null));

        var controller = new ContentController(mock.Object);

        var items = controller.GetContentItemsByContentType(10).ToArray();

        Assert.That(items, Has.Length.EqualTo(10));
    }

    [Test]

    public void ContentController_GetContentItemsByContentType_Invalid_Id_Returns_No_Elements()
    {
        var mock = new Mock<IDataService>();
        mock.Setup(ds => ds.GetContentItemsByContentType(It.IsAny<int>())).Returns(MockHelper.CreateEmptyContentItemReader());

        var controller = new ContentController(mock.Object);

        var items = controller.GetContentItemsByContentType(-1).ToArray();

        Assert.That(items, Is.Empty);
    }

    [Test]

    public void GetContentItemsByModuleId_With_Negative_ModuleId_Returns_ContentItems()
    {
        var mock = new Mock<IDataService>();
        mock.Setup(ds => ds.GetContentItemsByModuleId(-1)).Returns(MockHelper.CreateValidContentItemsReader(10, false, 0, null));
        mock.Setup(ds => ds.GetContentItemsByModuleId(0)).Returns(MockHelper.CreateValidContentItemReader());

        var controller = new ContentController(mock.Object);

        var negative = controller.GetContentItemsByModuleId(-1).ToArray();
        var positive = controller.GetContentItemsByModuleId(0).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(negative, Has.Length.EqualTo(10));
            Assert.That(positive, Has.Length.EqualTo(1));
        });
    }

    [Test]

    public void ContentController_GetUnIndexedContentItems_Calls_DataService()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetUnIndexedContentItems()).Returns(MockHelper.CreateValidContentItemReader());
        ContentController controller = new ContentController(mockDataService.Object);

        // Act
        IQueryable<ContentItem> contentItems = controller.GetUnIndexedContentItems();

        // Assert
        mockDataService.Verify(ds => ds.GetUnIndexedContentItems());
    }

    [Test]

    public void ContentController_GetUnIndexedContentItems_Returns_EmptyList_If_No_UnIndexed_Items()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetUnIndexedContentItems()).Returns(MockHelper.CreateEmptyContentItemReader());

        ContentController controller = new ContentController(mockDataService.Object);

        // Act
        IQueryable<ContentItem> contentItems = controller.GetUnIndexedContentItems();

        // Assert
        Assert.That(contentItems.Count(), Is.EqualTo(0));
    }

    [Test]

    public void ContentController_GetUnIndexedContentItems_Returns_List_Of_UnIndexedContentItems()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetUnIndexedContentItems()).Returns(MockHelper.CreateValidContentItemsReader(
            Constants.CONTENT_IndexedFalseItemCount,
            Constants.CONTENT_IndexedFalse,
            Null.NullInteger,
            Null.NullString));

        ContentController controller = new ContentController(mockDataService.Object);

        // Act
        IQueryable<ContentItem> contentItems = controller.GetUnIndexedContentItems();

        // Assert
        Assert.That(contentItems.Count(), Is.EqualTo(Constants.CONTENT_IndexedFalseItemCount));
        foreach (ContentItem content in contentItems)
        {
            Assert.That(content.Indexed, Is.False);
        }
    }

    [Test]
    public void ContentController_UpdateContentItem_Throws_On_Null_ContentItem()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        // Act, Arrange
        Assert.Throws<ArgumentNullException>(() => controller.UpdateContentItem(null));
    }

    [Test]
    public void ContentController_UpdateContentItem_Throws_On_Negative_ContentItemId()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        ContentItem content = new ContentItem();
        content.ContentItemId = Null.NullInteger;

        Assert.Throws<ArgumentOutOfRangeException>(() => controller.UpdateContentItem(content));
    }

    [Test]
    public void ContentController_UpdateContentItem_Calls_DataService_On_Valid_ContentItem()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        ComponentFactory.RegisterComponentInstance<IContentController>(controller);

        ContentItem content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Constants.CONTENT_UpdateContentItemId;
        content.Content = Constants.CONTENT_UpdateContent;
        content.ContentKey = Constants.CONTENT_UpdateContentKey;

        // Act
        controller.UpdateContentItem(content);

        // Assert
        mockDataService.Verify(ds => ds.UpdateContentItem(content, It.IsAny<int>()));
    }

    [Test]
    public void ContentController_AddMetaData_Throws_On_Null_ContentItem()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        // Act, Arrange
        Assert.Throws<ArgumentNullException>(() => controller.AddMetaData(null, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue));
    }

    [Test]
    public void ContentController_AddMetaData_Throws_On_Negative_ContentItemId()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        ContentItem content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Null.NullInteger;

        // Act, Arrange
        Assert.Throws<ArgumentOutOfRangeException>(() => controller.AddMetaData(content, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue));
    }

    [Test]
    public void ContentController_AddMetaData_Throws_On_Null_MetaDataName()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        ContentItem content = ContentTestHelper.CreateValidContentItem();

        // Act, Arrange
        Assert.Throws<ArgumentOutOfRangeException>(() => controller.AddMetaData(content, Null.NullString, Constants.CONTENT_ValidMetaDataValue));
    }

    [Test]
    public void ContentController_AddMetaData_Calls_DataService_On_Valid_Arguments()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        ContentItem content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Constants.CONTENT_ValidContentItemId;

        // Act
        controller.AddMetaData(content, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue);

        // Assert
        mockDataService.Verify(ds => ds.AddMetaData(content, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue));
    }

    [Test]
    public void ContentController_DeleteMetaData_Throws_On_Null_ContentItem()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        // Act, Arrange
        Assert.Throws<ArgumentNullException>(() => controller.AddMetaData(null, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue));
    }

    [Test]
    public void ContentController_DeleteMetaData_Throws_On_Negative_ContentItemId()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        ContentItem content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Null.NullInteger;

        // Act, Arrange
        Assert.Throws<ArgumentOutOfRangeException>(() => controller.DeleteMetaData(content, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue));
    }

    [Test]
    public void ContentController_DeleteMetaData_Throws_On_Null_MetaDataName()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        ContentItem content = ContentTestHelper.CreateValidContentItem();

        // Act, Arrange
        Assert.Throws<ArgumentOutOfRangeException>(() => controller.AddMetaData(content, Null.NullString, Constants.CONTENT_ValidMetaDataValue));
    }

    [Test]
    public void ContentController_DeleteMetaData_Calls_DataService_On_Valid_Arguments()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        ContentItem content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Constants.CONTENT_ValidContentItemId;

        // Act
        controller.DeleteMetaData(content, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue);

        // Assert
        mockDataService.Verify(ds => ds.DeleteMetaData(content, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue));
    }

    [Test]
    public void ContentController_GetMetaData_Throws_On_Negative_ContentItemId()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        ContentController controller = new ContentController(mockDataService.Object);

        // Act, Arrange
        Assert.Throws<ArgumentOutOfRangeException>(() => controller.GetMetaData(Null.NullInteger));
    }

    [Test]
    public void ContentController_GetMetaData_Calls_DataService_On_Valid_Arguments()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetMetaData(Constants.CONTENT_ValidContentItemId)).Returns(MockHelper.CreateValidMetaDataReader());
        ContentController controller = new ContentController(mockDataService.Object);

        // Act
        NameValueCollection metaData = controller.GetMetaData(Constants.CONTENT_ValidContentItemId);

        // Assert
        mockDataService.Verify(ds => ds.GetMetaData(Constants.CONTENT_ValidContentItemId));
    }

    [Test]
    public void ContentController_GetMetaData_Returns_NameValueCollection_Of_MetaData()
    {
        // Arrange
        Mock<IDataService> mockDataService = new Mock<IDataService>();
        mockDataService.Setup(ds => ds.GetMetaData(Constants.CONTENT_ValidContentItemId)).Returns(MockHelper.CreateValidMetaDataReader());

        mockDataService.Setup(ds =>
                ds.SynchronizeMetaData(
                    It.IsAny<ContentItem>(),
                    It.IsAny<IEnumerable<KeyValuePair<string, string>>>(),
                    It.IsAny<IEnumerable<KeyValuePair<string, string>>>()))
            .Callback<ContentItem, IEnumerable<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>>(
                (ci, added, deleted) =>
                {
                    deleted.ToList().ForEach(
                        item => mockDataService.Object.DeleteMetaData(ci, item.Key, item.Value));

                    added.ToList().ForEach(
                        item => mockDataService.Object.AddMetaData(ci, item.Key, item.Value));
                });

        var controller = new ContentController(mockDataService.Object);

        // Act
        var metaData = controller.GetMetaData(Constants.CONTENT_ValidContentItemId);

        // Assert
        Assert.That(metaData, Has.Count.EqualTo(Constants.CONTENT_MetaDataCount));
    }

    [Test]
    public void ContentController_Title_Is_Saved_On_Add()
    {
        var mockDataService = new Mock<IDataService>();

        mockDataService.Setup(
                ds =>
                    ds.AddContentItem(
                        It.IsAny<ContentItem>(),
                        It.IsAny<int>()))
            .Returns(Constants.CONTENT_AddContentItemId);

        mockDataService.Setup(ds =>
                ds.SynchronizeMetaData(
                    It.IsAny<ContentItem>(),
                    It.IsAny<IEnumerable<KeyValuePair<string, string>>>(),
                    It.IsAny<IEnumerable<KeyValuePair<string, string>>>()))
            .Callback<ContentItem, IEnumerable<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>>(
                (ci, added, deleted) =>
                {
                    deleted.ToList().ForEach(
                        item => mockDataService.Object.DeleteMetaData(ci, item.Key, item.Value));

                    added.ToList().ForEach(
                        item => mockDataService.Object.AddMetaData(ci, item.Key, item.Value));
                });

        // Return empty set of metadata.
        mockDataService.Setup(ds => ds.GetMetaData(It.IsAny<int>())).Returns(MockHelper.CreateValidMetaDataReader);

        var controller = new ContentController(mockDataService.Object);

        // The ContentExtensions methods look this up.
        ComponentFactory.RegisterComponentInstance<IContentController>(controller);

        var content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Constants.CONTENT_ValidContentItemId;
        content.ContentTitle = Constants.CONTENT_ValidTitle;

        // Act
        controller.AddContentItem(content);

        // Assert
        mockDataService.Verify(ds => ds.AddMetaData(content, AttachmentController.TitleKey, Constants.CONTENT_ValidTitle));
    }

    [Test]
    public void ContentController_Title_Is_Saved_On_Update()
    {
        var mockDataService = new Mock<IDataService>();

        mockDataService.Setup(ds => ds.AddContentItem(It.IsAny<ContentItem>(), It.IsAny<int>()))
            .Returns(Constants.CONTENT_AddContentItemId);

        mockDataService.Setup(ds =>
                ds.SynchronizeMetaData(
                    It.IsAny<ContentItem>(),
                    It.IsAny<IEnumerable<KeyValuePair<string, string>>>(),
                    It.IsAny<IEnumerable<KeyValuePair<string, string>>>()))
            .Callback<ContentItem, IEnumerable<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>>(
                (ci, added, deleted) =>
                {
                    deleted.ToList().ForEach(
                        item => mockDataService.Object.DeleteMetaData(ci, item.Key, item.Value));

                    added.ToList().ForEach(
                        item => mockDataService.Object.AddMetaData(ci, item.Key, item.Value));
                });

        // Return empty set of metadata.
        mockDataService.Setup(ds => ds.GetMetaData(It.IsAny<int>())).Returns(MockHelper.CreateValidMetaDataReader);

        var controller = new ContentController(mockDataService.Object);

        // The ContentExtensions methods look this up.
        ComponentFactory.RegisterComponentInstance<IContentController>(controller);

        var content = ContentTestHelper.CreateValidContentItem();
        content.ContentItemId = Constants.CONTENT_ValidContentItemId;
        content.ContentTitle = Constants.CONTENT_ValidTitle;

        // Act
        controller.AddContentItem(content);

        content.ContentTitle = Constants.CONTENT_ValidTitle2;
        controller.UpdateContentItem(content);

        // Assert
        mockDataService.Verify(ds => ds.AddMetaData(content, AttachmentController.TitleKey, Constants.CONTENT_ValidTitle));
        mockDataService.Verify(ds => ds.AddMetaData(content, AttachmentController.TitleKey, Constants.CONTENT_ValidTitle2));
    }
}
