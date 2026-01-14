// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Validators
{
    /// <summary>A validation error.</summary>
    public class ValidationError
    {
        /// <summary>Gets or sets the error message.</summary>
        public string ErrorMessage { get; set; }

        /// <summary>Gets or sets the name of the invalid property.</summary>
        public string PropertyName { get; set; }

        /// <summary>Gets or sets the validator.</summary>
        public object Validator { get; set; }
    }
}
