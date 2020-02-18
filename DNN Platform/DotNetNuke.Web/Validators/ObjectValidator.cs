// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Web.Validators
{
    public abstract class ObjectValidator
    {
        public abstract ValidationResult ValidateObject(object target);
    }
}
