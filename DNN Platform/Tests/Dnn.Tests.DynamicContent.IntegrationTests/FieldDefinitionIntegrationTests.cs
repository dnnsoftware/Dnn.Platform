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
using Dnn.DynamicContent;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Data;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;

// ReSharper disable UseStringInterpolation

namespace Dnn.Tests.DynamicContent.IntegrationTests
{
    [TestFixture]
    public class FieldDefinitionIntegrationTests : IntegrationTestBase
    {
        private readonly string _cacheKey = CachingProvider.GetCacheKey(FieldDefinitionManager.FieldDefinitionCacheKey);

        [SetUp]
        public void SetUp()
        {
            SetUpInternal();
            SetUpValidationRules(RecordCount);
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
            var fieldDefinitionController = new FieldDefinitionManager();
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
        public void AddFieldDefinition_Clears_Cache()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            SetUpFieldDefinitions(RecordCount);
            var fieldDefinitionController = new FieldDefinitionManager();
            var definition = new FieldDefinition
            {
                ContentTypeId = contentTypeId,
                DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                Name = "New_Type",
                Label = "Label"
            };

            //Act
            fieldDefinitionController.AddFieldDefinition(definition);

            //Assert
            MockCache.Verify(c =>c.Remove(GetCacheKey(contentTypeId)));
        }

        [Test]
        public void DeleteFieldDefinition_Deletes_Record_From_Database()
        {
            //Arrange
            var definitionId = 4;
            SetUpFieldDefinitions(RecordCount);
            var fieldDefinitionController = new FieldDefinitionManager();
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
            var fieldDefinitionController = new FieldDefinitionManager();
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
        public void DeleteFieldDefinition_Clears_Cache()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var definitionId = 4;
            SetUpFieldDefinitions(RecordCount);
            var fieldDefinitionController = new FieldDefinitionManager();
            var definition = new FieldDefinition
            {
                FieldDefinitionId = definitionId,
                ContentTypeId = contentTypeId,
            };

            //Act
            fieldDefinitionController.DeleteFieldDefinition(definition);

            //Assert
            MockCache.Verify(c => c.Remove(GetCacheKey(contentTypeId)));
        }

        [Test]
        public void GetFieldDefinitions_Returns_Records_For_ContentType_From_Database_If_Cache_Is_Null()
        {
            //Arrange
            var contentTypeId = 5;
            MockCache.Setup(c => c.GetItem(GetCacheKey(contentTypeId))).Returns(null);
            SetUpFieldDefinitions(RecordCount);
            var fieldDefinitionController = new FieldDefinitionManager();

            //Act
            var fields = fieldDefinitionController.GetFieldDefinitions(contentTypeId);

            //Assert
            Assert.AreEqual(1, fields.Count());
            foreach (var field in fields)
            {
                Assert.AreEqual(contentTypeId, field.ContentTypeId);
            }
        }

        [Test]
        public void GetFieldDefinitions_Returns_Records_From_Cache_If_Not_Null()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var cacheCount = 15;
            MockCache.Setup(c => c.GetItem(GetCacheKey(contentTypeId))).Returns(SetUpCache(cacheCount));
            SetUpFieldDefinitions(RecordCount);
            var fieldDefinitionController = new FieldDefinitionManager();

            //Act
            var fields = fieldDefinitionController.GetFieldDefinitions(contentTypeId);

            //Assert
            Assert.AreEqual(cacheCount, fields.Count());
            foreach (var field in fields)
            {
                Assert.AreEqual(contentTypeId, field.ContentTypeId);
            }
        }

        [Test]
        public void UpdateFieldDefinition_Updates_Correct_Record_In_Database()
        {
            //Arrange
            var definitionId = 4;
            SetUpFieldDefinitions(RecordCount);
            var fieldDefinitionController = new FieldDefinitionManager();
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

        [Test]
        public void UpdateFieldDefinition_Clears_Cache()
        {
            //Arrange
            var definitionId = 4;
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            SetUpFieldDefinitions(RecordCount);
            var fieldDefinitionController = new FieldDefinitionManager();
            var field = new FieldDefinition
            {
                FieldDefinitionId = definitionId,
                ContentTypeId = contentTypeId,
                DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                Name = "New_Definition",
                Label = "Label"
            };

            //Act
            fieldDefinitionController.UpdateFieldDefinition(field);

            //Assert
            MockCache.Verify(c => c.Remove(GetCacheKey(contentTypeId)));
        }

        private string GetCacheKey(int contentTypeId)
        {
            return String.Format("{0}_{1}_{2}", _cacheKey, FieldDefinitionManager.FieldDefinitionScope, contentTypeId);
        }

        private IQueryable<FieldDefinition> SetUpCache(int count)
        {
            var list = new List<FieldDefinition>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new FieldDefinition { FieldDefinitionId = i, ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId, DataTypeId = i, Name = String.Format("Type_{0}", i) });
            }
            return list.AsQueryable();
        }
    }
}
