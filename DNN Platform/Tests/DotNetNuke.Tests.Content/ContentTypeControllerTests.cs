#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Content.Mocks;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;

using NUnit.Framework;

namespace DotNetNuke.Tests.Content
{
    /// <summary>
    ///   Summary description for ContentTypeTests
    /// </summary>
    [TestFixture]
    public class ContentTypeControllerTests
    {
        private Mock<CachingProvider> mockCache;

        #region Test Initialize

        [SetUp]
        public void SetUp()
        {
            //Register MockCachingProvider
            mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(String.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        #endregion

        #region AddContentType

        [Test]
        public void ContentTypeController_AddContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.AddContentType(null));
        }

        [Test]
        public void ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            ContentType contentType = ContentTestHelper.CreateValidContentType();

            //Act
            int contentTypeId = contentTypeController.AddContentType(contentType);

            //Assert
            mockDataService.Verify(ds => ds.AddContentType(contentType));
        }

        [Test]
        public void ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddContentType(It.IsAny<ContentType>())).Returns(Constants.CONTENTTYPE_AddContentTypeId);
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            var contentType = ContentTestHelper.CreateValidContentType();

            //Act
            int contentTypeId = contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTypeId, contentTypeId);
        }

        [Test]
        public void ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddContentType(It.IsAny<ContentType>())).Returns(Constants.CONTENTTYPE_AddContentTypeId);
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            var contentType = ContentTestHelper.CreateValidContentType();

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTypeId, contentType.ContentTypeId);
        }

        #endregion

        #region DeleteContentType

        [Test]
        public void ContentTypeController_DeleteContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.DeleteContentType(null));
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            ContentType contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentTypeId = Null.NullInteger;

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.DeleteContentType(contentType));
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            var contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            mockDataService.Verify(ds => ds.DeleteContentType(contentType));
        }

        #endregion

        #region GetContentTypes

        [Test]
        public void ContentTypeController_GetContentTypes_Calls_DataService()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetContentTypes()).Returns(MockHelper.CreateValidContentTypesReader(Constants.CONTENTTYPE_ValidContentTypeCount));
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes();

            //Assert
            mockDataService.Verify(ds => ds.GetContentTypes());
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetContentTypes()).Returns(MockHelper.CreateEmptyContentTypeReader());
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes();

            //Assert
            Assert.IsNotNull(contentTypes);
            Assert.AreEqual(0, contentTypes.Count());
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetContentTypes()).Returns(MockHelper.CreateValidContentTypesReader(Constants.CONTENTTYPE_ValidContentTypeCount));
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes();

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidContentTypeCount, contentTypes.Count());
        }

        #endregion

        #region UpdateContentType

        [Test]
        public void ContentTypeController_UpdateContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.UpdateContentType(null));
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            var contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentType = Constants.CONTENTTYPE_InValidContentType;

            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.UpdateContentType(contentType));
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);

            var contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId;
            contentType.ContentType = Constants.CONTENTTYPE_UpdateContentType;

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            mockDataService.Verify(ds => ds.UpdateContentType(contentType));
        }

        #endregion
    }
}