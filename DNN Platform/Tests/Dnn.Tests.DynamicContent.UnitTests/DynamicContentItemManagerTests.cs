// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Tests.Utilities;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class DynamicContentItemManagerTests
    {
        private readonly JObject _testJson = new JObject(
                                new JProperty("FieldName1", 1),
                                new JProperty("FieldName2", true),
                                new JProperty("FieldName3", "abc")
                            );

        [TearDown]
        public void TearDown()
        {
            FieldDefinitionManager.ClearInstance();
            ModuleController.ClearInstance();
            DataTypeManager.ClearInstance();
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
        public void DynamicContentItem_Constructor_Throws_On_Null_ContentType_Property_Of_ContentItem()
        {            
            Assert.Throws<ArgumentNullException>(() => new DynamicContentItem(Constants.PORTAL_ValidPortalId, null));
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
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;
            var controller = new DynamicContentItemManager();
            var contentItem = new ContentItem
                                    {
                                        ModuleID = Constants.MODULE_ValidId,
                                        TabID = Constants.TAB_ValidId,
                                        ContentTypeId = contentTypeId
                                    };

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockModuleController = new Mock<IModuleController>();
            mockModuleController.Setup(c => c.GetModule(Constants.MODULE_ValidId, Constants.TAB_ValidId, false))
                .Returns(new ModuleInfo() {PortalID = portalId});
            ModuleController.SetTestableInstance(mockModuleController.Object);

            var contentType = GetContentType(contentTypeId, portalId);
            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, false))
                .Returns(contentType);
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            //Act, Assert
            Assert.Throws<ArgumentException>(() => controller.CreateContentItem(contentItem));
            mockFieldDefinitionController.VerifyAll();
            mockModuleController.VerifyAll();
            mockContentTypeController.VerifyAll();
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
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, false))
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

            var contentItem = new ContentItem { Content = _testJson.ToString(), ModuleID = moduleId, TabID = tabId, ContentTypeId = contentTypeId };

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
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, new DynamicContentType(Constants.PORTAL_ValidPortalId));

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.DeleteContentItem(contentItem));
        }

        [Test]
        public void DeleteContentItem_Calls_ContentController_UpdateContentItem()
        {
            //Arrange
            var contentItemId = Constants.CONTENT_ValidContentItemId;
            var controller = new DynamicContentItemManager();
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, new DynamicContentType(Constants.PORTAL_ValidPortalId)) { ContentItemId = contentItemId };

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
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, new DynamicContentType(Constants.PORTAL_ValidPortalId));

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.UpdateContentItem(contentItem));
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

        private DynamicContentType GetContentType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId, PortalId = portalId };

            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName1", PortalId = portalId });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName2", PortalId = portalId });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName3", PortalId = portalId });

            return contentType;
        }


    }
}
