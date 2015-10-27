// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Common.Utilities;
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

            var moduleInfo = new ModuleInfo()
            {
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _searchManager.GetSearchDocument(moduleInfo, dynamicContentItem);

            //Assert
            Assert.AreEqual(String.Join(", ",1.ToString(),true.ToString(),"Some text"),result.Body);
        }

        [Test]
        public void GetSearchDocument_ShouldGenerateValidKeywords_WhenContentItemIsSimple()
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

            var moduleInfo = new ModuleInfo()
            {
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _searchManager.GetSearchDocument(moduleInfo, dynamicContentItem);

            //Assert
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("FieldName1", 1.ToString())));
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("FieldName2", true.ToString())));
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("FieldName3", "Some text")));
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

            var dynamicContentItem = GetComplexDynamicContentItem(fieldContents, "ComplexFieldName1", subFieldContents);

            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(new ContentItem());

            var moduleInfo = new ModuleInfo()
            {
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _searchManager.GetSearchDocument(moduleInfo, dynamicContentItem);

            //Assert
            Assert.AreEqual(String.Join(", ", 1.ToString(), true.ToString(), "Some text", 2.ToString(), false.ToString(), "Other text"), result.Body);
        }

        [Test]
        public void GetSearchDocument_ShouldGenerateValidKeywords_WhenContentItemIsComplex()
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

            var dynamicContentItem = GetComplexDynamicContentItem(fieldContents, "ComplexFieldName1", subFieldContents);

            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(new ContentItem());

            var moduleInfo = new ModuleInfo()
            {
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _searchManager.GetSearchDocument(moduleInfo, dynamicContentItem);

            //Assert
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("FieldName1",1.ToString())));
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("FieldName2", true.ToString())));
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("FieldName3", "Some text")));

            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("ComplexFieldName1/FieldName4", 2.ToString())));
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("ComplexFieldName1/FieldName5", false.ToString())));
            Assert.IsTrue(result.Keywords.Contains(new KeyValuePair<string, string>("ComplexFieldName1/FieldName6", "Other text")));
        }

        [Test]
        public void GetSearchDocument_ShouldThrowException_WhenContentItemIsNull()
        {
            //Arrange
            var moduleInfo = new ModuleInfo()
            {
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            Action delegateFunction = () =>
            {
                _searchManager.GetSearchDocument(moduleInfo, null);
            };

            //Act
            var testDelegation = new TestDelegate(delegateFunction);
            

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
                                                                                                    IsReferenceType = true
                                                                                                })
                                                                        {
                                                                            Value = secondaryContentItem.Content
                                                                        });

            return primaryContentItem;
        }


        private DynamicContentItem GetSimpleDynamicContentItem( List<KeyValuePair<string, object>> fieldContents)
        {
            const int moduleId = Constants.MODULE_ValidId;
            const int tabId = Constants.TAB_ValidId;
            const int portalId = Constants.PORTAL_ValidPortalId;
            const int contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

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
