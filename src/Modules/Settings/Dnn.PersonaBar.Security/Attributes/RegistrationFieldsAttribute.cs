#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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