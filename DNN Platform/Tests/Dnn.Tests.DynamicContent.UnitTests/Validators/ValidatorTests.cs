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
using Dnn.DynamicContent;
using Dnn.DynamicContent.Validators;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests.Validators
{
    [TestFixture]
    public class ValidatorTests
    {
        [Test]
        public void RequiredValidator_Sets_IsValid_False_If_Value_Null()
        {
            //Arrange
            var validator = new RequiredValidator();

            //Act
            validator.Validate(null);

            //Assert
            Assert.IsFalse(validator.IsValid);
        }

        [Test]
        public void RequiredValidator_Sets_IsValid_False_If_Value_Empty_String()
        {
            //Arrange
            var validator = new RequiredValidator();

            //Act
            validator.Validate(String.Empty);

            //Assert
            Assert.IsFalse(validator.IsValid);
        }

        [Test]
        public void RequiredValidator_Sets_IsValid_True_If_Value_String()
        {
            //Arrange
            var validator = new RequiredValidator();

            //Act
            validator.Validate("SomeString");

            //Assert
            Assert.IsTrue(validator.IsValid);
        }

        [Test]
        public void RequiredValidator_Sets_IsValid_True_If_Value_Integer()
        {
            //Arrange
            var validator = new RequiredValidator();

            //Act
            validator.Validate(2);

            //Assert
            Assert.IsTrue(validator.IsValid);
        }

        [Test]
        public void RequiredValidator_Sets_IsValid_True_If_Value_Object()
        {
            //Arrange
            var validator = new RequiredValidator();

            //Act
            validator.Validate(new { Name = "Object"});

            //Assert
            Assert.IsTrue(validator.IsValid);
        }

        [Test]
        public void StringLengthValidator_Throws_If_MaxLength_Setting_Missing()
        {
            //Arrange
            var validator = new StringLengthValidator();
            validator.ValidatorSettings = new Dictionary<string, ValidatorSetting>();

            //Act, Assert
            Assert.Throws<KeyNotFoundException>(() => validator.Validate(null));
        }

        [Test]
        public void StringLengthValidator_Sets_IsValid_False_If_Value_Null()
        {
            //Arrange
            var maxLengthSetting = StringLengthValidator.MaxLengthSettingName;
            var validator = new StringLengthValidator
            {
                ValidatorSettings = new Dictionary<string, ValidatorSetting>
                {
                    { 
                        maxLengthSetting,  new ValidatorSetting
                                                {
                                                    SettingName = maxLengthSetting,
                                                    SettingValue = "20"
                                                }
                    }
                }
            };

            //Act
            validator.Validate(null);

            //Assert
            Assert.IsFalse(validator.IsValid);
        }

        [Test]
        public void StringLengthValidator_Sets_IsValid_False_If_Value_Integer()
        {
            //Arrange
            var maxLengthSetting = StringLengthValidator.MaxLengthSettingName;
            var validator = new StringLengthValidator
            {
                ValidatorSettings = new Dictionary<string, ValidatorSetting>
                {
                    { 
                        maxLengthSetting,  new ValidatorSetting
                                                {
                                                    SettingName = maxLengthSetting,
                                                    SettingValue = "20"
                                                }
                    }
                }
            };

            //Act
            validator.Validate(2);

            //Assert
            Assert.IsFalse(validator.IsValid);
        }

        [Test]
        public void StringLengthValidator_Sets_IsValid_False_If_Value_Object()
        {
            //Arrange
            var maxLengthSetting = StringLengthValidator.MaxLengthSettingName;
            var validator = new StringLengthValidator
            {
                ValidatorSettings = new Dictionary<string, ValidatorSetting>
                {
                      { 
                        maxLengthSetting,  new ValidatorSetting
                                                {
                                                    SettingName = maxLengthSetting,
                                                    SettingValue = "20"
                                                }
                    }
                }
            };

            //Act
            validator.Validate(new { Name = "Object" });

            //Assert
            Assert.IsFalse(validator.IsValid);
        }

        [Test]
        public void StringLengthValidator_Sets_IsValid_False_If_Value_Is_String_And_Longer_Than_MaxLength()
        {
            //Arrange
            var maxLengthSetting = StringLengthValidator.MaxLengthSettingName;
            var validator = new StringLengthValidator
            {
                ValidatorSettings = new Dictionary<string, ValidatorSetting>
                {
                    { 
                        maxLengthSetting,  new ValidatorSetting
                                                {
                                                    SettingName = maxLengthSetting,
                                                    SettingValue = "20"
                                                }
                    }
                }
            };

            //Act
            validator.Validate("This is a long string that is longer than the setting");

            //Assert
            Assert.IsFalse(validator.IsValid);
        }

        [Test]
        public void StringLengthValidator_Sets_IsValid_True_If_Value_Is_String_And_Shorter_Than_MaxLength()
        {
            //Arrange
            var maxLengthSetting = StringLengthValidator.MaxLengthSettingName;
            var validator = new StringLengthValidator
            {
                ValidatorSettings = new Dictionary<string, ValidatorSetting>
                {
                    { 
                        maxLengthSetting,  new ValidatorSetting
                                                {
                                                    SettingName = maxLengthSetting,
                                                    SettingValue = "30"
                                                }
                    }
                }
            };

            //Act
            validator.Validate("This is a short string");

            //Assert
            Assert.IsTrue(validator.IsValid);
        }
    }
}
