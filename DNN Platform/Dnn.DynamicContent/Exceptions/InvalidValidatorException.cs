// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Dnn.DynamicContent.Exceptions
{
    public class InvalidValidatorException : InvalidOperationException
    {
        public InvalidValidatorException(ValidatorType validatorType)
            : base(DotNetNuke.Services.Localization.Localization.GetExceptionMessage("InvalidValidatorException", String.Format("Could not create ValidatorType - - {0}.", validatorType.ValidatorClassName)))
        {
                
        }
    }
}
