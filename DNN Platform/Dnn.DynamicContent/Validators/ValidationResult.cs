// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

// ReSharper disable ConvertPropertyToExpressionBody

namespace Dnn.DynamicContent.Validators
{
    public class ValidationResult
    {
        private readonly List<ValidationFailure> _errors;

        public ValidationResult(IEnumerable<ValidationFailure> failures)
        {
            _errors = new List<ValidationFailure>();
            _errors.AddRange(failures);
        }

        public bool IsValid { get { return Errors.Count == 0; } }

        public IList<ValidationFailure> Errors { get { return _errors;  } }
    }
}
