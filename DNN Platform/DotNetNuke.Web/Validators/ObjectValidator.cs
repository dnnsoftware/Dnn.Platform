// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Validators
{
    /// <summary>Validates an object.</summary>
    public abstract class ObjectValidator
    {
        /// <summary>Validates the object.</summary>
        /// <param name="target">The target object.</param>
        /// <returns>A new <see cref="ValidationResult"/>.</returns>
        public abstract ValidationResult ValidateObject(object target);
    }
}
