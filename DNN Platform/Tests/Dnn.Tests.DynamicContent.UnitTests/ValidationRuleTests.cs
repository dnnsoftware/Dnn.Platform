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
            var mockValidatorTypeController = new Mock<IValidatorTypeManager>();
            ValidatorTypeManager.SetTestableInstance(mockValidatorTypeController.Object);

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
            var mockValidatorTypeController = new Mock<IValidatorTypeManager>();
            mockValidatorTypeController.Setup(dt => dt.GetValidatorTypes())
                .Returns(new List<ValidatorType>() { new ValidatorType() { ValidatorTypeId = validatorTypeId } }.AsQueryable());
            ValidatorTypeManager.SetTestableInstance(mockValidatorTypeController.Object);

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
