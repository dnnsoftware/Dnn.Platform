#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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