// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Security.Attributes;

namespace Dnn.PersonaBar.Security.Services.Dto
{
    public class UpdateBasicLoginSettingsRequest
    {
        [CultureCodeExist]
        public string CultureCode { get; set; }

        [AuthProviderSupported]
        public string DefaultAuthProvider { get; set; }

        [UserExist(RoleNames = new string[] { Library.Constants.AdminsRoleName })]
        public int PrimaryAdministratorId { get; set; }

        [TabExist]
        public int RedirectAfterLoginTabId { get; set; }

        [TabExist]
        public int RedirectAfterLogoutTabId { get; set; }

        public bool RequireValidProfileAtLogin { get; set; }

        public bool CaptchaLogin { get; set; }

        public bool CaptchaRetrivePassword { get; set; }

        public bool CaptchaChangePassword { get; set; }

        public bool HideLoginControl { get; set; }
    }
}
