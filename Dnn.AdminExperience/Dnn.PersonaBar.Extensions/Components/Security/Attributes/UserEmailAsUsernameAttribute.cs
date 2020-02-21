// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Security.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    class UserEmailAsUsernameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyName = validationContext.DisplayName;
            bool userEmailAsUsername;

            if (!bool.TryParse(value.ToString(), out userEmailAsUsername))
            {
                return new ValidationResult(string.Format(Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, value.ToString()));
            }

            if (userEmailAsUsername && UserController.GetDuplicateEmailCount() > 0)
            {
                return new ValidationResult(Localization.GetString(Components.Constants.ContainsDuplicateAddresses, Components.Constants.LocalResourcesFile));
            }

            return ValidationResult.Success;
        }
    }
}
