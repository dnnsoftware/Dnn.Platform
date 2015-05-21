// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Content;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Data;
using Moq;
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

            var mockDateUtilitesManager = new Mock<IDateUtilitiesManager>();
            mockDateUtilitesManager.Setup(dt => dt.GetDatabaseTime()).Returns(DateTime.UtcNow);
            DateUtilitiesManager.SetTestableInstance(mockDateUtilitesManager.Object);

            var contentTypeController = new DynamicContentTypeManager();
            var contentType = new DynamicContentType() { Name = "New_Type"};

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

            var mockDateUtilitesManager = new Mock<IDateUtilitiesManager>();
            mockDateUtilitesManager.Setup(dt => dt.GetDatabaseTime()).Returns(DateTime.UtcNow);
            DateUtilitiesManager.SetTestableInstance(mockDateUtilitesManager.Object);

            var contentTypeController = new DynamicContentTypeManager();
            var contentType = new DynamicContentType() { Name = "New_Type", PortalId = PortalId};

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
            var contentTypeController = new DynamicContentTypeManager();
            var contentType = new DynamicContentType() { ContentTypeId = typeId, Name = "Type_2" };

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
            var contentTypeController = new DynamicContentTypeManager();
            var contentType = new DynamicContentType() { ContentTypeId = typeId, Name = "Type_2" };

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
            var contentTypeController = new DynamicContentTypeManager();
            var contentType = new DynamicContentType() { ContentTypeId = typeId, Name = "New_Type", PortalId = PortalId };

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

            var mockDateUtilitesManager = new Mock<IDateUtilitiesManager>();
            mockDateUtilitesManager.Setup(dt => dt.GetDatabaseTime()).Returns(DateTime.UtcNow);
            DateUtilitiesManager.SetTestableInstance(mockDateUtilitesManager.Object);

            var contentTypeController = new DynamicContentTypeManager();

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
            var contentTypeController = new DynamicContentTypeManager();

            //Act
            var contentTypes = contentTypeController.GetContentTypes("Type", PortalId, pageIndex, pageSize, false);

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

            var mockDateUtilitesManager = new Mock<IDateUtilitiesManager>();
            mockDateUtilitesManager.Setup(dt => dt.GetDatabaseTime()).Returns(DateTime.UtcNow);
            DateUtilitiesManager.SetTestableInstance(mockDateUtilitesManager.Object);

            var contentTypeController = new DynamicContentTypeManager();
            var contentType = new DynamicContentType() { ContentTypeId = contentTypeId, Name = "NewType" };

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

            var mockDateUtilitesManager = new Mock<IDateUtilitiesManager>();
            mockDateUtilitesManager.Setup(dt => dt.GetDatabaseTime()).Returns(DateTime.UtcNow);
            DateUtilitiesManager.SetTestableInstance(mockDateUtilitesManager.Object);

            var contentTypeController = new DynamicContentTypeManager();
            var contentType = new DynamicContentType() { ContentTypeId = typeId, Name = "New_Type", PortalId = PortalId };

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            MockCache.Verify(r => r.Remove(String.Format(_cacheKey, PortalId)));
        }


    }
}
