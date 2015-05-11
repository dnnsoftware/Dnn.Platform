// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Data;
using DotNetNuke.Tests.Utilities;
using Moq;
using NUnit.Framework;

// ReSharper disable UseStringInterpolation

namespace Dnn.Tests.DynamicContent.IntegrationTests
{
    [TestFixture]
    public class ValidationRuleIntegrationTests : IntegrationTestBase
    {
        private readonly string _cacheKey = CachingProvider.GetCacheKey(ValidationRuleManager.ValidationRuleCacheKey);

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
            var mockValidationRuleController = new Mock<IValidationRuleManager>();
            mockValidationRuleController.Setup(vr => vr.GetValidationSettings(It.IsAny<int>()))
                                    .Returns(new Dictionary<string, ValidatorSetting>());
            ValidationRuleManager.SetTestableInstance(mockValidationRuleController.Object);

            SetUpValidationRules(RecordCount);

            var validationRuleController = new ValidationRuleManager();
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
        public void AddValidationRule_Clears_Cache()
        {
            //Arrange
            var mockValidationRuleController = new Mock<IValidationRuleManager>();
            mockValidationRuleController.Setup(vr => vr.GetValidationSettings(It.IsAny<int>()))
                                    .Returns(new Dictionary<string, ValidatorSetting>());
            ValidationRuleManager.SetTestableInstance(mockValidationRuleController.Object);

            var fieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId;
            SetUpValidationRules(RecordCount);

            var validationRuleController = new ValidationRuleManager();
            var validationRule = new ValidationRule
            {
                FieldDefinitionId = fieldDefinitionId,
                ValidatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId
            };

            //Act
            validationRuleController.AddValidationRule(validationRule);

            //Assert
            MockCache.Verify(c => c.Remove(GetCacheKey(fieldDefinitionId)));
        }

        [Test]
        public void DeleteValidationRule_Deletes_Record_From_Database()
        {
            //Arrange
            var mockValidationRuleController = new Mock<IValidationRuleManager>();
            mockValidationRuleController.Setup(vr => vr.GetValidationSettings(It.IsAny<int>()))
                                    .Returns(new Dictionary<string, ValidatorSetting>());
            ValidationRuleManager.SetTestableInstance(mockValidationRuleController.Object);


            var validationRuleId = 4;
            SetUpValidationRules(RecordCount);

            var validationRuleController = new ValidationRuleManager();
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
            var mockValidationRuleController = new Mock<IValidationRuleManager>();
            mockValidationRuleController.Setup(vr => vr.GetValidationSettings(It.IsAny<int>()))
                                    .Returns(new Dictionary<string, ValidatorSetting>());
            ValidationRuleManager.SetTestableInstance(mockValidationRuleController.Object);

            var validationRuleId = 4;
            SetUpValidationRules(RecordCount);

            var validationRuleController = new ValidationRuleManager();
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
        public void DeleteValidationRule_Clears_Cache()
        {
            //Arrange
            var mockValidationRuleController = new Mock<IValidationRuleManager>();
            mockValidationRuleController.Setup(vr => vr.GetValidationSettings(It.IsAny<int>()))
                                    .Returns(new Dictionary<string, ValidatorSetting>());
            ValidationRuleManager.SetTestableInstance(mockValidationRuleController.Object);

            var fieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId;
            var validationRuleId = 4;
            SetUpValidationRules(RecordCount);

            var validationRuleController = new ValidationRuleManager();
            var validationRule = new ValidationRule
            {
                ValidationRuleId = validationRuleId,
                FieldDefinitionId = fieldDefinitionId,
                ValidatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId
            };

            //Act
            validationRuleController.DeleteValidationRule(validationRule);

            //Assert
            MockCache.Verify(c => c.Remove(GetCacheKey(fieldDefinitionId)));
        }

        [Test]
        public void GetValidationRules_Returns_Records_For_FieldDefinition_From_Database_If_Cache_Is_Null()
        {
            //Arrange
            var fieldDefinitionId = 5;
            MockCache.Setup(c => c.GetItem(GetCacheKey(fieldDefinitionId))).Returns(null);
            SetUpValidationRules(RecordCount);

            var validationRuleController = new ValidationRuleManager();

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
        public void GetValidationRules_Returns_Records_From_Cache_If_Not_Null()
        {
            //Arrange
            var fieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId;
            var cacheCount = 15;
            MockCache.Setup(c => c.GetItem(GetCacheKey(fieldDefinitionId))).Returns(SetUpCache(cacheCount));
            SetUpValidationRules(RecordCount);

            var validationRuleController = new ValidationRuleManager();

            //Act
            var validationRules = validationRuleController.GetValidationRules(fieldDefinitionId);

            //Assert
            Assert.AreEqual(cacheCount, validationRules.Count());
            foreach (var validationRule in validationRules)
            {
                Assert.AreEqual(fieldDefinitionId, validationRule.FieldDefinitionId);
            }
        }

        [Test]
        public void UpdateValidationRule_Updates_Correct_Record_In_Database()
        {
            //Arrange
            var mockValidationRuleController = new Mock<IValidationRuleManager>();
            mockValidationRuleController.Setup(vr => vr.GetValidationSettings(It.IsAny<int>()))
                                    .Returns(new Dictionary<string, ValidatorSetting>());
            ValidationRuleManager.SetTestableInstance(mockValidationRuleController.Object);

            var validationRuleId = 4;
            SetUpValidationRules(RecordCount);

            var validationRuleController = new ValidationRuleManager();
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

        [Test]
        public void UpdateValidationRule_Clears_Cache()
        {
            //Arrange
            var mockValidationRuleController = new Mock<IValidationRuleManager>();
            mockValidationRuleController.Setup(vr => vr.GetValidationSettings(It.IsAny<int>()))
                                    .Returns(new Dictionary<string, ValidatorSetting>());
            ValidationRuleManager.SetTestableInstance(mockValidationRuleController.Object);

            var fieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId;
            var validationRuleId = 4;
            SetUpValidationRules(RecordCount);

            var validationRuleController = new ValidationRuleManager();
            var validationRule = new ValidationRule
            {
                ValidationRuleId = validationRuleId,
                FieldDefinitionId = fieldDefinitionId,
                ValidatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId
            };

            //Act
            validationRuleController.UpdateValidationRule(validationRule);

            //Assert
            MockCache.Verify(c => c.Remove(GetCacheKey(fieldDefinitionId)));
        }

        private string GetCacheKey(int fieldDefinitionId)
        {
            return String.Format("{0}_{1}_{2}", _cacheKey, ValidationRuleManager.ValidationRuleScope, fieldDefinitionId);
        }

        private IQueryable<ValidationRule> SetUpCache(int count)
        {
            var list = new List<ValidationRule>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new ValidationRule { FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId, ValidatorTypeId = i, ValidationRuleId = i});
            }
            return list.AsQueryable();
        }
    }
}