// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using Dnn.PersonaBar.Security.Components;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Registration;

    [AttributeUsage(AttributeTargets.Property)]
    class RegistrationFieldsAttribute : ValidationAttribute
    {
        public RegistrationFieldsAttribute(string registrationFormType, string requireUniqueDisplayName)
        {
            this.RegistrationFormTypePropertyName = registrationFormType;
            this.RequireUniqueDisplayNamePropertyName = requireUniqueDisplayName;
        }

        public string RegistrationFormTypePropertyName { get; private set; }
        public string RequireUniqueDisplayNamePropertyName { get; private set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var registrationFormTypeValue = string.Empty;

            try
            {
                registrationFormTypeValue = validationContext.ObjectType.GetProperty(this.RegistrationFormTypePropertyName).GetValue(validationContext.ObjectInstance, null).ToString();
            }
            catch
            {
                return new ValidationResult(string.Format(Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), this.RegistrationFormTypePropertyName, registrationFormTypeValue));
            }

            if (string.IsNullOrWhiteSpace(registrationFormTypeValue))
            {
                return new ValidationResult(string.Format(Localization.GetString(Constants.EmptyValue, Constants.LocalResourcesFile), this.RegistrationFormTypePropertyName));
            }

            int registrationFormType;

            if (!Int32.TryParse(registrationFormTypeValue, out registrationFormType))
            {
                return new ValidationResult(string.Format(Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), this.RegistrationFormTypePropertyName, registrationFormTypeValue));
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
                    requireUniqueDisplayNameValue = validationContext.ObjectType.GetProperty(this.RequireUniqueDisplayNamePropertyName).GetValue(validationContext.ObjectInstance, null).ToString();
                }
                catch
                {
                    return new ValidationResult(string.Format(Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), this.RequireUniqueDisplayNamePropertyName, requireUniqueDisplayNameValue));
                }

                if (string.IsNullOrWhiteSpace(requireUniqueDisplayNameValue))
                {
                    return new ValidationResult(string.Format(Localization.GetString(Constants.EmptyValue, Constants.LocalResourcesFile), this.RequireUniqueDisplayNamePropertyName));
                }

                bool requireUniqueDisplayName;

                if (!bool.TryParse(requireUniqueDisplayNameValue, out requireUniqueDisplayName))
                {
                    return new ValidationResult(string.Format(Localization.GetString(Constants.NotValid, Constants.LocalResourcesFile), this.RequireUniqueDisplayNamePropertyName, requireUniqueDisplayNameValue));
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
