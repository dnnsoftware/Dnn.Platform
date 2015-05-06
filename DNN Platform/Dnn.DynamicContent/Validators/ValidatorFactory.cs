// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.DynamicContent.Exceptions;
using DotNetNuke.Framework;

namespace Dnn.DynamicContent.Validators
{
    public class ValidatorFactory
    {
        public static IValidator CreateValidator(ValidationRule rule)
        {
            IValidator validator;
            var validatorType = rule.ValidatorType;

            switch (validatorType.Name)
            {
                case "Required":
                    validator = new RequiredValidator();
                    break;
                case "StringLength":
                    validator = new StringLengthValidator();
                    break;
                default:
                    var type = Reflection.CreateType(validatorType.ValidatorClassName);

                    if (type == null)
                    {
                        throw new InvalidValidatorException(validatorType);
                    }

                    validator = Reflection.CreateInstance(type) as IValidator;

                    if (validator == null)
                    {
                        throw new CreateValidatorException(validatorType);
                    }

                    break;

            }

            //Add Settings
            validator.ValidatorSettings = rule.ValidationSettings;

            return validator;
        }
    }
}
