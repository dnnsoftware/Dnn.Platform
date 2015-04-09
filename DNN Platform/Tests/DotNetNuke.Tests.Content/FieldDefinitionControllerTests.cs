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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.DynamicContent;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Content
{
    [TestFixture]
    public class FieldDefinitionControllerTests
    {
        private Mock<IDataContext> _mockDataContext;
        private Mock<IRepository<FieldDefinition>> _mockFieldDefinitionRepository;
        private Mock<CachingProvider> _mockCache;

        [SetUp]
        public void SetUp()
        {
            //Register MockCachingProvider
            _mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(String.Empty);

            _mockDataContext = new Mock<IDataContext>();

            _mockFieldDefinitionRepository = new Mock<IRepository<FieldDefinition>>();
            _mockDataContext.Setup(dc => dc.GetRepository<FieldDefinition>()).Returns(_mockFieldDefinitionRepository.Object);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void AddFieldDefinition_Throws_On_Null_FieldDefinition()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => fieldDefinitionController.AddFieldDefinition(null));
        }

        [Test]
        public void AddFieldDefinition_Throws_On_Negative_ContentTypeId_Property()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var definition = new FieldDefinition
            {
                ContentTypeId = Constants.CONTENTTYPE_InValidContentTypeId,
                DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                Name = "New_Type",
                Label = "Label"
            };

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => fieldDefinitionController.AddFieldDefinition(definition));
        }

        [Test]
        public void AddFieldDefinition_Throws_On_Negative_DataTypeId_Property()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var definition = new FieldDefinition
            {
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                DataTypeId = Constants.CONTENTTYPE_InValidDataTypeId,
                Name = "New_Type",
                Label = "Label"
            };

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => fieldDefinitionController.AddFieldDefinition(definition));
        }

        [Test]
        public void AddFieldDefinition_Throws_On_Empty_Name_Property()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var definition = new FieldDefinition
            {
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                Name = string.Empty,
                Label = "Label"
            };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => fieldDefinitionController.AddFieldDefinition(definition));
        }

        [Test]
        public void AddFieldDefinition_Throws_On_Empty_Label_Property()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var definition = new FieldDefinition
            {
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                Name = "New_Type",
                Label = string.Empty
            };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => fieldDefinitionController.AddFieldDefinition(definition));
        }

        [Test]
        public void AddFieldDefinition_Calls_Repository_Insert_On_Valid_Arguments()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var definition = new FieldDefinition
            {
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                Name = "New_Type",
                Label = "Label"
            };

            //Act
            int fieldDefinitionId = fieldDefinitionController.AddFieldDefinition(definition);

            //Assert
            _mockFieldDefinitionRepository.Verify(rep => rep.Insert(definition));
        }

        [Test]
        public void AddFieldDefinition_Returns_ValidId_On_Valid_FieldDefinition()
        {
            //Arrange
            _mockFieldDefinitionRepository.Setup(r => r.Insert(It.IsAny<FieldDefinition>()))
                            .Callback((FieldDefinition df) => df.FieldDefinitionId = Constants.CONTENTTYPE_AddFieldDefinitionId);

            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var definition = new FieldDefinition
            {
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                Name = "New_Type",
                Label = "Label"
            };

            //Act
            int fieldDefinitionId = fieldDefinitionController.AddFieldDefinition(definition);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddFieldDefinitionId, fieldDefinitionId);
        }

        [Test]
        public void AddFieldDefinition_Sets_ValidId_On_Valid_FieldDefinition()
        {
            //Arrange
            _mockFieldDefinitionRepository.Setup(r => r.Insert(It.IsAny<FieldDefinition>()))
                            .Callback((FieldDefinition dt) => dt.FieldDefinitionId = Constants.CONTENTTYPE_AddFieldDefinitionId);

            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

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
            Assert.AreEqual(Constants.CONTENTTYPE_AddFieldDefinitionId, definition.FieldDefinitionId);
        }

        [Test]
        public void DeleteFieldDefinition_Throws_On_Null_FieldDefinition()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => fieldDefinitionController.DeleteFieldDefinition(null));
        }

        [Test]
        public void DeleteFieldDefinition_Throws_On_Negative_FieldDefinitionId()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var definition = new FieldDefinition { FieldDefinitionId = Null.NullInteger };

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => fieldDefinitionController.DeleteFieldDefinition(definition));
        }

        [Test]
        public void DeleteFieldDefinition_Calls_Repository_Delete_On_Valid_FieldDefinitionId()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var definition = new FieldDefinition
            {
                FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId
            };

            //Act
            fieldDefinitionController.DeleteFieldDefinition(definition);

            //Assert
            _mockFieldDefinitionRepository.Verify(r => r.Delete(definition));
        }

        [Test]
        public void UpdateFieldDefinition_Throws_On_Null_FieldDefinition()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => fieldDefinitionController.UpdateFieldDefinition(null));
        }

        [Test]
        public void UpdateFieldDefinition_Throws_On_Negative_ContentTypeId_Property()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var definition = new FieldDefinition
                                    {
                                        FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                        ContentTypeId = Constants.CONTENTTYPE_InValidContentTypeId,
                                        DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                        Name = "New_Type",
                                        Label = "Label"
                                    };

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => fieldDefinitionController.UpdateFieldDefinition(definition));
        }

        [Test]
        public void UpdateFieldDefinition_Throws_On_Negative_DataTypeId_Property()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var definition = new FieldDefinition
                                    {
                                        FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                        DataTypeId = Constants.CONTENTTYPE_InValidDataTypeId,
                                        Name = "New_Type",
                                        Label = "Label"
                                    };

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => fieldDefinitionController.UpdateFieldDefinition(definition));
        }

        [Test]
        public void UpdateFieldDefinition_Throws_On_Empty_Name_Property()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);
            var field = new FieldDefinition
                            {
                                FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                Name = string.Empty,
                                Label = "Label"
                            };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => fieldDefinitionController.UpdateFieldDefinition(field));
        }

        [Test]
        public void UpdateFieldDefinition_Throws_On_Empty_Label_Property()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var field = new FieldDefinition
                                    {
                                        FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                        DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                        Name = "New_Type",
                                        Label = string.Empty
                                    };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => fieldDefinitionController.UpdateFieldDefinition(field));
        }

        [Test]
        public void UpdateFieldDefinition_Throws_On_Negative_FieldDefinitionId()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var field = new FieldDefinition
                            {
                                FieldDefinitionId = Constants.CONTENTTYPE_InValidFieldDefinitionId,
                                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                Name = "New_Type",
                                Label = "Label"
                            };

            Assert.Throws<ArgumentOutOfRangeException>(() => fieldDefinitionController.UpdateFieldDefinition(field));
        }

        [Test]
        public void UpdateFieldDefinition_Calls_Repository_Update()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionController(_mockDataContext.Object);

            var field = new FieldDefinition
                            {
                                FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                DataTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                Name = "New_Type",
                                Label = "Label"
                            };

            //Act
            fieldDefinitionController.UpdateFieldDefinition(field);

            //Assert
            _mockFieldDefinitionRepository.Verify(r => r.Update(field));
        }
    }
}
