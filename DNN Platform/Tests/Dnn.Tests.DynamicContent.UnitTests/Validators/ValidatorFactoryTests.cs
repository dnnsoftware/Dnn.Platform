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
using Dnn.DynamicContent.Exceptions;
using Dnn.DynamicContent.Validators;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests.Validators
{
    [TestFixture]
    public class ValidatorFactoryTests
    {
        private Mock<CachingProvider> _mockCache;
        private Mock<IValidatorTypeController> _mockValidatorTypeController;

        [SetUp]
        public void SetUp()
        {
            //Register MockCachingProvider
            _mockCache = MockComponentProvider.CreateNew<CachingProvider>();

            _mockValidatorTypeController = new Mock<IValidatorTypeController>();
            ValidatorTypeController.SetTestableInstance(_mockValidatorTypeController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
            ValidatorTypeController.ClearInstance();
        }

        [Test]
        public void CreateValidator_Returns_RequiredValidator_If_Name_Equals_Required()
        {
            //Arrange
            var validatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId;
            var type = new ValidatorType {Name = "Required", ValidatorTypeId = validatorTypeId };
            var rule = new ValidationRule() { ValidatorTypeId = validatorTypeId };
            _mockValidatorTypeController.Setup(vt => vt.GetValidatorTypes())
                .Returns(new List<ValidatorType>() { type }.AsQueryable());

            //Act
            var validator = ValidatorFactory.CreateValidator(rule);

            //Assert
            Assert.IsInstanceOf<RequiredValidator>(validator);
        }

        [Test]
        public void CreateValidator_Returns_StringLengthValidator_If_Name_Equals_StringLength()
        {
            //Arrange
            var validatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId;
            var type = new ValidatorType { Name = "StringLength", ValidatorTypeId = validatorTypeId };
            var rule = new ValidationRule() { ValidatorTypeId = validatorTypeId };
            _mockValidatorTypeController.Setup(vt => vt.GetValidatorTypes())
                .Returns(new List<ValidatorType>() { type }.AsQueryable());

            //Act
            var validator = ValidatorFactory.CreateValidator(rule);

            //Assert
            Assert.IsInstanceOf<StringLengthValidator>(validator);
        }

        [Test]
        public void CreateValidator_Returns_Validator_From_ClassName_Property_If_Name_Not_Recognized()
        {
            //Arrange
            var validatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId;
            var type = new ValidatorType
                            {
                                Name = "Fake Validator",
                                ValidatorTypeId = validatorTypeId,
                                ValidatorClassName = "Dnn.Tests.DynamicContent.UnitTests.Validators.FakeValidator, Dnn.Tests.DynamicContent.UnitTests"
            };
            var rule = new ValidationRule() { ValidatorTypeId = validatorTypeId };
            _mockValidatorTypeController.Setup(vt => vt.GetValidatorTypes())
                .Returns(new List<ValidatorType>() { type }.AsQueryable());

            //Act
            var validator = ValidatorFactory.CreateValidator(rule);

            //Assert
            Assert.IsInstanceOf<FakeValidator>(validator);
        }

        [Test]
        public void CreateValidator_Throws_If_ValidatorClassName_Cannot_Be_Found()
        {
            //Arrange
            var validatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId;
            var type = new ValidatorType
                            {
                                Name = "Fake Validator",
                                ValidatorTypeId = validatorTypeId,
                                ValidatorClassName = "Dnn.Tests.DynamicContent.UnitTests.Validators.UnknownType"
            };
            var rule = new ValidationRule() { ValidatorTypeId = validatorTypeId };
            _mockValidatorTypeController.Setup(vt => vt.GetValidatorTypes())
                .Returns(new List<ValidatorType>() { type }.AsQueryable());


            //Act, Assert
            Assert.Throws<InvalidValidatorException>(() => ValidatorFactory.CreateValidator(rule));
        }

        [Test]
        public void CreateValidator_Throws_If_Validator_Cannot_Be_Instantiated()
        {
            //Arrange
            var validatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId;
            var type = new ValidatorType
                                {
                                    Name = "Not A Validator",
                                    ValidatorTypeId = validatorTypeId,
                                    ValidatorClassName = "Dnn.Tests.DynamicContent.UnitTests.Validators.NotAValidator, Dnn.Tests.DynamicContent.UnitTests"
            };
            var rule = new ValidationRule() { ValidatorTypeId = validatorTypeId };
            _mockValidatorTypeController.Setup(vt => vt.GetValidatorTypes())
                .Returns(new List<ValidatorType>() { type }.AsQueryable());

            //Act, Assert
            Assert.Throws<CreateValidatorException>(() => ValidatorFactory.CreateValidator(rule));
        }

        [Test]
        public void CreateValidator_Sets_ValidatorSettings_Property()
        {
            //Arrange
            var validatorTypeId = Constants.CONTENTTYPE_ValidValidatorTypeId;
            var type = new ValidatorType { Name = "StringLength", ValidatorTypeId = validatorTypeId };
            var rule = new ValidationRule() { ValidatorTypeId = validatorTypeId };
            _mockValidatorTypeController.Setup(vt => vt.GetValidatorTypes())
                .Returns(new List<ValidatorType>() { type }.AsQueryable());

            //Act
            var validator = ValidatorFactory.CreateValidator(rule);

            //Assert
            Assert.AreSame(rule.ValidationSettings, validator.ValidatorSettings);
        }

    }
}
