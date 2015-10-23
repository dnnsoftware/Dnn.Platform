// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Dnn.DynamicContent;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class DynamicContentSearchManagerTests
    {
        private readonly JObject _testSimpleContentItem = new JObject(
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

        #region members
        //Mocks
        private Mock<IContentController> _mockContentController;
        private Mock<IFieldDefinitionManager> _mockFieldDefinitionManager;
        private Mock<IDynamicContentTypeManager> _mockDynamicContentTypeManager;
        private Mock<IModuleController> _mockModuleController;
        private Mock<ISearchHelper> _mockSearchHelper;

        //Testing
        private IDynamicContentSearchManager _searchManager;
        #endregion

        [SetUp]
        public void Setup()
        {
            _mockModuleController = new Mock<IModuleController>();
            _mockModuleController.Setup(m => m.GetModule(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new ModuleInfo());
            ModuleController.SetTestableInstance(_mockModuleController.Object);

            _mockSearchHelper = new Mock<ISearchHelper>();
            _mockSearchHelper.Setup(s => s.GetSearchTypeByName(It.IsAny<string>())).Returns(new SearchType());
            SearchHelper.SetTestableInstance(_mockSearchHelper.Object);

            _mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(_mockContentController.Object);

            _mockFieldDefinitionManager = new Mock<IFieldDefinitionManager>();
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionManager.Object);

            _mockDynamicContentTypeManager = new Mock<IDynamicContentTypeManager>();
            DynamicContentTypeManager.SetTestableInstance(_mockDynamicContentTypeManager.Object);

            _searchManager = DynamicContentSearchManager.Instance;

        }

        [TearDown]
        public void TearDown()
        {
            ContentController.ClearInstance();
            FieldDefinitionManager.ClearInstance();
            DynamicContentTypeManager.ClearInstance();
        }

        [Test]
        public void GetSearchDocument_ShouldGenerateValidBody_WhenContentItemIsSimple()
        {
            //Arrange
            var fieldContents = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>("FieldName1", 1),
                new KeyValuePair<string, object>("FieldName2", true),
                new KeyValuePair<string, object>("FieldName3", "Some text")
            };

            var dynamicContentItem = GetSimpleDynamicContentItem(fieldContents);
            
            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(new ContentItem());

            //Act
            var result = _searchManager.GetSearchDocument(dynamicContentItem);

            //Assert
            Assert.AreEqual(String.Join(",",1.ToString(),true.ToString(),"Some text"),result.Body);
        }

        [Test]
        public void GetSearchDocument_ShouldGenerateValidBody_WhenContentItemIsComplex()
        {
            //Arrange
            var fieldContents = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>("FieldName1", 1),
                new KeyValuePair<string, object>("FieldName2", true),
                new KeyValuePair<string, object>("FieldName3", "Some text")
            };

            var subFieldContents = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>("FieldName4", 2),
                new KeyValuePair<string, object>("FieldName5", false),
                new KeyValuePair<string, object>("FieldName6", "Other text")
            };

            var dynamicContentItem = GetComplexDynamicContentItem(fieldContents, subFieldContents);

            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(new ContentItem());

            //Act
            var result = _searchManager.GetSearchDocument(dynamicContentItem);

            //Assert
            Assert.AreEqual(String.Join(",", 1.ToString(), true.ToString(), "Some text", 2.ToString(), false.ToString(), "Other text"), result.Body);
        }

        [Test]
        public void GetSearchDocument_ShouldThrowException_WhenContentItemIsNull()
        {
            //Arrange
            Action delegateFunction = () =>
            {
                _searchManager.GetSearchDocument(null);
            };

            //Act
            var testDelegation = new TestDelegate(delegateFunction);
            

            //Assert
            Assert.Throws<ArgumentNullException>(testDelegation);
        }

        [Test]
        public void GetSearchDocument_ShouldThrowException_WhenContentItemIdIsNegative()
        {
            //Arrange

            var fieldContents = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>("FieldName1", 1),
                new KeyValuePair<string, object>("FieldName2", true),
                new KeyValuePair<string, object>("FieldName3", "Some text")
            };

            var dynamicContentItem = GetSimpleDynamicContentItem(fieldContents);
            dynamicContentItem.ContentItemId = Null.NullInteger;

            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(new ContentItem());


            Action delegateFunction = () =>
            {
                _searchManager.GetSearchDocument(dynamicContentItem);
            };

            //Act
            var testDelegation = new TestDelegate(delegateFunction);


            //Assert
            Assert.Throws<ArgumentOutOfRangeException>(testDelegation);
        }

        private DynamicContentItem GetComplexDynamicContentItem(List<KeyValuePair<string, object>> fieldContents,
            List<KeyValuePair<string, object>> secondFieldContents)
        {
            var secondaryContentItem = GetSimpleDynamicContentItem(secondFieldContents);
            var primaryContentItem = GetSimpleDynamicContentItem(fieldContents);
            primaryContentItem.Content.Fields.Add("ComplexFieldName", new DynamicContentField(new FieldDefinition()
                                                                                                {
                                                                                                    IsReferenceType = true
                                                                                                })
                                                                        {
                                                                            Value = secondaryContentItem.Content
                                                                        });

            return primaryContentItem;
        }


        private DynamicContentItem GetSimpleDynamicContentItem( List<KeyValuePair<string, object>> fieldContents)
        {
            var moduleId = Constants.MODULE_ValidId;
            var tabId = Constants.TAB_ValidId;
            var portalId = Constants.PORTAL_ValidPortalId;
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var fieldNames = fieldContents.Select(f => f.Key).ToArray();
            var dynamicContentType = GetContentType(contentTypeId, portalId, fieldNames);
            _mockDynamicContentTypeManager.Setup(c => c.GetContentTypes(portalId, false))
                .Returns(new List<DynamicContentType>() { dynamicContentType }.AsQueryable());
            
            var dynamicContent = new DynamicContentItem(portalId, dynamicContentType)
            {
                ContentItemId = Constants.CONTENT_ValidContentItemId,
                ModuleId = moduleId,
                TabId = tabId
            };

            foreach (var fieldContent in fieldContents)
            {
                dynamicContent.Content.Fields[fieldContent.Key].Value = fieldContent.Value;
            }

            return dynamicContent;
        }

        private DynamicContentType GetContentType(int contentTypeId, int portalId, string[] fieldNames)
        {
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId, PortalId = portalId };

            foreach (var fieldName in fieldNames)
            {
                contentType.FieldDefinitions.Add(new FieldDefinition() { ContentTypeId = contentTypeId, Name = fieldName, PortalId = portalId });    
            }
            
            return contentType;
        }

    }
}
