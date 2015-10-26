// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Exceptions;
using DotNetNuke.Tests.Utilities;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class DynamicContentItemTests
    {
        private const int CONTENTTYPE_Simple = 1;
        private const int CONTENTTYPE_Complex = 2;
        private const int CONTENTTYPE_ListContent = 3;
        private const int CONTENTTYPE_ListData = 4;
        private const int CONTENTTYPE_Child = 1;
        private const int DATATYPE_String = 1;
        private const int DATATYPE_Integer = 2;
        private const int DATATYPE_Boolean = 3;

        private static readonly JObject SimpleContentTypeJson = new JObject(
                                new JProperty("contentTypeId", CONTENTTYPE_Simple),
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

        private static readonly JObject ChildContentTypeJson = new JObject(
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
                            );

        private static readonly JObject ComplexContentTypeJson = new JObject(
                                new JProperty("contentTypeId", CONTENTTYPE_Complex),
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
                                                    ),
                                                 new JObject(
                                                    new JProperty("name", "FieldName4"),
                                                    new JProperty("value", ChildContentTypeJson)
                                                    )
                                                )
                                           )
                                        )
                                    )
                            );

        private static readonly JObject ListContentTypeJson = new JObject(
                                new JProperty("contentTypeId", CONTENTTYPE_ListContent),
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
                                                    ),
                                                 new JObject(
                                                    new JProperty("name", "FieldName4"),
                                                    new JProperty("value", new JArray(
                                                        ChildContentTypeJson, 
                                                        ChildContentTypeJson, 
                                                        ChildContentTypeJson))
                                                    )
                                                )
                                           )
                                        )
                                    )
                            );

        private static readonly JObject ListDataTypeJson = new JObject(
                        new JProperty("contentTypeId", CONTENTTYPE_ListData),
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
                                            ),
                                        new JObject(
                                            new JProperty("name", "FieldName4"),
                                            new JProperty("value", 
                                                new JArray(
                                                    new JObject(
                                                        new JProperty("name", "FieldName5"),
                                                            new JProperty("value", 2)
                                                    ),
                                                    new JObject(
                                                        new JProperty("name", "FieldName5"),
                                                            new JProperty("value", 3)
                                                    ),
                                                    new JObject(
                                                        new JProperty("name", "FieldName5"),
                                                            new JProperty("value", 4)
                                                    )
                                                )
                                            )
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
            DynamicContentTypeManager.ClearInstance();
            DataTypeManager.ClearInstance();
        }

        [Test]
        public void Constructor_Throws_On_Negative_PortalId()
        {
            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new DynamicContentItem(-1));
        }

        [Test]
        public void Constructor_Sets_PortalId()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;

            //Act
            var dynamicContent = new DynamicContentItem(portalId);

            //Assert
            Assert.AreEqual(portalId, dynamicContent.PortalId);
        }

        [Test]
        public void Constructor_Sets_Default_Properties()
        {
            //Arrange

            //Act
            var content = new DynamicContentItem(Constants.PORTAL_ValidPortalId);

            //Assert
            Assert.AreEqual(-1, content.ModuleId);
            Assert.AreEqual(-1, content.ContentItemId);
        }

        [Test]
        public void Constructor_Overload_Throws_On_Null_ContentType()
        {
            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => new DynamicContentItem(Constants.PORTAL_ValidPortalId, null));
        }

        [Test]
        public void Constructor_Overload_Sets_Default_Properties()
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_Simple;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId, PortalId = portalId };

            //Act
            var content = new DynamicContentItem(portalId, contentType);

            //Assert
            Assert.AreEqual(-1, content.ModuleId);
            Assert.AreEqual(-1, content.ContentItemId);
        }

        [Test]
        public void Constructor_Overload_Sets_ContentType()
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_Simple;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = new DynamicContentType() {ContentTypeId = contentTypeId, PortalId = portalId};

            //Act
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Assert
            Assert.AreSame(contentType, dynamicContent.ContentType);
        }

        [Test]
        public void Constructor_Overload_Sets_Fields()
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_Simple;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = GetSimpleContentType(contentTypeId, portalId);

            //Act
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Assert
            Assert.AreEqual(contentType.FieldDefinitions.Count, dynamicContent.Content.Fields.Count);
        }

        [Test]
        public void Constructor_Overload_Sets_FieldDefinition_PropertyOf_Fields()
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_Simple;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = GetSimpleContentType(contentTypeId, portalId);

            //Act
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Assert
            foreach (var fieldDefinition in contentType.FieldDefinitions)
            {
                Assert.AreSame(fieldDefinition, dynamicContent.Content.Fields[fieldDefinition.Name].Definition);
            }
        }

        [Test]
        public void Constructor_Overload_Throws_On_Negative_PortalId()
        {
            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new DynamicContentItem(-1, new DynamicContentType()));
        }

        [Test]
        public void FromJson_Throws_If_ContentTypeId_Is_String()
        {
            //Arrange
            var testJson = new JObject(
                                new JProperty("contentTypeId", "abc")
                            );

            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            //Act, Assert
            Assert.Throws<FormatException>(() => dynamicContent.FromJson(testJson.ToString()));
        }

        [Test]
        public void FromJson_Reads_ContentTypeId_From_Json_And_Sets_ContentType()
        {
            //Arrange
            var contentTypeId = 234;
            var testJson = new JObject(
                                new JProperty("contentTypeId", contentTypeId),
                                new JProperty("content", new JObject())
                            );

            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(GetSimpleContentType(contentTypeId, portalId));
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            //Act
            dynamicContent.FromJson(testJson.ToString());

            //Assert
            Assert.AreEqual(contentTypeId, dynamicContent.ContentType.ContentTypeId);
        }

        [Test]
        public void FromJson_Throws_If_ContentType_Null()
        {
            //Arrange
            var contentTypeId = 234;
            var testJson = new JObject(
                                new JProperty("contentTypeId", contentTypeId),
                                new JProperty("content", "")
                            );

            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId, false))
                .Returns(new List<DynamicContentType>().AsQueryable());
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            //Act, Assert
            Assert.Throws<JsonContentTypeInvalidException>(() => dynamicContent.FromJson(testJson.ToString()));
        }

        [Test]
        public void FromJson_Throws_If_Content_Null()
        {
            //Arrange
            var contentTypeId = 234;
            var testJson = new JObject(
                                new JProperty("contentTypeId", contentTypeId),
                                new JProperty("content", "")
                            );

            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(new DynamicContentType() {ContentTypeId = contentTypeId});
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            //Act, Assert
            Assert.Throws<JsonMissingContentException>(() => dynamicContent.FromJson(testJson.ToString()));
        }

        [Test]
        public void FromJson_Creates_Empty_Fields_Property_If_No_Fields()
        {
            //Arrange
            var contentTypeId = 234;
            var testJson = new JObject(
                                new JProperty("contentTypeId", contentTypeId),
                                new JProperty("content", new JObject())
                            );

            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(GetSimpleContentType(contentTypeId, portalId));
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            //Act
            dynamicContent.FromJson(testJson.ToString());

            //Assert
            Assert.AreEqual(0, dynamicContent.Content.Fields.Count);
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields_For_SimpleType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpSimpleType(portalId);

            //Act
            dynamicContent.FromJson(SimpleContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(3, dynamicContent.Content.Fields.Count);
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields_For_ComplexType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpComplexType(portalId);

            //Act
            dynamicContent.FromJson(ComplexContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(4, dynamicContent.Content.Fields.Count);
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields_For_ListContentType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpListContentType(portalId);

            //Act
            dynamicContent.FromJson(ListContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(4, dynamicContent.Content.Fields.Count);
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields_For_ListDataType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpListDataType(portalId);

            //Act
            dynamicContent.FromJson(ListDataTypeJson.ToString());

            //Assert
            Assert.AreEqual(4, dynamicContent.Content.Fields.Count);
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions_For_SimpleType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpSimpleType(portalId);

            //Act
            dynamicContent.FromJson(SimpleContentTypeJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName3"));
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions_For_ComplexType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpComplexType(portalId);

            //Act
            dynamicContent.FromJson(ComplexContentTypeJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName3"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName4"));
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions_For_ListContentType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpListContentType(portalId);

            //Act
            dynamicContent.FromJson(ListContentTypeJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName3"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName4"));
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions_For_ListDataType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpListDataType(portalId);

            //Act
            dynamicContent.FromJson(ListDataTypeJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName3"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName4"));
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_SimpleType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpSimpleType(portalId);

            //Act
            dynamicContent.FromJson(SimpleContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(1, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(true, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual("abc", dynamicContent.Content.Fields["FieldName3"].Value);
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ComplexType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpComplexType(portalId);

            //Act
            dynamicContent.FromJson(ComplexContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(1, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(true, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual("abc", dynamicContent.Content.Fields["FieldName3"].Value);
            Assert.IsInstanceOf<DynamicContentPart>(dynamicContent.Content.Fields["FieldName4"].Value);
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ListContentType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpListContentType(portalId);

            //Act
            dynamicContent.FromJson(ListContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(1, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(true, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual("abc", dynamicContent.Content.Fields["FieldName3"].Value);
            var list = dynamicContent.Content.Fields["FieldName4"].Value as List<DynamicContentField>;
            Assert.IsNotNull(list);
            foreach (var field in list)
            {
                Assert.IsInstanceOf<DynamicContentPart>(field.Value);
            }
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ListDataType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpListDataType(portalId);

            //Act
            dynamicContent.FromJson(ListDataTypeJson.ToString());

            //Assert
            Assert.AreEqual(1, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(true, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual("abc", dynamicContent.Content.Fields["FieldName3"].Value);
            var list = dynamicContent.Content.Fields["FieldName4"].Value as List<DynamicContentField>;
            Assert.IsNotNull(list);
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ComplexType_SubFields()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpComplexType(portalId);

            //Act
            dynamicContent.FromJson(ComplexContentTypeJson.ToString());

            //Assert
            var part = dynamicContent.Content.Fields["FieldName4"].Value as DynamicContentPart;
            Assert.IsNotNull(part);
            Assert.AreEqual(1, part.Fields["FieldName1"].Value);
            Assert.AreEqual(true, part.Fields["FieldName2"].Value);
            Assert.AreEqual("abc", part.Fields["FieldName3"].Value);

        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ListContentType_SubFields()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpListContentType(portalId);

            //Act
            dynamicContent.FromJson(ListContentTypeJson.ToString());

            //Assert
            var list = dynamicContent.Content.Fields["FieldName4"].Value as List<DynamicContentField>;
            Assert.IsNotNull(list);
            foreach (var item in list)
            {
                var part = item.Value as DynamicContentPart;
                Assert.IsNotNull(part);
                Assert.AreEqual(1, part.Fields["FieldName1"].Value);
                Assert.AreEqual(true, part.Fields["FieldName2"].Value);
                Assert.AreEqual("abc", part.Fields["FieldName3"].Value);
            }
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ListDataType_SubFields()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            SetUpListDataType(portalId);

            //Act
            dynamicContent.FromJson(ListDataTypeJson.ToString());

            //Assert
            var list = dynamicContent.Content.Fields["FieldName4"].Value as List<DynamicContentField>;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list[0].Value);
            Assert.AreEqual(3, list[1].Value);
            Assert.AreEqual(4, list[2].Value);
        }

        [Test]
        public void FromJson_DoesNotThrow_If_Cant_Match_Field_From_Json_With_FieldDefinitions()
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_Simple;

            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = GetSimpleContentType(contentTypeId, portalId);
            contentType.FieldDefinitions.RemoveAt(2);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(new DynamicContentType() { ContentTypeId = contentTypeId });
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var mockDataTypeManager = new Mock<IDataTypeManager>();
            mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());
            DataTypeManager.SetTestableInstance(mockDataTypeManager.Object);

            //Act
            var act = new TestDelegate(() => dynamicContent.FromJson(SimpleContentTypeJson.ToString()));
            
            // Assert
            Assert.DoesNotThrow(act);
        }

        [Test]
        public void FromJson_Set_DefaultValues_If_Cant_Match_Field_From_Json_With_FieldDefinitions()
        {
            //Arrange
            const int contentTypeId = CONTENTTYPE_Simple;
            const int portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new []
                {
                    new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName1", PortalId = portalId, FieldTypeId = DATATYPE_Integer},
                    new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName2", PortalId = portalId, FieldTypeId = DATATYPE_Boolean },
                    new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName3", PortalId = portalId, FieldTypeId = DATATYPE_String }
                }.AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);
            
            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(new DynamicContentType { ContentTypeId = contentTypeId });
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var mockDataTypeManager = new Mock<IDataTypeManager>();
            mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());
            DataTypeManager.SetTestableInstance(mockDataTypeManager.Object);

            //Act
            dynamicContent.FromJson((new JObject(
                            new JProperty("contentTypeId", CONTENTTYPE_Simple),
                            new JProperty("content",
                                new JObject(
                                    new JProperty("field",
                                            new JArray(
                                            new JObject(
                                                new JProperty("name", "NotDefinedFieldName"),
                                                new JProperty("value", 1)
                                                )
                                        )
                                )
                            )))).ToString());

            // Assert
            Assert.AreEqual(0, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(false, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual(string.Empty, dynamicContent.Content.Fields["FieldName3"].Value);
            Assert.IsFalse(dynamicContent.Content.Fields.ContainsKey("NotDefinedFieldName"));
        }

        [Test]
        public void ToJson_Generates_Json_String_For_SimpleType()
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_Simple;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = GetSimpleContentType(contentTypeId, portalId);

            var dynamicContent = new DynamicContentItem(portalId, contentType);
            dynamicContent.Content.Fields["FieldName1"].Value = 1;
            dynamicContent.Content.Fields["FieldName2"].Value = true;
            dynamicContent.Content.Fields["FieldName3"].Value = "abc";

            //Act
            var json = dynamicContent.ToJson();

            //Asert
            Assert.AreEqual(SimpleContentTypeJson.ToString(), json);
        }

        [Test]
        public void ToJson_Generates_Json_String_For_ComplexType()
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_Complex;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = GetComplexContentType(contentTypeId, portalId);
            var childType = GetSimpleContentType(CONTENTTYPE_Child, portalId);

            var dynamicContent = new DynamicContentItem(portalId, contentType);
            dynamicContent.Content.Fields["FieldName1"].Value = 1;
            dynamicContent.Content.Fields["FieldName2"].Value = true;
            dynamicContent.Content.Fields["FieldName3"].Value = "abc";

            var childPart = new DynamicContentPart(portalId, childType);
            childPart.Fields["FieldName1"].Value = 1;
            childPart.Fields["FieldName2"].Value = true;
            childPart.Fields["FieldName3"].Value = "abc";
            dynamicContent.Content.Fields["FieldName4"].Value = childPart;

            //Act
            var json = dynamicContent.ToJson();

            //Asert
            Assert.AreEqual(ComplexContentTypeJson.ToString(), json);
        }

        [Test]
        public void ToJson_Generates_Json_String_For_ListContentType()
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_ListContent;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(GetComplexContentType(contentTypeId, portalId));
            mockContentTypeController.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                        .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var contentType = GetListContentType(contentTypeId, portalId);
            var childType = GetSimpleContentType(CONTENTTYPE_Child, portalId);

            var dynamicContent = new DynamicContentItem(portalId, contentType);
            dynamicContent.Content.Fields["FieldName1"].Value = 1;
            dynamicContent.Content.Fields["FieldName2"].Value = true;
            dynamicContent.Content.Fields["FieldName3"].Value = "abc";

            var childPart = new DynamicContentPart(portalId, childType);
            childPart.Fields["FieldName1"].Value = 1;
            childPart.Fields["FieldName2"].Value = true;
            childPart.Fields["FieldName3"].Value = "abc";

            var list = new List<DynamicContentPart> {childPart, childPart, childPart};
            dynamicContent.Content.Fields["FieldName4"].Value = list;

            //Act
            var json = dynamicContent.ToJson();

            //Asert
            Assert.AreEqual(ListContentTypeJson.ToString(), json);
        }

        [Test]
        public void ToJson_Generates_Json_String_For_ListDataType()
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_ListData;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(GetListDataType(contentTypeId, portalId));
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var contentType = GetListDataType(contentTypeId, portalId);

            var dynamicContent = new DynamicContentItem(portalId, contentType);
            dynamicContent.Content.Fields["FieldName1"].Value = 1;
            dynamicContent.Content.Fields["FieldName2"].Value = true;
            dynamicContent.Content.Fields["FieldName3"].Value = "abc";

            var list = new List<DynamicContentField>();
            for (int i = 2; i < 5; i++)
            {
                list.Add(new DynamicContentField(new FieldDefinition(portalId) {Name = "FieldName5"}) { Value = i });
            }
            dynamicContent.Content.Fields["FieldName4"].Value = list;

            //Act
            var json = dynamicContent.ToJson();

            //Asert
            Assert.AreEqual(ListDataTypeJson.ToString(), json);
        }

        private DynamicContentType GetComplexContentType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId, PortalId = portalId };

            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName1", PortalId = portalId, FieldTypeId = DATATYPE_Integer, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName2", PortalId = portalId, FieldTypeId = DATATYPE_Boolean, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName3", PortalId = portalId, FieldTypeId = DATATYPE_String, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName4", PortalId = portalId, FieldTypeId = CONTENTTYPE_Child, IsReferenceType = true });

            return contentType;
        }

        private DynamicContentType GetListContentType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId, PortalId = portalId };

            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName1", PortalId = portalId, FieldTypeId = DATATYPE_Integer, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName2", PortalId = portalId, FieldTypeId = DATATYPE_Boolean, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName3", PortalId = portalId, FieldTypeId = DATATYPE_String, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName4", PortalId = portalId, FieldTypeId = CONTENTTYPE_Child, IsReferenceType = true, IsList = true });

            return contentType;
        }

        private DynamicContentType GetListDataType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId, PortalId = portalId };

            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName1", PortalId = portalId, FieldTypeId = DATATYPE_Integer, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName2", PortalId = portalId, FieldTypeId = DATATYPE_Boolean, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName3", PortalId = portalId, FieldTypeId = DATATYPE_String, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName4", PortalId = portalId, FieldTypeId = DATATYPE_Integer, IsReferenceType = false, IsList = true });

            return contentType;
        }

        private DynamicContentType GetSimpleContentType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType() {ContentTypeId = contentTypeId, PortalId = portalId};

            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName1", PortalId = portalId, FieldTypeId = DATATYPE_Integer, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName2", PortalId = portalId, FieldTypeId = DATATYPE_Boolean, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName3", PortalId = portalId, FieldTypeId = DATATYPE_String, IsReferenceType = false });

            return contentType;
        }

        private List<DataType> GetDataTypes()
        {
            var dataTypes = new List<DataType>();

            dataTypes.Add(new DataType { DataTypeId = DATATYPE_Integer, UnderlyingDataType = UnderlyingDataType.Integer});
            dataTypes.Add(new DataType { DataTypeId = DATATYPE_String, UnderlyingDataType = UnderlyingDataType.String });
            dataTypes.Add(new DataType { DataTypeId = DATATYPE_Boolean, UnderlyingDataType = UnderlyingDataType.Boolean });

            return dataTypes;
        }

        private void SetUpComplexType(int portalId)
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_Complex;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(GetComplexContentType(contentTypeId, portalId));
            mockContentTypeController.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                        .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var mockDataTypeManager = new Mock<IDataTypeManager>();
            mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());
            DataTypeManager.SetTestableInstance(mockDataTypeManager.Object);
        }

        private void SetUpListContentType(int portalId)
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_ListContent;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(GetListContentType(contentTypeId, portalId));
            mockContentTypeController.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                        .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var mockDataTypeManager = new Mock<IDataTypeManager>();
            mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());
            DataTypeManager.SetTestableInstance(mockDataTypeManager.Object);
        }

        private void SetUpListDataType(int portalId)
        {
            var contentTypeId = CONTENTTYPE_ListData;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(GetListDataType(contentTypeId, portalId));
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var mockDataTypeManager = new Mock<IDataTypeManager>();
            mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());
            DataTypeManager.SetTestableInstance(mockDataTypeManager.Object);
        }

        private void SetUpSimpleType(int portalId)
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_Simple;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(GetSimpleContentType(contentTypeId, portalId));
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            var mockDataTypeManager = new Mock<IDataTypeManager>();
            mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());
            DataTypeManager.SetTestableInstance(mockDataTypeManager.Object);
        }
    }
}
