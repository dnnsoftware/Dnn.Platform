// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Tests.Utilities;
using Moq;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class DynamicContentSearchManagerTests
    {
        //Mocks
        private MockRepository _mockRepository;
        private Mock<IContentController> _mockContentController;
        private Mock<IFieldDefinitionManager> _mockFieldDefinitionManager;
        private Mock<ISearchHelper> _mockSearchHelper;

        //Testing
        private DynamicContentSearchManager _searchManager;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Default);

            _mockSearchHelper = _mockRepository.Create<ISearchHelper>();
            _mockSearchHelper.Setup(s => s.GetSearchTypeByName("module")).Returns(new SearchType());
            SearchHelper.SetTestableInstance(_mockSearchHelper.Object);

            _mockContentController = _mockRepository.Create<IContentController>();
            ContentController.SetTestableInstance(_mockContentController.Object);

            _mockFieldDefinitionManager = _mockRepository.Create<IFieldDefinitionManager>();
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionManager.Object);
            
            _searchManager = new DynamicContentSearchManager();
        }

        [TearDown]
        public void TearDown()
        {
            ContentController.ClearInstance();
            FieldDefinitionManager.ClearInstance();
            ModuleController.ClearInstance();
            SearchHelper.ClearInstance();
        }

        [Test]
        public void GetSearchDocument_ShouldGenerateValidBody_WhenContentItemIsSimple()
        {
            //Arrange
            var fieldContents = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("FieldName1", 1),
                new KeyValuePair<string, object>("FieldName2", true),
                new KeyValuePair<string, object>("FieldName3", "Some text")
            };

            var dynamicContentItem = GetSimpleDynamicContentItem(fieldContents);
            
            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(new ContentItem());

            var moduleInfo = new ModuleInfo
            {
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _searchManager.GetSearchDocument(moduleInfo, dynamicContentItem);

            //Assert
            Assert.AreEqual(String.Join(", ",1.ToString(),true.ToString(),"Some text"),result.Body);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetSearchDocument_ShouldGenerateValidKeywords_WhenContentItemIsSimple()
        {
            //Arrange
            var fieldContents = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("FieldName1", 1),
                new KeyValuePair<string, object>("FieldName2", true),
                new KeyValuePair<string, object>("FieldName3", "Some text")
            };

            var dynamicContentItem = GetSimpleDynamicContentItem(fieldContents);

            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(new ContentItem());

            var moduleInfo = new ModuleInfo
            {
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _searchManager.GetSearchDocument(moduleInfo, dynamicContentItem);

            //Assert
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("ContentType", Constants.CONTENTTYPE_ValidContentTypeId.ToString())));

            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("FieldName1", 1.ToString())));
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("FieldName2", true.ToString())));
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("FieldName3", "Some text")));
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetSearchDocument_ShouldGenerateValidBody_WhenContentItemIsComplex()
        {
            //Arrange
            var fieldContents = new List<KeyValuePair<string, object>>
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

            var dynamicContentItem = GetComplexDynamicContentItem(fieldContents, "ComplexFieldName1", subFieldContents);

            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(new ContentItem());

            var moduleInfo = new ModuleInfo
            {
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _searchManager.GetSearchDocument(moduleInfo, dynamicContentItem);

            //Assert
            Assert.AreEqual(String.Join(", ", 1.ToString(), true.ToString(), "Some text", 2.ToString(), false.ToString(), "Other text"), result.Body);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetSearchDocument_ShouldGenerateValidKeywords_WhenContentItemIsComplex()
        {
            //Arrange
            var fieldContents = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("FieldName1", 1),
                new KeyValuePair<string, object>("FieldName2", true),
                new KeyValuePair<string, object>("FieldName3", "Some text")
            };

            var subFieldContents = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("FieldName4", 2),
                new KeyValuePair<string, object>("FieldName5", false),
                new KeyValuePair<string, object>("FieldName6", "Other text")
            };

            var dynamicContentItem = GetComplexDynamicContentItem(fieldContents, "ComplexFieldName1", subFieldContents);

            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(new ContentItem());

            var moduleInfo = new ModuleInfo
            {
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _searchManager.GetSearchDocument(moduleInfo, dynamicContentItem);

            //Assert
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("ContentType", Constants.CONTENTTYPE_ValidContentTypeId.ToString())));

            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("FieldName1",1.ToString())));
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("FieldName2", true.ToString())));
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("FieldName3", "Some text")));

            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("ComplexFieldName1/FieldName4", 2.ToString())));
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("ComplexFieldName1/FieldName5", false.ToString())));
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("ComplexFieldName1/FieldName6", "Other text")));
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetSearchDocument_ShouldThrowException_WhenContentItemIsNull()
        {
            //Arrange
            var moduleInfo = new ModuleInfo
            {
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };
            
            //Act
            var testDelegation = new TestDelegate(() => _searchManager.GetSearchDocument(moduleInfo, null));
            
            //Assert
            Assert.Throws<ArgumentNullException>(testDelegation);
        }

        private DynamicContentItem GetComplexDynamicContentItem(List<KeyValuePair<string, object>> fieldContents, string complexFieldName,
            List<KeyValuePair<string, object>> secondFieldContents)
        {
            var secondaryContentItem = GetSimpleDynamicContentItem(secondFieldContents);
            var primaryContentItem = GetSimpleDynamicContentItem(fieldContents);
            primaryContentItem.Content.Fields.Add(complexFieldName, new DynamicContentField(new FieldDefinition()
                                                                                                {
                                                                                                    IsReferenceType = true,
                                                                                                    Name = complexFieldName
                                                                                                })
                                                                        {
                                                                            Value = secondaryContentItem.Content
                                                                        });

            return primaryContentItem;
        }


        private DynamicContentItem GetSimpleDynamicContentItem( List<KeyValuePair<string, object>> fieldContents)
        {
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            _mockFieldDefinitionManager.Setup(f => f.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition>().AsQueryable());

            var fieldNames = fieldContents.Select(f => f.Key).ToArray();
            var dynamicContentType = GetContentType(contentTypeId, portalId, fieldNames);
            
            var dynamicContent = new DynamicContentItem(portalId, dynamicContentType)
            {
                ContentItemId = Constants.CONTENT_ValidContentItemId
            };

            foreach (var fieldContent in fieldContents)
            {
                dynamicContent.Content.Fields[fieldContent.Key].Value = fieldContent.Value;
            }

            return dynamicContent;
        }

        private DynamicContentType GetContentType(int contentTypeId, int portalId, string[] fieldNames)
        {
            var contentType = new DynamicContentType { ContentTypeId = contentTypeId, PortalId = portalId };

            foreach (var fieldName in fieldNames)
            {
                contentType.FieldDefinitions.Add(new FieldDefinition { ContentTypeId = contentTypeId, Name = fieldName, PortalId = portalId });    
            }
            
            return contentType;
        }

    }
}
