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
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Content
{
    [TestFixture]
    public class ContentTypeTests
    {
        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void Constructor_Sets_Default_Properties()
        {
            //Arrange

            //Act
            var contentType = new ContentType();

            //Assert
            Assert.AreEqual(-1, contentType.ContentTypeId);
            Assert.AreEqual(String.Empty, contentType.ContentType);
        }

        [Test]
        public void Constructor_Overload_Sets_Default_Properties()
        {
            //Arrange

            //Act
            var contentType = new ContentType(Constants.CONTENTTYPE_ValidContentType);

            //Assert
            Assert.AreEqual(-1, contentType.ContentTypeId);
            Assert.AreEqual(Constants.CONTENTTYPE_ValidContentType, contentType.ContentType);
        }

        [Test]
        public void ToString_Method_Returns_ContentType()
        {
            //Arrange
            var contentType = new ContentType(Constants.CONTENTTYPE_ValidContentType);

            //Act
            var actualValue = contentType.ToString();

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidContentType, actualValue);
        }

        [Test]
        public void DesktopModule_Property_Calls_ContentTypeControllery_Get()
        {
            //Arrange
            var mockContentTypeController = new Mock<IContentTypeController>();
            ContentTypeController.SetTestableInstance(mockContentTypeController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            ContentType actualValue = ContentType.DesktopModule;

            //Assert
            mockContentTypeController.Verify(c => c.GetContentTypes());
        }

        [Test]
        public void DesktopModule_Property_Returns_DesktopModule_ContentType()
        {
            //Arrange
            const string expectedValue = ContentType.DesktopModuleContentTypeName;
            var mockContentTypeController = new Mock<IContentTypeController>();
            mockContentTypeController.Setup(c => c.GetContentTypes()).Returns(GetContentTypes().AsQueryable());
            ContentTypeController.SetTestableInstance(mockContentTypeController.Object);

            //Act
            ContentType actualValue = ContentType.DesktopModule;

            //Assert
            Assert.AreEqual(expectedValue, actualValue.ContentType);
        }

        [Test]
        public void Module_Property_Calls_Repository_Get()
        {
            //Arrange
            var mockContentTypeController = new Mock<IContentTypeController>();
            ContentTypeController.SetTestableInstance(mockContentTypeController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            ContentType actualValue = ContentType.Module;

            //Assert
            mockContentTypeController.Verify(c => c.GetContentTypes());
        }

        [Test]
        public void Module_Property_Returns_Module_ContentType()
        {
            //Arrange
            const string expectedValue = ContentType.ModuleContentTypeName;
            var mockContentTypeController = new Mock<IContentTypeController>();
            mockContentTypeController.Setup(c => c.GetContentTypes()).Returns(GetContentTypes().AsQueryable());
            ContentTypeController.SetTestableInstance(mockContentTypeController.Object);

            //Act
            ContentType actualValue = ContentType.Module;

            //Assert
            Assert.AreEqual(expectedValue, actualValue.ContentType);
        }

        [Test]
        public void Tab_Property_Calls_Repository_Get()
        {
            //Arrange
            var mockContentTypeController = new Mock<IContentTypeController>();
            ContentTypeController.SetTestableInstance(mockContentTypeController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            ContentType actualValue = ContentType.Tab;

            //Assert
            mockContentTypeController.Verify(c => c.GetContentTypes());
        }

        [Test]
        public void Tab_Property_Returns_Tab_ContentType()
        {
            //Arrange
            const string expectedValue = ContentType.TabContentTypeName;
            var mockContentTypeController = new Mock<IContentTypeController>();
            mockContentTypeController.Setup(c => c.GetContentTypes()).Returns(GetContentTypes().AsQueryable());
            ContentTypeController.SetTestableInstance(mockContentTypeController.Object);

            //Act
            ContentType actualValue = ContentType.Tab;

            //Assert
            Assert.AreEqual(expectedValue, actualValue.ContentType);
        }

        private List<ContentType> GetContentTypes()
        {
            var list = new List<ContentType>
                                {
                                    new ContentType(ContentType.DesktopModuleContentTypeName),
                                    new ContentType(ContentType.ModuleContentTypeName),
                                    new ContentType(ContentType.TabContentTypeName)
                                };

            return list;
        }
    }
}
