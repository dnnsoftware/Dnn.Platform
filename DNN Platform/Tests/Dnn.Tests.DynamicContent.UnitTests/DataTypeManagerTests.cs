// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Exceptions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

// ReSharper disable UseStringInterpolation
// ReSharper disable BuiltInTypeReferenceStyle

namespace Dnn.Tests.DynamicContent.UnitTests
{
    class DataTypeManagerTests
    {
        private Mock<IDataContext> _mockDataContext;
        private Mock<IRepository<DataType>> _mockDataTypeRepository;
        private Mock<IRepository<FieldDefinition>> _mockFieldDefinitionRepository;
        // ReSharper disable once NotAccessedField.Local
        private Mock<CachingProvider> _mockCache;
        
        [SetUp]
        public void SetUp()
        {
            //Register MockCachingProvider
            _mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(String.Empty);

            _mockDataContext = new Mock<IDataContext>();

            _mockDataTypeRepository = new Mock<IRepository<DataType>>();
            _mockDataContext.Setup(dc => dc.GetRepository<DataType>()).Returns(_mockDataTypeRepository.Object);

            _mockFieldDefinitionRepository = new Mock<IRepository<FieldDefinition>>();
            _mockDataContext.Setup(dc => dc.GetRepository<FieldDefinition>()).Returns(_mockFieldDefinitionRepository.Object);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
            ContentController.ClearInstance();
        }

