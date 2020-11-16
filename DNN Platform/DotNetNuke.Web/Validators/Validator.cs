// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Validators
{
    using System.Collections.Generic;
    using System.Linq;

    public class Validator
    {
        private readonly IList<ObjectValidator> _Validators;

        public Validator()
        {
            this._Validators = new List<ObjectValidator>();
        }

        public Validator(ObjectValidator validator)
            : this()
        {
            this._Validators.Add(validator);
        }

        public IList<ObjectValidator> Validators
        {
            get
            {
                return this._Validators;
            }
        }

        public ValidationResult ValidateObject(object target)
        {
            return this._Validators.Aggregate(ValidationResult.Successful, (result, validator) => result.CombineWith(validator.ValidateObject(target) ?? ValidationResult.Successful));
        }
    }
}
