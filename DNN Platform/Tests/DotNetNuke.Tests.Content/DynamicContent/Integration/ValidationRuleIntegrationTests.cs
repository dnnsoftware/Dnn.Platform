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

using System.Linq;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Content.DynamicContent;
using DotNetNuke.Tests.Content.Integration;
using DotNetNuke.Tests.Data;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;

// ReSharper disable UseStringInterpolation

namespace DotNetNuke.Tests.Content.DynamicContent.Integration
{
    [TestFixture]
    public class ValidationRuleIntegrationTests : IntegrationTestBase
    {
        private const string CreateValidationRuleTableSql = @"
            CREATE TABLE ContentTypes_ValidationRules(
	            ValidationRuleID int IDENTITY(1,1) NOT NULL,
                FieldDefinitionID int NOT NULL,
                ValidatorTypeID int NOT NULL)";

        private const string InsertValidationRuleSql = @"INSERT INTO ContentTypes_ValidationRules 
                                                            (FieldDefinitionID, ValidatorTypeID) 
                                                            VALUES ({0}, {1})";

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
        public void AddValidationRule_Inserts_New_Record_In_Database()
        {
            //Arrange
            SetUpValidationRules(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var validationRuleController = new ValidationRuleController(dataContext);
            var validationRule = new ValidationRule
                                {
                                    FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                    ValidatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId
                                };

            //Act
            validationRuleController.AddValidationRule(validationRule);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_ValidationRules");

            Assert.AreEqual(RecordCount + 1, actualCount);
        }

        [Test]
        public void DeleteValidationRule_Deletes_Record_From_Database()
        {
            //Arrange
            var validationRuleId = 4;
            SetUpValidationRules(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var validationRuleController = new ValidationRuleController(dataContext);
            var validationRule = new ValidationRule
                                    {
                                        ValidationRuleId = validationRuleId,
                                        FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                        ValidatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId
                                    };

            //Act
            validationRuleController.DeleteValidationRule(validationRule);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_ValidationRules");

            Assert.AreEqual(RecordCount - 1, actualCount);
        }

        [Test]
        public void DeleteValidationRule_Deletes_Correct_Record_From_Database()
        {
            //Arrange
            var validationRuleId = 4;
            SetUpValidationRules(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var validationRuleController = new ValidationRuleController(dataContext);
            var validationRule = new ValidationRule
                                    {
                                        ValidationRuleId = validationRuleId,
                                        FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                        ValidatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId
                                    };

            //Act
            validationRuleController.DeleteValidationRule(validationRule);

            //Assert
            DataAssert.RecordWithIdNotPresent(DatabaseName, "ContentTypes_ValidationRules", "ValidationRuleId", validationRuleId);
        }

        [Test]
        public void GetValidationRules_Overload_Returns_Records_For_FieldDefinition_From_Database()
        {
            //Arrange
            var fieldDefinitionId = 5;
            SetUpValidationRules(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var validationRuleController = new ValidationRuleController(dataContext);

            //Act
            var validationRules = validationRuleController.GetValidationRules(fieldDefinitionId);

            //Assert
            Assert.AreEqual(1, validationRules.Count());
            foreach (var validationRule in validationRules)
            {
                Assert.AreEqual(fieldDefinitionId, validationRule.FieldDefinitionId);
            }
        }

        [Test]
        public void UpdateValidationRule_Updates_Correct_Record_In_Database()
        {
            //Arrange
            var validationRuleId = 4;
            SetUpValidationRules(RecordCount);
            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var validationRuleController = new ValidationRuleController(dataContext);
            var validationRule = new ValidationRule
                                        {
                                            ValidationRuleId = validationRuleId,
                                            FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                            ValidatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId
                                        };

            //Act
            validationRuleController.UpdateValidationRule(validationRule);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_ValidationRules");
            Assert.AreEqual(RecordCount, actualCount);

            DataAssert.IsFieldValueEqual(Constants.CONTENTTYPE_ValidFieldDefinitionId, DatabaseName, "ContentTypes_ValidationRules", "FieldDefinitionID", "ValidationRuleId", validationRuleId);
        }

        private void SetUpValidationRules(int count)
        {
            DataUtil.CreateDatabase(DatabaseName);
            DataUtil.ExecuteNonQuery(DatabaseName, CreateValidationRuleTableSql);

            for (int i = 0; i < count; i++)
            {
                DataUtil.ExecuteNonQuery(DatabaseName, string.Format(InsertValidationRuleSql, i, i));
            }
        }
    }
}