#region Usings

using System;
using System.Linq;
using System.Reflection;

#endregion

namespace DotNetNuke.Web.Validators
{
    public abstract class AttributeBasedObjectValidator<TAttribute> : PropertyBasedObjectValidator where TAttribute : Attribute
    {
        protected override ValidationResult ValidateProperty(object target, PropertyInfo targetProperty)
        {
            return targetProperty.GetCustomAttributes(true).OfType<TAttribute>().Aggregate(ValidationResult.Successful,
                                                                                           (result, attribute) =>
                                                                                           result.CombineWith(ValidateAttribute(target, targetProperty, attribute) ?? ValidationResult.Successful));
        }


        protected abstract ValidationResult ValidateAttribute(object target, PropertyInfo targetProperty, TAttribute attribute);
    }
}
