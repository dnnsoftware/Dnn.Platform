// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    [AttributeUsage(AttributeTargets.Property)]
    class CultureCodeExistAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyName = validationContext.DisplayName;

            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(string.Format(Localization.GetString(Components.Constants.EmptyValue, Components.Constants.LocalResourcesFile), propertyName));
            }

            var allLocales = LocaleController.Instance.GetLocales(PortalController.Instance.GetCurrentPortalSettings().PortalId);

            if (!allLocales.Select(l => l.Value.Code.ToLowerInvariant()).Contains(value.ToString().ToLowerInvariant()))
            {
                return new ValidationResult(string.Format(Localization.GetString(Components.Constants.NotValid, Components.Constants.LocalResourcesFile), propertyName, value.ToString()));
            }

            return ValidationResult.Success;
        }
    }
}
