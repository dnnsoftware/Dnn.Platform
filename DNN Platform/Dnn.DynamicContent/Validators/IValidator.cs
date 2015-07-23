// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Dnn.DynamicContent.Validators
{
    public interface IValidator
    {
        bool IsValid { get; set; }

        void Validate(object value);

        IDictionary<string, ValidatorSetting> ValidatorSettings { get; set; }
    }
}
