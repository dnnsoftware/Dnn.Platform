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

using System.Collections.Generic;
using DotNetNuke.Common;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Security
{
	public class RegistrationSettings
	{

		#region Properties
		public bool RandomPassword { get; set; }
		public int RedirectAfterRegistration { get; set; }
        public int RedirectAfterLogout { get; set; }
        public int RedirectAfterLogin { get; set; }
        public string RegistrationFields { get; set; }
		public string ExcludeTerms { get; set; }
		public string ExcludeTermsRegex { get; set; }
		public int RegistrationFormType { get; set; }
		public bool RequirePasswordConfirm { get; set; }
		public bool RequireUniqueDisplayName { get; set; }
		public bool UseAuthProviders { get; set; }
		public bool UseEmailAsUserName { get; set; }
		public bool UseProfanityFilter { get; set; }
		public bool RequireValidProfile { get; set; }
		public bool RequireValidProfileAtLogin { get; set; }
		public bool UseCaptcha { get; set; }
		public string UserNameValidator { get; set; }
		public string DisplayNameFormat { get; set; }
		public string EmailValidator { get; set; }
		#endregion

		#region Constructors

		public RegistrationSettings()
		{
			RandomPassword = false;
			RedirectAfterRegistration = -1;
            RedirectAfterLogout = -1;
            RedirectAfterLogin = -1;
            RegistrationFields = string.Empty;
			ExcludeTerms = string.Empty;
			ExcludeTermsRegex = Null.NullString;
			RegistrationFormType = 0;
			RequirePasswordConfirm = true;
			RequireUniqueDisplayName = false;
			UseAuthProviders = false;
			UseEmailAsUserName = false;
			UseProfanityFilter = false;
			RequireValidProfile = false;
			RequireValidProfileAtLogin = true;
			UseCaptcha = false;
			UserNameValidator = Globals.glbUserNameRegEx;
			DisplayNameFormat = string.Empty;
			EmailValidator = Globals.glbEmailRegEx;
		}
		public RegistrationSettings(Dictionary<string, string> settings): this()
		{
            RandomPassword = settings.GetValueOrDefault("Registration_RandomPassword", RandomPassword);
            RedirectAfterRegistration = settings.GetValueOrDefault("Redirect_AfterRegistration", RedirectAfterRegistration);
            RedirectAfterLogout = settings.GetValueOrDefault("Redirect_AfterLogout", RedirectAfterLogout);
            RedirectAfterLogin = settings.GetValueOrDefault("Redirect_AfterLogin", RedirectAfterLogin);
            RegistrationFields = settings.GetValueOrDefault("Registration_RegistrationFields", RegistrationFields);
            ExcludeTerms = settings.GetValueOrDefault("Registration_ExcludeTerms", ExcludeTerms);
            RegistrationFormType = settings.GetValueOrDefault("Registration_RegistrationFormType", RegistrationFormType);
            RequirePasswordConfirm = settings.GetValueOrDefault("Registration_RequireConfirmPassword", RequirePasswordConfirm);
            RequireUniqueDisplayName = settings.GetValueOrDefault("Registration_RequireUniqueDisplayName", RequireUniqueDisplayName);
            UseAuthProviders = settings.GetValueOrDefault("Registration_UseAuthProviders", UseAuthProviders);
            UseEmailAsUserName = settings.GetValueOrDefault("Registration_UseEmailAsUserName", UseEmailAsUserName);
            UseProfanityFilter = settings.GetValueOrDefault("Registration_UseProfanityFilter", UseProfanityFilter);
            RequireValidProfile = settings.GetValueOrDefault("Security_RequireValidProfile", RequireValidProfile);
            RequireValidProfileAtLogin = settings.GetValueOrDefault("Security_RequireValidProfileAtLogin", RequireValidProfileAtLogin);
            UseCaptcha = settings.GetValueOrDefault("Security_CaptchaRegister", UseCaptcha);
            UserNameValidator = settings.GetValueOrDefault("Security_UserNameValidation", UserNameValidator);
            DisplayNameFormat = settings.GetValueOrDefault("Security_DisplayNameFormat", DisplayNameFormat);
            EmailValidator = settings.GetValueOrDefault("Security_EmailValidation", EmailValidator);

			ExcludeTermsRegex = "^(?:(?!" + ExcludeTerms.Replace(" ", "").Replace(",", "|") + ").)*$\\r?\\n?";
		}
		#endregion

	}
}
