// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using Moq;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    class ValidationRuleTests
    {
        [Test]
        public void Constructor_Sets_Default_Properties()
        {
            //Arrange

            //Act
            var rule = new ValidationRule();

            //Assert
            Assert.AreEqual(-1, rule.ValidatorTypeId);
            Assert.AreEqual(-1, rule.ValidationRuleId);
            Assert.AreEqual(-1, rule.FieldDefinitionId);
        }

        [Test]
        public void Constructor_Instantiates_Settings_Collection()
        {
            //Arrange

            //Act
            var rule = new ValidationRule();

            //Assert
            Assert.AreEqual(0, rule.ValidationSettings.Count);
        }

        [Test]
        public void ValidatorType_Property_Calls_ValidatorTypeController_Get()
        {
            //Arrange
            var rule = new ValidationRule();
            var mockValidatorTypeController = new Mock<IValidatorTypeController>();
            ValidatorTypeController.SetTestableInstance(mockValidatorTypeController.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var validatorType = rule.ValidatorType;

            //Assert
            mockValidatorTypeController.Verify(c => c.GetValidatorTypes(), Times.Once);
        }

        [Test]
        public void ValidatorType_Property_Calls_ValidatorTypeController_Get_Once_Only()
        {
            //Arrange
            var validatorTypeId = 2;
            var rule = new ValidationRule() { ValidatorTypeId = validatorTypeId };
            var mockValidatorTypeController = new Mock<IValidatorTypeController>();
            mockValidatorTypeController.Setup(dt => dt.GetValidatorTypes())
                .Returns(new List<ValidatorType>() { new ValidatorType() { ValidatorTypeId = validatorTypeId } }.AsQueryable());
            ValidatorTypeController.SetTestableInstance(mockValidatorTypeController.Object);

            //Act
            // ReSharper disable UnusedVariable
            var validatorType = rule.ValidatorType;
            var validatorType1 = rule.ValidatorType;
            var validatorType2 = rule.ValidatorType;
            // ReSharper restore UnusedVariable

            //Assert
            mockValidatorTypeController.Verify(c => c.GetValidatorTypes(), Times.AtMostOnce);
        }

    }
}
