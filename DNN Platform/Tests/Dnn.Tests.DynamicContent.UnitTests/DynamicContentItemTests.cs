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
                                            new JProperty("FieldName1", 1),                                                
                                            new JProperty("FieldName2", true),                                                
                                            new JProperty("FieldName3", "abc")                                                    
                                        );

        private static readonly JObject ChildContentTypeJson = new JObject(
                                            new JProperty("FieldName1", 1),
                                            new JProperty("FieldName2", true),
                                            new JProperty("FieldName3", "abc")
                                        );

        private static readonly JObject ComplexContentTypeJson = new JObject(                                    
                                            new JProperty("FieldName1", 1),
                                            new JProperty("FieldName2", true),
                                            new JProperty("FieldName3", "abc"),
                                            new JProperty("FieldName4", ChildContentTypeJson)
                                        );

        private static readonly JObject ListContentTypeJson = new JObject(
                                            new JProperty("FieldName1", 1),
                                            new JProperty("FieldName2", true),
                                            new JProperty("FieldName3", "abc"),
                                            new JProperty("FieldName4", new JArray(
                                                ChildContentTypeJson,
                                                ChildContentTypeJson,
                                                ChildContentTypeJson)
                                            )
                                        );

        private static readonly JObject ListDataTypeJson = new JObject(
                                            new JProperty("FieldName1", 1),
                                            new JProperty("FieldName2", true),
                                            new JProperty("FieldName3", "abc"),
                                            new JProperty("FieldName4", new JArray(2,3,4))
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
            Assert.Throws<ArgumentOutOfRangeException>(() => new DynamicContentItem(-1, new DynamicContentType(Constants.PORTAL_ValidPortalId)));
        }

        [Test]
        public void Constructor_Sets_PortalId()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;

            //Act
            var dynamicContent = new DynamicContentItem(portalId, new DynamicContentType(Constants.PORTAL_ValidPortalId));

            //Assert
            Assert.AreEqual(portalId, dynamicContent.PortalId);
        }

        [Test]
        public void Constructor_Sets_Default_Properties()
        {
            //Arrange

            //Act
            var content = new DynamicContentItem(Constants.PORTAL_ValidPortalId, new DynamicContentType(Constants.PORTAL_ValidPortalId));

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
            mockFieldDefinitionController.VerifyAll();
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
            mockFieldDefinitionController.VerifyAll();
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
            mockFieldDefinitionController.VerifyAll();
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
            mockFieldDefinitionController.VerifyAll();
        }

        [Test]
        public void Constructor_Overload_Throws_On_Negative_PortalId()
        {
            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new DynamicContentItem(-1, new DynamicContentType()));
        }


        [Test]
        public void FromJson_Throws_If_No_Content()
        {
            //Arrange
            var contentTypeId = 234;
            var testJson = "";

            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId, new DynamicContentType(Constants.PORTAL_ValidPortalId));

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(new DynamicContentType() {ContentTypeId = contentTypeId});
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            //Act, Assert
            Assert.Throws<ArgumentException>(() => dynamicContent.FromJson(testJson));
        }

        [Test]
        public void FromJson_Creates_Empty_Fields_Property_If_No_Fields()
        {
            //Arrange
            var contentTypeId = 234;
            var portalId = Constants.PORTAL_ValidPortalId;
            var testJson = new JObject();

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockDataTypeManager = new Mock<IDataTypeManager>();
            mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());
            DataTypeManager.SetTestableInstance(mockDataTypeManager.Object);

            var contentType = GetSimpleContentType(contentTypeId, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(testJson.ToString());

            //Assert
            Assert.AreEqual(3, dynamicContent.Content.Fields.Count);
            Assert.AreEqual(0, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(false, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual("", dynamicContent.Content.Fields["FieldName3"].Value);

            mockFieldDefinitionController.VerifyAll();
            mockDataTypeManager.VerifyAll();
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields_For_SimpleType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            
            SetUpSimpleType(portalId);

            var contentType = GetSimpleContentType(CONTENTTYPE_Simple, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(SimpleContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(3, dynamicContent.Content.Fields.Count);
            VerifyAllSimpleType();
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields_For_ComplexType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;

            SetUpComplexType(portalId);

            var contentType = GetComplexContentType(CONTENTTYPE_Complex, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ComplexContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(4, dynamicContent.Content.Fields.Count);
            VerifyAllComplexType();
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields_For_ListContentType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;

            SetUpListContentType(portalId);

            var contentType = GetListContentType(CONTENTTYPE_ListContent, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ListContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(4, dynamicContent.Content.Fields.Count);
            VerifyAllListContentType();
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields_For_ListDataType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            
            SetUpListDataType(portalId);

            var contentType = GetListDataType(CONTENTTYPE_ListData, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ListDataTypeJson.ToString());

            //Assert
            Assert.AreEqual(4, dynamicContent.Content.Fields.Count);
            VerifyAllListDataType();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions_For_SimpleType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            
            SetUpSimpleType(portalId);

            var contentType = GetSimpleContentType(CONTENTTYPE_Simple, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(SimpleContentTypeJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName3"));
            VerifyAllSimpleType();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions_For_ComplexType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;

            SetUpComplexType(portalId);

            var contentTypeId = CONTENTTYPE_Complex;
            var contentType = GetComplexContentType(contentTypeId, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ComplexContentTypeJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName3"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName4"));
            VerifyAllComplexType();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions_For_ListContentType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;

            SetUpListContentType(portalId);

            var contentType = GetListContentType(CONTENTTYPE_ListContent, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ListContentTypeJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName3"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName4"));
            VerifyAllListContentType();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions_For_ListDataType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            
            SetUpListDataType(portalId);

            var contentType = GetListDataType(CONTENTTYPE_ListData, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ListDataTypeJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName3"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName4"));

            VerifyAllListDataType();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_SimpleType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            
            SetUpSimpleType(portalId);
            
            var contentType = GetSimpleContentType(CONTENTTYPE_Simple, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(SimpleContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(1, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(true, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual("abc", dynamicContent.Content.Fields["FieldName3"].Value);
            VerifyAllSimpleType();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ComplexType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var contentTypeId = CONTENTTYPE_Complex;

            SetUpComplexType(portalId);

            var contentType = GetComplexContentType(contentTypeId, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ComplexContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(1, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(true, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual("abc", dynamicContent.Content.Fields["FieldName3"].Value);
            Assert.IsInstanceOf<DynamicContentPart>(dynamicContent.Content.Fields["FieldName4"].Value);            
            VerifyAllComplexType();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ListContentType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;

            SetUpListContentType(portalId);

            var contentType = GetListContentType(CONTENTTYPE_ListContent, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

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

            VerifyAllListContentType();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ListDataType()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            
            SetUpListDataType(portalId);

            var contentType = GetListDataType(CONTENTTYPE_ListData, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ListDataTypeJson.ToString());

            //Assert
            Assert.AreEqual(1, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(true, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual("abc", dynamicContent.Content.Fields["FieldName3"].Value);
            var list = dynamicContent.Content.Fields["FieldName4"].Value as List<DynamicContentField>;
            Assert.IsNotNull(list);
            VerifyAllListDataType();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ComplexType_SubFields()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;

            SetUpComplexType(portalId);

            var contentType = GetComplexContentType(CONTENTTYPE_Complex, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ComplexContentTypeJson.ToString());

            //Assert
            var part = dynamicContent.Content.Fields["FieldName4"].Value as DynamicContentPart;
            Assert.IsNotNull(part);
            Assert.AreEqual(1, part.Fields["FieldName1"].Value);
            Assert.AreEqual(true, part.Fields["FieldName2"].Value);
            Assert.AreEqual("abc", part.Fields["FieldName3"].Value);
            VerifyAllComplexType();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ListContentType_SubFields()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;

            SetUpListContentType(portalId);

            var contentType = GetListContentType(CONTENTTYPE_ListContent, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

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

            VerifyAllListContentType();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ListDataType_SubFields()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;

            SetUpListDataType(portalId);

            var contentType = GetListDataType(CONTENTTYPE_ListData, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ListDataTypeJson.ToString());

            //Assert
            var list = dynamicContent.Content.Fields["FieldName4"].Value as List<DynamicContentField>;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list[0].Value);
            Assert.AreEqual(3, list[1].Value);
            Assert.AreEqual(4, list[2].Value);
            VerifyAllListDataType();
        }

        [Test]
        public void FromJson_DoesNotThrow_If_Cant_Match_Field_From_Json_With_FieldDefinitions()
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_Simple;

            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId, new DynamicContentType(Constants.PORTAL_ValidPortalId));

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = GetSimpleContentType(contentTypeId, portalId);
            contentType.FieldDefinitions.RemoveAt(2);

            //Act
            var act = new TestDelegate(() => dynamicContent.FromJson(SimpleContentTypeJson.ToString()));
            
            // Assert
            Assert.DoesNotThrow(act);
            mockFieldDefinitionController.VerifyAll();
        }

        [Test]
        public void FromJson_Set_DefaultValues_If_Cant_Match_Field_From_Json_With_FieldDefinitions()
        {
            //Arrange
            const int contentTypeId = CONTENTTYPE_Simple;
            const int portalId = Constants.PORTAL_ValidPortalId;

            SetUpSimpleType(portalId);

            var contentType = GetSimpleContentType(CONTENTTYPE_Simple, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);
            
            //Act
            dynamicContent.FromJson((new JObject(new JProperty("NotDefinedFieldName", 1))).ToString());

            // Assert
            Assert.AreEqual(0, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(false, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual(string.Empty, dynamicContent.Content.Fields["FieldName3"].Value);
            Assert.IsFalse(dynamicContent.Content.Fields.ContainsKey("NotDefinedFieldName"));
            VerifyAllSimpleType();
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

            var list = new List<DynamicContentPart> { childPart, childPart, childPart };
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
            dynamicContent.Content.Fields["FieldName4"].Value = new [] { 2, 3, 4 };
            
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

        private Mock<IFieldDefinitionManager> _mockFieldDefinitionController;
        private Mock<IDynamicContentTypeManager> _mockContentTypeController;
        private Mock<IDataTypeManager> _mockDataTypeManager;

        private void SetUpComplexType(int portalId)
        {
            var contentTypeId = CONTENTTYPE_Complex;

            _mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            _mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionController.Object);

            _mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            _mockContentTypeController.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                        .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));
            DynamicContentTypeManager.SetTestableInstance(_mockContentTypeController.Object);

            _mockDataTypeManager = new Mock<IDataTypeManager>();
            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());
            DataTypeManager.SetTestableInstance(_mockDataTypeManager.Object);
        }

        private void VerifyAllComplexType()
        {
            _mockFieldDefinitionController.VerifyAll();
            _mockContentTypeController.VerifyAll();
            _mockDataTypeManager.VerifyAll();
        }

        private void SetUpListContentType(int portalId)
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_ListContent;

            _mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            _mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionController.Object);

            _mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            //_mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
            //            .Returns(GetListContentType(contentTypeId, portalId));
            _mockContentTypeController.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                        .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));
            DynamicContentTypeManager.SetTestableInstance(_mockContentTypeController.Object);

            _mockDataTypeManager = new Mock<IDataTypeManager>();
            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());
            DataTypeManager.SetTestableInstance(_mockDataTypeManager.Object);
        }

        private void VerifyAllListContentType()
        {
            _mockFieldDefinitionController.VerifyAll();
            _mockContentTypeController.VerifyAll();
            _mockDataTypeManager.VerifyAll();
        }

        private void SetUpListDataType(int portalId)
        {
            var contentTypeId = CONTENTTYPE_ListData;

            _mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            _mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionController.Object);

            _mockDataTypeManager = new Mock<IDataTypeManager>();
            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());
            DataTypeManager.SetTestableInstance(_mockDataTypeManager.Object);
        }

        private void VerifyAllListDataType()
        {
            _mockFieldDefinitionController.VerifyAll();
            _mockDataTypeManager.VerifyAll();
        }

        private void SetUpSimpleType(int portalId)
        {
            //Arrange
            var contentTypeId = CONTENTTYPE_Simple;

            _mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            _mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionController.Object);

            //_mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            //_mockContentTypeController.Setup(c => c.GetContentType(contentTypeId, portalId, true))
            //            .Returns(GetSimpleContentType(contentTypeId, portalId));
            //DynamicContentTypeManager.SetTestableInstance(_mockContentTypeController.Object);

            _mockDataTypeManager = new Mock<IDataTypeManager>();
            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());
            DataTypeManager.SetTestableInstance(_mockDataTypeManager.Object);
        }

        private void VerifyAllSimpleType()
        {
            _mockFieldDefinitionController.VerifyAll();
            //_mockContentTypeController.VerifyAll();
            _mockDataTypeManager.VerifyAll();
        }
    }
}
