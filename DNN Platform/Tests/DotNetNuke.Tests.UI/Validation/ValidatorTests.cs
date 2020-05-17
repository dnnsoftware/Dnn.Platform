﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Linq;

using DotNetNuke.Tests.Utilities;
using DotNetNuke.Web.Validators;

using Moq;

using NUnit.Framework;

namespace DotNetNuke.Tests.UI.Validation
{
    [TestFixture]
    public class ValidatorTests
    {
        private static readonly ValidationResult FailedResult = new ValidationResult(new[] {new ValidationError()});

        private static readonly ValidationResult AnotherFailedResult = new ValidationResult(new[] {new ValidationError()});

        #region Tests

        [Test]
        public void ValidateObject_Returns_Successful_Result_If_All_Validators_Return_Successful()
        {
            // Arrange
            var validator = new Validator();
            object target = new object();
            SetupValidators(validator, target, ValidationResult.Successful, ValidationResult.Successful, ValidationResult.Successful);

            // Act
            ValidationResult result = validator.ValidateObject(target);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidateObject_Returns_Failed_Result_If_Any_Validator_Returns_Failed()
        {
            // Arrange
            var validator = new Validator();
            object target = new object();
            SetupValidators(validator, target, ValidationResult.Successful, ValidationResult.Successful, FailedResult, ValidationResult.Successful);

            // Act
            ValidationResult result = validator.ValidateObject(target);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ValidateObject_Collects_Errors_From_All_Validators()
        {
            // Arrange
            var validator = new Validator();
            object target = new object();
            SetupValidators(validator, target, ValidationResult.Successful, FailedResult, AnotherFailedResult, ValidationResult.Successful);

            // Act
            ValidationResult result = validator.ValidateObject(target);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(2, result.Errors.Count());
            EnumerableAssert.ElementsMatch(new[] {FailedResult, AnotherFailedResult}, result.Errors, (e, a) => ReferenceEquals(e.Errors.First(), a));
        }

        #endregion

        #region Helpers

        private static void SetupValidators(Validator validator, object target, params ValidationResult[] validationResults)
        {
            validator.Validators.Clear();
            for (int i = 0; i < validationResults.Length; i++)
            {
                var mockValidator = new Mock<ObjectValidator>();
                mockValidator.Setup(v => v.ValidateObject(target)).Returns(validationResults[i]);
                validator.Validators.Add(mockValidator.Object);
            }
        }

        #endregion
    }
}
