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
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Content.Mocks;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;
// ReSharper disable UnusedVariable
// ReSharper disable UseStringInterpolation

namespace DotNetNuke.Tests.Content
{
    /// <summary>
    ///   Summary description for ContentTypeTests
    /// </summary>
    [TestFixture]
    public class ContentTypeControllerTests
    {
        private Mock<IDataContext> _mockDataContext;
        private Mock<IRepository<ContentType>> _mockContentTypeRepository;
        private Mock<IRepository<ContentTypeDataType>> _mockDataTypeRepository;
        private Mock<IRepository<ContentTypeFieldDefinition>> _mockFieldDefinitionRepository;
        private Mock<CachingProvider> _mockCache;
        private string _contentTypeCacheKey;

        [SetUp]
        public void SetUp()
        {
            //Register MockCachingProvider
            _mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(String.Empty);

            _contentTypeCacheKey = CachingProvider.GetCacheKey(DataCache.ContentTypesCacheKey);

            _mockDataContext = new Mock<IDataContext>();

            _mockContentTypeRepository = new Mock<IRepository<ContentType>>();
            _mockDataContext.Setup(dc => dc.GetRepository<ContentType>()).Returns(_mockContentTypeRepository.Object);

            _mockDataTypeRepository = new Mock<IRepository<ContentTypeDataType>>();
            _mockDataContext.Setup(dc => dc.GetRepository<ContentTypeDataType>()).Returns(_mockDataTypeRepository.Object);

            _mockFieldDefinitionRepository = new Mock<IRepository<ContentTypeFieldDefinition>>();
            _mockDataContext.Setup(dc => dc.GetRepository<ContentTypeFieldDefinition>()).Returns(_mockFieldDefinitionRepository.Object);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void ContentTypeController_Constructor_Throws_On_Null_DataContext()
        {
            IDataContext dataContent = null;

            //Arrange, Act, Arrange
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => new ContentTypeController(dataContent));
        }

