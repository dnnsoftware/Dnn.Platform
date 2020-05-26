// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Linq;
using System.Reflection;

#endregion

namespace DotNetNuke.Web.Validators
{
    public abstract class PropertyBasedObjectValidator : ObjectValidator
    {
        public override ValidationResult ValidateObject(object target)
        {
            return target.GetType().GetProperties().Aggregate(ValidationResult.Successful, (result, member) => result.CombineWith(ValidateProperty(target, member) ?? ValidationResult.Successful));
        }

        protected abstract ValidationResult ValidateProperty(object target, PropertyInfo targetProperty);
    }
}
