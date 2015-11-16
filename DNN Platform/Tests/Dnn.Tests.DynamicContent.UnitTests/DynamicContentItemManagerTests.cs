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


        private MockRepository _mockRepository;
        private Mock<IFieldDefinitionManager> _mockFieldDefinitionManager;
        private Mock<IContentController> _mockContentController;
        private Mock<IDynamicContentTypeManager> _mockDynamicContentTypeManager;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Default);

            _mockFieldDefinitionManager = _mockRepository.Create<IFieldDefinitionManager>();
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionManager.Object);

            _mockDynamicContentTypeManager = _mockRepository.Create<IDynamicContentTypeManager>();
            DynamicContentTypeManager.SetTestableInstance(_mockDynamicContentTypeManager.Object);

            _mockContentController = _mockRepository.Create<IContentController>();
            ContentController.SetTestableInstance(_mockContentController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            FieldDefinitionManager.ClearInstance();
            DynamicContentTypeManager.ClearInstance();
            DataTypeManager.ClearInstance();
            ContentController.ClearInstance();
        }


        #region AddContentItem Tests
        [Test]
        public void AddContentItem_Throws_On_Null_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();

            //Act
            var act = new TestDelegate(() => controller.AddContentItem(null));

            //Assert
            Assert.Throws<ArgumentNullException>(act);
        }
        
        [Test]
        public void AddContentItem_Throws_On_Negative_ContentTypeId_Property_Of_ContentType_Property_Of_ContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, new DynamicContentType {PortalId = Constants.PORTAL_ValidPortalId});

            //Act
            var act = new TestDelegate(() => controller.AddContentItem(contentItem));

            //Assert
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }

        [Test]
        public void AddContentItem_Calls_ContentController_AddContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            const int contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            const int portalId = Constants.PORTAL_ValidPortalId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var dynamicContent = new DynamicContentItem(portalId, GetContentType(contentTypeId, portalId));

            //Act
            controller.AddContentItem(dynamicContent);

            //Assert
            _mockContentController.Verify(c => c.AddContentItem(It.IsAny<ContentItem>()), Times.Once);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void AddContentItem_Calls_ContentController_AddContentItem_With_Valid_ContentItem()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var controller = new DynamicContentItemManager();

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var dynamicContent = new DynamicContentItem(portalId, GetContentType(contentTypeId, portalId));
            dynamicContent.Content.Fields["FieldName1"].Value = 1;
            dynamicContent.Content.Fields["FieldName2"].Value = true;
            dynamicContent.Content.Fields["FieldName3"].Value = "abc";


            ContentItem contentItem = null;
            _mockContentController.Setup(c => c.AddContentItem(It.IsAny<ContentItem>()))
                                    .Callback<ContentItem>(c => contentItem = c);

            //Act
            controller.AddContentItem(dynamicContent);

            //Assert
            Assert.AreEqual(contentTypeId, contentItem.ContentTypeId);
            Assert.AreEqual(String.Empty, contentItem.ContentKey);
            Assert.AreEqual(_testJson.ToString(), contentItem.Content);
            _mockRepository.VerifyAll();
        }
        #endregion

        #region CreateContentItem Tests
        [Test]
        public void CreateContentItem_Throws_On_Null_ContentType()
        {
            //Arrange
            const int portalId = Constants.CONTENT_ValidPortalId;
            var controller = new DynamicContentItemManager();

            //Act
            var act =
                new TestDelegate(
                    () =>
                        controller.CreateContentItem((DynamicContentType) null, portalId));
            //Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        [Test]
        public void CreateContentItem_Throws_If_ContentType_Has_No_Fields_Defined()
        {
            //Arrange
            const int portalId = Constants.CONTENT_ValidPortalId;
            var controller = new DynamicContentItemManager();
            var contentType = new DynamicContentType { PortalId = Constants.PORTAL_ValidPortalId };
            
            //Act
            var act =
                new TestDelegate(
                    () =>
                        controller.CreateContentItem(contentType, portalId));

            //Assert
            Assert.Throws<InvalidOperationException>(act);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void CreateContentItem_Returns_Valid_DyamicContentItem()
        {
            //Arrange
            const int portalId = Constants.CONTENT_ValidPortalId;
            const int contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var controller = new DynamicContentItemManager();

            //Act
            var contentType = GetContentType(contentTypeId, portalId);
            var dynamicContent = controller.CreateContentItem(contentType, portalId);

            //Assert
            Assert.AreSame(contentType, dynamicContent.ContentType);
            Assert.AreEqual(contentType.FieldDefinitions.Count, dynamicContent.Content.Fields.Count);
            Assert.AreEqual(portalId, dynamicContent.PortalId);
            foreach (var field in contentType.FieldDefinitions)
            {
                Assert.AreSame(field, dynamicContent.Content.Fields[field.Name].Definition);
            }
            _mockRepository.VerifyAll();
        }

        [Test]
        public void CreateContentItem_Overload_Throws_On_Null_ContentItem()
        {
            //Arrange
            const int portalId = Constants.CONTENT_ValidPortalId;
            var controller = new DynamicContentItemManager();
            ContentItem contentItem = null;

            //Act, Assert
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => controller.CreateContentItem(contentItem, portalId));
        }

        [Test]
        public void CreateContentItem_Overload_Throws_On_Null_Or_Empty_Content()
        {
            //Arrange
            const int contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            const int portalId = Constants.PORTAL_ValidPortalId;
            var controller = new DynamicContentItemManager();
            var contentItem = new ContentItem
                                    {
                                        ContentTypeId = contentTypeId
                                    };

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            
            var contentType = GetContentType(contentTypeId, portalId);
            _mockDynamicContentTypeManager.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                .Returns(contentType);

            //Act
            var act = new TestDelegate(() => controller.CreateContentItem(contentItem, portalId));

            //Assert
            Assert.Throws<ArgumentException>(act);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void CreateContentItem_Overload_Returns_Valid_Content()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            var controller = new DynamicContentItemManager();
            const int contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var contentType = GetContentType(contentTypeId, portalId);

            _mockDynamicContentTypeManager.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                .Returns(contentType);
            
            var mockDataTypeManager = _mockRepository.Create<IDataTypeManager>();
            mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>()))
                .Returns(() => new List<DataType>
                                    {
                                        new DataType { DataTypeId = -1, UnderlyingDataType = UnderlyingDataType.String}
                                    }
                                .AsQueryable());
            DataTypeManager.SetTestableInstance(mockDataTypeManager.Object);

            var contentItem = new ContentItem { Content = _testJson.ToString(), ContentTypeId = contentTypeId };

            //Act
            var dynamicContent = controller.CreateContentItem(contentItem, portalId);

            //Assert
            Assert.AreEqual(contentTypeId, dynamicContent.ContentType.ContentTypeId);
            Assert.AreEqual(contentType.FieldDefinitions.Count, dynamicContent.Content.Fields.Count);
            Assert.AreEqual(portalId, dynamicContent.PortalId);
            foreach (var field in contentType.FieldDefinitions)
            {
                Assert.AreSame(field, dynamicContent.Content.Fields[field.Name].Definition);
            }
            _mockRepository.VerifyAll();
        }
        #endregion

        #region DeleteContentItem Tests
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
            const int contentItemId = Constants.CONTENT_ValidContentItemId;
            var controller = new DynamicContentItemManager();
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, new DynamicContentType(Constants.PORTAL_ValidPortalId)) { ContentItemId = contentItemId };
            
            //Act
            controller.DeleteContentItem(contentItem);

            //Assert
            _mockContentController.Verify(c => c.DeleteContentItem(contentItemId), Times.Once);
            _mockRepository.VerifyAll();
        }
        #endregion

        #region UpdateContentItem Tests
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
        public void UpdateContentItem_Calls_ContentController_UpdateContentItem()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            const int contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            const int portalId = Constants.PORTAL_ValidPortalId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var dynamicContent = new DynamicContentItem(Constants.PORTAL_ValidPortalId, GetContentType(contentTypeId, portalId))
                                        {
                                            ContentItemId = Constants.CONTENT_ValidContentItemId
                                        };
            
            //Act
            controller.UpdateContentItem(dynamicContent);

            //Assert
            _mockContentController.Verify(c => c.UpdateContentItem(It.IsAny<ContentItem>()), Times.Once);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateContentItem_Calls_ContentController_UpdateContentItem_With_Valid_ContentItem()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentItemId = Constants.CONTENT_ValidContentItemId;
            const int contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var controller = new DynamicContentItemManager();

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var dynamicContent = new DynamicContentItem(portalId, GetContentType(contentTypeId, portalId))
                                        {
                                            ContentItemId = contentItemId
                                        };

            dynamicContent.Content.Fields["FieldName1"].Value = 1;
            dynamicContent.Content.Fields["FieldName2"].Value = true;
            dynamicContent.Content.Fields["FieldName3"].Value = "abc";


            ContentItem contentItem = null;
            _mockContentController.Setup(c => c.UpdateContentItem(It.IsAny<ContentItem>()))
                                    .Callback<ContentItem>(c => contentItem = c);

            //Act
            controller.UpdateContentItem(dynamicContent);

            //Assert
            Assert.AreEqual(contentItemId, contentItem.ContentItemId);
            Assert.AreEqual(contentTypeId, contentItem.ContentTypeId);
            Assert.AreEqual(String.Empty, contentItem.ContentKey);
            Assert.AreEqual(_testJson.ToString(), contentItem.Content);
            _mockRepository.VerifyAll();
        }
        #endregion

        private static DynamicContentType GetContentType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType { ContentTypeId = contentTypeId, PortalId = portalId };

            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName1", PortalId = portalId });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName2", PortalId = portalId });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName3", PortalId = portalId });

            return contentType;
        }
    }
}
