// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Tests.Utilities;
using Moq;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class FieldDefinitionTests
    {
        [TearDown]
        public void TearDown()
        {
            DataTypeManager.ClearInstance();
        }

        [Test]
        public void Constructor_Sets_Default_Properties()
        {
            //Arrange

            //Act
            var field = new FieldDefinition();

            //Assert
            Assert.AreEqual(-1, field.FieldDefinitionId);
            Assert.AreEqual(-1, field.ContentTypeId);
            Assert.AreEqual(-1, field.DataTypeId);
            Assert.AreEqual(String.Empty, field.Name);
            Assert.AreEqual(String.Empty, field.Label);
            Assert.AreEqual(String.Empty, field.Description);
        }

        [Test]
        public void Constructor_Instantiates_ValidationRules_Collection()
        {
            //Arrange

            //Act
            var field = new FieldDefinition();

            //Assert
            Assert.AreEqual(0, field.ValidationRules.Count);
        }
        [Test]
        //TODO - update when FieldDefinition Todo is completed
        public void DataType_Property_Calls_DataTypeController_Get()
        {
            //Arrange
            var field = new FieldDefinition();
            var mockDataTypeController = new Mock<IDataTypeManager>();
            DataTypeManager.SetTestableInstance(mockDataTypeController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var dataType = field.DataType;

            //Assert
            mockDataTypeController.Verify(c => c.GetDataTypes(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
        }

        [Test]
        //TODO - update when FieldDefinition Todo is completed
        public void DataType_Property_Calls_DataTypeController_Get_Once_Only()
        {
            //Arrange
            var datatTypeId = 2;
            var field = new FieldDefinition() {DataTypeId = datatTypeId};
            var mockDataTypeController = new Mock<IDataTypeManager>();
            mockDataTypeController.Setup(dt => dt.GetDataTypes(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new List<DataType>() {new DataType() { DataTypeId = datatTypeId } }.AsQueryable());
            DataTypeManager.SetTestableInstance(mockDataTypeController.Object);

            //Act
            // ReSharper disable UnusedVariable
            var dataType = field.DataType;
            var dataType1 = field.DataType;
            var dataType2 = field.DataType;
            // ReSharper restore UnusedVariable

            //Assert
            mockDataTypeController.Verify(c => c.GetDataTypes(It.IsAny<int>(), It.IsAny<bool>()), Times.AtMostOnce);
        }

        [Test]
        public void ValidationRules_Property_Creates_New_List_If_FieldDefinition_Negative()
        {
            //Arrange
            var field = new FieldDefinition();
            var mockValidationRuleController = new Mock<IValidationRuleManager>();
            ValidationRuleManager.SetTestableInstance(mockValidationRuleController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var dataType = field.ValidationRules;

            //Assert
            mockValidationRuleController.Verify(c => c.GetValidationRules(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void ValidationRules_Property_Calls_ValidationRuleController_Get_If_FieldDefinition_Not_Negative()
        {
            //Arrange
            var fieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId;
            var field = new FieldDefinition() { FieldDefinitionId = fieldDefinitionId };

            var mockValidationRuleController = new Mock<IValidationRuleManager>();
            ValidationRuleManager.SetTestableInstance(mockValidationRuleController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var dataType = field.ValidationRules;

            //Assert
            mockValidationRuleController.Verify(c => c.GetValidationRules(It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void ValidationRules_Property_Calls_ValidationRuleController_Get_Once_Only()
        {
            //Arrange
            var fieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId;
            var field = new FieldDefinition() { FieldDefinitionId = fieldDefinitionId };
            var mockValidationRuleController = new Mock<IValidationRuleManager>();
            mockValidationRuleController.Setup(dt => dt.GetValidationRules(fieldDefinitionId))
                .Returns(new List<ValidationRule>() { new ValidationRule() { FieldDefinitionId = fieldDefinitionId } }.AsQueryable());
            ValidationRuleManager.SetTestableInstance(mockValidationRuleController.Object);

            //Act
            // ReSharper disable UnusedVariable
            var mockDataTypeController = field.ValidationRules;
            var validationRules1 = field.ValidationRules;
            var validationRules2 = field.ValidationRules;
            // ReSharper restore UnusedVariable

            //Assert
            mockValidationRuleController.Verify(c => c.GetValidationRules(fieldDefinitionId), Times.AtMostOnce);
        }
    }
}
