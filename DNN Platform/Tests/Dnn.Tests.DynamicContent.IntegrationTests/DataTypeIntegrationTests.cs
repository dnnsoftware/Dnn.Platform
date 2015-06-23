// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Common;
using Dnn.DynamicContent.Exceptions;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Content;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Data;
using DotNetNuke.Tests.Utilities;
using Moq;
using NUnit.Framework;

// ReSharper disable UseStringInterpolation
// ReSharper disable BuiltInTypeReferenceStyle

namespace Dnn.Tests.DynamicContent.IntegrationTests
{
    [TestFixture]
    public class DataTypeIntegrationTests : IntegrationTestBase
    {
        private readonly string _cacheKey = CachingProvider.GetCacheKey(DataTypeManager.DataTypeCacheKey) + "_PortalId_{0}";

        [SetUp]
        public void SetUp()
        {
            SetUpInternal();
        }

        [TearDown]
        public void TearDown()
        {
            TearDownInternal();
            ContentController.ClearInstance();
            DateUtilitiesManager.ClearInstance();
        }

        [Test]
        public void AddDataType_Inserts_New_Record_In_Database()
        {
            //Arrange
            SetUpDataTypes(RecordCount);

            var mockDateUtilitesManager = new Mock<IDateUtilitiesManager>();
            mockDateUtilitesManager.Setup(dt => dt.GetDatabaseTime()).Returns(DateTime.UtcNow);
            DateUtilitiesManager.SetTestableInstance(mockDateUtilitesManager.Object);

            var dataTypeController = new DataTypeManager();
            var dataType = new DataType() { Name = "New_Type" };

            //Act
            dataTypeController.AddDataType(dataType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_DataTypes");

            Assert.AreEqual(RecordCount + 1, actualCount);
        }

        [Test]
        public void AddDataType_Clears_Cache()
        {
            //Arrange
            SetUpDataTypes(RecordCount);

            var mockDateUtilitesManager = new Mock<IDateUtilitiesManager>();
            mockDateUtilitesManager.Setup(dt => dt.GetDatabaseTime()).Returns(DateTime.UtcNow);
            DateUtilitiesManager.SetTestableInstance(mockDateUtilitesManager.Object);

            var dataTypeController = new DataTypeManager();
            var dataType = new DataType() { Name = "New_Type" };

            //Act
            dataTypeController.AddDataType(dataType);

            //Assert
            MockCache.Verify(c => c.Remove(String.Format(_cacheKey, -1)));
        }

        [Test]
        public void DeleteDataType_Deletes_Record_From_Database_If_Not_In_Use()
        {
            //Arrange
            var dataTypeId = 6;
            SetUpDataTypes(RecordCount);
            SetUpFieldDefinitions(5);

            var dataTypeController = new DataTypeManager();
            var dataType = new DataType() { DataTypeId = dataTypeId, Name = "New_Type" };

            //Act
            dataTypeController.DeleteDataType(dataType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_DataTypes");

            Assert.AreEqual(RecordCount - 1, actualCount);
        }

        [Test]
        public void DeleteDataType_Deletes_Correct_Record_From_Database_If_Not_In_Use()
        {
            //Arrange
            var dataTypeId = 6;
            SetUpDataTypes(RecordCount);
            SetUpFieldDefinitions(5);
            var dataTypeController = new DataTypeManager();
            var dataType = new DataType() { DataTypeId = dataTypeId, Name = "New_Type" };

            //Act
            dataTypeController.DeleteDataType(dataType);

            //Assert
            DataAssert.RecordWithIdNotPresent(DatabaseName, "ContentTypes_DataTypes", "DataTypeId", dataTypeId);
        }

        [Test]
        public void DeleteDataType_Throws_If_DataType_Used()
        {
            //Arrange
            var dataTypeId = 6;
            SetUpDataTypes(RecordCount);
            SetUpFieldDefinitions(RecordCount);
            var dataTypeController = new DataTypeManager();
            var dataType = new DataType() { DataTypeId = dataTypeId, Name = "New_Type" };

            //Act, Assert
            Assert.Throws<DataTypeInUseException>(() => dataTypeController.DeleteDataType(dataType));
        }

        [Test]
        public void DeleteDataType_Clears_Cache()
        {
            //Arrange
            var dataTypeId = 6;
            SetUpDataTypes(RecordCount);
            SetUpFieldDefinitions(5);

            var dataTypeController = new DataTypeManager();
            var dataType = new DataType() { DataTypeId = dataTypeId, Name = "New_Type" };

            //Act
            dataTypeController.DeleteDataType(dataType);

            //Assert
            MockCache.Verify(c => c.Remove(String.Format(_cacheKey, -1)));
        }

        [Test]
        public void GetDataType_Fetches_Record_If_Id_Valid()
        {
            //Arrange
            var dataTypeId = Constants.CONTENTTYPE_ValidDataTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;
            MockCache.Setup(c => c.GetItem(String.Format(_cacheKey, portalId))).Returns(null);
            SetUpDataTypes(RecordCount, portalId);

            var dataTypeController = new DataTypeManager();

            //Act
            var dataType = dataTypeController.GetDataType(dataTypeId, portalId);

            //Assert
            Assert.NotNull(dataType);
        }

        [Test]
        public void GetDataType_Returns_Null_If_Id_InValid()
        {
            //Arrange
            var dataTypeId = 999;
            var portalId = Constants.PORTAL_ValidPortalId;
            MockCache.Setup(c => c.GetItem(String.Format(_cacheKey, portalId))).Returns(null);
            SetUpDataTypes(RecordCount, portalId);

            var dataTypeController = new DataTypeManager();

            //Act
            var dataType = dataTypeController.GetDataType(dataTypeId, portalId);

            //Assert
            Assert.Null(dataType);
        }

        [Test]
        public void GetDataTypes_Fetches_Records_From_Database_If_Cache_Is_Null()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            MockCache.Setup(c => c.GetItem(String.Format(_cacheKey, portalId))).Returns(null);
            SetUpDataTypes(RecordCount, portalId);

            var dataTypeController = new DataTypeManager();

            //Act
            var dataTypes = dataTypeController.GetDataTypes(portalId);

            //Assert
            Assert.AreEqual(RecordCount, dataTypes.Count());
        }

        [Test]
        public void GetDataTypes_Fetches_Records_From_Cache_If_Not_Null()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var cacheCount = 15;
            MockCache.Setup(c => c.GetItem(String.Format(_cacheKey, portalId))).Returns(SetUpCache(cacheCount));

            SetUpDataTypes(RecordCount);

            var dataTypeController = new DataTypeManager();

            //Act
            var dataTypes = dataTypeController.GetDataTypes(portalId);

            //Assert
            Assert.AreEqual(cacheCount, dataTypes.Count());
        }

