// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Dnn.DynamicContent.Exceptions
{
    public class CreateValidatorException : InvalidOperationException
    {
        public CreateValidatorException(ValidatorType validatorType)
            : base(DotNetNuke.Services.Localization.Localization.GetExceptionMessage("CreateValidatorException", String.Format("Could not instantiate Validator - {0}.", validatorType.ValidatorClassName)))
        {
                
        }
    }
}
