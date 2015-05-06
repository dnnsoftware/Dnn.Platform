// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
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
            Assert.AreEqual(String.Empty, type.ContentType);
        }

        [Test]
        public void FieldDefinitions_Property_Creates_New_List_If_ContentTypeId_Negative()
        {
            //Arrange
            var contentType = new DynamicContentType();
            var mockFieldDefinitionController = new Mock<IFieldDefinitionController>();
            FieldDefinitionController.SetTestableInstance(mockFieldDefinitionController.Object);

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
            var mockFieldDefinitionController = new Mock<IFieldDefinitionController>();
            FieldDefinitionController.SetTestableInstance(mockFieldDefinitionController.Object);

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
            var mockFieldDefinitionController = new Mock<IFieldDefinitionController>();
            mockFieldDefinitionController.Setup(fd => fd.GetFieldDefinitions(contentTypeId))
                .Returns(new List<FieldDefinition> { new FieldDefinition() { ContentTypeId = contentTypeId } }.AsQueryable());
            FieldDefinitionController.SetTestableInstance(mockFieldDefinitionController.Object);

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
            var mockFieldDefinitionController = new Mock<IContentTemplateController>();
            ContentTemplateController.SetTestableInstance(mockFieldDefinitionController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var fields = contentType.Templates;

            //Assert
            mockFieldDefinitionController.Verify(c => c.GetContentTemplates(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void Templates_Property_Calls_ContentTemplateController_Get_If_ContentTypeId_Not_Negative()
        {
            //Arrange
            var contentTypeId = 3;
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId };
            var mockFieldDefinitionController = new Mock<IContentTemplateController>();
            ContentTemplateController.SetTestableInstance(mockFieldDefinitionController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var fields = contentType.Templates;

            //Assert
            mockFieldDefinitionController.Verify(c => c.GetContentTemplates(It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void Templates_Property_Calls_ContentTemplateController_Get_Once_Only()
        {
            //Arrange
            var contentTypeId = 3;
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId };
            var mockFieldDefinitionController = new Mock<IContentTemplateController>();
            mockFieldDefinitionController.Setup(fd => fd.GetContentTemplates(contentTypeId))
                .Returns(new List<ContentTemplate> { new ContentTemplate() { ContentTypeId = contentTypeId } }.AsQueryable());
            ContentTemplateController.SetTestableInstance(mockFieldDefinitionController.Object);

            //Act
            // ReSharper disable UnusedVariable
            var fields = contentType.FieldDefinitions;
            var fields1 = contentType.FieldDefinitions;
            var fields2 = contentType.FieldDefinitions;
            // ReSharper restore UnusedVariable

            //Assert
            mockFieldDefinitionController.Verify(c => c.GetContentTemplates(contentTypeId), Times.AtMostOnce);
        }
    }
}
