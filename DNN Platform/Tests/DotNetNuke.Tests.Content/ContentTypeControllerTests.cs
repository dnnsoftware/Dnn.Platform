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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Content.Mocks;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;
// ReSharper disable UnusedVariable

namespace DotNetNuke.Tests.Content
{
    /// <summary>
    ///   Summary description for ContentTypeTests
    /// </summary>
    [TestFixture]
    public class ContentTypeControllerTests
    {
        private Mock<IDataContext> _mockDataContext;
        private Mock<IRepository<ContentType>> _mockRepository;
        private Mock<CachingProvider> _mockCache;
        private string _contentTypeCacheKey;

        [SetUp]
        public void SetUp()
        {
            //Register MockCachingProvider
            _mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(String.Empty);

            _contentTypeCacheKey = CachingProvider.GetCacheKey(DataCache.ContentTypesCacheKey);

            _mockDataContext = new Mock<IDataContext>();
            _mockRepository = new Mock<IRepository<ContentType>>();
            _mockDataContext.Setup(dc => dc.GetRepository<ContentType>()).Returns(_mockRepository.Object);

        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void ContentTypeController_Constructor_Throws_On_Null_DataContext()
        {
            IDataContext dataContent = null;

            //Arrange, Act, Arrange
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => new ContentTypeController(dataContent));
        }

        [Test]
        public void ContentTypeController_AddContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.AddContentType(null));
        }

        [Test]
        public void ContentTypeController_AddContentType_Throws_On_Empty_ContentType_Proeprty()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTypeController.AddContentType(new ContentType()));
        }

        [Test]
        public void ContentTypeController_AddContentType_Calls_Repository_Insert_On_Valid_Arguments()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            ContentType contentType = ContentTestHelper.CreateValidContentType();

            //Act
            int contentTypeId = contentTypeController.AddContentType(contentType);

            //Assert
            _mockRepository.Verify(rep => rep.Insert(contentType));
        }

        [Test]
        public void ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType()
        {
            //Arrange
            _mockRepository.Setup(r => r.Insert(It.IsAny<ContentType>()))
                            .Callback((ContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

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
            _mockRepository.Setup(r => r.Insert(It.IsAny<ContentType>()))
                            .Callback((ContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var contentType = ContentTestHelper.CreateValidContentType();

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTypeId, contentType.ContentTypeId);
        }

        [Test]
        public void ContentTypeController_ClearContentTypeCache_Clears_Cache()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            ContentType contentType = ContentTestHelper.CreateValidContentType();

            //Act
            contentTypeController.ClearContentTypeCache();

            //Assert
            _mockCache.Verify(r => r.Remove(_contentTypeCacheKey));
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.DeleteContentType(null));
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            ContentType contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentTypeId = Null.NullInteger;

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.DeleteContentType(contentType));
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Calls_Repository_Delete_On_Valid_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            _mockRepository.Verify(r => r.Delete(contentType));
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Calls_Repository_Get()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes();

            //Assert
            _mockRepository.Verify(r => r.Get());
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
        {
            //Arrange
            _mockRepository.Setup(r => r.Get()).Returns(new List<ContentType>());
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

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
            _mockRepository.Setup(r => r.Get()).Returns(MockHelper.CreateValidContentTypes(Constants.CONTENTTYPE_ValidContentTypeCount));
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes();

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidContentTypeCount, contentTypes.Count());
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.UpdateContentType(null));
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Throws_On_Empty_ContentType_Property()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);
            var contentType = new ContentType {ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId};

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTypeController.UpdateContentType(contentType));
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentType = Constants.CONTENTTYPE_InValidContentType;

            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.UpdateContentType(contentType));
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Calls_Repository_Update_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId;
            contentType.ContentType = Constants.CONTENTTYPE_UpdateContentType;

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockRepository.Verify(r => r.Update(contentType));
        }
    }
}