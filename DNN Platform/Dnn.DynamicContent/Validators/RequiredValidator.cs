// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Dnn.DynamicContent.Validators
{
    public class RequiredValidator : BaseValidator
    {
        public override void Validate(object value)
        {
            IsValid = true;
            if (value == null)
            {
                IsValid = false;
            }
            else
            {
                var stringValue = value as String;
                if (stringValue != null)
                {
                    if (String.IsNullOrEmpty(stringValue))
                    {
                        IsValid = false;
                    }
                }
            }
        }
    }
}
