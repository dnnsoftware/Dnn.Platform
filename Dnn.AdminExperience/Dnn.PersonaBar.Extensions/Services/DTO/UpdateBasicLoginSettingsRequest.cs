// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
