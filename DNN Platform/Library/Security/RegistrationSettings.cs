#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Security
{
	public class RegistrationSettings
	{

		#region Properties
		public bool RandomPassword { get; set; }
		public int RedirectAfterRegistration { get; set; }
		public int RedirectAfterLogout { get; set; }
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
			RedirectAfterRegistration = Null.NullInteger;
			RedirectAfterLogout = Null.NullInteger;
			RegistrationFields = Null.NullString;
			ExcludeTerms = Null.NullString;
			ExcludeTermsRegex = Null.NullString;
			RegistrationFormType = Null.NullInteger;
			RequirePasswordConfirm = false;
			RequireUniqueDisplayName = false;
			UseAuthProviders = false;
			UseEmailAsUserName = false;
			UseProfanityFilter = false;
			RequireValidProfile = false;
			RequireValidProfileAtLogin = false;
			UseCaptcha = false;
			UserNameValidator = Null.NullString;
			DisplayNameFormat = Null.NullString;
			EmailValidator = Null.NullString;
		}
		public RegistrationSettings(Dictionary<string, string> settings): this()
		{
			RandomPassword = settings.GetValue("Registration_RandomPassword", RandomPassword);
			RedirectAfterRegistration = settings.GetValue("Redirect_AfterRegistration", RedirectAfterRegistration);
			RedirectAfterLogout = settings.GetValue("Redirect_AfterLogout", RedirectAfterLogout);
			RegistrationFields = settings.GetValue("Registration_RegistrationFields", RegistrationFields);
			ExcludeTerms = settings.GetValue("Registration_ExcludeTerms", ExcludeTerms);
			RegistrationFormType = settings.GetValue("Registration_RegistrationFormType", RegistrationFormType);
			RequirePasswordConfirm = settings.GetValue("Registration_RequireConfirmPassword", RequirePasswordConfirm);
			RequireUniqueDisplayName = settings.GetValue("Registration_RequireUniqueDisplayName", RequireUniqueDisplayName);
			UseAuthProviders = settings.GetValue("Registration_UseAuthProviders", UseAuthProviders);
			UseEmailAsUserName = settings.GetValue("Registration_UseEmailAsUserName", UseEmailAsUserName);
			UseProfanityFilter = settings.GetValue("Registration_UseProfanityFilter", UseProfanityFilter);
			RequireValidProfile = settings.GetValue("Security_RequireValidProfile", RequireValidProfile);
			RequireValidProfileAtLogin = settings.GetValue("Security_RequireValidProfileAtLogin", RequireValidProfileAtLogin);
			UseCaptcha = settings.GetValue("Security_CaptchaRegister", UseCaptcha);
			UserNameValidator = settings.GetValue("Security_UserNameValidation", UserNameValidator);
			DisplayNameFormat = settings.GetValue("Security_DisplayNameFormat", DisplayNameFormat);
			EmailValidator = settings.GetValue("Security_EmailValidation", EmailValidator);

			ExcludeTermsRegex = "^(?:(?!" + ExcludeTerms.Replace(" ", "").Replace(",", "|") + ").)*$\\r?\\n?";
		}
		#endregion

	}
}
