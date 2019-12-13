// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Linq;
using DotNetNuke.Services.Localization;
using System.ComponentModel.DataAnnotations;

namespace Dnn.PersonaBar.Security.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    class UserRegistrationOptionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyName = validationContext.DisplayName;
            int userRegistrationOptionId;

            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(string.Format(Localization.GetString(Components.Constants.EmptyValue, Components.Constants.LocalResourcesFile), propertyName));
            }

            if (!Int32.TryParse(value.ToString(), out userRegistrationOptionId))
            {
                return new ValidationResult(string.Format(Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, value.ToString()));
            }

            var allOptions = Helper.RegistrationSettingsHelper.GetUserRegistrationOptions();

            if (!allOptions.Select(o => o.Value).Contains(userRegistrationOptionId))
            {
                return new ValidationResult(string.Format(Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, value.ToString()));
            }

            return ValidationResult.Success;
        }
    }
}
