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
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.DynamicContent;
using DotNetNuke.Tests.Data;
using DotNetNuke.Tests.Utilities;
using Moq;
using NUnit.Framework;
// ReSharper disable UseStringInterpolation
// ReSharper disable BuiltInTypeReferenceStyle

namespace DotNetNuke.Tests.Content.Integration
{
    [TestFixture]
    public class DataTypeIntegrationTests : IntegrationTestBase
    {
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

        private const string InsertDataTypeSql = "INSERT INTO ContentTypes_DataTypes (Name) VALUES ('{0}')";

        private const string InsertFieldDefinitionSql = @"INSERT INTO ContentTypes_FieldDefinitions 
                                                            (ContentTypeID, DataTypeID, Name, Label, Description) 
                                                            VALUES ({0}, {1}, '{2}', '{3}', '{4}')";
        [SetUp]
        public void SetUp()
        {
            SetUpInternal();

            DataUtil.CreateDatabase(DatabaseName);
        }

        [TearDown]
        public void TearDown()
        {
            TearDownInternal();
            ContentController.ClearInstance();
        }

        [Test]
        public void AddDataType_Inserts_New_Record_In_Database()
        {
            //Arrange
            SetUpDataTypes(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var dataTypeController = new DataTypeController(dataContext);
            var dataType = new DataType() { Name = "New_Type" };

            //Act
            dataTypeController.AddDataType(dataType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_DataTypes");

            Assert.AreEqual(RecordCount + 1, actualCount);
        }

        [Test]
        public void DeleteDataType_Deletes_Record_From_Database_If_Not_In_Use()
        {
            //Arrange
            var dataTypeId = 6;
            SetUpDataTypes(RecordCount);
            SetUpFieldDefinitions(5);

            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var dataTypeController = new DataTypeController(dataContext);
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
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var dataTypeController = new DataTypeController(dataContext);
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
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var dataTypeController = new DataTypeController(dataContext);
            var dataType = new DataType() { DataTypeId = dataTypeId, Name = "New_Type" };

            //Act, Assert
            Assert.Throws<InvalidOperationException>(() => dataTypeController.DeleteDataType(dataType));
        }

        [Test]
        public void UpdateDataType_Updates_Correct_Record_In_Database()
        {
            //Arrange
            var dataTypeId = 2;
            SetUpDataTypes(RecordCount);
            SetUpFieldDefinitions(5);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var dataTypeController = new DataTypeController(dataContext);
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
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var dataTypeController = new DataTypeController(dataContext);
            var dataType = new DataType() { DataTypeId = dataTypeId, Name = "NewType" };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);
            mockContentController.Setup(c => c.GetContentItemsByContentType(dataTypeId))
                .Returns(new List<ContentItem>() { new ContentItem() }.AsQueryable());

            //Act, Assert
            Assert.Throws<InvalidOperationException>(() => dataTypeController.UpdateDataType(dataType));
        }

        [Test]
        public void UpdateDataType__Updates_Correct_Record_In_Database_If_DataType_Used_And_OverrideFlag_Set()
        {
            //Arrange
            var dataTypeId = 2;
            SetUpDataTypes(RecordCount);
            SetUpFieldDefinitions(5);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var dataTypeController = new DataTypeController(dataContext);
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

        private void SetUpDataTypes(int count)
        {
            DataUtil.ExecuteNonQuery(DatabaseName, CreateDataTypeTableSql);

            for (int i = 0; i < count; i++)
            {
                DataUtil.ExecuteNonQuery(DatabaseName, String.Format(InsertDataTypeSql, String.Format("Type_{0}", i)));
            }
        }

        private void SetUpFieldDefinitions(int count)
        {
            DataUtil.ExecuteNonQuery(DatabaseName, CreateFieldDefinitionTableSql);

            for (int i = 0; i < count; i++)
            {
                DataUtil.ExecuteNonQuery(DatabaseName, string.Format(InsertFieldDefinitionSql, i, i, string.Format("Name_{0}", i), string.Format("Label_{0}", i), String.Format("Description_{0}", i)));
            }
        }

    }
}
