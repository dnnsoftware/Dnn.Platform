﻿// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Tests.Utilities;
using Moq;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    class DynamicContentTypeTests
    {
        [Test]
        public void Constructor_Sets_Default_Properties()
        {
            //Arrange

            //Act
            var type = new DynamicContentType();

            //Assert
            Assert.AreEqual(-1, type.PortalId);
            Assert.AreEqual(true, type.IsDynamic);
            Assert.AreEqual(-1, type.ContentTypeId);
            Assert.AreEqual(String.Empty, type.Name);
        }

        [Test]
        public void Constructor_Sets_PortalId_Property()
        {
            //Arrange

            //Act
            var contentType = new DynamicContentType(Constants.CONTENT_ValidPortalId);

            //Assert
            Assert.AreEqual(Constants.CONTENT_ValidPortalId, contentType.PortalId);
        }

        [TestCase(-1, true)]
        [TestCase(0, false)]
        public void IsSystem_Returns_Correct_Value(int portalId, bool isSystem)
        {
            //Arrange

            //Act
            var contentType = new DynamicContentType(portalId);

            //Assert
            Assert.AreEqual(isSystem, contentType.IsSystem);
        }

        [Test]
        public void FieldDefinitions_Property_Creates_New_List_If_ContentTypeId_Negative()
        {
            //Arrange
            var contentType = new DynamicContentType();
            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var fields = contentType.FieldDefinitions;

            //Assert
            mockFieldDefinitionController.Verify(c => c.GetFieldDefinitions(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void FieldDefinitions_Property_Calls_FieldDefinitionController_Get_If_ContentTypeId_Not_Negative()
        {
            //Arrange
            var contentTypeId = 3;
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId };
            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var fields = contentType.FieldDefinitions;

            //Assert
            mockFieldDefinitionController.Verify(c => c.GetFieldDefinitions(It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void FieldDefinitions_Property_Calls_FieldDefinitionController_Get_Once_Only()
        {
            //Arrange
            var contentTypeId = 3;
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId };
            var mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            mockFieldDefinitionController.Setup(fd => fd.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition> { new FieldDefinition() { ContentTypeId = contentTypeId } }.AsQueryable());
            FieldDefinitionManager.SetTestableInstance(mockFieldDefinitionController.Object);

            //Act
            // ReSharper disable UnusedVariable
            var fields = contentType.FieldDefinitions;
            var fields1 = contentType.FieldDefinitions;
            var fields2 = contentType.FieldDefinitions;
            // ReSharper restore UnusedVariable

            //Assert
            mockFieldDefinitionController.Verify(c => c.GetFieldDefinitions(contentTypeId), Times.AtMostOnce);
        }

        [Test]
        public void Templates_Property_Creates_New_List_If_ContentTypeId_Negative()
        {
            //Arrange
            var contentType = new DynamicContentType();
            var mockFieldDefinitionController = new Mock<IContentTemplateManager>();
            ContentTemplateManager.SetTestableInstance(mockFieldDefinitionController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var fields = contentType.Templates;

            //Assert
            mockFieldDefinitionController.Verify(c => c.GetContentTemplates(It.IsAny<int>(), It.IsAny<int>(), false), Times.Never);
        }

        [Test]
        public void Templates_Property_Calls_ContentTemplateController_Get_If_ContentTypeId_Not_Negative()
        {
            //Arrange
            var contentTypeId = 3;
            var portalId = Constants.PORTAL_ValidPortalId;
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId, PortalId = portalId };
            var mockFieldDefinitionController = new Mock<IContentTemplateManager>();
            ContentTemplateManager.SetTestableInstance(mockFieldDefinitionController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var fields = contentType.Templates;

            //Assert
            mockFieldDefinitionController.Verify(c => c.GetContentTemplatesByContentType(contentTypeId), Times.Once);
        }

        [Test]
        public void Templates_Property_Calls_ContentTemplateController_Get_Once_Only()
        {
            //Arrange
            var contentTypeId = 3;
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId };
            var mockFieldDefinitionController = new Mock<IContentTemplateManager>();
            mockFieldDefinitionController.Setup(fd => fd.GetContentTemplates(contentTypeId, It.IsAny<int>(), false))
                .Returns(new List<ContentTemplate> { new ContentTemplate() { ContentTypeId = contentTypeId } }.AsQueryable());
            ContentTemplateManager.SetTestableInstance(mockFieldDefinitionController.Object);

            //Act
            // ReSharper disable UnusedVariable
            var fields = contentType.FieldDefinitions;
            var fields1 = contentType.FieldDefinitions;
            var fields2 = contentType.FieldDefinitions;
            // ReSharper restore UnusedVariable

            //Assert
            mockFieldDefinitionController.Verify(c => c.GetContentTemplates(contentTypeId, It.IsAny<int>(), false), Times.AtMostOnce);
        }
    }
}
