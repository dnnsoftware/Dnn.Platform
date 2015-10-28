// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentViewer.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Tests.Utilities;
using Moq;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class DynamicContentViewerBusinessControllerTests
    {
        #region members
        //Mocks
        private Mock<IDynamicContentSearchManager> _mockDynamicContentSearchManager;
        private Mock<IDynamicContentViewerManager> _mockDynamicContentViewerManager;
        private Mock<IContentController> _mockContentController;

        private Mock<IFieldDefinitionManager> _mockFieldDefinitionManager;
        private Mock<IDynamicContentTypeManager> _mockDynamicContentTypeManager;
        
        //Testing
        private BusinessController _businessController;
        #endregion

        [SetUp]
        public void Setup()
        {
            _mockDynamicContentViewerManager = new Mock<IDynamicContentViewerManager>();
            DynamicContentViewerManager.SetTestableInstance(_mockDynamicContentViewerManager.Object);

            _mockDynamicContentSearchManager = new Mock<IDynamicContentSearchManager>();
            DynamicContentSearchManager.SetTestableInstance(_mockDynamicContentSearchManager.Object);

            _mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(_mockContentController.Object);

            _mockFieldDefinitionManager = new Mock<IFieldDefinitionManager>();
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionManager.Object);

            _mockDynamicContentTypeManager = new Mock<IDynamicContentTypeManager>();
            DynamicContentTypeManager.SetTestableInstance(_mockDynamicContentTypeManager.Object);

            _businessController = new BusinessController();

        }

        [TearDown]
        public void TearDown()
        {
            DynamicContentTypeManager.ClearInstance();
            DynamicContentItemManager.ClearInstance();
            ContentController.ClearInstance();
        }

        [Test]
        public void GetModifiedSearchDocuments_ShouldReturnOneSearchDocument_WhenModuleHasContentItemAndItHasBeenModified()
        {
            //Arrange
            var fieldContents = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>("FieldName1", 1),
                new KeyValuePair<string, object>("FieldName2", true),
                new KeyValuePair<string, object>("FieldName3", "Some text")
            };

            var dynamicContentItem = GetSimpleDynamicContentItem(fieldContents);

            _mockDynamicContentViewerManager.Setup(vm => vm.GetContentTypeId(It.IsAny<ModuleInfo>()))
                .Returns(Constants.CONTENTTYPE_ValidContentTypeId);
            _mockDynamicContentViewerManager.Setup(vm => vm.GetContentItem(It.IsAny<ModuleInfo>()))
                .Returns(dynamicContentItem);

            var contentItem = new ContentItem();
            SetPrivateProperty(contentItem, "LastModifiedOnDate", DateTime.UtcNow.AddHours(-1));
            
            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(contentItem);

            _mockDynamicContentSearchManager.Setup(
                sm => sm.GetSearchDocument(It.IsAny<ModuleInfo>(), dynamicContentItem)).Returns(new SearchDocument());

            var moduleInfo = new ModuleInfo()
            {
                PortalID = Constants.PORTAL_ValidPortalId,
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _businessController.GetModifiedSearchDocuments(moduleInfo, DateTime.UtcNow.AddDays(-1));

            //Assert
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetModifiedSearchDocuments_ShouldNotReturnAnySearchDocument_WhenContentItemHasNotBeenModified()
        {
            //Arrange
            var fieldContents = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>("FieldName1", 1),
                new KeyValuePair<string, object>("FieldName2", true),
                new KeyValuePair<string, object>("FieldName3", "Some text")
            };

            var dynamicContentItem = GetSimpleDynamicContentItem(fieldContents);

            _mockDynamicContentViewerManager.Setup(vm => vm.GetContentTypeId(It.IsAny<ModuleInfo>()))
                .Returns(Constants.CONTENTTYPE_ValidContentTypeId);
            _mockDynamicContentViewerManager.Setup(vm => vm.GetContentItem(It.IsAny<ModuleInfo>()))
                .Returns(dynamicContentItem);

            var contentItem = new ContentItem();
            
            //The LastModifiedDate will be lesser than Indexing date
            SetPrivateProperty(contentItem, "LastModifiedOnDate", DateTime.UtcNow.AddDays(-5));

            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(contentItem);

            _mockDynamicContentSearchManager.Setup(
                sm => sm.GetSearchDocument(It.IsAny<ModuleInfo>(), dynamicContentItem)).Returns(new SearchDocument());

            var moduleInfo = new ModuleInfo()
            {
                PortalID = Constants.PORTAL_ValidPortalId,
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _businessController.GetModifiedSearchDocuments(moduleInfo, DateTime.UtcNow.AddDays(-1));

            //Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetModifiedSearchDocuments_ShouldNotReturnAnySearchDocument_WhenModuleHasNotContentItem()
        {
            //Arrange
            _mockDynamicContentViewerManager.Setup(vm => vm.GetContentTypeId(It.IsAny<ModuleInfo>()))
                .Returns(Null.NullInteger);
            
            var moduleInfo = new ModuleInfo()
            {
                PortalID = Constants.PORTAL_ValidPortalId,
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _businessController.GetModifiedSearchDocuments(moduleInfo, DateTime.UtcNow.AddDays(-1));

            //Assert
            Assert.AreEqual(0, result.Count);
        }

        private void SetPrivateProperty<TInstance, TField>(TInstance instance, string fieldName, TField value)
        {
            Type type = typeof(TInstance);

            var propertyInfo = type.GetProperty(fieldName);
            //We access to the DeclaringType in order to ensure Get and Set
            var declaringPropertyInfo = propertyInfo.DeclaringType.GetProperty(fieldName);

            declaringPropertyInfo.SetValue(instance, value);
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
