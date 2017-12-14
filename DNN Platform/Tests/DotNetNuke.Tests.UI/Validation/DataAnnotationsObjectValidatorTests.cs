#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.ComponentModel.DataAnnotations;
using System.Linq;

using DotNetNuke.Web.Validators;

using NUnit.Framework;

using ValidationResult = DotNetNuke.Web.Validators.ValidationResult;

namespace DotNetNuke.Tests.UI.Validation
{
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

        #region Nested type: TestClass

        public class TestClass
        {
            [Required(ErrorMessage = "Dude, you forgot to enter a {0}")]
            public object Foo { get; set; }

            [StringLength(5, ErrorMessageResourceName = "ErrorMessage", ErrorMessageResourceType = typeof (DataAnnotationsObjectValidatorTests))]
            public string Bar { get; set; }
        }

        #endregion

        #region Tests

        [Test]
        public void ValidateObject_Returns_Successful_Result_If_All_Attributes_On_All_Properties_Validate()
        {
            // Arrange
            TestClass cls = new TestClass {Foo = new object(), Bar = "Baz"};
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
            TestClass cls = new TestClass {Foo = new object(), Bar = "BarBaz"};
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
            TestClass cls = new TestClass {Foo = null, Bar = "BarBaz"};
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
            TestClass cls = new TestClass {Foo = null, Bar = "BarBaz"};
            DataAnnotationsObjectValidator validator = new DataAnnotationsObjectValidator();

            // Act
            ValidationResult result = validator.ValidateObject(cls);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsInstanceOf<RequiredAttribute>(result.Errors.Single(e => e.PropertyName == "Foo").Validator);
            Assert.IsInstanceOf<StringLengthAttribute>(result.Errors.Single(e => e.PropertyName == "Bar").Validator);
        }

        #endregion
    }
}