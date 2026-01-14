// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Validators
{
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;

    /// <summary>A validator which applies <see cref="ValidationAttribute"/> on an object's properties.</summary>
    public class DataAnnotationsObjectValidator : AttributeBasedObjectValidator<ValidationAttribute>
    {
        /// <inheritdoc />
        protected override ValidationResult ValidateAttribute(object target, PropertyInfo targetProperty, ValidationAttribute attribute)
        {
            return !attribute.IsValid(targetProperty.GetValue(target, []))
                ? new ValidationResult([this.CreateError(targetProperty.Name, attribute)])
                : ValidationResult.Successful;
        }

        /// <summary>Creates an error for an invalid property value.</summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="attribute">The invalid attribute.</param>
        /// <returns>A new <see cref="ValidationError"/> instance.</returns>
        protected virtual ValidationError CreateError(string propertyName, ValidationAttribute attribute)
        {
            return new ValidationError { ErrorMessage = attribute.FormatErrorMessage(propertyName), PropertyName = propertyName, Validator = attribute, };
        }
    }
}
