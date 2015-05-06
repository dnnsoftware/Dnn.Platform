// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Dnn.DynamicContent.Validators
{
    public class ValidationFailure
    {
        public ValidationFailure(DynamicContentField field, ValidationRule rule)
        {
            AttemptedValue = field.Value;
            PropertyName = field.Definition.Name;

            ErrorMessage = rule.ValidatorType.ErrorMessage;
        }

        public object AttemptedValue { get; private set; }

        public string ErrorMessage { get; private set; }

        public string PropertyName { get; private set; }

        public override string ToString()
        {
            return ErrorMessage;
        }
    }
}
