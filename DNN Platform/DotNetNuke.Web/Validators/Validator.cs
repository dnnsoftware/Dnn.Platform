// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Validators
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>A validator which applies multiple <see cref="ObjectValidator"/> instances to an object.</summary>
    public class Validator
    {
        private readonly List<ObjectValidator> validators;

        /// <summary>Initializes a new instance of the <see cref="Validator"/> class.</summary>
        public Validator()
        {
            this.validators = [];
        }

        /// <summary>Initializes a new instance of the <see cref="Validator"/> class.</summary>
        /// <param name="validator">The validator.</param>
        public Validator(ObjectValidator validator)
            : this()
        {
            this.validators.Add(validator);
        }

        /// <summary>Gets the list of validators.</summary>
        public IList<ObjectValidator> Validators
        {
            get
            {
                return this.validators;
            }
        }

        /// <summary>Validates the object.</summary>
        /// <param name="target">The target object.</param>
        /// <returns>A new <see cref="ValidationResult"/>.</returns>
        public ValidationResult ValidateObject(object target)
        {
            return this.validators.Aggregate(ValidationResult.Successful, (result, validator) => result.CombineWith(validator.ValidateObject(target) ?? ValidationResult.Successful));
        }
    }
}
