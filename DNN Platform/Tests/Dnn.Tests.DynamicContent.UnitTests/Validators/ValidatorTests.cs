// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
