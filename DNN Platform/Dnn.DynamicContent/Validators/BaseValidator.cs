// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Dnn.DynamicContent.Validators
{
    public abstract class BaseValidator : IValidator
    {
        protected BaseValidator()
        {
            IsValid = false;
        }

        public bool IsValid { get; set; }

        public abstract void Validate(object value);

        public IDictionary<string, ValidatorSetting> ValidatorSettings { get; set; }
    }
}
