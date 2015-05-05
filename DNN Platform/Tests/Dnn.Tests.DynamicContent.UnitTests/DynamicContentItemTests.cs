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
            DynamicContentTypeManager.ClearInstance();
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
            Assert.Throws<ArgumentNullException>(() => new DynamicContentItem(null));
        }

        [Test]
        public void Constructor_Overload_Sets_Default_Properties()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId, PortalId = portalId };

            //Act
            var content = new DynamicContentItem(contentType);

            //Assert
            Assert.AreEqual(-1, content.ModuleId);
            Assert.AreEqual(-1, content.ContentItemId);
        }

        [Test]
        public void Constructor_Overload_Sets_ContentType()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = new DynamicContentType() {ContentTypeId = contentTypeId, PortalId = portalId};

            //Act
            var dynamicContent = new DynamicContentItem(contentType);

            //Assert
            Assert.AreSame(contentType, dynamicContent.ContentType);
        }

        [Test]
        public void Constructor_Overload_Sets_Fields()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = GetContentType(contentTypeId, portalId);

            //Act
            var dynamicContent = new DynamicContentItem(contentType);

            //Assert
            Assert.AreEqual(contentType.FieldDefinitions.Count, dynamicContent.Fields.Count);
        }

        [Test]
        public void Constructor_Overload_Sets_FieldDefinition_ProeprtyOf_Fields()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = GetContentType(contentTypeId, portalId);

            //Act
            var dynamicContent = new DynamicContentItem(contentType);

            //Assert
            foreach (var fieldDefinition in contentType.FieldDefinitions)
            {
                Assert.AreSame(fieldDefinition, dynamicContent.Fields[fieldDefinition.Name].Definition);
            }
        }

        [Test]
        public void Constructor_Overload_Throws_On_Negative_PortalId_Property_Of_ContentType()
        {
            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new DynamicContentItem(new DynamicContentType()));
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
            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId))
                .Returns(new List<DynamicContentType>() {new DynamicContentType() {ContentTypeId = contentTypeId}}.AsQueryable());
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
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId))
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
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId))
                .Returns(new List<DynamicContentType>() { new DynamicContentType() { ContentTypeId = contentTypeId } }.AsQueryable());
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

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId))
                .Returns(new List<DynamicContentType>() { new DynamicContentType() { ContentTypeId = contentTypeId } }.AsQueryable());
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            //Act
            dynamicContent.FromJson(testJson.ToString());

            //Assert
            Assert.AreEqual(0, dynamicContent.Fields.Count);
        }

        [Test]
        public void FromJson_Reads_Fields_From_Json_And_Sets_Fields()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId))
                .Returns(new List<DynamicContentType>() { GetContentType(contentTypeId, portalId) }.AsQueryable());
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            //Act
            dynamicContent.FromJson(_testJson.ToString());

            //Assert
            Assert.AreEqual(3, dynamicContent.Fields.Count);
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_With_FieldDefinitions()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId))
                .Returns(new List<DynamicContentType>() { GetContentType(contentTypeId, portalId) }.AsQueryable());
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            //Act
            dynamicContent.FromJson(_testJson.ToString());

            //Assert
            Assert.IsTrue(dynamicContent.Fields.ContainsKey("FieldName1"));
            Assert.IsTrue(dynamicContent.Fields.ContainsKey("FieldName2"));
            Assert.IsTrue(dynamicContent.Fields.ContainsKey("FieldName3"));
        }

        [Test]
        public void FromJson_Matches_Fields_From_Json_And_Sets_Value()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId))
                .Returns(new List<DynamicContentType>() { GetContentType(contentTypeId, portalId) }.AsQueryable());
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            //Act
            dynamicContent.FromJson(_testJson.ToString());

            //Assert
            Assert.AreEqual(1, dynamicContent.Fields["FieldName1"].Value);
            Assert.AreEqual(true, dynamicContent.Fields["FieldName2"].Value);
            Assert.AreEqual("abc", dynamicContent.Fields["FieldName3"].Value);
        }

        [Test]
        public void FromJson_Throws_If_Cant_Match_Field_From_Json_With_FieldDefinitions()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var portalId = Constants.PORTAL_ValidPortalId;
            var dynamicContent = new DynamicContentItem(portalId);

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = GetContentType(contentTypeId, portalId);
            contentType.FieldDefinitions.RemoveAt(2);

            var mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            mockContentTypeController.Setup(c => c.GetContentTypes(portalId))
                .Returns(new List<DynamicContentType>() { contentType }.AsQueryable());
            DynamicContentTypeManager.SetTestableInstance(mockContentTypeController.Object);

            //Act, Assert
            Assert.Throws<JsonInvalidFieldException>(() => dynamicContent.FromJson(_testJson.ToString()));
        }

        [Test]
        public void ToJson_Generates_Json_String()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;

            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            var contentType = GetContentType(contentTypeId, portalId);

            var dynamicContent = new DynamicContentItem(contentType);
            dynamicContent.Fields["FieldName1"].Value = 1;
            dynamicContent.Fields["FieldName2"].Value = true;
            dynamicContent.Fields["FieldName3"].Value = "abc";

            //Act
            var json = dynamicContent.ToJson();

            //Asert
            Assert.AreEqual(_testJson.ToString(), json);
        }

        private DynamicContentType GetContentType(int contentTypeId, int portalId)
        {
            var contentType = new DynamicContentType() {ContentTypeId = contentTypeId, PortalId = portalId};

            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName1"});
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName2" });
            contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = "FieldName3" });

            return contentType;
        }

    }
}
