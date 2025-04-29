// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Content;

using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Tests.Content.Mocks;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using FileController = DotNetNuke.Entities.Content.AttachmentController;
using Util = DotNetNuke.Entities.Content.Common.Util;

[TestFixture]
public class AttachmentControllerTests
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

    public void Test_Add_File_To_Content_Item_Without_Metadata()
    {
        var dataService = DataServiceFactory();

        dataService.Setup(ds => ds.AddContentItem(It.IsAny<ContentItem>(), It.IsAny<int>()))
            .Returns(Constants.CONTENT_AddContentItemId);

        // Return empty set of metadata.
        dataService.Setup(ds => ds.GetMetaData(It.IsAny<int>())).Returns(MockHelper.CreateEmptyMetaDataReader);

        var content = ContentTestHelper.CreateValidContentItem();
        content.Metadata.Clear();

        var contentId = Util.GetContentController().AddContentItem(content);
        Assert.Multiple(() =>
        {
            Assert.That(contentId, Is.EqualTo(Constants.CONTENT_AddContentItemId));
            Assert.That(content.Metadata, Is.Empty);
        });

        dataService.Setup(ds => ds.GetContentItem(It.IsAny<int>()))
            .Returns<int>(y => MockHelper.CreateValidContentItemReader(content));

        var fileController = ComponentFactory.GetComponent<IAttachmentController>();

        fileController.AddFileToContent(contentId, ContentTestHelper.CreateValidFile(0));

        dataService.Verify(
            ds => ds.AddMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, new[] { 0 }.ToJson()));
    }

    [Test]

    public void Test_Load_Attachments_From_DataService()
    {
        var files = new List<IFileInfo>
        {
            ContentTestHelper.CreateValidFile(0),
            ContentTestHelper.CreateValidFile(1),
            ContentTestHelper.CreateValidFile(2),
        };

        var dataService = DataServiceFactory();

        dataService.Setup(ds => ds.GetContentItem(It.IsAny<int>())).Returns(MockHelper.CreateValidContentItemReader);

        dataService.Setup(
            ds =>
                ds.GetMetaData(It.IsAny<int>())).Returns(
            () => MockHelper.CreateMetaDataReaderWithFiles(files, new IFileInfo[0], new IFileInfo[0]));

        var contentItem = Util.GetContentController().GetContentItem(Constants.CONTENT_ValidContentItemId);
        Assert.That(contentItem, Is.Not.Null);

        var serialized = contentItem.Metadata[FileController.FilesKey];
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Is.Not.Empty);

            Assert.That(contentItem.Files, Is.Not.Empty);
        });
        Assert.That(contentItem.Files, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(contentItem.Files[0].FileId, Is.EqualTo(0));
            Assert.That(contentItem.Files[1].FileId, Is.EqualTo(1));
            Assert.That(contentItem.Files[2].FileId, Is.EqualTo(2));
        });
    }

    [Test]
    public void Test_Add_Attachments_With_FileController()
    {
        var dataService = DataServiceFactory();

        dataService.Setup(
            ds =>
                ds.GetContentItem(It.IsAny<int>())).Returns(MockHelper.CreateValidContentItemReader);

        // Use a closure to store the metadata locally in this method.
        var data = new Dictionary<string, string>();

        dataService.Setup(
            ds =>
                ds.GetMetaData(It.IsAny<int>())).Returns(
            () => MockHelper.CreateMetaDataReaderFromDictionary(data));

        dataService.Setup(
                ds =>
                    ds.AddMetaData(
                        It.IsAny<ContentItem>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()))
            .Callback<ContentItem, string, string>((ci, name, value) => data[name] = value);

        var contentController = Util.GetContentController();

        var contentItem = contentController.GetContentItem(Constants.CONTENT_ValidContentItemId);
        Assert.That(contentItem, Is.Not.Null);

        var serialized = contentItem.Metadata[FileController.FilesKey];
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Is.Null);
            Assert.That(contentItem.Files, Is.Empty);
        });

        var fileManager = ComponentFactory.GetComponent<IFileManager>();

        // Add some files.
        var fileController = ComponentFactory.GetComponent<IAttachmentController>();
        fileController.AddFileToContent(contentItem.ContentItemId, fileManager.GetFile(0));
        fileController.AddFileToContent(contentItem.ContentItemId, fileManager.GetFile(1));

        contentItem = contentController.GetContentItem(Constants.CONTENT_ValidContentItemId);

        Assert.That(contentItem.Files, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(contentItem.Files[0].FileId, Is.EqualTo(0));
            Assert.That(contentItem.Files[1].FileId, Is.EqualTo(1));
        });

        dataService.Verify(
            ds => ds.DeleteMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0]"), Times.Once());

        dataService.Verify(
            ds => ds.AddMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0]"), Times.Once());

        dataService.Verify(
            ds => ds.AddMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0,1]"), Times.Once());
    }

    [Test]
    public void Set_MetaData_To_Empty_Value_Deletes_Row()
    {
        var data = new Dictionary<string, string>();

        var dataService = DataServiceFactoryWithLocalMetaData(ref data);

        dataService.Setup(ds => ds.GetContentItem(It.IsAny<int>())).Returns(MockHelper.CreateValidContentItemReader);

        var contentController = Util.GetContentController();

        var fileManager = ComponentFactory.GetComponent<IFileManager>();

        var contentItem = contentController.GetContentItem(Constants.CONTENT_ValidContentItemId);
        Assert.That(contentItem, Is.Not.Null);

        var serialized = contentItem.Metadata[FileController.FilesKey];
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Is.Null);
            Assert.That(contentItem.Files, Is.Empty);
        });

        // Add some files.
        var fileController = ComponentFactory.GetComponent<IAttachmentController>();
        fileController.AddFileToContent(contentItem.ContentItemId, fileManager.GetFile(0));
        fileController.AddFileToContent(contentItem.ContentItemId, fileManager.GetFile(1));

        contentItem = contentController.GetContentItem(Constants.CONTENT_ValidContentItemId);

        Assert.That(contentItem.Files, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(contentItem.Files[0].FileId, Is.EqualTo(0));
            Assert.That(contentItem.Files[1].FileId, Is.EqualTo(1));
        });
        Assert.That(contentItem.Metadata[FileController.FilesKey], Is.Not.Empty);

        contentItem.Files.Clear();

        contentController.UpdateContentItem(contentItem);

        dataService.Verify(ds => ds.DeleteMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0]"), Times.Once());
        dataService.Verify(ds => ds.DeleteMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0,1]"), Times.Once());
        dataService.Verify(ds => ds.AddMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0]"), Times.Once());
        dataService.Verify(ds => ds.AddMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0,1]"), Times.Once());

        var emptyFiles = fileController.GetFilesByContent(contentItem.ContentItemId);
        Assert.That(emptyFiles, Is.Empty);
    }

    /// <remarks>This test should be moved elsewhere (cb).</remarks>
    [Test]
    public void Set_MetaData_To_Same_Value_Doesnt_Update_Database_Entry()
    {
        var metadata = new Dictionary<string, string>();

        var dataService = DataServiceFactoryWithLocalMetaData(ref metadata);

        var contentController = Util.GetContentController();

        var contentItem = ContentTestHelper.CreateValidContentItem();

        contentController.AddContentItem(contentItem);

        dataService.Verify(ds => ds.AddMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Never());
        dataService.Verify(ds => ds.DeleteMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Never());

        contentItem.ContentTitle = "Foobar";

        contentController.UpdateContentItem(contentItem);

        dataService.Verify(ds => ds.AddMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Once());
        dataService.Verify(ds => ds.DeleteMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Never());

        contentItem.ContentTitle = "Foobar";

        contentController.UpdateContentItem(contentItem);

        // Should be a no-op since no real data changed
        dataService.Verify(ds => ds.AddMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Once());
        dataService.Verify(ds => ds.DeleteMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Never());

        // Really update
        contentItem.ContentTitle = "SNAFU";

        contentController.UpdateContentItem(contentItem);

        dataService.Verify(ds => ds.AddMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Exactly(2));
        dataService.Verify(ds => ds.DeleteMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Once());
    }

    private static Mock<IDataService> DataServiceFactory()
    {
        var dataService = new Mock<IDataService>();

        dataService.Setup(ds =>
                ds.SynchronizeMetaData(
                    It.IsAny<ContentItem>(),
                    It.IsAny<IEnumerable<KeyValuePair<string, string>>>(),
                    It.IsAny<IEnumerable<KeyValuePair<string, string>>>()))
            .Callback<ContentItem, IEnumerable<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>>(
                (ci, added, deleted) =>
                {
                    deleted.ToList().ForEach(
                        item => dataService.Object.DeleteMetaData(ci, item.Key, item.Value));

                    added.ToList().ForEach(
                        item => dataService.Object.AddMetaData(ci, item.Key, item.Value));
                });

        // Register controller types that are dependent on our IDataService.
        var contentController = new ContentController(dataService.Object);

        ComponentFactory.RegisterComponentInstance<IAttachmentController>(new FileController(contentController));
        ComponentFactory.RegisterComponentInstance<IContentController>(contentController);
        ComponentFactory.RegisterComponentInstance<IFileManager>(MockHelper.CreateMockFileManager().Object);

        return dataService;
    }

    private static Mock<IDataService> DataServiceFactoryWithLocalMetaData(ref Dictionary<string, string> metadata)
    {
        var dataService = DataServiceFactory();

        var closure = metadata;

        dataService.Setup(ds => ds.GetMetaData(It.IsAny<int>())).Returns(() => MockHelper.CreateMetaDataReaderFromDictionary(closure));
        dataService.Setup(ds => ds.AddMetaData(It.IsAny<ContentItem>(), It.IsAny<string>(), It.IsAny<string>())).
            Callback<ContentItem, string, string>((ci, name, value) => closure[name] = value);
        dataService.Setup(ds => ds.DeleteMetaData(It.IsAny<ContentItem>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<ContentItem, string, string>((ci, key, val) => closure.Remove(key));

        return dataService;
    }
}
