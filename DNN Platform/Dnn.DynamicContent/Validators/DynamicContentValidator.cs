// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using DotNetNuke.Common;

namespace Dnn.DynamicContent.Validators
{
    public class DynamicContentValidator
    {
        public DynamicContentValidator(DynamicContentItem content)
        {
            Requires.NotNull("content", content);

            ContentItem = content;
        }

        public DynamicContentItem ContentItem { get; set; }

        public ValidationResult Validate()
        {
            var failures = new List<ValidationFailure>();

            foreach (var field in ContentItem.Fields.Values)
            {
                var definition = field.Definition;

                foreach (var rule in definition.ValidationRules)
                {
                    IValidator validator = ValidatorFactory.CreateValidator(rule);
                    validator.Validate(field.Value);

                    if (!validator.IsValid)
                    {
                        //Log failure
                        failures.Add(new ValidationFailure(field, rule));
                    }
                }
            }

            return new ValidationResult(failures);
        }
    }
}
