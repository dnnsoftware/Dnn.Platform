// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Validators
{
    using System.Linq;
    using System.Reflection;

    /// <summary>A validator which validates each property of an object.</summary>
    public abstract class PropertyBasedObjectValidator : ObjectValidator
    {
        /// <inheritdoc />
        public override ValidationResult ValidateObject(object target)
        {
            return target.GetType().GetProperties().Aggregate(ValidationResult.Successful, (result, member) => result.CombineWith(this.ValidateProperty(target, member) ?? ValidationResult.Successful));
        }

        /// <summary>Validates the given property.</summary>
        /// <param name="target">The target object.</param>
        /// <param name="targetProperty">The property to validate.</param>
        /// <returns>A new <see cref="ValidationResult"/>.</returns>
        protected abstract ValidationResult ValidateProperty(object target, PropertyInfo targetProperty);
    }
}
