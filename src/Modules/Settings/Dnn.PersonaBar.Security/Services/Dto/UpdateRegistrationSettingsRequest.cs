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

using Dnn.PersonaBar.Security.Attributes;

namespace Dnn.PersonaBar.Security.Services.Dto
{
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