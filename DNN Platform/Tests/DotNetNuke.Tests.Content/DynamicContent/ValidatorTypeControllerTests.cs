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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.DynamicContent;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

// ReSharper disable UseStringInterpolation
// ReSharper disable BuiltInTypeReferenceStyle

namespace DotNetNuke.Tests.Content.DynamicContent
{
    class ValidatorTypeControllerTests
    {
        private Mock<IDataContext> _mockDataContext;
        private Mock<IRepository<ValidatorType>> _mockValidatorTypeRepository;
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

            _mockValidatorTypeRepository = new Mock<IRepository<ValidatorType>>();
            _mockDataContext.Setup(dc => dc.GetRepository<ValidatorType>()).Returns(_mockValidatorTypeRepository.Object);

            _mockFieldDefinitionRepository = new Mock<IRepository<FieldDefinition>>();
            _mockDataContext.Setup(dc => dc.GetRepository<FieldDefinition>()).Returns(_mockFieldDefinitionRepository.Object);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void AddValidatorType_Throws_On_Null_ValidatorType()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => validatorTypeController.AddValidatorType(null));
        }

        [Test]
        public void AddValidatorType_Throws_On_Empty_Name_Property()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);
            var validatorType = new ValidatorType()
                                        {
                                            Name = String.Empty,
                                            ValidatorClassName = Constants.CONTENTTYPE_ValidValidatorClassName,
                                            ErrorKey = Constants.CONTENTTYPE_ValidValidatorErrorKey,
                                            ErrorMessage = Constants.CONTENTTYPE_ValidValidatorErrorMessage
                                        };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => validatorTypeController.AddValidatorType(validatorType));
        }

        [Test]
        public void AddValidatorType_Throws_On_Empty_ValidatorClassName_Property()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);
            var validatorType = new ValidatorType()
                                        {
                                            Name = Constants.CONTENTTYPE_ValidValidatorTypeName,
                                            ValidatorClassName = String.Empty,
                                            ErrorKey = Constants.CONTENTTYPE_ValidValidatorErrorKey,
                                            ErrorMessage = Constants.CONTENTTYPE_ValidValidatorErrorMessage
                                        };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => validatorTypeController.AddValidatorType(validatorType));
        }

        [Test]
        public void AddValidatorType_Throws_On_Empty_ErrorKey_And_ErrorMessage_Property()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);
            var validatorType = new ValidatorType()
                                        {
                                            Name = Constants.CONTENTTYPE_ValidValidatorTypeName,
                                            ValidatorClassName = Constants.CONTENTTYPE_ValidValidatorClassName,
                                            ErrorKey = String.Empty,
                                            ErrorMessage = String.Empty
            };

            //Act, Arrange
            Assert.Throws<InvalidOperationException>(() => validatorTypeController.AddValidatorType(validatorType));
        }

        [Test]
        public void AddValidatorType_Calls_Repository_Insert_On_Valid_Arguments()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

            var validatorType = new ValidatorType()
                                    {
                                        Name = Constants.CONTENTTYPE_ValidValidatorTypeName,
                                        ValidatorClassName = Constants.CONTENTTYPE_ValidValidatorClassName,
                                        ErrorKey = Constants.CONTENTTYPE_ValidValidatorErrorKey,
                                        ErrorMessage = Constants.CONTENTTYPE_ValidValidatorErrorMessage
                                    };

            //Act
            // ReSharper disable once UnusedVariable
            int validatorTypeId = validatorTypeController.AddValidatorType(validatorType);

            //Assert
            _mockValidatorTypeRepository.Verify(rep => rep.Insert(validatorType));
        }

        [Test]
        public void AddValidatorType_Returns_ValidId_On_Valid_ValidatorType()
        {
            //Arrange
            _mockValidatorTypeRepository.Setup(r => r.Insert(It.IsAny<ValidatorType>()))
                            .Callback((ValidatorType dt) => dt.ValidatorTypeId = Constants.CONTENTTYPE_AddValidatorTypeId);

            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

            var validatorType = new ValidatorType()
                                    {
                                        Name = Constants.CONTENTTYPE_ValidValidatorTypeName,
                                        ValidatorClassName = Constants.CONTENTTYPE_ValidValidatorClassName,
                                        ErrorKey = Constants.CONTENTTYPE_ValidValidatorErrorKey,
                                        ErrorMessage = Constants.CONTENTTYPE_ValidValidatorErrorMessage
                                    };

            //Act
            int validatorTypeId = validatorTypeController.AddValidatorType(validatorType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddValidatorTypeId, validatorTypeId);
        }

        [Test]
        public void AddValidatorType_Sets_ValidId_On_Valid_ValidatorType()
        {
            //Arrange
            _mockValidatorTypeRepository.Setup(r => r.Insert(It.IsAny<ValidatorType>()))
                            .Callback((ValidatorType dt) => dt.ValidatorTypeId = Constants.CONTENTTYPE_AddValidatorTypeId);

            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

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
            Assert.AreEqual(Constants.CONTENTTYPE_AddValidatorTypeId, validatorType.ValidatorTypeId);
        }

        [Test]
        public void DeleteValidatorType_Throws_On_Null_ValidatorType()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => validatorTypeController.DeleteValidatorType(null));
        }

        [Test]
        public void DeleteValidatorType_Throws_On_Negative_ValidatorTypeId()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

            var validatorType = new ValidatorType {ValidatorTypeId = Null.NullInteger};

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => validatorTypeController.DeleteValidatorType(validatorType));
        }

        [Test]
        public void DeleteValidatorType_Calls_Repository_Delete_If_ValidatorType_Valid()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

            var validatorType = new ValidatorType {ValidatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId};

            //Act
            validatorTypeController.DeleteValidatorType(validatorType);

            //Assert
            _mockValidatorTypeRepository.Verify(r => r.Delete(validatorType));
        }

        [Test]
        public void GetValidatorTypes_Calls_Repository_Get()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var validatorTypes = validatorTypeController.GetValidatorTypes();

            //Assert
            _mockValidatorTypeRepository.Verify(r => r.Get());
        }

        [Test]
        public void GetValidatorTypes_Returns_Empty_List_Of_ValidatorTypes_If_No_ValidatorTypes()
        {
            //Arrange
            _mockValidatorTypeRepository.Setup(r => r.Get())
                .Returns(GetValidValidatorTypes(0));
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

            //Act
            var validatorTypes = validatorTypeController.GetValidatorTypes();

            //Assert
            Assert.IsNotNull(validatorTypes);
            Assert.AreEqual(0, validatorTypes.Count());
        }

        [Test]
        public void GetValidatorTypes_Returns_List_Of_ValidatorTypes()
        {
            //Arrange
            _mockValidatorTypeRepository.Setup(r => r.Get())
                .Returns(GetValidValidatorTypes(Constants.CONTENTTYPE_ValidValidatorTypeCount));
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

            //Act
            var validatorTypes = validatorTypeController.GetValidatorTypes();

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidValidatorTypeCount, validatorTypes.Count());
        }

        [Test]
        public void UpdateValidatorType_Throws_On_Null_ContentType()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => validatorTypeController.UpdateValidatorType(null));
        }

        [Test]
        public void UpdateValidatorType_Throws_On_Empty_Name_Property()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);
            var validatorType = new ValidatorType
                                    {
                                        ValidatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId,
                                        Name = String.Empty,
                                        ValidatorClassName = Constants.CONTENTTYPE_ValidValidatorClassName,
                                        ErrorKey = Constants.CONTENTTYPE_ValidValidatorErrorKey,
                                        ErrorMessage = Constants.CONTENTTYPE_ValidValidatorErrorMessage
                                    };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => validatorTypeController.UpdateValidatorType(validatorType));
        }

        [Test]
        public void UpdateValidatorType_Throws_On_Empty_ValidatorClassName_Property()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);
            var validatorType = new ValidatorType
                                        {
                                            ValidatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId,
                                            Name = Constants.CONTENTTYPE_ValidValidatorTypeName,
                                            ValidatorClassName = String.Empty,
                                            ErrorKey = Constants.CONTENTTYPE_ValidValidatorErrorKey,
                                            ErrorMessage = Constants.CONTENTTYPE_ValidValidatorErrorMessage
                                        };
            //Act, Arrange
            Assert.Throws<ArgumentException>(() => validatorTypeController.UpdateValidatorType(validatorType));
        }

        [Test]
        public void UpdateValidatorType_Throws_On_Empty_ErrorKey_And_ErrorMessage_Property()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);
            var validatorType = new ValidatorType
                                        {
                                            ValidatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId,
                                            Name = Constants.CONTENTTYPE_ValidValidatorTypeName,
                                            ValidatorClassName = Constants.CONTENTTYPE_ValidValidatorClassName,
                                            ErrorKey = String.Empty,
                                            ErrorMessage = String.Empty
                                        };

            //Act, Arrange
            Assert.Throws<InvalidOperationException>(() => validatorTypeController.UpdateValidatorType(validatorType));
        }

        [Test]
        public void UpdateValidatorType_Throws_On_Negative_ValidatorTypeId()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

            var validatorType = new ValidatorType
                                        {
                                            ValidatorTypeId = Constants.CONTENTTYPE_InValidValidatorTypeId,
                                            Name = Constants.CONTENTTYPE_ValidValidatorTypeName,
                                            ValidatorClassName = Constants.CONTENTTYPE_ValidValidatorClassName,
                                            ErrorKey = Constants.CONTENTTYPE_ValidValidatorErrorKey,
                                            ErrorMessage = Constants.CONTENTTYPE_ValidValidatorErrorMessage
                                        };

            Assert.Throws<ArgumentOutOfRangeException>(() => validatorTypeController.UpdateValidatorType(validatorType));
        }

        [Test]
        public void UpdateValidatorType_Calls_Repository_Update_If_ValidatorType_Is_Valid()
        {
            //Arrange
            var validatorTypeController = new ValidatorTypeController(_mockDataContext.Object);

            var validatorType = new ValidatorType()
                                    {
                                        ValidatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId,
                                        Name = Constants.CONTENTTYPE_ValidValidatorTypeName,
                                        ValidatorClassName = Constants.CONTENTTYPE_ValidValidatorClassName,
                                        ErrorKey = Constants.CONTENTTYPE_ValidValidatorErrorKey,
                                        ErrorMessage = Constants.CONTENTTYPE_ValidValidatorErrorMessage
                                    };


            //Act
            validatorTypeController.UpdateValidatorType(validatorType);

            //Assert
            _mockValidatorTypeRepository.Verify(r => r.Update(validatorType));
        }

        private List<ValidatorType> GetValidValidatorTypes(int count)
        {
            var list = new List<ValidatorType>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new ValidatorType() { ValidatorTypeId = i, Name = String.Format("Name_{0}", i) });
            }

            return list;
        }
    }
}
