// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System.ComponentModel.DataAnnotations;
using System.Reflection;

#endregion

namespace DotNetNuke.Web.Validators
{
    public class DataAnnotationsObjectValidator : AttributeBasedObjectValidator<ValidationAttribute>
    {
        protected override ValidationResult ValidateAttribute(object target, PropertyInfo targetProperty, ValidationAttribute attribute)
        {
            return !attribute.IsValid(targetProperty.GetValue(target, new object[] {})) ? new ValidationResult(new[] {CreateError(targetProperty.Name, attribute)}) : ValidationResult.Successful;
        }


        protected virtual ValidationError CreateError(string propertyName, ValidationAttribute attribute)
        {
            return new ValidationError {ErrorMessage = attribute.FormatErrorMessage(propertyName), PropertyName = propertyName, Validator = attribute};
        }
    }
}
