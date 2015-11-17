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

namespace Dnn.Tests.DynamicContentViewer.UnitTests
{
    [TestFixture]
    public class DynamicContentViewerBusinessControllerTests
    {
        //Mocks
        private MockRepository _mockRepository;
        private Mock<IDynamicContentSearchManager> _mockDynamicContentSearchManager;
        private Mock<IDynamicContentViewerManager> _mockDynamicContentViewerManager;
        private Mock<IContentController> _mockContentController;
        private Mock<IFieldDefinitionManager> _mockFieldDefinitionManager;
        
        //System Under Test
        private BusinessController _businessController;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Default);

            _mockDynamicContentViewerManager = _mockRepository.Create<IDynamicContentViewerManager>();
            DynamicContentViewerManager.SetTestableInstance(_mockDynamicContentViewerManager.Object);

            _mockDynamicContentSearchManager = _mockRepository.Create<IDynamicContentSearchManager>();
            DynamicContentSearchManager.SetTestableInstance(_mockDynamicContentSearchManager.Object);

            _mockContentController = _mockRepository.Create<IContentController>();
            ContentController.SetTestableInstance(_mockContentController.Object);

            _mockFieldDefinitionManager = _mockRepository.Create<IFieldDefinitionManager>();
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionManager.Object);

            _businessController = new BusinessController();

        }

        [TearDown]
        public void TearDown()
        {
            ContentController.ClearInstance();
            DynamicContentSearchManager.ClearInstance();
            FieldDefinitionManager.ClearInstance();
            DynamicContentViewerManager.ClearInstance();
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

            _mockDynamicContentViewerManager.Setup(vm => vm.GetContentItem(It.IsAny<ModuleInfo>()))
                .Returns(dynamicContentItem);

            var contentItem = new ContentItem();
            SetPrivateProperty(contentItem, "LastModifiedOnDate", DateTime.UtcNow.AddHours(-1));
            
            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(contentItem);

            _mockDynamicContentSearchManager.Setup(
                sm => sm.GetSearchDocument(It.IsAny<ModuleInfo>(), dynamicContentItem)).Returns(new SearchDocument());

            var moduleInfo = new ModuleInfo
            {
                PortalID = Constants.PORTAL_ValidPortalId,
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _businessController.GetModifiedSearchDocuments(moduleInfo, DateTime.UtcNow.AddDays(-1));

            //Assert
            Assert.AreEqual(1, result.Count);
            _mockRepository.VerifyAll();
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

            _mockDynamicContentViewerManager.Setup(vm => vm.GetContentItem(It.IsAny<ModuleInfo>()))
                .Returns(dynamicContentItem);

            var contentItem = new ContentItem();
            
            //The LastModifiedDate will be lesser than Indexing date
            SetPrivateProperty(contentItem, "LastModifiedOnDate", DateTime.UtcNow.AddDays(-5));

            _mockContentController.Setup(c => c.GetContentItem(It.IsAny<int>())).Returns(contentItem);
            
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
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetModifiedSearchDocuments_ShouldNotReturnAnySearchDocument_WhenModuleHasNotContentItem()
        {
            //Arrange
            var moduleInfo = new ModuleInfo
            {
                PortalID = Constants.PORTAL_ValidPortalId,
                ModuleID = Constants.MODULE_ValidId,
                TabID = Constants.TAB_ValidId
            };

            //Act
            var result = _businessController.GetModifiedSearchDocuments(moduleInfo, DateTime.UtcNow.AddDays(-1));

            //Assert
            Assert.AreEqual(0, result.Count);
            _mockRepository.VerifyAll();
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
