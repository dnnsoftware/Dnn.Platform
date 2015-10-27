// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Localization;
using Dnn.DynamicContent.Repositories;
using Dnn.Tests.DynamicContent.UnitTests.Builders;
using DotNetNuke.Data;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Localization;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

// ReSharper disable UseStringInterpolation

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class FieldDefinitionManagerTests
    {
        private Mock<IDataContext> _mockDataContext;
        private Mock<IRepository<FieldDefinition>> _mockFieldDefinitionDataContext;
        private Mock<IRepository<ContentTypeLocalization>> _mockContentTypeLocalizationDataContext;
        private Mock<IValidationRuleManager>  _mockValidationRuleController;
        private Mock<IDynamicContentTypeManager> _mockContentTypeController;
        private IFieldDefinitionRepository _fieldDefinitionRepositoryMockedDataContext;
        private Mock<IContentTypeLocalizationManager> _mockContentTypeLocalizationManager;
        private Mock<ILocaleController> _mockLocaleController;

        // ReSharper disable once NotAccessedField.Local
        private Mock<CachingProvider> _mockCache;

        [SetUp]
        public void SetUp()
        {
            //Register MockCachingProvider
            _mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(String.Empty);

            _mockDataContext = new Mock<IDataContext>();

            _mockFieldDefinitionDataContext = new Mock<IRepository<FieldDefinition>>();
            _mockContentTypeLocalizationDataContext = new Mock<IRepository<ContentTypeLocalization>>();
            _mockDataContext.Setup(dc => dc.GetRepository<FieldDefinition>()).Returns(_mockFieldDefinitionDataContext.Object);
            _mockDataContext.Setup(dc => dc.GetRepository<ContentTypeLocalization>()).Returns(_mockContentTypeLocalizationDataContext.Object);

            _mockValidationRuleController = new Mock<IValidationRuleManager>();
            _mockValidationRuleController.Setup(vr => vr.GetValidationRules(It.IsAny<int>()))
                .Returns(new List<ValidationRule>().AsQueryable());
            ValidationRuleManager.SetTestableInstance(_mockValidationRuleController.Object);

            _mockContentTypeController = new Mock<IDynamicContentTypeManager>();
            _mockContentTypeController.Setup(m => m.GetContentType(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).
                Returns(() => new DynamicContentTypeBuilder().Build());
            DynamicContentTypeManager.SetTestableInstance(_mockContentTypeController.Object);

            _fieldDefinitionRepositoryMockedDataContext = new FieldDefinitionRepository(_mockDataContext.Object);
            FieldDefinitionRepository.SetTestableInstance(_fieldDefinitionRepositoryMockedDataContext);

            _mockContentTypeLocalizationManager = new Mock<IContentTypeLocalizationManager>();
            ContentTypeLocalizationManager.SetTestableInstance(_mockContentTypeLocalizationManager.Object);

            _mockLocaleController = new Mock<ILocaleController>();
            _mockLocaleController.Setup(m => m.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>());
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
            ValidationRuleManager.ClearInstance();
            DynamicContentTypeManager.ClearInstance();
            ContentTypeLocalizationManager.ClearInstance();
        }

        [Test]
        public void AddFieldDefinition_Throws_On_Null_FieldDefinition()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => fieldDefinitionController.AddFieldDefinition(null));
        }

        [Test]
        public void AddFieldDefinition_Throws_On_Empty_Name_Property()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();

            var definition = new FieldDefinition
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                        FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
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
            var fieldDefinitionController = new FieldDefinitionManager();

            var definition = new FieldDefinition
                            {
                                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
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
            var fieldDefinitionController = new FieldDefinitionManager();

            var definition = new FieldDefinition
                                {
                                    ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                    FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                    Name = "New_Type",
                                    Label = "Label"
                                };

            //Act
            // ReSharper disable once UnusedVariable
            int fieldDefinitionId = fieldDefinitionController.AddFieldDefinition(definition);

            //Assert
            _mockFieldDefinitionDataContext.Verify(rep => rep.Insert(definition));
        }

        [Test]
        public void AddFieldDefinition_Returns_ValidId_On_Valid_FieldDefinition()
        {
            //Arrange
            _mockFieldDefinitionDataContext.Setup(r => r.Insert(It.IsAny<FieldDefinition>()))
                            .Callback((FieldDefinition df) => df.FieldDefinitionId = Constants.CONTENTTYPE_AddFieldDefinitionId);

            var fieldDefinitionController = new FieldDefinitionManager();

            var definition = new FieldDefinition
            {
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
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
            _mockFieldDefinitionDataContext.Setup(r => r.Insert(It.IsAny<FieldDefinition>()))
                            .Callback((FieldDefinition dt) => dt.FieldDefinitionId = Constants.CONTENTTYPE_AddFieldDefinitionId);

            var fieldDefinitionController = new FieldDefinitionManager();

            var definition = new FieldDefinition
            {
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                Name = "New_Type",
                Label = "Label"
            };

            //Act
            fieldDefinitionController.AddFieldDefinition(definition);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddFieldDefinitionId, definition.FieldDefinitionId);
        }

        [Test]
        public void AddFieldDefinition_Adds_New_ValidationRules_On_Valid_FieldDefinition()
        {
            //Arrange
            _mockFieldDefinitionDataContext.Setup(r => r.Insert(It.IsAny<FieldDefinition>()))
                            .Callback((FieldDefinition dt) => dt.FieldDefinitionId = Constants.CONTENTTYPE_AddFieldDefinitionId);

            var fieldDefinitionController = new FieldDefinitionManager();

            var definition = new FieldDefinition
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                        FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                        Name = "New_Type",
                                        Label = "Label"
                                    };

            var validationRuleCount = 5;
            for(int i = 0; i < validationRuleCount; i++)
            {
                definition.ValidationRules.Add(new ValidationRule());
            }

            //Act
            fieldDefinitionController.AddFieldDefinition(definition);

            //Assert
            _mockValidationRuleController.Verify(v => v.AddValidationRule(It.IsAny<ValidationRule>()), Times.Exactly(validationRuleCount));
        }

        [Test]
        public void AddFieldDefinition_Sets_FieldDefinitionId_Property_Of_New_ValidationRules_On_Valid_FieldDefinition()
        {
            //Arrange
            var fieldDefinitionId = Constants.CONTENTTYPE_AddFieldDefinitionId;
            _mockFieldDefinitionDataContext.Setup(r => r.Insert(It.IsAny<FieldDefinition>()))
                            .Callback((FieldDefinition dt) => dt.FieldDefinitionId = fieldDefinitionId);

            var fieldDefinitionController = new FieldDefinitionManager();

            var definition = new FieldDefinition
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                        FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                        Name = "New_Type",
                                        Label = "Label"
                                    };

            var validationRuleCount = 5;
            for (int i = 0; i < validationRuleCount; i++)
            {
                definition.ValidationRules.Add(new ValidationRule());
            }

            //Act
            fieldDefinitionController.AddFieldDefinition(definition);

            //Assert
            foreach (var rule in definition.ValidationRules)
            {
                Assert.AreEqual(fieldDefinitionId, rule.FieldDefinitionId);
            }
        }

        [Test]
        public void DeleteFieldDefinition_Throws_On_Null_FieldDefinition()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => fieldDefinitionController.DeleteFieldDefinition(null));
        }

        [Test]
        public void DeleteFieldDefinition_Calls_Repository_Delete_On_Valid_FieldDefinitionId()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();
            var definition = new FieldDefinition
            {
                FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId
            };

            //Act
            fieldDefinitionController.DeleteFieldDefinition(definition);

            //Assert
            _mockFieldDefinitionDataContext.Verify(r => r.Delete(definition));
        }

        [Test]
        public void DeleteFieldDefinition_Deletes_ValidatioRules_On_Valid_FieldDefinitionId()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();
            var definition = new FieldDefinition
                                    {
                                        FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId
                                    };
            var validationRuleCount = Constants.CONTENTTYPE_ValidValidationRuleCount;
            for (int i = 0; i < validationRuleCount; i++)
            {
                definition.ValidationRules.Add(new ValidationRule());
            }

            //Act
            fieldDefinitionController.DeleteFieldDefinition(definition);

            //Assert
            _mockValidationRuleController.Verify(v => v.DeleteValidationRule(It.IsAny<ValidationRule>()), Times.Exactly(validationRuleCount));
        }

        [Test]
        public void GetFieldDefinitions_Overload_Calls_Repository_Get()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();

            //Act
            // ReSharper disable once UnusedVariable
            var fieldDefinitions = fieldDefinitionController.GetFieldDefinitions(Constants.CONTENTTYPE_ValidContentTypeId);

            //Assert
            _mockFieldDefinitionDataContext.Verify(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId));
        }

        [Test]
        public void GetFieldDefinitions_Overload_Returns_Empty_List_Of_FieldDefinitions_If_No_FieldDefinitions()
        {
            //Arrange
            _mockFieldDefinitionDataContext.Setup(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId))
                .Returns(new List<FieldDefinition>());
            var fieldDefinitionController = new FieldDefinitionManager();

            //Act
            var fieldDefinitions = fieldDefinitionController.GetFieldDefinitions(Constants.CONTENTTYPE_ValidContentTypeId);

            //Assert
            Assert.IsNotNull(fieldDefinitions);
            Assert.AreEqual(0, fieldDefinitions.Count());
        }

        [Test]
        public void GetFieldDefinitions_Overload_Returns_List_Of_FieldDefinitions()
        {
            //Arrange
            _mockFieldDefinitionDataContext.Setup(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId))
                .Returns(GetValidFieldDefinitions(Constants.CONTENTTYPE_ValidFieldDefinitionCount));
            var fieldDefinitionController = new FieldDefinitionManager();

            //Act
            var fieldDefinitions = fieldDefinitionController.GetFieldDefinitions(Constants.CONTENTTYPE_ValidContentTypeId);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidFieldDefinitionCount, fieldDefinitions.Count());
        }

        [Test]
        public void UpdateFieldDefinition_Throws_On_Null_FieldDefinition()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => fieldDefinitionController.UpdateFieldDefinition(null));
        }

        [Test]
        public void UpdateFieldDefinition_Throws_On_Empty_Name_Property()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();
            var field = new FieldDefinition
                            {
                                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
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
            var fieldDefinitionController = new FieldDefinitionManager();

            var field = new FieldDefinition
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                        FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                        FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                        Name = "New_Type",
                                        Label = string.Empty
                                    };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => fieldDefinitionController.UpdateFieldDefinition(field));
        }

        [Test]
        public void UpdateFieldDefinition_Calls_Repository_Update()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();

            var field = new FieldDefinition
                            {
                                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                Name = "New_Type",
                                Label = "Label"
                            };

            //Act
            fieldDefinitionController.UpdateFieldDefinition(field);

            //Assert
            _mockFieldDefinitionDataContext.Verify(r => r.Update(field));
        }

        [Test]
        public void UpdateFieldDefinition_Adds_New_ValidationRules_On_Valid_FieldDefinition()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();

            var field = new FieldDefinition
                                {
                                    ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                    FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                    FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                    Name = "New_Type",
                                    Label = "Label"
                                };

            var validationRuleCount = 5;
            for (int i = 0; i < validationRuleCount; i++)
            {
                field.ValidationRules.Add(new ValidationRule());
            }

            //Act
            fieldDefinitionController.UpdateFieldDefinition(field);

            //Assert
            _mockValidationRuleController.Verify(vr => vr.AddValidationRule(It.IsAny<ValidationRule>()), Times.Exactly(validationRuleCount));
        }

        [Test]
        public void UpdateFieldDefinition_Sets_FieldDefinitionId_Property_Of_New_ValidationRules_On_Valid_FieldDefinition()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();

            var fieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId;
            var field = new FieldDefinition
                                {
                                    ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                    FieldDefinitionId = fieldDefinitionId,
                                    FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                    Name = "New_Type",
                                    Label = "Label"
                                };

            var validationRuleCount = 5;
            for (int i = 0; i < validationRuleCount; i++)
            {
                field.ValidationRules.Add(new ValidationRule());
            }

            //Act
            fieldDefinitionController.UpdateFieldDefinition(field);

            //Assert
            foreach (var rule in field.ValidationRules)
            {
                Assert.AreEqual(fieldDefinitionId, rule.FieldDefinitionId);
            }
        }

        [Test]
        public void UpdateFieldDefinition_Updates_Existing_ValidationRules_On_Valid_FieldDefinition()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();

            var field = new FieldDefinition
                            {
                                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                                FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                                Name = "New_Type",
                                Label = "Label"
                            };

            var validationRuleCount = 5;
            for (int i = 0; i < validationRuleCount; i++)
            {
                field.ValidationRules.Add(new ValidationRule() {ValidationRuleId = Constants.CONTENTTYPE_ValidValidationRuleId});
            }

            //Act
            fieldDefinitionController.UpdateFieldDefinition(field);

            //Assert
            _mockValidationRuleController.Verify(vr => vr.UpdateValidationRule(It.IsAny<ValidationRule>()), Times.Exactly(validationRuleCount));
        }

        private List<FieldDefinition> GetValidFieldDefinitions(int count)
        {
            var list = new List<FieldDefinition>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new FieldDefinition() { FieldTypeId = i, Name = String.Format("Name_{0}", i) });
            }

            return list;
        }

    }
}
