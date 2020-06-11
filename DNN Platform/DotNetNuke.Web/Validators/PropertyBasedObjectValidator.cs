﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            return target.GetType().GetProperties().Aggregate(ValidationResult.Successful, (result, member) => result.CombineWith(this.ValidateProperty(target, member) ?? ValidationResult.Successful));
        }

        protected abstract ValidationResult ValidateProperty(object target, PropertyInfo targetProperty);
    }
}
