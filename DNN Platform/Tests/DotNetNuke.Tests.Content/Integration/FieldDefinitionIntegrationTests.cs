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
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Content.DynamicContent;
using DotNetNuke.Tests.Data;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;
// ReSharper disable UseStringInterpolation

namespace DotNetNuke.Tests.Content.Integration
{
    [TestFixture]
    public class FieldDefinitionIntegrationTests : IntegrationTestBase
    {
        private const string CreateFieldDefinitionTableSql = @"
            CREATE TABLE ContentTypes_FieldDefinitions(
	            FieldDefinitionID int IDENTITY(1,1) NOT NULL,
                ContentTypeID int NOT NULL,
                DataTypeID int NOT NULL,
	            Name nvarchar(100) NOT NULL,
	            Label nvarchar(100) NOT NULL,
	            Description nvarchar(2000) NULL)";

        private const string InsertFieldDefinitionSql = @"INSERT INTO ContentTypes_FieldDefinitions 
                                                            (ContentTypeID, DataTypeID, Name, Label, Description) 
                                                            VALUES ({0}, {1}, '{2}', '{3}', '{4}')";

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
        public void AddFieldDefinition_Inserts_New_Record_In_Database()
        {
            //Arrange
            SetUpFieldDefinitions(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var fieldDefinitionController = new FieldDefinitionController(dataContext);
            var definition = new FieldDefinition
            {
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                Name = "New_Type",
                Label = "Label"
            };

            //Act
            fieldDefinitionController.AddFieldDefinition(definition);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_FieldDefinitions");

            Assert.AreEqual(RecordCount + 1, actualCount);
        }

        [Test]
        public void DeleteFieldDefinition_Deletes_Record_From_Database()
        {
            //Arrange
            var definitionId = 4;
            SetUpFieldDefinitions(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var fieldDefinitionController = new FieldDefinitionController(dataContext);
            var definition = new FieldDefinition
            {
                FieldDefinitionId = definitionId
            };

            //Act
            fieldDefinitionController.DeleteFieldDefinition(definition);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_FieldDefinitions");

            Assert.AreEqual(RecordCount - 1, actualCount);
        }

        [Test]
        public void DeleteFieldDefinition_Deletes_Correct_Record_From_Database()
        {
            //Arrange
            var definitionId = 4;
            SetUpFieldDefinitions(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var fieldDefinitionController = new FieldDefinitionController(dataContext);
            var definition = new FieldDefinition
            {
                FieldDefinitionId = definitionId
            };

            //Act
            fieldDefinitionController.DeleteFieldDefinition(definition);

            //Assert
            DataAssert.RecordWithIdNotPresent(DatabaseName, "ContentTypes_FieldDefinitions", "FieldDefinitionId", definitionId);
        }

        [Test]
        public void UpdateFieldDefinition_Updates_Correct_Record_In_Database()
        {
            //Arrange
            var definitionId = 4;
            SetUpFieldDefinitions(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var fieldDefinitionController = new FieldDefinitionController(dataContext);
            var field = new FieldDefinition
                            {
                                FieldDefinitionId = definitionId,
                                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                Name = "New_Definition",
                                Label = "Label"
                            };

            //Act
            fieldDefinitionController.UpdateFieldDefinition(field);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_FieldDefinitions");
            Assert.AreEqual(RecordCount, actualCount);

            DataAssert.IsFieldValueEqual("New_Definition", DatabaseName, "ContentTypes_FieldDefinitions", "Name", "FieldDefinitionId", definitionId);
        }



        private void SetUpFieldDefinitions(int count)
        {
            DataUtil.CreateDatabase(DatabaseName);
            DataUtil.ExecuteNonQuery(DatabaseName, CreateFieldDefinitionTableSql);

            for (int i = 0; i < count; i++)
            {
                DataUtil.ExecuteNonQuery(DatabaseName, string.Format(InsertFieldDefinitionSql, i, i, string.Format("Name_{0}", i), string.Format("Label_{0}", i), String.Format("Description_{0}", i)));
            }
        }
    }
}
