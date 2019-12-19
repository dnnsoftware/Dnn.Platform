// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using Dnn.PersonaBar.Security.Components;
using DotNetNuke.Services.Registration;

namespace Dnn.PersonaBar.Security.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    class RegistrationFieldsAttribute : ValidationAttribute
    {
        public string RegistrationFormTypePropertyName { get; private set; }
        public string RequireUniqueDisplayNamePropertyName { get; private set; }

        public RegistrationFieldsAttribute(string registrationFormType, string requireUniqueDisplayName)
        {
            RegistrationFormTypePropertyName = registrationFormType;
            RequireUniqueDisplayNamePropertyName = requireUniqueDisplayName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var registrationFormTypeValue = string.Empty;

            try
            {
                registrationFormTypeValue = validationContext.ObjectType.GetProperty(RegistrationFormTypePropertyName).GetValue(validationContext.ObjectInstance, null).ToString();
            }
            catch
            {
                return new ValidationResult(string.Format(Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), RegistrationFormTypePropertyName, registrationFormTypeValue));
            }

            if (string.IsNullOrWhiteSpace(registrationFormTypeValue))
            {
                return new ValidationResult(string.Format(Localization.GetString(Constants.EmptyValue, Constants.LocalResourcesFile), RegistrationFormTypePropertyName));
            }

            int registrationFormType;

            if (!Int32.TryParse(registrationFormTypeValue, out registrationFormType))
            {
                return new ValidationResult(string.Format(Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), RegistrationFormTypePropertyName, registrationFormTypeValue));
            }

            if (registrationFormType == 1)
            {
                var registrationFields = value.ToString();
                var registrationTokens = registrationFields.Split(',');
                var portalId = PortalController.Instance.GetCurrentPortalSettings().PortalId;

                foreach (var registrationField in registrationTokens)
                {
                    if (!string.IsNullOrWhiteSpace(registrationField) && RegistrationProfileController.Instance.Search(portalId, registrationField).Count() == 0)
                    {
                        return new ValidationResult(string.Format(Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), validationContext.DisplayName, registrationField));
                    }
                }

                if (!registrationFields.Contains("Email"))
                {
                    return new ValidationResult(Localization.GetString(Constants.NoEmail, Constants.LocalResourcesFile));
                }

                var requireUniqueDisplayNameValue = string.Empty;

                try
                {
                    requireUniqueDisplayNameValue = validationContext.ObjectType.GetProperty(RequireUniqueDisplayNamePropertyName).GetValue(validationContext.ObjectInstance, null).ToString();
                }
                catch
                {
                    return new ValidationResult(string.Format(Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), RequireUniqueDisplayNamePropertyName, requireUniqueDisplayNameValue));
                }

                if (string.IsNullOrWhiteSpace(requireUniqueDisplayNameValue))
                {
                    return new ValidationResult(string.Format(Localization.GetString(Constants.EmptyValue, Constants.LocalResourcesFile), RequireUniqueDisplayNamePropertyName));
                }

                bool requireUniqueDisplayName;

                if (!bool.TryParse(requireUniqueDisplayNameValue, out requireUniqueDisplayName))
                {
                    return new ValidationResult(string.Format(Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), RequireUniqueDisplayNamePropertyName, requireUniqueDisplayNameValue));
                }

                if (!registrationFields.Contains("DisplayName") && requireUniqueDisplayName)
                {
                    PortalController.UpdatePortalSetting(portalId, "Registration_RegistrationFormType", "0", false);
                    return new ValidationResult(Localization.GetString(Constants.NoDisplayName, Constants.LocalResourcesFile));
                }
            }

            return ValidationResult.Success;
        }
    }
}
