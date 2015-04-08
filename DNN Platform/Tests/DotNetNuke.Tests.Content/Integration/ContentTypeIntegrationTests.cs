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
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Tests.Data;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;
using DataUtil = DotNetNuke.Tests.Data.DataUtil;

namespace DotNetNuke.Tests.Content.Integration
{
    [TestFixture]
    class ContentTypeIntegrationTests
    {
        private Mock<CachingProvider> _mockCache;

        private const string DatabaseName = "Test.sdf";
        private const string ConnectionStringName = "PetaPoco";
        private const string CreateContentTypeTableSql = @"
            CREATE TABLE ContentTypes(
	            ContentTypeID int IDENTITY(1,1) NOT NULL,
	            ContentType nvarchar(100) NOT NULL,
	            PortalID int NOT NULL,
	            IsStructured bit NOT NULL)";
        private const string CreateDataTypeTableSql = @"
            CREATE TABLE ContentTypes_DataTypes(
	            DataTypeID int IDENTITY(1,1) NOT NULL,
	            Name nvarchar(100) NOT NULL)";
        private const string CreateFieldDefinitionTableSql = @"
            CREATE TABLE ContentTypes_FieldDefinitions(
	            FieldDefinitionID int IDENTITY(1,1) NOT NULL,
                ContentTypeID int NOT NULL,
                DataTypeID int NOT NULL,
	            Name nvarchar(100) NOT NULL,
	            Label nvarchar(100) NOT NULL,
	            Description nvarchar(2000) NULL)";
        private const string InsertContentTypeSql = "INSERT INTO ContentTypes (ContentType, PortalID, IsStructured) VALUES ('{0}',{1}, {2})";
        private const string InsertDataTypeSql = "INSERT INTO ContentTypes_DataTypes (Name) VALUES ('{0}')";
        private const string InsertFieldDefinitionSql = @"INSERT INTO ContentTypes_FieldDefinitions 
                                                            (ContentTypeID, DataTypeID, Name, Label, Description) 
                                                            VALUES ({0}, {1}, '{2}', '{3}', '{4}')";
        private const int PortalId = 0;
        private const int RecordCount = 10;

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new SqlDataProvider());
            ComponentFactory.RegisterComponentSettings<SqlDataProvider>(new Dictionary<string, string>()
                                {
                                    {"name", "SqlDataProvider"},
                                    {"type", "DotNetNuke.Data.SqlDataProvider, DotNetNuke"},
                                    {"connectionStringName", "SiteSqlServer"},
                                    {"objectQualifier", ""},
                                    {"databaseOwner", "dbo."}
                                });

