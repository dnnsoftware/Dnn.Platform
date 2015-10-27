// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Localization;
using Dnn.DynamicContent.Repositories;
using Dnn.Tests.DynamicContent.UnitTests.Builders;
using DotNetNuke.Tests.Utilities;
using Moq;
using NUnit.Framework;

// ReSharper disable UseStringInterpolation

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class FieldDefinitionManagerTests
    {
        private MockRepository _mockRepository;
        private Mock<IValidationRuleManager> _mockValidationRuleController;
        private Mock<IDynamicContentTypeManager> _mockContentTypeController;
        private Mock<IFieldDefinitionRepository> _mockFieldDefinitionRepository;
        private Mock<IContentTypeLocalizationManager> _mockContentTypeLocalizationManager;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Default);
            
            _mockValidationRuleController = _mockRepository.Create<IValidationRuleManager>();
            ValidationRuleManager.SetTestableInstance(_mockValidationRuleController.Object);

            _mockContentTypeController = _mockRepository.Create<IDynamicContentTypeManager>();
            DynamicContentTypeManager.SetTestableInstance(_mockContentTypeController.Object);
            _mockContentTypeController.Setup(m => m.GetContentType(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).
                Returns(() => new DynamicContentTypeBuilder().Build());

            _mockFieldDefinitionRepository = _mockRepository.Create<IFieldDefinitionRepository>();
            FieldDefinitionRepository.SetTestableInstance(_mockFieldDefinitionRepository.Object);

            _mockContentTypeLocalizationManager = new Mock<IContentTypeLocalizationManager>();
            ContentTypeLocalizationManager.SetTestableInstance(_mockContentTypeLocalizationManager.Object);
        }

        [TearDown]
        public void TearDown()
        {
            ValidationRuleManager.ClearInstance();
            DynamicContentTypeManager.ClearInstance();
            ContentTypeLocalizationManager.ClearInstance();
            FieldDefinitionRepository.ClearInstance();
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
            _mockFieldDefinitionRepository.Verify(rep => rep.Add(definition));
        }

        [Test]
        public void AddFieldDefinition_Returns_ValidId_On_Valid_FieldDefinition()
        {
            //Arrange
            _mockFieldDefinitionRepository.Setup(r => r.Add(It.IsAny<FieldDefinition>()))
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
            _mockFieldDefinitionRepository.Setup(r => r.Add(It.IsAny<FieldDefinition>()))
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
            _mockFieldDefinitionRepository.Setup(r => r.Add(It.IsAny<FieldDefinition>()))
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
            _mockFieldDefinitionRepository.Setup(r => r.Add(It.IsAny<FieldDefinition>()))
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
                FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
            };

            var mockLocalization = new Mock<IContentTypeLocalizationManager>();
            ContentTypeLocalizationManager.SetTestableInstance(mockLocalization.Object);

            _mockFieldDefinitionRepository.Setup(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId))
                .Returns(new [] {definition});

            //Act
            fieldDefinitionController.DeleteFieldDefinition(definition);

            //Assert
            _mockFieldDefinitionRepository.Verify(r => r.Delete(definition));
            _mockFieldDefinitionRepository.VerifyAll();
        }

        [Test]
        public void DeleteFieldDefinition_Deletes_ValidatioRules_On_Valid_FieldDefinitionId()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();
            var definition = new FieldDefinition
                                    {
                                        FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
                                    };
            var validationRuleCount = Constants.CONTENTTYPE_ValidValidationRuleCount;
            for (int i = 0; i < validationRuleCount; i++)
            {
                definition.ValidationRules.Add(new ValidationRule());
            }

            var mockLocalization = new Mock<IContentTypeLocalizationManager>();
            ContentTypeLocalizationManager.SetTestableInstance(mockLocalization.Object);

            _mockFieldDefinitionRepository.Setup(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId))
                .Returns(new[] { definition });

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
            _mockFieldDefinitionRepository.Verify(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId));
        }

        [Test]
        public void GetFieldDefinitions_Overload_Returns_Empty_List_Of_FieldDefinitions_If_No_FieldDefinitions()
        {
            //Arrange
            _mockFieldDefinitionRepository.Setup(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId))
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
            _mockFieldDefinitionRepository.Setup(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId))
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

            _mockFieldDefinitionRepository.Setup(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId))
                .Returns(new[] { field });

            //Act
            fieldDefinitionController.UpdateFieldDefinition(field);

            //Assert
            _mockFieldDefinitionRepository.Verify(r => r.Update(field));
            _mockFieldDefinitionRepository.VerifyAll();
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

            _mockFieldDefinitionRepository.Setup(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId))
                .Returns(new[] { field });

            //Act
            fieldDefinitionController.UpdateFieldDefinition(field);

            //Assert
            _mockValidationRuleController.Verify(vr => vr.AddValidationRule(It.IsAny<ValidationRule>()), Times.Exactly(validationRuleCount));
            _mockFieldDefinitionRepository.VerifyAll();
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

            _mockFieldDefinitionRepository.Setup(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId))
                .Returns(new[] { field });

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

            _mockFieldDefinitionRepository.VerifyAll();
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


            _mockFieldDefinitionRepository.Setup(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId))
                .Returns(new[] { field });

            var validationRuleCount = 5;
            for (int i = 0; i < validationRuleCount; i++)
            {
                field.ValidationRules.Add(new ValidationRule() {ValidationRuleId = Constants.CONTENTTYPE_ValidValidationRuleId});
            }

            //Act
            fieldDefinitionController.UpdateFieldDefinition(field);

            //Assert
            _mockValidationRuleController.Verify(vr => vr.UpdateValidationRule(It.IsAny<ValidationRule>()), Times.Exactly(validationRuleCount));
            _mockValidationRuleController.VerifyAll();
        }

        [Test]
        public void UpdateFieldDefinition_Throws_InvalidOperationException_When_Trying_To_Change_PortalId()
        {
            //Arrange
            var fieldDefinitionController = new FieldDefinitionManager();

            var storedDefinition = new FieldDefinition
            {
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                Name = "New_Type",
                Label = "Label",
                PortalId = Constants.CONTENT_ValidPortalId
            };

            _mockFieldDefinitionRepository.Setup(r => r.Get(Constants.CONTENTTYPE_ValidContentTypeId))
                .Returns(new[] { storedDefinition });

            const int invalidPortalId = Constants.CONTENT_ValidPortalId + 1;

            var definition = new FieldDefinition
            {
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId,
                FieldTypeId = Constants.CONTENTTYPE_ValidDataTypeId,
                Name = "New_Type",
                Label = "Label",
                PortalId = invalidPortalId
            };

            //Act, Arrange
            Assert.Throws<InvalidOperationException>(() => fieldDefinitionController.UpdateFieldDefinition(definition));
            _mockFieldDefinitionRepository.VerifyAll();
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
