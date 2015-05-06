// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using DotNetNuke.Services.Localization;

namespace Dnn.DynamicContent.Exceptions
{
    public class InvalidValidationTypeException : InvalidOperationException
    {
        public InvalidValidationTypeException(ValidatorType validatorType)
            : base(Localization.GetExceptionMessage("ErrorMessageCannotBeNullOrEmpty", "Both the ErorrKey and the ErrorMessage cannot be null or empty."))
        {
        }
    }
}
