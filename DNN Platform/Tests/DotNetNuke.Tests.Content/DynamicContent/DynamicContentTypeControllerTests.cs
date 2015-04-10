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
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.DynamicContent;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Content.DynamicContent
{
    [TestFixture]
    public class DynamicContentTypeControllerTests
    {
        private Mock<IDataContext> _mockDataContext;
        private Mock<IRepository<DynamicContentType>> _mockContentTypeRepository;
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

            _mockContentTypeRepository = new Mock<IRepository<DynamicContentType>>();
            _mockDataContext.Setup(dc => dc.GetRepository<DynamicContentType>()).Returns(_mockContentTypeRepository.Object);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void Constructor_Throws_On_Null_DataContext()
        {
            IDataContext dataContent = null;

            //Arrange, Act, Arrange
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => new DynamicContentTypeController(dataContent));
        }

        [Test]
        public void AddContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.AddContentType(null));
        }

        [Test]
        public void AddContentType_Throws_On_Empty_ContentType_Property()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTypeController.AddContentType(new DynamicContentType()));
        }

        [Test]
        public void AddContentType_Calls_Repository_Insert_On_Valid_Arguments()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            var contentType = new DynamicContentType { ContentType = Constants.CONTENTTYPE_ValidContentType };

            //Act
            // ReSharper disable once UnusedVariable
            int contentTypeId = contentTypeController.AddContentType(contentType);

            //Assert
            _mockContentTypeRepository.Verify(rep => rep.Insert(contentType));
        }

        [Test]
        public void AddContentType_Returns_ValidId_On_Valid_ContentType()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            var contentType = new DynamicContentType { ContentType = Constants.CONTENTTYPE_ValidContentType };

            //Act
            int contentTypeId = contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTypeId, contentTypeId);
        }

        [Test]
        public void AddContentType_Sets_ValidId_On_Valid_ContentType()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((ContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            var contentType = new DynamicContentType { ContentType = Constants.CONTENTTYPE_ValidContentType };

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTypeId, contentType.ContentTypeId);
        }

        [Test]
        public void DeleteContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.DeleteContentType(null));
        }

        [Test]
        public void DeleteContentType_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            var contentType = new DynamicContentType
            {
                ContentType = Constants.CONTENTTYPE_ValidContentType,
                ContentTypeId = Null.NullInteger
            };

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.DeleteContentType(contentType));
        }

        [Test]
        public void DeleteContentType_Calls_Repository_Delete_On_Valid_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            var contentType = new DynamicContentType
            {
                ContentType = Constants.CONTENTTYPE_ValidContentType,
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
            };

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Delete(contentType));
        }

        [Test]
        public void GetContentTypes_Calls_Repository_Get()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Get(Constants.PORTAL_ValidPortalId));
        }

        [Test]
        public void GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Get(Constants.PORTAL_ValidPortalId))
                .Returns(new List<DynamicContentType>());
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId);

            //Assert
            Assert.IsNotNull(contentTypes);
            Assert.AreEqual(0, contentTypes.Count());
        }

        [Test]
        public void GetContentTypes_Returns_List_Of_ContentTypes()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Get(Constants.PORTAL_ValidPortalId))
                .Returns(CreateValidContentTypes(Constants.CONTENTTYPE_ValidContentTypeCount));
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidContentTypeCount, contentTypes.Count());
        }

        [Test]
        public void GetContentTypes_Overload_Calls_Repository_GetPage()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId, Constants.PAGE_First, Constants.PAGE_RecordCount);

            //Assert
            _mockContentTypeRepository.Verify(r => r.GetPage(Constants.PORTAL_ValidPortalId, Constants.PAGE_First, Constants.PAGE_RecordCount));
        }

        [Test]
        public void GetContentTypes_Overload_Returns_PagedList()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);
            _mockContentTypeRepository.Setup(r => r.GetPage(Constants.PORTAL_ValidPortalId, Constants.PAGE_First, Constants.PAGE_RecordCount))
                .Returns(new PagedList<DynamicContentType>(new List<DynamicContentType>(), Constants.PAGE_First, Constants.PAGE_RecordCount));

            //Act
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId, Constants.PAGE_First, Constants.PAGE_RecordCount);

            //Assert
            Assert.IsInstanceOf<IPagedList<DynamicContentType>>(contentTypes);
        }

        [Test]
        public void UpdateContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.UpdateContentType(null));
        }

        [Test]
        public void UpdateContentType_Throws_On_Empty_ContentType_Property()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);
            var contentType = new DynamicContentType { ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTypeController.UpdateContentType(contentType));
        }

        [Test]
        public void UpdateContentType_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            var contentType = new DynamicContentType
            {
                ContentTypeId = Constants.CONTENTTYPE_InValidContentTypeId,
                ContentType = Constants.CONTENTTYPE_ValidContentType
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.UpdateContentType(contentType));
        }

        [Test]
        public void UpdateContentType_Calls_Repository_Update_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeController(_mockDataContext.Object);

            var contentType = new DynamicContentType
            {
                ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId,
                ContentType = Constants.CONTENTTYPE_UpdateContentType
            };

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Update(contentType));
        }

        private List<DynamicContentType> CreateValidContentTypes(int count)
        {
            var contentTypes = new List<DynamicContentType>();
            for (int i = Constants.CONTENTTYPE_ValidContentTypeId; i < Constants.CONTENTTYPE_ValidContentTypeId + count; i++)
            {
                string contentType = Constants.CONTENTTYPE_ValidContentType;

                contentTypes.Add(new DynamicContentType { ContentTypeId = i, ContentType = contentType });
            }

            return contentTypes;
        }
    }
}
