// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Dnn.DynamicContent.Validators
{
    public class StringLengthValidator : BaseValidator
    {
        public const string MaxLengthSettingName = "MaxLength";

        public override void Validate(object value)
        {
            var maxLengthSetting = ValidatorSettings[MaxLengthSettingName];

            IsValid = true;
            if (value == null)
            {
                IsValid = false;
            }
            else
            {
                var stringValue = value as String;
                if (stringValue == null)
                {
                    IsValid = false;
                }
                else
                {
                    int maxLength = -1;

                    maxLength = Int32.Parse(maxLengthSetting.SettingValue);

                    if (stringValue.Length > maxLength)
                    {
                        IsValid = false;
                    }
                }
            }
        }
    }
}
