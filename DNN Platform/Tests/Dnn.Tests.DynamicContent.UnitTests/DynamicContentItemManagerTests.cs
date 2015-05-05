#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Entities.Content;
using DotNetNuke.Tests.Utilities;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class DynamicContentItemManagerTests
    {
        private JObject _testJson = new JObject(
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

        [TearDown]
        public void TearDown()
        {
            FieldDefinitionManager.ClearInstance();
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
            var contentItem = new DynamicContentItem(new DynamicContentType() {PortalId = Constants.PORTAL_ValidPortalId});

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

            var contentItem = new DynamicContentItem(GetContentType(contentTypeId, Constants.PORTAL_ValidPortalId));

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

            var dynamicContent = new DynamicContentItem(GetContentType(contentTypeId, portalId))
            {
                ModuleId = Constants.MODULE_ValidId
            };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);

            //Act
            controller.AddContentItem(dynamicContent);

            //Assert
            mockContentController.Verify((c) => c.AddContentItem(It.IsAny<ContentItem>()), Times.Once);
        }

        [Test]
        public void AddContentItem_Calls_ContentController_AddContentItem_With_Valid_ContentItem()
        {
            //Arrange
            var moduleId = Constants.MODULE_ValidId;
            var portalId = Constants.PORTAL_ValidPortalId;
            var controller = new DynamicContentItemManager();
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId))
                .Returns(new List<DynamicContentType>() { GetContentType(contentTypeId, portalId) }.AsQueryable());
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var dynamicContent = new DynamicContentItem(GetContentType(contentTypeId, portalId))
                                        {
                                            ModuleId = moduleId
                                        };
            dynamicContent.Fields["FieldName1"].Value = 1;
            dynamicContent.Fields["FieldName2"].Value = true;
            dynamicContent.Fields["FieldName3"].Value = "abc";


            ContentItem contentItem = null;
            var mockContentController = new Mock<IContentController>();
            mockContentController.Setup(c => c.AddContentItem(It.IsAny<ContentItem>()))
                                    .Callback<ContentItem>((c) => contentItem = c);
            ContentController.SetTestableInstance(mockContentController.Object);

            //Act
            controller.AddContentItem(dynamicContent);

            //Assert
            Assert.AreEqual(-1, contentItem.TabID);
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
            Assert.Throws<ArgumentNullException>(() => controller.CreateContentItem(Constants.MODULE_ValidId, contentType));
        }

        [Test]
        public void CreateContentItem_Throws_If_ContentType_Has_Negative_PortalId()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentType = new DynamicContentType() {PortalId = Constants.PORTAL_InValidPortalId };

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.CreateContentItem(Constants.MODULE_ValidId, contentType));
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
            Assert.Throws<InvalidOperationException>(() => controller.CreateContentItem(Constants.MODULE_ValidId, contentType));
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
            var dynamicContent = controller.CreateContentItem(moduleId, contentType);

            //Assert
            Assert.AreSame(contentType, dynamicContent.ContentType);
            Assert.AreEqual(contentType.FieldDefinitions.Count, dynamicContent.Fields.Count);
            Assert.AreEqual(portalId, dynamicContent.PortalId);
            Assert.AreEqual(moduleId, dynamicContent.ModuleId);
            foreach (var field in contentType.FieldDefinitions)
            {
                Assert.AreSame(field, dynamicContent.Fields[field.Name].Definition);
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
            Assert.Throws<ArgumentNullException>(() => controller.CreateContentItem(Constants.PORTAL_ValidPortalId, contentItem));
        }

        [Test]
        public void CreateContentItem_Overload_Throws_On_Negative_PortalId()
        {
            //Arrange
            var controller = new DynamicContentItemManager();

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.CreateContentItem(Constants.PORTAL_InValidPortalId, new ContentItem()));
        }

        [Test]
        public void CreateContentItem_Overload_Throws_On_Null_Or_Empty_Content()
        {
            //Arrange
            var controller = new DynamicContentItemManager();

            //Act, Assert
            Assert.Throws<ArgumentException>(() => controller.CreateContentItem(Constants.PORTAL_ValidPortalId, new ContentItem()));
        }

        [Test]
        public void CreateContentItem_Overload_Returns_Valid_Content()
        {
            //Arrange
            var controller = new DynamicContentItemManager();
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;
            var moduleId = Constants.MODULE_ValidId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = GetContentType(contentTypeId, portalId);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId))
                .Returns(new List<DynamicContentType>() { contentType }.AsQueryable());
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var contentItem = new ContentItem { Content = _testJson.ToString(), ModuleID = moduleId };

            //Act
            var dynamicContent = controller.CreateContentItem(portalId, contentItem);

            //Assert
            Assert.AreEqual(contentTypeId, dynamicContent.ContentType.ContentTypeId);
            Assert.AreEqual(contentType.FieldDefinitions.Count, dynamicContent.Fields.Count);
            Assert.AreEqual(portalId, dynamicContent.PortalId);
            Assert.AreEqual(moduleId, dynamicContent.ModuleId);
            foreach (var field in contentType.FieldDefinitions)
            {
                Assert.AreSame(field, dynamicContent.Fields[field.Name].Definition);
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
            mockContentController.Verify((c) => c.DeleteContentItem(contentItemId), Times.Once);
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
            var contentItem = new DynamicContentItem(new DynamicContentType() {PortalId = Constants.PORTAL_ValidPortalId})
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

            var contentItem = new DynamicContentItem(GetContentType(contentTypeId, Constants.PORTAL_ValidPortalId))
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
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var dynamicContent = new DynamicContentItem(GetContentType(contentTypeId, portalId))
                                        {
                                            ModuleId = Constants.MODULE_ValidId,
                                            ContentItemId = Constants.CONTENT_ValidContentItemId
                                        };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);

            //Act
            controller.UpdateContentItem(dynamicContent);

            //Assert
            mockContentController.Verify((c) => c.UpdateContentItem(It.IsAny<ContentItem>()), Times.Once);
        }

        [Test]
        public void UpdateContentItem_Calls_ContentController_UpdateContentItem_With_Valid_ContentItem()
        {
            //Arrange
            var moduleId = Constants.MODULE_ValidId;
            var portalId = Constants.PORTAL_ValidPortalId;
            var contentItemId = Constants.CONTENT_ValidContentItemId;
            var controller = new DynamicContentItemManager();
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId))
                .Returns(new List<DynamicContentType>() { GetContentType(contentTypeId, portalId) }.AsQueryable());
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var dynamicContent = new DynamicContentItem(GetContentType(contentTypeId, portalId))
                                        {
                                            ModuleId = moduleId,
                                            ContentItemId = contentItemId
            };

            dynamicContent.Fields["FieldName1"].Value = 1;
            dynamicContent.Fields["FieldName2"].Value = true;
            dynamicContent.Fields["FieldName3"].Value = "abc";


            ContentItem contentItem = null;
            var mockContentController = new Mock<IContentController>();
            mockContentController.Setup(c => c.UpdateContentItem(It.IsAny<ContentItem>()))
                                    .Callback<ContentItem>((c) => contentItem = c);
            ContentController.SetTestableInstance(mockContentController.Object);

            //Act
            controller.UpdateContentItem(dynamicContent);

            //Assert
            Assert.AreEqual(-1, contentItem.TabID);
            Assert.AreEqual(contentItemId, contentItem.ContentItemId);
            Assert.AreEqual(moduleId, contentItem.ModuleID);
            Assert.AreEqual(contentTypeId, contentItem.ContentTypeId);
            Assert.AreEqual(String.Empty, contentItem.ContentKey);
            Assert.AreEqual(_testJson.ToString(), contentItem.Content);
        }

        private DynamicContentType GetContentType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId, PortalId = portalId };

            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName1" });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName2" });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName3" });

            return contentType;
        }


    }
}
