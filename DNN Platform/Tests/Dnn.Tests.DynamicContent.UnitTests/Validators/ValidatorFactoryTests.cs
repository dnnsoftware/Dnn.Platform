// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        private Mock<IValidatorTypeManager> _mockValidatorTypeController;

        [SetUp]
        public void SetUp()
        {
            //Register MockCachingProvider
            _mockCache = MockComponentProvider.CreateNew<CachingProvider>();

            _mockValidatorTypeController = new Mock<IValidatorTypeManager>();
            ValidatorTypeManager.SetTestableInstance(_mockValidatorTypeController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
            ValidatorTypeManager.ClearInstance();
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
