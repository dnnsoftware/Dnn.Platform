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
using DotNetNuke.Entities.Content.DynamicContent;
using DotNetNuke.Tests.Utilities;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Content.DynamicContent
{
    [TestFixture]
    public class FieldDefinitionTests
    {
        [TearDown]
        public void TearDown()
        {
            DataTypeController.ClearInstance();
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
        public void DataType_Property_Calls_DataTypeController_Get()
        {
            //Arrange
            var field = new FieldDefinition();
            var mockDataTypeController = new Mock<IDataTypeController>();
            DataTypeController.SetTestableInstance(mockDataTypeController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var dataType = field.DataType;

            //Assert
            mockDataTypeController.Verify(c => c.GetDataTypes(), Times.Once);
        }

        [Test]
        public void DataType_Property_Calls_DataTypeController_Get_Once_Only()
        {
            //Arrange
            var datatTypeId = 2;
            var field = new FieldDefinition() {DataTypeId = datatTypeId};
            var mockDataTypeController = new Mock<IDataTypeController>();
            mockDataTypeController.Setup(dt => dt.GetDataTypes())
                .Returns(new List<DataType>() {new DataType() { DataTypeId = datatTypeId } }.AsQueryable());
            DataTypeController.SetTestableInstance(mockDataTypeController.Object);

            //Act
            // ReSharper disable UnusedVariable
            var dataType = field.DataType;
            var dataType1 = field.DataType;
            var dataType2 = field.DataType;
            // ReSharper restore UnusedVariable

            //Assert
            mockDataTypeController.Verify(c => c.GetDataTypes(), Times.AtMostOnce);
        }

        [Test]
        public void ValidationRules_Property_Creates_New_List_If_FieldDefinition_Negative()
        {
            //Arrange
            var field = new FieldDefinition();
            var mockValidationRuleController = new Mock<IValidationRuleController>();
            ValidationRuleController.SetTestableInstance(mockValidationRuleController.Object);

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

            var mockValidationRuleController = new Mock<IValidationRuleController>();
            ValidationRuleController.SetTestableInstance(mockValidationRuleController.Object);

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
            var mockValidationRuleController = new Mock<IValidationRuleController>();
            mockValidationRuleController.Setup(dt => dt.GetValidationRules(fieldDefinitionId))
                .Returns(new List<ValidationRule>() { new ValidationRule() { FieldDefinitionId = fieldDefinitionId } }.AsQueryable());
            ValidationRuleController.SetTestableInstance(mockValidationRuleController.Object);

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
