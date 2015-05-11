// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
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
    public class ValidatorTypeIntegrationTests : IntegrationTestBase
    {
        private readonly string _cacheKey = CachingProvider.GetCacheKey(ValidatorTypeManager.ValidatorTypeCacheKey);

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
        }

        [Test]
        public void AddValidatorType_Inserts_New_Record_In_Database()
        {
            //Arrange
            SetUpValidatorTypes(RecordCount);

            var validatorTypeController = new ValidatorTypeManager();
            var validatorType = new ValidatorType()
                                    {
                                        Name = Constants.CONTENTTYPE_ValidValidatorTypeName,
                                        ValidatorClassName = Constants.CONTENTTYPE_ValidValidatorClassName,
                                        ErrorKey = Constants.CONTENTTYPE_ValidValidatorErrorKey,
                                        ErrorMessage = Constants.CONTENTTYPE_ValidValidatorErrorMessage
                                    };

            //Act
            validatorTypeController.AddValidatorType(validatorType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_ValidatorTypes");

            Assert.AreEqual(RecordCount + 1, actualCount);
        }

        [Test]
        public void AddValidatorType_Clears_Cache()
        {
            //Arrange
            SetUpValidatorTypes(RecordCount);

            var validatorTypeController = new ValidatorTypeManager();
            var validatorType = new ValidatorType()
                                    {
                                        Name = Constants.CONTENTTYPE_ValidValidatorTypeName,
                                        ValidatorClassName = Constants.CONTENTTYPE_ValidValidatorClassName,
                                        ErrorKey = Constants.CONTENTTYPE_ValidValidatorErrorKey,
                                        ErrorMessage = Constants.CONTENTTYPE_ValidValidatorErrorMessage
                                    };

            //Act
            validatorTypeController.AddValidatorType(validatorType);

            //Assert
            MockCache.Verify(c => c.Remove(_cacheKey));
        }

        [Test]
        public void DeleteValidatorType_Deletes_Record_From_Database_If_Valid()
        {
            //Arrange
            var validatorTypeId = 6;
            SetUpValidatorTypes(RecordCount);

            var dataContext = new PetaPocoDataContext(ConnectionStringName);
            var validatorTypeController = new ValidatorTypeManager(dataContext);
            var validatorType = new ValidatorType() { ValidatorTypeId = validatorTypeId, Name = "New_Type" };

            //Act
            validatorTypeController.DeleteValidatorType(validatorType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_ValidatorTypes");

            Assert.AreEqual(RecordCount - 1, actualCount);
        }

        [Test]
        public void DeleteValidatorType_Clears_Cache()
        {
            //Arrange
            var validatorTypeId = 6;
            SetUpValidatorTypes(RecordCount);

            var validatorTypeController = new ValidatorTypeManager();
            var validatorType = new ValidatorType() { ValidatorTypeId = validatorTypeId, Name = "New_Type" };

            //Act
            validatorTypeController.DeleteValidatorType(validatorType);

            //Assert
            MockCache.Verify(c => c.Remove(_cacheKey));
        }

        [Test]
        public void GetValidatorTypes_Fetches_Records_From_Database_If_Cache_Is_Null()
        {
            //Arrange
            MockCache.Setup(c => c.GetItem(_cacheKey)).Returns(null);
            SetUpValidatorTypes(RecordCount);

            var validatorTypeController = new ValidatorTypeManager();

            //Act
            var validatorTypes = validatorTypeController.GetValidatorTypes();

            //Assert
            Assert.AreEqual(RecordCount, validatorTypes.Count());
        }

        [Test]
        public void GetValidatorTypes_Fetches_Records_From_Cache_If_Not_Null()
        {
            //Arrange
            var cacheCount = 15;
            MockCache.Setup(c => c.GetItem(_cacheKey)).Returns(SetUpCache(cacheCount));

            SetUpValidatorTypes(RecordCount);

            var validatorTypeController = new ValidatorTypeManager();

            //Act
            var validatorTypes = validatorTypeController.GetValidatorTypes();

            //Assert
            Assert.AreEqual(cacheCount, validatorTypes.Count());
        }

        [Test]
        public void UpdateValidatorType_Updates_Correct_Record_In_Database()
        {
            //Arrange
            var validatorTypeId = 2;
            SetUpValidatorTypes(RecordCount);

            var validatorTypeController = new ValidatorTypeManager();
            var validatorType = new ValidatorType
                                    {
                                        ValidatorTypeId = validatorTypeId,
                                        Name = Constants.CONTENTTYPE_ValidValidatorTypeName,
                                        ValidatorClassName = Constants.CONTENTTYPE_ValidValidatorClassName,
                                        ErrorKey = Constants.CONTENTTYPE_ValidValidatorErrorKey,
                                        ErrorMessage = Constants.CONTENTTYPE_ValidValidatorErrorMessage
                                    };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);
            mockContentController.Setup(c => c.GetContentItemsByContentType(Constants.CONTENTTYPE_ValidContentTypeId))
                    .Returns(new List<ContentItem>().AsQueryable());

            //Act
            validatorTypeController.UpdateValidatorType(validatorType);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_ValidatorTypes");
            Assert.AreEqual(RecordCount, actualCount);

            DataAssert.IsFieldValueEqual(Constants.CONTENTTYPE_ValidValidatorTypeName, DatabaseName, "ContentTypes_ValidatorTypes", "Name", "ValidatorTypeId", validatorTypeId);
        }

        [Test]
        public void UpdateValidatorType_Clears_Cache()
        {
            //Arrange
            var validatorTypeId = 2;
            SetUpValidatorTypes(RecordCount);

            var validatorTypeController = new ValidatorTypeManager();
            var validatorType = new ValidatorType
                                    {
                                        ValidatorTypeId = validatorTypeId,
                                        Name = Constants.CONTENTTYPE_ValidValidatorTypeName,
                                        ValidatorClassName = Constants.CONTENTTYPE_ValidValidatorClassName,
                                        ErrorKey = Constants.CONTENTTYPE_ValidValidatorErrorKey,
                                        ErrorMessage = Constants.CONTENTTYPE_ValidValidatorErrorMessage
                                    };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);
            mockContentController.Setup(c => c.GetContentItemsByContentType(Constants.CONTENTTYPE_ValidContentTypeId))
                    .Returns(new List<ContentItem>().AsQueryable());

            //Act
            validatorTypeController.UpdateValidatorType(validatorType);

            //Assert
            MockCache.Verify(c => c.Remove(_cacheKey));
        }

        private IQueryable<ValidatorType> SetUpCache(int count)
        {
            var list = new List<ValidatorType>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new ValidatorType { ValidatorTypeId = i, Name = String.Format("Type_{0}", i) });
            }
            return list.AsQueryable();
        }
    }
}