        [Test]
        public void AddDataType_Throws_On_Null_DataType()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => dataTypeController.AddDataType(null));
        }

        [Test]
        public void AddDataType_Throws_On_Empty_Name_Property()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => dataTypeController.AddDataType(new DataType()));
        }

        [Test]
        public void AddDataType_Calls_Repository_Insert_On_Valid_Arguments()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            var dataType = new DataType() { Name = "DataType1" };

            //Act
            // ReSharper disable once UnusedVariable
            int dataTypeId = dataTypeController.AddDataType(dataType);

            //Assert
            _mockDataTypeRepository.Verify(rep => rep.Insert(dataType));
        }

        [Test]
        public void AddDataType_Returns_ValidId_On_Valid_DataType()
        {
            //Arrange
            _mockDataTypeRepository.Setup(r => r.Insert(It.IsAny<DataType>()))
                            .Callback((DataType dt) => dt.DataTypeId = Constants.CONTENTTYPE_AddDataTypeId);

            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            var dataType = new DataType() { Name = "DataType1" };

            //Act
            int dataTypeId = dataTypeController.AddDataType(dataType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddDataTypeId, dataTypeId);
        }

        [Test]
        public void AddDataType_Sets_ValidId_On_Valid_DataType()
        {
            //Arrange
            _mockDataTypeRepository.Setup(r => r.Insert(It.IsAny<DataType>()))
                            .Callback((DataType dt) => dt.DataTypeId = Constants.CONTENTTYPE_AddDataTypeId);

            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            var dataType = new DataType() { Name = "DataType1" };

            //Act
            dataTypeController.AddDataType(dataType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddDataTypeId, dataType.DataTypeId);
        }

        [Test]
        public void DeleteDataType_Throws_On_Null_DataType()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => dataTypeController.DeleteDataType(null));
        }

        [Test]
        public void DeleteDataType_Throws_On_Negative_DataTypeId()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            var dataType = new DataType {DataTypeId = Null.NullInteger};

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => dataTypeController.DeleteDataType(dataType));
        }

        [Test]
        public void DeleteDataType_Calls_FieldDefinition_Repository_Find_On_Valid_DataTypeId()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            var dataType = new DataType { DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId };

            //Act
            dataTypeController.DeleteDataType(dataType);

            //Assert
            _mockFieldDefinitionRepository.Verify(r => r.Find(DataTypeManager.FindWhereDataTypeSql, Constants.CONTENTTYPE_ValidDataTypeId));
        }

        [Test]
        public void DeleteDataType_Calls_Repository_Delete_If_DataType_UnUsed()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);
            _mockFieldDefinitionRepository.Setup(r => r.Find(DataTypeManager.FindWhereDataTypeSql, Constants.CONTENTTYPE_ValidDataTypeId))
                .Returns(new List<FieldDefinition>());

            var dataType = new DataType {DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId};

            //Act
            dataTypeController.DeleteDataType(dataType);

            //Assert
            _mockDataTypeRepository.Verify(r => r.Delete(dataType));
        }

        [Test]
        public void DeleteDataType_Throws_If_DataType_Used()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);
            _mockFieldDefinitionRepository.Setup(r => r.Find(DataTypeManager.FindWhereDataTypeSql, Constants.CONTENTTYPE_ValidDataTypeId))
                .Returns(new List<FieldDefinition> { new FieldDefinition() });

            var dataType = new DataType { DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId };

            //Act, Assert
            Assert.Throws<DataTypeInUseException>(() => dataTypeController.DeleteDataType(dataType));
        }

        [Test]
        public void GetDataTypes_Calls_Repository_Get()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var dataTypes = dataTypeController.GetDataTypes();

            //Assert
            _mockDataTypeRepository.Verify(r => r.Get());
        }

        [Test]
        public void GetDataTypess_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
        {
            //Arrange
            _mockDataTypeRepository.Setup(r => r.Get())
                .Returns(GetValidDataTypes(0));
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            //Act
            var dataTypes = dataTypeController.GetDataTypes();

            //Assert
            Assert.IsNotNull(dataTypes);
            Assert.AreEqual(0, dataTypes.Count());
        }

        [Test]
        public void GetDataTypes_Returns_List_Of_ContentTypes()
        {
            //Arrange
            _mockDataTypeRepository.Setup(r => r.Get())
                .Returns(GetValidDataTypes(Constants.CONTENTTYPE_ValidDataTypeCount));
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            //Act
            var dataTypes = dataTypeController.GetDataTypes();

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidDataTypeCount, dataTypes.Count());
        }

        [Test]
        public void UpdateDataType_Throws_On_Null_ContentType()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => dataTypeController.UpdateDataType(null));
        }

        [Test]
        public void UpdateDataType_Throws_On_Empty_ContentType_Property()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);
            var dataType = new DataType { DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => dataTypeController.UpdateDataType(dataType));
        }

        [Test]
        public void UpdateDataType_Throws_On_Negative_DataTypeId()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            var dataType = new DataType { DataTypeId = Constants.CONTENTTYPE_InValidDataTypeId, Name = "Test" };

            Assert.Throws<ArgumentOutOfRangeException>(() => dataTypeController.UpdateDataType(dataType));
        }

        [Test]
        public void UpdateDataType_Calls_Repository_Update_If_DataType_Is_UnUsed()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);

            var dataType = new DataType
                {
                    DataTypeId = Constants.CONTENTTYPE_UpdateDataTypeId,
                    Name = "New_Name"
                };

            //Act
            dataTypeController.UpdateDataType(dataType);

            //Assert
            _mockDataTypeRepository.Verify(r => r.Update(dataType));
        }

        [Test]
        public void UpdateDataType_Calls_Repository_Update_If_DataType_Is_Used_But_No_ContentItems()
        {
            //Arrange
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);
            _mockFieldDefinitionRepository.Setup(r => r.Find(DataTypeManager.FindWhereDataTypeSql, Constants.CONTENTTYPE_ValidDataTypeId))
                .Returns(new List<FieldDefinition> { new FieldDefinition() { ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId } });

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);
            mockContentController.Setup(c => c.GetContentItemsByContentType(Constants.CONTENTTYPE_ValidContentTypeId))
                .Returns(new List<ContentItem>().AsQueryable());

            var dataType = new DataType
                            {
                                DataTypeId = Constants.CONTENTTYPE_UpdateDataTypeId,
                                Name = "New_Name"
                            };

            //Act
            dataTypeController.UpdateDataType(dataType);

            //Assert
            _mockDataTypeRepository.Verify(r => r.Update(dataType));
        }

        [Test]
        public void UpdateDataType_Throws_If_DataType_Is_Used_And_Has_ContentItems()
        {
            //Arrange
            var dataTypeId = Constants.CONTENTTYPE_UpdateDataTypeId;
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);
            _mockFieldDefinitionRepository.Setup(r => r.Find(DataTypeManager.FindWhereDataTypeSql, dataTypeId))
                .Returns(new List<FieldDefinition> { new FieldDefinition() { ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId} });

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);
            mockContentController.Setup(c => c.GetContentItemsByContentType(Constants.CONTENTTYPE_ValidContentTypeId))
                .Returns(new List<ContentItem>() {new ContentItem()}.AsQueryable());

            var dataType = new DataType
                                {
                                    DataTypeId = dataTypeId,
                                    Name = "New_Name"
                                };

            //Act, Assert
            Assert.Throws<DataTypeInUseException>(() => dataTypeController.UpdateDataType(dataType));
        }

        [Test]
        public void UpdateDataType_Calls_Repository_Update_If_DataType_Is_Used_And_Has_ContentItems_And_OverrideFlag_Set()
        {
            //Arrange
            var dataTypeId = Constants.CONTENTTYPE_UpdateDataTypeId;
            var dataTypeController = new DataTypeManager(_mockDataContext.Object);
            _mockFieldDefinitionRepository.Setup(r => r.Find(DataTypeManager.FindWhereDataTypeSql, dataTypeId))
                .Returns(new List<FieldDefinition> { new FieldDefinition() { ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId } });

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);
            mockContentController.Setup(c => c.GetContentItemsByContentType(Constants.CONTENTTYPE_ValidContentTypeId))
                .Returns(new List<ContentItem>() { new ContentItem() }.AsQueryable());

            var dataType = new DataType
            {
                DataTypeId = dataTypeId,
                Name = "New_Name"
            };

            //Act
            dataTypeController.UpdateDataType(dataType, true);

            //Assert
            _mockDataTypeRepository.Verify(r => r.Update(dataType));
        }

        private List<DataType> GetValidDataTypes(int count)
        {
            var list = new List<DataType>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new DataType() { DataTypeId = i, Name = String.Format("Name_{0}", i) });
            }

            return list;
        }
    }
}
