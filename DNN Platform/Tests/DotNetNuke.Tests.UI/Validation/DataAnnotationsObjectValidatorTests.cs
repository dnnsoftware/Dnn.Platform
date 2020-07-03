// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.UI.Validation
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using DotNetNuke.Web.Validators;
    using NUnit.Framework;

    using ValidationResult = DotNetNuke.Web.Validators.ValidationResult;

    [TestFixture]
    public class DataAnnotationsObjectValidatorTests
    {
        private const string ErrorMessageConst = "Yo, you have to specify 5 characters for Bar";

        public static string ErrorMessage
        {
            get
            {
                return ErrorMessageConst;
            }
        }

        [Test]
        public void ValidateObject_Returns_Successful_Result_If_All_Attributes_On_All_Properties_Validate()
        {
            // Arrange
            TestClass cls = new TestClass { Foo = new object(), Bar = "Baz" };
            DataAnnotationsObjectValidator validator = new DataAnnotationsObjectValidator();

            // Act
            ValidationResult result = validator.ValidateObject(cls);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidateObject_Returns_Failed_Result_If_Any_Attribute_Does_Not_Validate()
        {
            // Arrange
            TestClass cls = new TestClass { Foo = new object(), Bar = "BarBaz" };
            DataAnnotationsObjectValidator validator = new DataAnnotationsObjectValidator();

            // Act
            ValidationResult result = validator.ValidateObject(cls);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ValidateObject_Collects_Error_Messages_From_Validation_Attributes()
        {
            // Arrange
            TestClass cls = new TestClass { Foo = null, Bar = "BarBaz" };
            DataAnnotationsObjectValidator validator = new DataAnnotationsObjectValidator();

            // Act
            ValidationResult result = validator.ValidateObject(cls);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Dude, you forgot to enter a Foo", result.Errors.Where(e => e.PropertyName == "Foo").Single().ErrorMessage);
            Assert.AreEqual("Yo, you have to specify 5 characters for Bar", result.Errors.Where(e => e.PropertyName == "Bar").Single().ErrorMessage);
        }

        [Test]
        public void ValidateObject_Collects_ValidationAttribute_Objects_From_Failed_Validation()
        {
            // Arrange
            TestClass cls = new TestClass { Foo = null, Bar = "BarBaz" };
            DataAnnotationsObjectValidator validator = new DataAnnotationsObjectValidator();

            // Act
            ValidationResult result = validator.ValidateObject(cls);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsInstanceOf<RequiredAttribute>(result.Errors.Single(e => e.PropertyName == "Foo").Validator);
            Assert.IsInstanceOf<StringLengthAttribute>(result.Errors.Single(e => e.PropertyName == "Bar").Validator);
        }

        public class TestClass
        {
            [Required(ErrorMessage = "Dude, you forgot to enter a {0}")]
            public object Foo { get; set; }

            [StringLength(5, ErrorMessageResourceName = "ErrorMessage", ErrorMessageResourceType = typeof(DataAnnotationsObjectValidatorTests))]
            public string Bar { get; set; }
        }
    }
}
