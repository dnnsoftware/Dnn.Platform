// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;

    [AttributeUsage(AttributeTargets.Property)]
    internal class UserEmailAsUsernameAttribute : ValidationAttribute
    {
        /// <inheritdoc/>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyName = validationContext.DisplayName;

            if (!bool.TryParse(value.ToString(), out var userEmailAsUsername))
            {
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, value.ToString()));
            }

            if (userEmailAsUsername && UserController.GetDuplicateEmailCount() > 0)
            {
                return new ValidationResult(Localization.GetString(Components.Constants.ContainsDuplicateAddresses, Components.Constants.LocalResourcesFile));
            }

            return ValidationResult.Success;
        }
    }
}
