// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Services.Dto
{
    using Dnn.PersonaBar.Security.Attributes;

    public class UpdateRegistrationSettingsRequest
    {
        [UserRegistrationOption]
        public string UserRegistration { get; set; }

        public bool UseAuthenticationProviders { get; set; }

        public string ExcludedTerms { get; set; }

        public bool UseProfanityFilter { get; set; }

        [RegistrationFormTypeOption]
        public int RegistrationFormType { get; set; }

        [RegistrationFields("RegistrationFormType", "RequireUniqueDisplayName")]
        public string RegistrationFields { get; set; }

        public bool RequireUniqueDisplayName { get; set; }

        [UserEmailAsUsername]
        public bool UseEmailAsUsername { get; set; }

        public string DisplayNameFormat { get; set; }

        public string UserNameValidation { get; set; }

        public string EmailAddressValidation { get; set; }

        public bool UseRandomPassword { get; set; }

        public bool RequirePasswordConfirmation { get; set; }

        public bool RequireValidProfile { get; set; }

        public bool UseCaptchaRegister { get; set; }

        [TabExist]
        public int RedirectAfterRegistrationTabId { get; set; }

        public bool EnableRegisterNotification { get; set; }
    }
}
