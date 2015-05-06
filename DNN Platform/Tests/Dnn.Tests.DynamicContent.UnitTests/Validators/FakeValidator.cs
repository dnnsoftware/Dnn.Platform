// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Validators;

namespace Dnn.Tests.DynamicContent.UnitTests.Validators
{
    public class FakeValidator : IValidator
    {
        public bool IsValid { get; set; }
        public void Validate(object value)
        {
            throw new System.NotImplementedException();
        }

        public IDictionary<string, ValidatorSetting> ValidatorSettings { get; set; }
    }
}
