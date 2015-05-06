// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using DotNetNuke.Services.Localization;

namespace Dnn.DynamicContent.Exceptions
{
    public class InvalidValidatorException : InvalidOperationException
    {
        public InvalidValidatorException(ValidatorType validatorType)
            : base(Localization.GetExceptionMessage("InvalidValidatorException", String.Format("Could not create ValidatorType - - {0}.", validatorType.ValidatorClassName)))
        {
                
        }
    }
}
