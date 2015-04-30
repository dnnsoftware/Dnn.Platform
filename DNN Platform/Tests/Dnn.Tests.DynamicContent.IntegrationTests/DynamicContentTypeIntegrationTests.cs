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
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Content;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Data;
using NUnit.Framework;

// ReSharper disable UseStringInterpolation

namespace Dnn.Tests.DynamicContent.IntegrationTests
{
    [TestFixture]
    public class DynamicContentTypeIntegrationTests : IntegrationTestBase
    {
        private readonly string _cacheKey = CachingProvider.GetCacheKey(DataCache.ContentTypesCacheKey) + "_PortalId_{0}";

        [SetUp]
        public void SetUp()
        {
            SetUpInternal();
        }

        [TearDown]
        public void TearDown()
        {
            TearDownInternal();
        }

        [Test]
        public void AddContentType_Inserts_New_Record_In_Database()
        {
            //Arrange
            SetUpContentTypes(RecordCount);
            var contentTypeController = new DynamicContentTypeController();
            var contentType = new DynamicContentType() {ContentType = "New_Type"};

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes");

            Assert.AreEqual(RecordCount + 1, actualCount);
        }

        [Test]
        public void AddContentType_Clears_Cache()
        {
            //Arrange
            SetUpContentTypes(RecordCount);
            var contentTypeController = new DynamicContentTypeController();
            var contentType = new DynamicContentType() { ContentType = "New_Type", PortalId = PortalId};

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            MockCache.Verify(r => r.Remove(String.Format(_cacheKey, PortalId)));
        }

        [Test]
        public void DeleteContentType_Deletes_Record_From_Database()
        {
            //Arrange
            var typeId = 2;
            SetUpContentTypes(RecordCount);
            var contentTypeController = new DynamicContentTypeController();
            var contentType = new DynamicContentType() { ContentTypeId = typeId, ContentType = "Type_2" };

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes");

            Assert.AreEqual(RecordCount - 1, actualCount);
        }

        [Test]
        public void DeleteContentType_Deletes_Correct_Record_From_Database()
        {
            //Arrange
            var typeId = 2;
            SetUpContentTypes(RecordCount);
            var contentTypeController = new DynamicContentTypeController();
            var contentType = new DynamicContentType() { ContentTypeId = typeId, ContentType = "Type_2" };

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            DataAssert.RecordWithIdNotPresent(DatabaseName, "ContentTYpes", "ContentTypeId", typeId);
        }

        [Test]
        public void DeleteContentType_Clears_Cache()
        {
            //Arrange
            var typeId = 2;
            SetUpContentTypes(RecordCount);
            var contentTypeController = new DynamicContentTypeController();
            var contentType = new DynamicContentType() { ContentTypeId = typeId, ContentType = "New_Type", PortalId = PortalId };

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            MockCache.Verify(r => r.Remove(String.Format(_cacheKey, PortalId)));
        }

        [Test]
        public void GetContentTypes_Returns_Records_For_Portal_From_Database()
        {
            //Arrange
            SetUpContentTypes(RecordCount);
            var contentTypeController = new DynamicContentTypeController();

            //Act
            var contentTypes = contentTypeController.GetContentTypes(PortalId);

            //Assert
            Assert.AreEqual(RecordCount/2, contentTypes.Count());
            foreach (var contentType in contentTypes)
            {
                Assert.AreEqual(PortalId, contentType.PortalId);
            }
        }

        [TestCase(20, 0, 5, 2)]
        [TestCase(20, 1, 5, 2)]
        [TestCase(30, 2, 5, 3)]
        public void GetContentTypes_Overload_Returns_PagedList_For_Portal_From_Database(int recordCount, int pageIndex, int pageSize, int pageCount)
        {
            //Arrange
            SetUpContentTypes(recordCount);
            var contentTypeController = new DynamicContentTypeController();

            //Act
            var contentTypes = contentTypeController.GetContentTypes(PortalId, pageIndex, pageSize);

            //Assert
            Assert.AreEqual(recordCount / 2, contentTypes.TotalCount);
            Assert.AreEqual(pageCount, contentTypes.PageCount);
            Assert.AreEqual(pageSize, contentTypes.PageSize);
            Assert.AreEqual(pageIndex == 0, contentTypes.IsFirstPage);
            Assert.AreEqual(pageIndex < pageCount - 1, contentTypes.HasNextPage);
            Assert.AreEqual(pageIndex > 0, contentTypes.HasPreviousPage);
            Assert.AreEqual(pageIndex == pageCount - 1, contentTypes.IsLastPage);

            foreach (var contentType in contentTypes)
            {
                Assert.AreEqual(PortalId, contentType.PortalId);
            }
        }

        [Test]
        public void UpdateContentType_Updates_Correct_Record_In_Database()
        {
            //Arrange
            var contentTypeId = 2;
            SetUpContentTypes(RecordCount);
            var contentTypeController = new DynamicContentTypeController();
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId, ContentType = "NewType" };

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes");
            Assert.AreEqual(RecordCount, actualCount);

            DataAssert.IsFieldValueEqual("NewType", DatabaseName, "ContentTypes", "ContentType", "ContentTypeId", contentTypeId);
        }

        [Test]
        public void UpdateContentType_Clears_Cache()
        {
            //Arrange
            var typeId = 2;
            SetUpContentTypes(RecordCount);
            var contentTypeController = new DynamicContentTypeController();
            var contentType = new DynamicContentType() { ContentTypeId = typeId, ContentType = "New_Type", PortalId = PortalId };

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            MockCache.Verify(r => r.Remove(String.Format(_cacheKey, PortalId)));
        }


    }
}
