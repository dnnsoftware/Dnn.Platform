// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

        private MockRepository _mockRepository;
        private Mock<IFieldDefinitionManager> _mockFieldDefinitionManager;
        private Mock<IDynamicContentTypeManager> _mockContentTypeManager;
        private Mock<IDataTypeManager> _mockDataTypeManager;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Default);

            _mockFieldDefinitionManager = _mockRepository.Create<IFieldDefinitionManager>();
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionManager.Object);

            _mockContentTypeManager = _mockRepository.Create<IDynamicContentTypeManager>();
            DynamicContentTypeManager.SetTestableInstance(_mockContentTypeManager.Object);

            _mockDataTypeManager = _mockRepository.Create<IDataTypeManager>();
            DataTypeManager.SetTestableInstance(_mockDataTypeManager.Object);
        }

        [TearDown]
        public void TearDown()
        {
            FieldDefinitionManager.ClearInstance();
            DynamicContentTypeManager.ClearInstance();
            DataTypeManager.ClearInstance();
        }

        #region Constructor Tests
        [Test]
        public void Constructor_Throws_On_Negative_PortalId()
        {
            // Act
            var act =
                new TestDelegate(
                    () => new DynamicContentItem(-1, new DynamicContentType(Constants.PORTAL_ValidPortalId)));

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }

        [Test]
        public void Constructor_Sets_PortalId()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;

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
            Assert.AreEqual(-1, content.ContentItemId);
        }

        [Test]
        public void Constructor_Overload_Throws_On_Null_ContentType()
        {
            // Act
            var act = new TestDelegate(() => new DynamicContentItem(Constants.PORTAL_ValidPortalId, null));

            // Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        [Test]
        public void Constructor_Overload_Sets_Default_Properties()
        {
            //Arrange
            const int contentTypeId = CONTENTTYPE_Simple;
            const int portalId = Constants.PORTAL_ValidPortalId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var contentType = new DynamicContentType { ContentTypeId = contentTypeId, PortalId = portalId };

            //Act
            var content = new DynamicContentItem(portalId, contentType);

            //Assert
            Assert.AreEqual(-1, content.ContentItemId);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Constructor_Overload_Sets_ContentType()
        {
            //Arrange
            const int contentTypeId = CONTENTTYPE_Simple;
            const int portalId = Constants.PORTAL_ValidPortalId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var contentType = new DynamicContentType() {ContentTypeId = contentTypeId, PortalId = portalId};

            //Act
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Assert
            Assert.AreSame(contentType, dynamicContent.ContentType);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Constructor_Overload_Sets_Fields()
        {
            //Arrange
            const int contentTypeId = CONTENTTYPE_Simple;
            const int portalId = Constants.PORTAL_ValidPortalId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var contentType = GetSimpleContentType(contentTypeId, portalId);

            //Act
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Assert
            Assert.AreEqual(contentType.FieldDefinitions.Count, dynamicContent.Content.Fields.Count);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Constructor_Overload_Sets_FieldDefinition_PropertyOf_Fields()
        {
            //Arrange
            const int contentTypeId = CONTENTTYPE_Simple;
            const int portalId = Constants.PORTAL_ValidPortalId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var contentType = GetSimpleContentType(contentTypeId, portalId);

            //Act
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Assert
            foreach (var fieldDefinition in contentType.FieldDefinitions)
            {
                Assert.AreSame(fieldDefinition, dynamicContent.Content.Fields[fieldDefinition.Name].Definition);
            }
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Constructor_Overload_Throws_On_Negative_PortalId()
        {
            //Act
            var act = new TestDelegate(() => new DynamicContentItem(-1, new DynamicContentType()));

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }
        #endregion

        #region FromJson Tests
        [Test]
        public void FromJson_Throws_If_No_Content()
        {
            //Arrange
            const string testJson = "";

            const int portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId, new DynamicContentType(Constants.PORTAL_ValidPortalId));
            
            //Act
            var act = new TestDelegate(() => dynamicContent.FromJson(testJson));

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void FromJson_Creates_Empty_Fields_Property_If_No_Fields()
        {
            //Arrange
            const int contentTypeId = 234;
            const int portalId = Constants.PORTAL_ValidPortalId;
            var testJson = new JObject();

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

            var contentType = GetSimpleContentType(contentTypeId, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(testJson.ToString());

            //Assert
            Assert.AreEqual(3, dynamicContent.Content.Fields.Count);
            Assert.AreEqual(0, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(false, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual("", dynamicContent.Content.Fields["FieldName3"].Value);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields_For_SimpleType()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_Simple;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

            var contentType = GetSimpleContentType(CONTENTTYPE_Simple, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(SimpleContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(3, dynamicContent.Content.Fields.Count);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields_For_ComplexType()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_Complex;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockContentTypeManager.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                        .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

            var contentType = GetComplexContentType(CONTENTTYPE_Complex, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ComplexContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(4, dynamicContent.Content.Fields.Count);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields_For_ListContentType()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_ListContent;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockContentTypeManager.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                        .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

            var contentType = GetListContentType(CONTENTTYPE_ListContent, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ListContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(4, dynamicContent.Content.Fields.Count);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields_For_ListDataType()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_ListData;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

            var contentType = GetListDataType(CONTENTTYPE_ListData, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ListDataTypeJson.ToString());

            //Assert
            Assert.AreEqual(4, dynamicContent.Content.Fields.Count);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions_For_SimpleType()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_Simple;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

            var contentType = GetSimpleContentType(CONTENTTYPE_Simple, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(SimpleContentTypeJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName3"));
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions_For_ComplexType()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_Complex;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockContentTypeManager.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                        .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

            var contentType = GetComplexContentType(contentTypeId, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ComplexContentTypeJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName3"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName4"));
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions_For_ListContentType()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_ListContent;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockContentTypeManager.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                        .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

            var contentType = GetListContentType(CONTENTTYPE_ListContent, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ListContentTypeJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName3"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName4"));
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions_For_ListDataType()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_ListData;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

            var contentType = GetListDataType(CONTENTTYPE_ListData, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ListDataTypeJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName3"));
            Assert.IsTrue(dynamicContent.Content.Fields.ContainsKey("FieldName4"));
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_SimpleType()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_Simple;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());
            
            var contentType = GetSimpleContentType(CONTENTTYPE_Simple, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(SimpleContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(1, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(true, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual("abc", dynamicContent.Content.Fields["FieldName3"].Value);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ComplexType()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_Complex;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockContentTypeManager.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                        .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

            var contentType = GetComplexContentType(contentTypeId, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);

            //Act
            dynamicContent.FromJson(ComplexContentTypeJson.ToString());

            //Assert
            Assert.AreEqual(1, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(true, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual("abc", dynamicContent.Content.Fields["FieldName3"].Value);
            Assert.IsInstanceOf<DynamicContentPart>(dynamicContent.Content.Fields["FieldName4"].Value);            
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ListContentType()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_ListContent;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockContentTypeManager.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                        .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

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
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ListDataType()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_ListData;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

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
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ComplexType_SubFields()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_Complex;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockContentTypeManager.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                        .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

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
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ListContentType_SubFields()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_ListContent;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>()))
                .Returns(GetDataTypes().AsQueryable());

            _mockContentTypeManager.Setup(c => c.GetContentType(CONTENTTYPE_Child, portalId, true))
                .Returns(GetSimpleContentType(CONTENTTYPE_Child, portalId));

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
            _mockRepository.VerifyAll();
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value_For_ListDataType_SubFields()
        {
            //Arrange
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_ListData;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

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
            _mockRepository.VerifyAll();
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
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = CONTENTTYPE_Simple;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            _mockDataTypeManager.Setup(d => d.GetDataTypes(portalId, It.IsAny<bool>())).Returns(GetDataTypes().AsQueryable());

            var contentType = GetSimpleContentType(CONTENTTYPE_Simple, portalId);
            var dynamicContent = new DynamicContentItem(portalId, contentType);
            
            //Act
            dynamicContent.FromJson((new JObject(new JProperty("NotDefinedFieldName", 1))).ToString());

            // Assert
            Assert.AreEqual(0, dynamicContent.Content.Fields["FieldName1"].Value);
            Assert.AreEqual(false, dynamicContent.Content.Fields["FieldName2"].Value);
            Assert.AreEqual(string.Empty, dynamicContent.Content.Fields["FieldName3"].Value);
            Assert.IsFalse(dynamicContent.Content.Fields.ContainsKey("NotDefinedFieldName"));
            _mockRepository.VerifyAll();
        }
        #endregion

        #region ToJson Tests
        [Test]
        public void ToJson_Generates_Json_String_For_SimpleType()
        {
            //Arrange
            const int contentTypeId = CONTENTTYPE_Simple;
            const int portalId = Constants.PORTAL_ValidPortalId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var contentType = GetSimpleContentType(contentTypeId, portalId);

            var dynamicContent = new DynamicContentItem(portalId, contentType);
            dynamicContent.Content.Fields["FieldName1"].Value = 1;
            dynamicContent.Content.Fields["FieldName2"].Value = true;
            dynamicContent.Content.Fields["FieldName3"].Value = "abc";

            //Act
            var json = dynamicContent.ToJson();

            //Asert
            Assert.AreEqual(SimpleContentTypeJson.ToString(), json);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void ToJson_Generates_Json_String_For_ComplexType()
        {
            //Arrange
            const int contentTypeId = CONTENTTYPE_Complex;
            const int portalId = Constants.PORTAL_ValidPortalId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

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
            _mockRepository.VerifyAll();
        }

        [Test]
        public void ToJson_Generates_Json_String_For_ListContentType()
        {
            //Arrange
            const int contentTypeId = CONTENTTYPE_ListContent;
            const int portalId = Constants.PORTAL_ValidPortalId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

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
            _mockRepository.VerifyAll();
        }

        [Test]
        public void ToJson_Generates_Json_String_For_ListDataType()
        {
            //Arrange
            const int contentTypeId = CONTENTTYPE_ListData;
            const int portalId = Constants.PORTAL_ValidPortalId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var contentType = GetListDataType(contentTypeId, portalId);
            _mockContentTypeManager.Setup(c => c.GetContentType(contentTypeId, portalId, true))
                        .Returns(contentType);

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
        #endregion

        private static DynamicContentType GetComplexContentType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType { ContentTypeId = contentTypeId, PortalId = portalId };

            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName1", PortalId = portalId, FieldTypeId = DATATYPE_Integer, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName2", PortalId = portalId, FieldTypeId = DATATYPE_Boolean, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName3", PortalId = portalId, FieldTypeId = DATATYPE_String, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName4", PortalId = portalId, FieldTypeId = CONTENTTYPE_Child, IsReferenceType = true });

            return contentType;
        }

        private static DynamicContentType GetListContentType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType { ContentTypeId = contentTypeId, PortalId = portalId };

            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName1", PortalId = portalId, FieldTypeId = DATATYPE_Integer, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName2", PortalId = portalId, FieldTypeId = DATATYPE_Boolean, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName3", PortalId = portalId, FieldTypeId = DATATYPE_String, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName4", PortalId = portalId, FieldTypeId = CONTENTTYPE_Child, IsReferenceType = true, IsList = true });

            return contentType;
        }

        private static DynamicContentType GetListDataType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType { ContentTypeId = contentTypeId, PortalId = portalId };

            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName1", PortalId = portalId, FieldTypeId = DATATYPE_Integer, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName2", PortalId = portalId, FieldTypeId = DATATYPE_Boolean, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName3", PortalId = portalId, FieldTypeId = DATATYPE_String, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName4", PortalId = portalId, FieldTypeId = DATATYPE_Integer, IsReferenceType = false, IsList = true });

            return contentType;
        }

        private static DynamicContentType GetSimpleContentType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType {ContentTypeId = contentTypeId, PortalId = portalId};

            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName1", PortalId = portalId, FieldTypeId = DATATYPE_Integer, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName2", PortalId = portalId, FieldTypeId = DATATYPE_Boolean, IsReferenceType = false });
            contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = "FieldName3", PortalId = portalId, FieldTypeId = DATATYPE_String, IsReferenceType = false });

            return contentType;
        }

        private static IEnumerable<DataType> GetDataTypes()
        {
            return new List<DataType>
            {
                new DataType {DataTypeId = DATATYPE_Integer, UnderlyingDataType = UnderlyingDataType.Integer},
                new DataType {DataTypeId = DATATYPE_String, UnderlyingDataType = UnderlyingDataType.String},
                new DataType {DataTypeId = DATATYPE_Boolean, UnderlyingDataType = UnderlyingDataType.Boolean}
            };
        }
    }
}