        [Test]
        public void UpdateDataType_Updates_Correct_Record_In_Database()
        {
            //Arrange
            var dataTypeId = 2;
            SetUpDataTypes(RecordCount);
            SetUpFieldDefinitions(5);

            var mockDateUtilitesManager = new Mock<IDateUtilitiesManager>();
            mockDateUtilitesManager.Setup(dt => dt.GetDatabaseTime()).Returns(DateTime.UtcNow);
            DateUtilitiesManager.SetTestableInstance(mockDateUtilitesManager.Object);

            var dataTypeController = new DataTypeManager();
            var dataType = new DataType() { DataTypeId = dataTypeId, Name = "NewType" };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);
            mockContentController.Setup(c => c.GetContentItemsByContentType(Constants.CONTENTTYPE_ValidContentTypeId))
                    .Returns(new List<ContentItem>().AsQueryable());

            //Act
            dataTypeController.UpdateDataType(dataType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_DataTypes");
            Assert.AreEqual(RecordCount, actualCount);

            DataAssert.IsFieldValueEqual("NewType", DatabaseName, "ContentTypes_DataTypes", "Name", "DataTypeId", dataTypeId);
        }

        [Test]
        public void UpdateDataType_Throws_If_DataType_Used()
        {
            //Arrange
            var dataTypeId = 2;
            SetUpDataTypes(RecordCount);
            SetUpFieldDefinitions(5);

            var mockDateUtilitesManager = new Mock<IDateUtilitiesManager>();
            mockDateUtilitesManager.Setup(dt => dt.GetDatabaseTime()).Returns(DateTime.UtcNow);
            DateUtilitiesManager.SetTestableInstance(mockDateUtilitesManager.Object);

            var dataTypeController = new DataTypeManager();
            var dataType = new DataType() { DataTypeId = dataTypeId, Name = "NewType" };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);
            mockContentController.Setup(c => c.GetContentItemsByContentType(dataTypeId))
                .Returns(new List<ContentItem>() { new ContentItem() }.AsQueryable());

            //Act, Assert
            Assert.Throws<DataTypeInUseException>(() => dataTypeController.UpdateDataType(dataType));
        }

        [Test]
        public void UpdateDataType_Updates_Correct_Record_In_Database_If_DataType_Used_And_OverrideFlag_Set()
        {
            //Arrange
            var dataTypeId = 2;
            SetUpDataTypes(RecordCount);
            SetUpFieldDefinitions(5);

            var mockDateUtilitesManager = new Mock<IDateUtilitiesManager>();
            mockDateUtilitesManager.Setup(dt => dt.GetDatabaseTime()).Returns(DateTime.UtcNow);
            DateUtilitiesManager.SetTestableInstance(mockDateUtilitesManager.Object);

            var dataTypeController = new DataTypeManager();
            var dataType = new DataType() { DataTypeId = dataTypeId, Name = "NewType" };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);
            mockContentController.Setup(c => c.GetContentItemsByContentType(dataTypeId))
                .Returns(new List<ContentItem>() { new ContentItem() }.AsQueryable());

            //Act
            dataTypeController.UpdateDataType(dataType, true);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_DataTypes");
            Assert.AreEqual(RecordCount, actualCount);

            DataAssert.IsFieldValueEqual("NewType", DatabaseName, "ContentTypes_DataTypes", "Name", "DataTypeId", dataTypeId);
        }

        [Test]
        public void UpdateDataType_Clears_Cache()
        {
            //Arrange
            var dataTypeId = 2;
            SetUpDataTypes(RecordCount);
            SetUpFieldDefinitions(5);

            var mockDateUtilitesManager = new Mock<IDateUtilitiesManager>();
            mockDateUtilitesManager.Setup(dt => dt.GetDatabaseTime()).Returns(DateTime.UtcNow);
            DateUtilitiesManager.SetTestableInstance(mockDateUtilitesManager.Object);

            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var dataTypeController = new DataTypeManager(dataContext);
            var dataType = new DataType() { DataTypeId = dataTypeId, Name = "NewType" };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);
            mockContentController.Setup(c => c.GetContentItemsByContentType(Constants.CONTENTTYPE_ValidContentTypeId))
                    .Returns(new List<ContentItem>().AsQueryable());

            //Act
            dataTypeController.UpdateDataType(dataType);

            //Assert
            MockCache.Verify(c => c.Remove(String.Format(_cacheKey, -1)));
        }

        private IQueryable<DataType> SetUpCache(int count)
        {
            var list = new List<DataType>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new DataType { DataTypeId = i, Name = String.Format("Type_{0}", i) });
            }
            return list.AsQueryable();
        }
    }
}
