﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;
using System.Linq;

#endregion

namespace DotNetNuke.Web.Validators
{
    public class Validator
    {
        private readonly IList<ObjectValidator> _Validators;

        public Validator()
        {
            _Validators = new List<ObjectValidator>();
        }

        public Validator(ObjectValidator validator) : this()
        {
            _Validators.Add(validator);
        }

        public IList<ObjectValidator> Validators
        {
            get
            {
                return _Validators;
            }
        }

        public ValidationResult ValidateObject(object target)
        {
            return _Validators.Aggregate(ValidationResult.Successful, (result, validator) => result.CombineWith(validator.ValidateObject(target) ?? ValidationResult.Successful));
        }
    }
}