        [Test]
        public void ContentTypeController_AddContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.AddContentType(null));
        }

        [Test]
        public void ContentTypeController_AddContentType_Throws_On_Empty_ContentType_Property()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTypeController.AddContentType(new ContentType()));
        }

        [Test]
        public void ContentTypeController_AddContentType_Calls_Repository_Insert_On_Valid_Arguments()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            ContentType contentType = ContentTestHelper.CreateValidContentType();

            //Act
            int contentTypeId = contentTypeController.AddContentType(contentType);

            //Assert
            _mockContentTypeRepository.Verify(rep => rep.Insert(contentType));
        }

        [Test]
        public void ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<ContentType>()))
                            .Callback((ContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var contentType = ContentTestHelper.CreateValidContentType();

            //Act
            int contentTypeId = contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTypeId, contentTypeId);
        }

        [Test]
        public void ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<ContentType>()))
                            .Callback((ContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var contentType = ContentTestHelper.CreateValidContentType();

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTypeId, contentType.ContentTypeId);
        }

        [Test]
        public void ContentTypeController_AddDataType_Throws_On_Null_DataType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.AddDataType(null));
        }

        [Test]
        public void ContentTypeController_AddDataType_Throws_On_Empty_Name_Property()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTypeController.AddDataType(new ContentTypeDataType()));
        }

        [Test]
        public void ContentTypeController_AddDataType_Calls_Repository_Insert_On_Valid_Arguments()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var dataType = new ContentTypeDataType() {Name = "DataType1"};

            //Act
            int dataTypeId = contentTypeController.AddDataType(dataType);

            //Assert
            _mockDataTypeRepository.Verify(rep => rep.Insert(dataType));
        }

        [Test]
        public void ContentTypeController_AddDataType_Returns_ValidId_On_Valid_DataType()
        {
            //Arrange
            _mockDataTypeRepository.Setup(r => r.Insert(It.IsAny<ContentTypeDataType>()))
                            .Callback((ContentTypeDataType dt) => dt.DataTypeId = Constants.CONTENTTYPE_AddDataTypeId);

            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var dataType = new ContentTypeDataType() { Name = "DataType1" };

            //Act
            int dataTypeId = contentTypeController.AddDataType(dataType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddDataTypeId, dataTypeId);
        }

        [Test]
        public void ContentTypeController_AddDataType_Sets_ValidId_On_Valid_DataType()
        {
            //Arrange
            _mockDataTypeRepository.Setup(r => r.Insert(It.IsAny<ContentTypeDataType>()))
                            .Callback((ContentTypeDataType dt) => dt.DataTypeId = Constants.CONTENTTYPE_AddDataTypeId);

            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var dataType = new ContentTypeDataType() { Name = "DataType1" };

            //Act
            contentTypeController.AddDataType(dataType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddDataTypeId, dataType.DataTypeId);
        }

        [Test]
        public void ContentTypeController_AddFieldDefinition_Throws_On_Null_FieldDefinition()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.AddFieldDefinition(null));
        }

        [Test]
        public void ContentTypeController_AddFieldDefinition_Throws_On_Negative_ContentTypeId_Property()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var definition = new ContentTypeFieldDefinition
                                {
                                    ContentTypeId = Constants.CONTENTTYPE_InValidContentTypeId,
                                    DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                    Name = "New_Type",
                                    Label = "Label"
                                };

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.AddFieldDefinition(definition));
        }

        [Test]
        public void ContentTypeController_AddFieldDefinition_Throws_On_Negative_DataTypeId_Property()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var definition = new ContentTypeFieldDefinition
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                        DataTypeId = Constants.CONTENTTYPE_InValidDataTypeId,
                                        Name = "New_Type",
                                        Label = "Label"
                                    };

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.AddFieldDefinition(definition));
        }

        [Test]
        public void ContentTypeController_AddFieldDefinition_Throws_On_Empty_Name_Property()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var definition = new ContentTypeFieldDefinition
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                        DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                        Name = string.Empty,
                                        Label = "Label"
                                    };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTypeController.AddFieldDefinition(definition));
        }

        [Test]
        public void ContentTypeController_AddFieldDefinition_Throws_On_Empty_Label_Property()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var definition = new ContentTypeFieldDefinition
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                        DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                        Name = "New_Type",
                                        Label = string.Empty
                                    };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTypeController.AddFieldDefinition(definition));
        }

        [Test]
        public void ContentTypeController_AddFieldDefinition_Calls_Repository_Insert_On_Valid_Arguments()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var definition = new ContentTypeFieldDefinition
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                        DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                        Name = "New_Type",
                                        Label = "Label"
                                    };

            //Act
            int fieldDefinitionId = contentTypeController.AddFieldDefinition(definition);

            //Assert
            _mockFieldDefinitionRepository.Verify(rep => rep.Insert(definition));
        }

        [Test]
        public void ContentTypeController_AddFieldDefinition_Returns_ValidId_On_Valid_FieldDefinition()
        {
            //Arrange
            _mockFieldDefinitionRepository.Setup(r => r.Insert(It.IsAny<ContentTypeFieldDefinition>()))
                            .Callback((ContentTypeFieldDefinition df) => df.FieldDefinitionId = Constants.CONTENTTYPE_AddFieldDefinitionId);

            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var definition = new ContentTypeFieldDefinition
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                        DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                        Name = "New_Type",
                                        Label = "Label"
                                    };

            //Act
            int fieldDefinitionId = contentTypeController.AddFieldDefinition(definition);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddFieldDefinitionId, fieldDefinitionId);
        }

        [Test]
        public void ContentTypeController_AddFieldDefinition_Sets_ValidId_On_Valid_FieldDefinition()
        {
            //Arrange
            _mockFieldDefinitionRepository.Setup(r => r.Insert(It.IsAny<ContentTypeFieldDefinition>()))
                            .Callback((ContentTypeFieldDefinition dt) => dt.FieldDefinitionId = Constants.CONTENTTYPE_AddFieldDefinitionId);

            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

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
            Assert.AreEqual(Constants.CONTENTTYPE_AddFieldDefinitionId, definition.FieldDefinitionId);
        }

        [Test]
        public void ContentTypeController_ClearContentTypeCache_Clears_Cache()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            ContentType contentType = ContentTestHelper.CreateValidContentType();

            //Act
            contentTypeController.ClearContentTypeCache();

            //Assert
            _mockCache.Verify(r => r.Remove(_contentTypeCacheKey));
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.DeleteContentType(null));
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            ContentType contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentTypeId = Null.NullInteger;

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.DeleteContentType(contentType));
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Calls_Repository_Delete_On_Valid_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Delete(contentType));
        }

        [Test]
        public void ContentTypeController_DeleteDataType_Throws_On_Null_DataType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.DeleteDataType(null));
        }

        [Test]
        public void ContentTypeController_DeleteDataType_Throws_On_Negative_DataTypeId()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var dataType = new ContentTypeDataType();
            dataType.DataTypeId = Null.NullInteger;

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.DeleteDataType(dataType));
        }

        [Test]
        public void ContentTypeController_DeleteDataType_Calls_Repository_Delete_On_Valid_DataTypeId()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var dataType = new ContentTypeDataType();
            dataType.DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId;

            //Act
            contentTypeController.DeleteDataType(dataType);

            //Assert
            _mockDataTypeRepository.Verify(r => r.Delete(dataType));
        }

        [Test]
        public void ContentTypeController_DeleteFieldDefinition_Throws_On_Null_FieldDefinition()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.DeleteFieldDefinition(null));
        }

        [Test]
        public void ContentTypeController_DeleteFieldDefinition_Throws_On_Negative_FieldDefinitionId()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var definition = new ContentTypeFieldDefinition {FieldDefinitionId = Null.NullInteger};

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.DeleteFieldDefinition(definition));
        }

        [Test]
        public void ContentTypeController_DeleteFieldDefinition_Calls_Repository_Delete_On_Valid_FieldDefinitionId()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var definition = new ContentTypeFieldDefinition
                                    {
                                        FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId
                                    };

            //Act
            contentTypeController.DeleteFieldDefinition(definition);

            //Assert
            _mockFieldDefinitionRepository.Verify(r => r.Delete(definition));
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Calls_Repository_Get()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes();

            //Assert
            _mockContentTypeRepository.Verify(r => r.Get());
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Get())
                .Returns(new List<ContentType>());
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes();

            //Assert
            Assert.IsNotNull(contentTypes);
            Assert.AreEqual(0, contentTypes.Count());
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Get())
                .Returns(MockHelper.CreateValidContentTypes(Constants.CONTENTTYPE_ValidContentTypeCount));
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes();

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidContentTypeCount, contentTypes.Count());
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Overload_Calls_Repository_Get()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Get(Constants.PORTAL_ValidPortalId));
        }
        
        [Test]
        public void ContentTypeController_GetContentTypes_Overload_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Get(Constants.PORTAL_ValidPortalId))
                .Returns(new List<ContentType>());
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId);

            //Assert
            Assert.IsNotNull(contentTypes);
            Assert.AreEqual(0, contentTypes.Count());
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Overload_Returns_List_Of_ContentTypes()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Get(Constants.PORTAL_ValidPortalId))
                .Returns(MockHelper.CreateValidContentTypes(Constants.CONTENTTYPE_ValidContentTypeCount));
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidContentTypeCount, contentTypes.Count());
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Overload_Calls_Repository_GetPage()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId, Constants.PAGE_First, Constants.PAGE_RecordCount);

            //Assert
            _mockContentTypeRepository.Verify(r => r.GetPage(Constants.PORTAL_ValidPortalId, Constants.PAGE_First, Constants.PAGE_RecordCount));
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Overload_Returns_PagedList()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);
            _mockContentTypeRepository.Setup(r => r.GetPage(Constants.PORTAL_ValidPortalId, Constants.PAGE_First, Constants.PAGE_RecordCount))
                .Returns(new PagedList<ContentType>(new List<ContentType>(), Constants.PAGE_First, Constants.PAGE_RecordCount));

            //Act
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId, Constants.PAGE_First, Constants.PAGE_RecordCount);

            //Assert
            Assert.IsInstanceOf<IPagedList<ContentType>>(contentTypes);
        }

        [Test]
        public void ContentTypeController_GetDataTypes_Calls_Repository_Get()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var dataTypes = contentTypeController.GetDataTypes();

            //Assert
            _mockDataTypeRepository.Verify(r => r.Get());
        }

        [Test]
        public void ContentTypeController_GetDataTypess_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
        {
            //Arrange
            _mockDataTypeRepository.Setup(r => r.Get())
                .Returns(GetValidDataTypes(0));
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var dataTypes = contentTypeController.GetDataTypes();

            //Assert
            Assert.IsNotNull(dataTypes);
            Assert.AreEqual(0, dataTypes.Count());
        }

        [Test]
        public void ContentTypeController_GetDataTypes_Returns_List_Of_ContentTypes()
        {
            //Arrange
            _mockDataTypeRepository.Setup(r => r.Get())
                .Returns(GetValidDataTypes(Constants.CONTENTTYPE_ValidDataTypeCount));
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var dataTypes = contentTypeController.GetDataTypes();

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidDataTypeCount, dataTypes.Count());
        }

        [Test]
        public void ContentTypeController_GetStructuredContentTypes_Calls_Repository_Find()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetStructuredContentTypes(Constants.PORTAL_ValidPortalId);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Find(ContentTypeController.StructuredWhereClause, Constants.PORTAL_ValidPortalId));
        }

        [Test]
        public void ContentTypeController_GetStructuredContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Get(Constants.PORTAL_ValidPortalId))
                .Returns(new List<ContentType>());
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetStructuredContentTypes(Constants.PORTAL_ValidPortalId);

            //Assert
            Assert.IsNotNull(contentTypes);
            Assert.AreEqual(0, contentTypes.Count());
        }

        [Test]
        public void ContentTypeController_GetStructuredContentTypes_Returns_List_Of_ContentTypes()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Find(ContentTypeController.StructuredWhereClause, Constants.PORTAL_ValidPortalId))
                .Returns(MockHelper.CreateValidContentTypes(Constants.CONTENTTYPE_ValidContentTypeCount));
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetStructuredContentTypes(Constants.PORTAL_ValidPortalId);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidContentTypeCount, contentTypes.Count());
        }

        [Test]
        public void ContentTypeController_GetStructuredContentTypes_Overload_Calls_Repository_Find()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetStructuredContentTypes(Constants.PORTAL_ValidPortalId, Constants.PAGE_First, Constants.PAGE_RecordCount);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Find(Constants.PAGE_First, Constants.PAGE_RecordCount, ContentTypeController.StructuredWhereClause, Constants.PORTAL_ValidPortalId));
        }

        [Test]
        public void ContentTypeController_GetStructuredContentTypess_Overload_Returns_PagedList()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);
            _mockContentTypeRepository.Setup(r => r.Find(Constants.PAGE_First, Constants.PAGE_RecordCount, ContentTypeController.StructuredWhereClause, Constants.PORTAL_ValidPortalId))
                .Returns(new PagedList<ContentType>(new List<ContentType>(), Constants.PAGE_First, Constants.PAGE_RecordCount));

            //Act
            var contentTypes = contentTypeController.GetStructuredContentTypes(Constants.PORTAL_ValidPortalId, Constants.PAGE_First, Constants.PAGE_RecordCount);

            //Assert
            Assert.IsInstanceOf<IPagedList<ContentType>>(contentTypes);
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.UpdateContentType(null));
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Throws_On_Empty_ContentType_Property()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);
            var contentType = new ContentType {ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId};

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTypeController.UpdateContentType(contentType));
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentType = Constants.CONTENTTYPE_InValidContentType;

            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.UpdateContentType(contentType));
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Calls_Repository_Update_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId;
            contentType.ContentType = Constants.CONTENTTYPE_UpdateContentType;

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Update(contentType));
        }

        [Test]
        public void ContentTypeController_UpdateDataType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.UpdateDataType(null));
        }

        [Test]
        public void ContentTypeController_UpdateDataType_Throws_On_Empty_ContentType_Property()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);
            var dataType = new ContentTypeDataType { DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTypeController.UpdateDataType(dataType));
        }

        [Test]
        public void ContentTypeController_UpdateDataType_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var dataType = new ContentTypeDataType {DataTypeId = Constants.CONTENTTYPE_InValidDataTypeId, Name = "Test"};

            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.UpdateDataType(dataType));
        }

        [Test]
        public void ContentTypeController_UpdateDataType_Calls_Repository_Update_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new ContentTypeController(_mockDataContext.Object);

            var dataType = new ContentTypeDataType { DataTypeId = Constants.CONTENTTYPE_UpdateDataTypeId };
            dataType.Name = "New_Name";

            //Act
            contentTypeController.UpdateDataType(dataType);

            //Assert
            _mockDataTypeRepository.Verify(r => r.Update(dataType));
        }

        private List<ContentTypeDataType> GetValidDataTypes(int count)
        {
            var list = new List<ContentTypeDataType>();

            for (int i = 1; i <= count; i++)
            {
                list.Add( new ContentTypeDataType() {DataTypeId = i, Name = string.Format("Name_{0}", i)});
            }

            return list;
        }
    }
}