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
            mockFieldDefinitionController.Verify(c => c.GetContentTemplates(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void Templates_Property_Calls_ContentTemplateController_Get_If_ContentTypeId_Not_Negative()
        {
            //Arrange
            var contentTypeId = 3;
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId };
            var mockFieldDefinitionController = new Mock<IContentTemplateManager>();
            ContentTemplateManager.SetTestableInstance(mockFieldDefinitionController.Object);

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
            var mockFieldDefinitionController = new Mock<IContentTemplateManager>();
            mockFieldDefinitionController.Setup(fd => fd.GetContentTemplates(contentTypeId))
                .Returns(new List<ContentTemplate> { new ContentTemplate() { ContentTypeId = contentTypeId } }.AsQueryable());
            ContentTemplateManager.SetTestableInstance(mockFieldDefinitionController.Object);

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
