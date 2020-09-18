// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Validators
{
    using System;
    using System.Linq;
    using System.Reflection;

    public abstract class AttributeBasedObjectValidator<TAttribute> : PropertyBasedObjectValidator
        where TAttribute : Attribute
    {
        protected override ValidationResult ValidateProperty(object target, PropertyInfo targetProperty)
        {
            return targetProperty.GetCustomAttributes(true).OfType<TAttribute>().Aggregate(
                ValidationResult.Successful,
                (result, attribute) =>
                                                                                           result.CombineWith(this.ValidateAttribute(target, targetProperty, attribute) ?? ValidationResult.Successful));
        }

        protected abstract ValidationResult ValidateAttribute(object target, PropertyInfo targetProperty, TAttribute attribute);
    }
}