            _mockCache = MockComponentProvider.CreateNew<CachingProvider>();

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString("PerformanceSetting")).Returns("3");
            HostController.RegisterInstance(mockHostController.Object);

            var mockLogController = new Mock<ILogController>();
            LogController.SetTestableInstance(mockLogController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            DataUtil.DeleteDatabase(DatabaseName);
            LogController.ClearInstance();
        }

        [Test]
        public void ContentTypeController_AddContentType_Inserts_New_Record_In_Database()
        {
            //Arrange
            SetUpContentTypes(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);
            var contentType = new ContentType() {ContentType = "New_Type"};

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes");

            Assert.AreEqual(RecordCount + 1, actualCount);
        }

        [Test]
        public void ContentTypeController_AddDataType_Inserts_New_Record_In_Database()
        {
            //Arrange
            SetUpDataTypes(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);
            var dataType = new ContentTypeDataType() { Name = "New_Type" };

            //Act
            contentTypeController.AddDataType(dataType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_DataTypes");

            Assert.AreEqual(RecordCount + 1, actualCount);
        }

        [Test]
        public void ContentTypeController_AddFieldDefinition_Inserts_New_Record_In_Database()
        {
            //Arrange
            SetUpFieldDefinitions(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);
            var definition = new ContentTypeFieldDefinition
                                {
                                    ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                    DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                    Name = "New_Type",
                                    Label = "Label"
                                };

            //Act
            contentTypeController.AddFieldDefinition(definition);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_FieldDefinitions");

            Assert.AreEqual(RecordCount + 1, actualCount);
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Deletes_Record_From_Database()
        {
            //Arrange
            var typeId = 2;
            SetUpContentTypes(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);
            var contentType = new ContentType() { ContentTypeId = typeId, ContentType = "Type_2" };

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes");

            Assert.AreEqual(RecordCount - 1, actualCount);
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Deletes_Correct_Record_From_Database()
        {
            //Arrange
            var typeId = 2;
            SetUpContentTypes(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);
            var contentType = new ContentType() { ContentTypeId = typeId, ContentType = "Type_2" };

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            DataAssert.RecordWithIdNotPresent(DatabaseName, "ContentTYpes", "ContentTypeId", typeId);
        }

        [Test]
        public void ContentTypeController_DeleteDataType_Deletes_Record_From_Database()
        {
            //Arrange
            var dataTypeId = 6;
            SetUpDataTypes(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);
            var dataType = new ContentTypeDataType() { DataTypeId = dataTypeId, Name = "New_Type" };

            //Act
            contentTypeController.DeleteDataType(dataType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_DataTypes");

            Assert.AreEqual(RecordCount - 1, actualCount);
        }

        [Test]
        public void ContentTypeController_DeleteDataType_Deletes_Correct_Record_From_Database()
        {
            //Arrange
            var dataTypeId = 6;
            SetUpDataTypes(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);
            var dataType = new ContentTypeDataType() { DataTypeId = dataTypeId, Name = "New_Type" };

            //Act
            contentTypeController.DeleteDataType(dataType);

            //Assert
            DataAssert.RecordWithIdNotPresent(DatabaseName, "ContentTypes_DataTypes", "DataTypeId", dataTypeId);
        }

        [Test]
        public void ContentTypeController_DeleteFieldDefinition_Deletes_Record_From_Database()
        {
            //Arrange
            var definitionId = 4;
            SetUpFieldDefinitions(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);
            var definition = new ContentTypeFieldDefinition
                                    {
                                        FieldDefinitionId = definitionId
                                    };

            //Act
            contentTypeController.DeleteFieldDefinition(definition);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_FieldDefinitions");

            Assert.AreEqual(RecordCount - 1, actualCount);
        }

        [Test]
        public void ContentTypeController_DeleteFieldDefinition_Deletes_Correct_Record_From_Database()
        {
            //Arrange
            var definitionId = 4;
            SetUpFieldDefinitions(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);
            var definition = new ContentTypeFieldDefinition
                                    {
                                        FieldDefinitionId = definitionId
                                    };

            //Act
            contentTypeController.DeleteFieldDefinition(definition);

            //Assert
            DataAssert.RecordWithIdNotPresent(DatabaseName, "ContentTypes_FieldDefinitions", "FieldDefinitionId", definitionId);
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Returns_All_Records_From_Database()
        {
            //Arrange
            SetUpContentTypes(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);

            //Act
            var contentTypes = contentTypeController.GetContentTypes();

            //Assert
            Assert.AreEqual(RecordCount, contentTypes.Count());
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Overload_Returns_Records_For_Portal_From_Database()
        {
            //Arrange
            SetUpContentTypes(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);

            //Act
            var contentTypes = contentTypeController.GetContentTypes(PortalId);

            //Assert
            Assert.AreEqual(RecordCount/2, contentTypes.Count());
            foreach (var contentType in contentTypes)
            {
                Assert.AreEqual(PortalId, contentType.PortalID);
            }
        }

        [TestCase(20, 0, 5, 2)]
        [TestCase(20, 1, 5, 2)]
        [TestCase(30, 2, 5, 3)]
        public void ContentTypeController_GetContentTypes_Overload_Returns_PagedList_For_Portal_From_Database(int recordCount, int pageIndex, int pageSize, int pageCount)
        {
            //Arrange
            SetUpContentTypes(recordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);

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
                Assert.AreEqual(PortalId, contentType.PortalID);
            }
        }

        [Test]
        public void ContentTypeController_GetStructuredContentTypes_Returns_Records_For_Portal_From_Database()
        {
            //Arrange
            SetUpContentTypes(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);

            //Act
            var contentTypes = contentTypeController.GetStructuredContentTypes(PortalId);

            //Assert
            Assert.AreEqual(RecordCount / 2, contentTypes.Count());
            foreach (var contentType in contentTypes)
            {
                Assert.AreEqual(true, contentType.IsStructured);
            }
        }

        [TestCase(20, 0, 5, 2)]
        [TestCase(20, 1, 5, 2)]
        [TestCase(30, 2, 5, 3)]
        public void ContentTypeController_GetStructuredContentTypess_Overload_Returns_PagedList_For_Portal_From_Database(int recordCount, int pageIndex, int pageSize, int pageCount)
        {
            //Arrange
            SetUpContentTypes(recordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);

            //Act
            var contentTypes = contentTypeController.GetStructuredContentTypes(PortalId, pageIndex, pageSize);

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
                Assert.AreEqual(true, contentType.IsStructured);
            }
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Updates_Correct_Record_In_Database()
        {
            //Arrange
            var contentTypeId = 2;
            SetUpContentTypes(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);
            var contentType = new ContentType() { ContentTypeId = contentTypeId, ContentType = "NewType" };

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes");
            Assert.AreEqual(RecordCount, actualCount);

            DataAssert.IsFieldValueEqual<string>("NewType", DatabaseName, "ContentTypes", "ContentType", "ContentTypeId", contentTypeId);
        }

        [Test]
        public void ContentTypeController_UpdateDataType_Updates_Correct_Record_In_Database()
        {
            //Arrange
            var dataTypeId = 2;
            SetUpDataTypes(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var contentTypeController = new ContentTypeController(dataContext);
            var dataType = new ContentTypeDataType() { DataTypeId = dataTypeId, Name = "NewType" };

            //Act
            contentTypeController.UpdateDataType(dataType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_DataTypes");
            Assert.AreEqual(RecordCount, actualCount);

            DataAssert.IsFieldValueEqual<string>("NewType", DatabaseName, "ContentTypes_DataTypes", "Name", "DataTypeId", dataTypeId);
        }

        private void SetUpContentTypes(int count)
        {
            DataUtil.CreateDatabase(DatabaseName);
            DataUtil.ExecuteNonQuery(DatabaseName, CreateContentTypeTableSql);

            for (int i = 0; i < count; i++)
            {
                int isStructured = 0;
                int portalId = -1;
                if(i % 2 == 0)
                {
                    isStructured = 1;
                    portalId = PortalId;
                }
                DataUtil.ExecuteNonQuery(DatabaseName, String.Format(InsertContentTypeSql, String.Format("Type_{0}", i), portalId, isStructured));
            }
        }

        private void SetUpDataTypes(int count)
        {
            DataUtil.CreateDatabase(DatabaseName);
            DataUtil.ExecuteNonQuery(DatabaseName, CreateDataTypeTableSql);

            for (int i = 0; i < count; i++)
            {
                DataUtil.ExecuteNonQuery(DatabaseName, String.Format(InsertDataTypeSql, String.Format("Type_{0}", i)));
            }
        }

        private void SetUpFieldDefinitions(int count)
        {
            DataUtil.CreateDatabase(DatabaseName);
            DataUtil.ExecuteNonQuery(DatabaseName, CreateFieldDefinitionTableSql);

            for (int i = 0; i < count; i++)
            {
                DataUtil.ExecuteNonQuery(DatabaseName, String.Format(InsertFieldDefinitionSql, i, i,  String.Format("Name_{0}", i), String.Format("Label_{0}", i), String.Format("Description_{0}", i)));
            }
        }

    }
}
