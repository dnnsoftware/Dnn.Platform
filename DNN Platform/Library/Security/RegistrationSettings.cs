﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
