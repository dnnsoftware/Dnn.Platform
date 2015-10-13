// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using Dnn.DynamicContent;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Search.Controllers;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Constants = DotNetNuke.Tests.Utilities.Constants;
using System.Reflection;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class DynamicContentItemManagerTests
    {
        private readonly JObject _testJson = new JObject(
                        new JProperty("contentTypeId", Constants.CONTENTTYPE_ValidContentTypeId),
                        new JProperty("content",
                            new JObject(
                                new JProperty("field",
                                      new JArray(
                                        new JObject(
                                            new JProperty("name", "FieldName1"),
                                            new JProperty("value", 1)
                                            ),
                                        new JObject(
                                            new JProperty("name", "FieldName2"),
                                            new JProperty("value", true)
                                            ),
                                        new JObject(
                                            new JProperty("name", "FieldName3"),
                                            new JProperty("value", "abc")
                                            )
                                        )
                                    )
                                )
                            )
                    );

        public enum SearchTypeIds
        {
            ModuleSearchTypeId = 1,
            TabSearchTypeId,
            DocumentSearchTypeId,
            UrlSearchTypeId,
            OtherSearchTypeId,
            UnknownSearchTypeId
        }

        private const int ModuleSearchTypeId = (int)SearchTypeIds.ModuleSearchTypeId;
        private const int TabSearchTypeId = (int)SearchTypeIds.TabSearchTypeId;
        private const int DocumentSearchTypeId = (int)SearchTypeIds.DocumentSearchTypeId;
        private const int UrlSearchTypeId = (int)SearchTypeIds.UrlSearchTypeId;
        private const int OtherSearchTypeId = (int)SearchTypeIds.OtherSearchTypeId;
        private const int UnknownSearchTypeId = (int)SearchTypeIds.UnknownSearchTypeId;
        private const string ModuleSearchTypeName = "module";
        private const string OtherSearchTypeName = "other";
        private const string TabSearchTypeName = "tab";
        private const string DocumentSearchTypeName = "document";
        private const string UrlSearchTypeName = "url";
        private const string ModuleResultControllerClass = "DotNetNuke.Services.Search.Crawlers.ModuleResultController, DotNetNuke";
        private const string FakeResultControllerClass = "DotNetNuke.Tests.Core.Controllers.Search.FakeResultController, DotNetNuke.Tests.Core";
        private const string NoPermissionFakeResultControllerClass = "DotNetNuke.Tests.Core.Controllers.Search.NoPermissionFakeResultController, DotNetNuke.Tests.Core";
        private const string CultureEnUs = "en-US";
        private const string CultureEnCa = "en-CA";
        private const string CultureEsEs = "es-ES";
        private const string Line1 = "The quick brown fox jumps over the lazy dog";
        private const string SearchKeyword1 = "brown";
        private const string Line2 = "The quick black fox jumps over the lazy dog";
        private const string SearchKeyword2 = "black";

        private Mock<IHostController> _mockHostController;
        private Mock<CachingProvider> _mockCachingProvider;
        private Mock<DataProvider> _mockDataProvider;
        private Mock<ITermController> _mockTermController;
        private Mock<IModuleController> _mockModuleController;
        private Mock<ILocaleController> _mockLocaleController;
        private Mock<ISearchHelper> _mockSearchHelper;

        private SearchControllerImpl _searchController;
        private IInternalSearchController _internalSearchController;
        private LuceneControllerImpl _luceneController;


        private const string SearchIndexFolder = @"App_Data\Search";

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            MockComponentProvider.ResetContainer();

            _mockDataProvider = MockComponentProvider.CreateDataProvider();
            _mockLocaleController = MockComponentProvider.CreateLocaleController();
            _mockCachingProvider = MockComponentProvider.CreateDataCacheProvider();
            _mockTermController = MockComponentProvider.CreateNew<ITermController>();
            _mockModuleController = MockComponentProvider.CreateNew<IModuleController>();
            ModuleController.SetTestableInstance(_mockModuleController.Object);


            _mockHostController = new Mock<IHostController>();
            _mockSearchHelper = new Mock<ISearchHelper>();

            //Standard DataProvider Path for Logging
            _mockDataProvider.Setup(d => d.GetProviderPath()).Returns("");
            DataTableReader searchTypes = null;
            _mockDataProvider.Setup(ds => ds.GetAllSearchTypes())
                     .Callback(() => searchTypes = GetAllSearchTypes().CreateDataReader())
                     .Returns(() => searchTypes);
            _mockDataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(GetPortalsCallBack);

            _mockTermController.Setup(c => c.GetTermsByContent(It.IsAny<int>())).Returns(new List<Term>().AsQueryable());

            _mockModuleController.Setup(c => c.GetModule(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(new ModuleInfo{ModuleDefID = 100});

            SetupSearchHelper();

            CreateNewLuceneControllerInstance();
        }

        [TearDown]
        public void TearDown()
        {
            FieldDefinitionManager.ClearInstance();
            ModuleController.ClearInstance();
            DataTypeManager.ClearInstance();

            _luceneController.Dispose();
            InternalSearchController.ClearInstance();
            SearchHelper.ClearInstance();
            LuceneController.ClearInstance();
            _luceneController = null;

            DeleteIndexFolder();
        }

        [Test]
        public void AddContentItem_Throws_On_Null_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => controller.AddContentItem(null));
        }

        [Test]
        public void AddContentItem_Throws_On_Null_ContentType_Property_Of_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId);

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => controller.AddContentItem(contentItem));
        }

        [Test]
        public void AddContentItem_Throws_On_Negative_ContentTypeId_Property_Of_ContentType_Property_Of_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, new DynamicContentType() {PortalId = Constants.PORTAL_ValidPortalId});

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.AddContentItem(contentItem));
        }

        [Test]
        public void AddContentItem_Throws_On_Negative_ModuleId_Property_Of_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, GetContentType(contentTypeId, Constants.PORTAL_ValidPortalId));

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.AddContentItem(contentItem));
        }

        [Test]
        public void AddContentItem_Calls_ContentController_AddContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var dynamicContent = new DynamicContentItem(portalId, GetContentType(contentTypeId, portalId))
            {
                ModuleId = Constants.MODULE_ValidId
            };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);

            //Act
            controller.AddContentItem(dynamicContent);

            //Assert
            mockContentController.Verify(c => c.AddContentItem(It.IsAny<ContentItem>()), Times.Once);
        }

        [Test]
        public void AddContentItem_Calls_ContentController_AddContentItem_With_Valid_ContentItem()
        {
            //Arrange
            var moduleId = Constants.MODULE_ValidId;
            var tabId = Constants.TAB_ValidId;
            var portalId = Constants.PORTAL_ValidPortalId;
            var controller = new DynamicContentItemManager();
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId, false))
                .Returns(new List<DynamicContentType>() { GetContentType(contentTypeId, portalId) }.AsQueryable());
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var dynamicContent = new DynamicContentItem(portalId, GetContentType(contentTypeId, portalId))
                                        {
                                            ModuleId = moduleId,
                                            TabId = tabId
                                        };
            dynamicContent.Content.Fields["FieldName1"].Value = 1;
            dynamicContent.Content.Fields["FieldName2"].Value = true;
            dynamicContent.Content.Fields["FieldName3"].Value = "abc";


            ContentItem contentItem = null;
            var mockContentController = new Mock<IContentController>();
            mockContentController.Setup(c => c.AddContentItem(It.IsAny<ContentItem>()))
                                    .Callback<ContentItem>(c => contentItem = c);
            ContentController.SetTestableInstance(mockContentController.Object);

            //Act
            controller.AddContentItem(dynamicContent);

            //Assert
            Assert.AreEqual(tabId, contentItem.TabID);
            Assert.AreEqual(moduleId, contentItem.ModuleID);
            Assert.AreEqual(contentTypeId, contentItem.ContentTypeId);
            Assert.AreEqual(String.Empty, contentItem.ContentKey);
            Assert.AreEqual(_testJson.ToString(), contentItem.Content);
        }

        [Test]
        public void CreateContentItem_Throws_On_Null_ContentType()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            DynamicContentType contentType = null;

            //Act, Assert
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => controller.CreateContentItem(Constants.PORTAL_ValidPortalId, Constants.TAB_ValidId, Constants.MODULE_ValidId, contentType));
        }

        [Test]
        public void CreateContentItem_Throws_If_Negative_PortalId()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentType = new DynamicContentType();

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.CreateContentItem(Constants.PORTAL_InValidPortalId, Constants.TAB_ValidId, Constants.MODULE_ValidId, contentType));
        }

        [Test]
        public void CreateContentItem_Throws_If_Negative_TabId()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentType = new DynamicContentType();

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.CreateContentItem(Constants.PORTAL_ValidPortalId, Constants.TAB_InValidId, Constants.MODULE_ValidId, contentType));
        }

        [Test]
        public void CreateContentItem_Throws_If_Negative_ModuleId()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentType = new DynamicContentType();

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.CreateContentItem(Constants.PORTAL_ValidPortalId, Constants.TAB_ValidId, Constants.MODULE_InValidId, contentType));
        }

        [Test]
        public void CreateContentItem_Throws_If_ContentType_Has_No_Fields_Defined()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentType = new DynamicContentType() { PortalId = Constants.PORTAL_ValidPortalId };

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(c => c.GetFieldDefinitions(It.IsAny<int>()))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            //Act, Assert
            Assert.Throws<InvalidOperationException>(() => controller.CreateContentItem(Constants.PORTAL_ValidPortalId, Constants.TAB_ValidId, Constants.MODULE_ValidId, contentType));
        }

        [Test]
        public void CreateContentItem_Returns_Valid_DyamicContentItem()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;
            var moduleId = Constants.MODULE_ValidId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var controller = new DynamicContentItemManager();

            //Act
            var contentType = GetContentType(contentTypeId, portalId);
            var dynamicContent = controller.CreateContentItem(portalId, Constants.TAB_ValidId, moduleId, contentType);

            //Assert
            Assert.AreSame(contentType, dynamicContent.ContentType);
            Assert.AreEqual(contentType.FieldDefinitions.Count, dynamicContent.Content.Fields.Count);
            Assert.AreEqual(portalId, dynamicContent.PortalId);
            Assert.AreEqual(moduleId, dynamicContent.ModuleId);
            foreach (var field in contentType.FieldDefinitions)
            {
                Assert.AreSame(field, dynamicContent.Content.Fields[field.Name].Definition);
            }
        }

        [Test]
        public void CreateContentItem_Overload_Throws_On_Null_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            ContentItem contentItem = null;

            //Act, Assert
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => controller.CreateContentItem(contentItem));
        }

        [Test]
        public void CreateContentItem_Overload_Throws_On_Negative_ModuleId_Property()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            ContentItem contentItem = new ContentItem
                                            {
                                                ModuleID = Constants.MODULE_InValidId,
                                                TabID = Constants.TAB_ValidId
                                            };

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.CreateContentItem(contentItem));
        }

        [Test]
        public void CreateContentItem_Overload_Throws_On_Negative_TabId_Property()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            ContentItem contentItem = new ContentItem
                                            {
                                                ModuleID = Constants.MODULE_ValidId,
                                                TabID = Constants.TAB_InValidId
                                            };

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.CreateContentItem(contentItem));
        }

        [Test]
        public void CreateContentItem_Overload_Throws_On_Null_Or_Empty_Content()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            ContentItem contentItem = new ContentItem
                                            {
                                                ModuleID = Constants.MODULE_ValidId,
                                                TabID = Constants.TAB_ValidId
                                            };

            var mockModuleController = new Mock<IModuleController>();
            mockModuleController.Setup(c => c.GetModule(Constants.MODULE_ValidId, Constants.TAB_ValidId, false))
                .Returns(new ModuleInfo() {PortalID = Constants.PORTAL_ValidPortalId});
            ModuleController.SetTestableInstance(mockModuleController.Object);

            //Act, Assert
            Assert.Throws<ArgumentException>(() => controller.CreateContentItem(contentItem));
        }

        [Test]
        public void CreateContentItem_Overload_Returns_Valid_Content()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;
            var moduleId = Constants.MODULE_ValidId;
            var tabId = Constants.TAB_ValidId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = GetContentType(contentTypeId, portalId);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                .Returns(contentType);
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var mockModuleController = new Mock<IModuleController>();
            mockModuleController.Setup(c => c.GetModule(moduleId, tabId, false))
                .Returns(new ModuleInfo() { PortalID = portalId });
            ModuleController.SetTestableInstance(mockModuleController.Object);

            var mockDataTypeManager = new Mock<IDataTypeManager>();
            mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>()))
                .Returns(() => new List<DataType>
                                    {
                                        new DataType() { DataTypeId = -1, UnderlyingDataType = UnderlyingDataType.String}
                                    }
                                .AsQueryable());
            DataTypeManager.SetTestableInstance(mockDataTypeManager.Object);

            var contentItem = new ContentItem { Content = _testJson.ToString(), ModuleID = moduleId, TabID = tabId };

            //Act
            var dynamicContent = controller.CreateContentItem(contentItem);

            //Assert
            Assert.AreEqual(contentTypeId, dynamicContent.ContentType.ContentTypeId);
            Assert.AreEqual(contentType.FieldDefinitions.Count, dynamicContent.Content.Fields.Count);
            Assert.AreEqual(portalId, dynamicContent.PortalId);
            Assert.AreEqual(moduleId, dynamicContent.ModuleId);
            foreach (var field in contentType.FieldDefinitions)
            {
                Assert.AreSame(field, dynamicContent.Content.Fields[field.Name].Definition);
            }
        }

        [Test]
        public void DeleteContentItem_Throws_On_Null_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => controller.DeleteContentItem(null));
        }

        [Test]
        public void DeleteContentItem_Throws_On_Negative_ContentItemId()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId);

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.DeleteContentItem(contentItem));
        }

        [Test]
        public void DeleteContentItem_Calls_ContentController_UpdateContentItem()
        {
            //Arrange
            var contentItemId = Constants.CONTENT_ValidContentItemId;
            var controller = new DynamicContentItemManager();
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId) {ContentItemId = contentItemId};

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);

            //Act
            controller.DeleteContentItem(contentItem);

            //Assert
            mockContentController.Verify(c => c.DeleteContentItem(contentItemId), Times.Once);
        }

        [Test]
        public void UpdateContentItem_Throws_On_Null_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => controller.UpdateContentItem(null));
        }

        [Test]
        public void UpdateContentItem_Throws_On_Negative_ContentItemId()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId);

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.UpdateContentItem(contentItem));
        }

        [Test]
        public void UpdateContentItem_Throws_On_Null_ContentType_Property_Of_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId)
                                    {
                                        ContentItemId = Constants.CONTENT_ValidContentItemId
                                    };

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => controller.UpdateContentItem(contentItem));
        }

        [Test]
        public void UpdateContentItem_Throws_On_Negative_ContentTypeId_Property_Of_ContentType_Property_Of_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, new DynamicContentType() {PortalId = Constants.PORTAL_ValidPortalId})
                                    {
                                        ContentItemId = Constants.CONTENT_ValidContentItemId
                                    };

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.UpdateContentItem(contentItem));
        }

        [Test]
        public void UpdateContentItem_Throws_On_Negative_ModuleId_Property_Of_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, GetContentType(contentTypeId, Constants.PORTAL_ValidPortalId))
                                    {
                                        ContentItemId = Constants.CONTENT_ValidContentItemId
                                    };

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.UpdateContentItem(contentItem));
        }

        [Test]
        public void UpdateContentItem_Throws_On_Negative_TabId_Property_Of_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, GetContentType(contentTypeId, Constants.PORTAL_ValidPortalId))
            {
                ContentItemId = Constants.CONTENT_ValidContentItemId,
                TabId = Constants.TAB_InValidId
            };

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.UpdateContentItem(contentItem));
        }

        [Test]
        public void UpdateContentItem_Calls_ContentController_UpdateContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var dynamicContent = new DynamicContentItem(Constants.PORTAL_ValidPortalId, GetContentType(contentTypeId, portalId))
                                        {
                                            ModuleId = Constants.MODULE_ValidId,
                                            TabId = Constants.TAB_ValidId,
                                            ContentItemId = Constants.CONTENT_ValidContentItemId
                                        };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);

            //Act
            controller.UpdateContentItem(dynamicContent);

            //Assert
            mockContentController.Verify(c => c.UpdateContentItem(It.IsAny<ContentItem>()), Times.Once);
        }

        [Test]
        public void UpdateContentItem_Calls_ContentController_UpdateContentItem_With_Valid_ContentItem()
        {
            //Arrange
            var moduleId = Constants.MODULE_ValidId;
            var tabId = Constants.TAB_ValidId;
            var portalId = Constants.PORTAL_ValidPortalId;
            var contentItemId = Constants.CONTENT_ValidContentItemId;
            var controller = new DynamicContentItemManager();
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId, false))
                .Returns(new List<DynamicContentType>() { GetContentType(contentTypeId, portalId) }.AsQueryable());
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var dynamicContent = new DynamicContentItem(portalId, GetContentType(contentTypeId, portalId))
                                        {
                                            ModuleId = moduleId,
                                            TabId = tabId,
                                            ContentItemId = contentItemId
                                        };

            dynamicContent.Content.Fields["FieldName1"].Value = 1;
            dynamicContent.Content.Fields["FieldName2"].Value = true;
            dynamicContent.Content.Fields["FieldName3"].Value = "abc";


            ContentItem contentItem = null;
            var mockContentController = new Mock<IContentController>();
            mockContentController.Setup(c => c.UpdateContentItem(It.IsAny<ContentItem>()))
                                    .Callback<ContentItem>(c => contentItem = c);
            ContentController.SetTestableInstance(mockContentController.Object);

            //Act
            controller.UpdateContentItem(dynamicContent);

            //Assert
            Assert.AreEqual(tabId, contentItem.TabID);
            Assert.AreEqual(contentItemId, contentItem.ContentItemId);
            Assert.AreEqual(moduleId, contentItem.ModuleID);
            Assert.AreEqual(contentTypeId, contentItem.ContentTypeId);
            Assert.AreEqual(String.Empty, contentItem.ContentKey);
            Assert.AreEqual(_testJson.ToString(), contentItem.Content);
        }

        [Test]
        public void ContentItem_Should_Save_Into_Search_Index_Correctly()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentController = new Mock<IContentController>();
            mockContentController.Setup(c => c.AddContentItem(It.IsAny<ContentItem>()))
                                    .Returns(Constants.CONTENT_AddContentItemId);
            ContentController.SetTestableInstance(mockContentController.Object);

            var dynamicContent = new DynamicContentItem(portalId, GetContentType(contentTypeId, portalId))
            {
                ModuleId = Constants.MODULE_ValidId,
                TabId = Constants.TAB_ValidId
            };
            dynamicContent.Content.Fields["FieldName1"].Value = Line1;

            //Act
            dynamicContent.ContentItemId = controller.AddContentItem(dynamicContent);

            //Assert
            _internalSearchController.Commit();
            WaitAssertEqual(1, () => SearchForKeyword(SearchKeyword1), "Add Content Item Should Save Search Document");

            //Act
            dynamicContent.Content.Fields["FieldName1"].Value = Line2;
            controller.UpdateContentItem(dynamicContent);

            //Assert
            _internalSearchController.Commit();
            WaitAssertEqual(0, () => SearchForKeyword(SearchKeyword1), "Update Content Item Should Delete Old Search Document");
            WaitAssertEqual(1, () => SearchForKeyword(SearchKeyword2), "Update Content Item Should Update Search Document");

            //Act
            controller.DeleteContentItem(dynamicContent);

            //Assert
            _internalSearchController.Commit();
            Thread.Sleep(100);
            WaitAssertEqual(0, () => SearchForKeyword(SearchKeyword2), "Delete Content Item Should Delete Search Document");
        }

        private void WaitAssertEqual(int expected, Func<SearchResults> searchMethod, string description)
        {
            int result = -1;
            for (var i = 0; i < 60; i++)
            {
                result = searchMethod().Results.Count;
                if (result != expected)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    break;
                }
            }

            Assert.AreEqual(expected, result, description);
        }

        #region Private Methods

        private DynamicContentType GetContentType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId, PortalId = portalId };

            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName1", PortalId = portalId });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName2", PortalId = portalId });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName3", PortalId = portalId });

            return contentType;
        }

        private void CreateNewLuceneControllerInstance(bool reCreate = false)
        {
            InternalSearchController.SetTestableInstance(new InternalSearchControllerImpl());
            _internalSearchController = InternalSearchController.Instance;
            _searchController = new SearchControllerImpl();

            if (!reCreate)
            {
                DeleteIndexFolder();

                if (_luceneController != null)
                {
                    LuceneController.ClearInstance();
                    _luceneController.Dispose();
                }
                _luceneController = new LuceneControllerImpl();
                LuceneController.SetTestableInstance(_luceneController);
            }
        }

        private void DeleteIndexFolder()
        {
            try
            {
                var indexPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), SearchIndexFolder);
                if (Directory.Exists(indexPath))
                    Directory.Delete(indexPath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void SetupSearchHelper()
        {
            _mockSearchHelper.Setup(c => c.GetSearchMinMaxLength()).Returns(new Tuple<int, int>(DotNetNuke.Services.Search.Internals.Constants.DefaultMinLen, DotNetNuke.Services.Search.Internals.Constants.DefaultMaxLen));
            _mockSearchHelper.Setup(c => c.GetSynonyms(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns<int, string, string>(GetSynonymsCallBack);
            _mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>())).Returns((string name) => new SearchType { SearchTypeId = 0, SearchTypeName = name });
            _mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>())).Returns<string>(GetSearchTypeByNameCallback);
            _mockSearchHelper.Setup(x => x.GetSearchTypes()).Returns(GetSearchTypes());
            _mockSearchHelper.Setup(x => x.GetSearchStopWords(It.IsAny<int>(), It.IsAny<string>())).Returns(new SearchStopWords());
            _mockSearchHelper.Setup(x => x.GetSearchStopWords(0, CultureEsEs)).Returns(
                new SearchStopWords
                {
                    PortalId = 0,
                    CultureCode = CultureEsEs,
                    StopWords = "los,de,el",
                });
            _mockSearchHelper.Setup(x => x.GetSearchStopWords(0, CultureEnUs)).Returns(
                new SearchStopWords
                {
                    PortalId = 0,
                    CultureCode = CultureEnUs,
                    StopWords = "the,over",
                });
            _mockSearchHelper.Setup(x => x.GetSearchStopWords(0, CultureEnCa)).Returns(
                new SearchStopWords
                {
                    PortalId = 0,
                    CultureCode = CultureEnCa,
                    StopWords = "the,over",
                });

            _mockSearchHelper.Setup(x => x.RephraseSearchText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns<string, bool, bool>(new SearchHelperImpl().RephraseSearchText);
            _mockSearchHelper.Setup(x => x.StripTagsNoAttributes(It.IsAny<string>(), It.IsAny<bool>())).Returns((string html, bool retainSpace) => html);
            SearchHelper.SetTestableInstance(_mockSearchHelper.Object);
        }

        private SearchType GetSearchTypeByNameCallback(string searchTypeName)
        {
            var searchType = new SearchType { SearchTypeName = searchTypeName, SearchTypeId = 0 };
            switch (searchTypeName)
            {
                case ModuleSearchTypeName:
                    searchType.SearchTypeId = ModuleSearchTypeId;
                    break;
                case TabSearchTypeName:
                    searchType.SearchTypeId = TabSearchTypeId;
                    break;
                case OtherSearchTypeName:
                    searchType.SearchTypeId = OtherSearchTypeId;
                    break;
                case DocumentSearchTypeName:
                    searchType.SearchTypeId = DocumentSearchTypeId;
                    break;
                case UrlSearchTypeName:
                    searchType.SearchTypeId = UrlSearchTypeId;
                    break;
            }

            return searchType;
        }

        private IList<string> GetSynonymsCallBack(int portalId, string cultureCode, string term)
        {
            var synonyms = new List<string>();
            if (term == "fox")
                synonyms.Add("wolf");

            return synonyms;
        }

        private DataTable GetAllSearchTypes()
        {
            var dtSearchTypes = new DataTable("SearchTypes");
            var pkId = dtSearchTypes.Columns.Add("SearchTypeId", typeof(int));
            dtSearchTypes.Columns.Add("SearchTypeName", typeof(string));
            dtSearchTypes.Columns.Add("SearchResultClass", typeof(string));
            dtSearchTypes.PrimaryKey = new[] { pkId };

            //Create default Crawler
            dtSearchTypes.Rows.Add(ModuleSearchTypeId, ModuleSearchTypeName, FakeResultControllerClass);
            dtSearchTypes.Rows.Add(TabSearchTypeId, TabSearchTypeName, FakeResultControllerClass);
            dtSearchTypes.Rows.Add(OtherSearchTypeId, OtherSearchTypeName, FakeResultControllerClass);
            dtSearchTypes.Rows.Add(DocumentSearchTypeId, DocumentSearchTypeName, NoPermissionFakeResultControllerClass);
            dtSearchTypes.Rows.Add(UrlSearchTypeId, UrlSearchTypeName, FakeResultControllerClass);

            return dtSearchTypes;
        }

        private IEnumerable<SearchType> GetSearchTypes()
        {
            var searchTypes = new List<SearchType>
                {
                    new SearchType {SearchTypeId = ModuleSearchTypeId, SearchTypeName = ModuleSearchTypeName, SearchResultClass = FakeResultControllerClass},
                    new SearchType {SearchTypeId = TabSearchTypeId, SearchTypeName = TabSearchTypeName, SearchResultClass = FakeResultControllerClass},
                    new SearchType {SearchTypeId = OtherSearchTypeId, SearchTypeName = OtherSearchTypeName, SearchResultClass = FakeResultControllerClass},
                    new SearchType {SearchTypeId = DocumentSearchTypeId, SearchTypeName = DocumentSearchTypeName, SearchResultClass = NoPermissionFakeResultControllerClass},
                    new SearchType {SearchTypeId = UrlSearchTypeId, SearchTypeName = UrlSearchTypeName, SearchResultClass = FakeResultControllerClass}
                };

            return searchTypes;
        }

        private SearchResults SearchForKeyword(string keyword, int searchTypeId = Constants.CONTENTTYPE_ValidContentTypeId, bool useWildcard = true, bool allowLeadingWildcard = false)
        {
            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { searchTypeId }, WildCardSearch = useWildcard, AllowLeadingWildcard = allowLeadingWildcard };
            return _searchController.SiteSearch(query);
        }

        private IDataReader GetPortalsCallBack(string culture)
        {
            return GetPortalCallBack(Constants.PORTAL_ValidPortalId, CultureEnUs);
        }

        private IDataReader GetPortalCallBack(int portalId, string culture)
        {
            var table = new DataTable("Portal");

            var cols = new[]
			           	{
			           		"PortalID", "PortalGroupID", "PortalName", "LogoFile", "FooterText", "ExpiryDate", "UserRegistration", "BannerAdvertising", "AdministratorId", "Currency", "HostFee",
			           		"HostSpace", "PageQuota", "UserQuota", "AdministratorRoleId", "RegisteredRoleId", "Description", "KeyWords", "BackgroundFile", "GUID", "PaymentProcessor", "ProcessorUserId",
			           		"ProcessorPassword", "SiteLogHistory", "Email", "DefaultLanguage", "TimezoneOffset", "AdminTabId", "HomeDirectory", "SplashTabId", "HomeTabId", "LoginTabId", "RegisterTabId",
			           		"UserTabId", "SearchTabId", "Custom404TabId", "Custom500TabId", "SuperTabId", "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate", "CultureCode"
			           	};

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            const int homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright 2011 by DotNetNuke Corporation", null,
                    "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website", "DotNetNuke, DNN, Content, Management, CMS", null,
                    "1057AC7A-3C08-4849-A3A6-3D2AB4662020", null, null, null, "0", "admin@change.me", "en-US", "-8", "58", "Portals/0",
                    null, homePage.ToString("D"), null, null, "57", "56", "-1", "-1", "7", "-1", "2011-08-25 07:34:11", "-1", "2011-08-25 07:34:29", culture);

            return table.CreateDataReader();
        }


        #endregion
    }
}
