// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Services.Dto;

using Dnn.PersonaBar.Security.Attributes;

/// <summary>
/// Information required to update the basic login settings.
/// </summary>
public class UpdateBasicLoginSettingsRequest
{
    /// <summary>
    /// Gets or sets the culture code.
    /// </summary>
    [CultureCodeExist]
    public string CultureCode { get; set; }

    /// <summary>
    /// Gets or sets the default authentication provider.
    /// </summary>
    [AuthProviderSupported]
    public string DefaultAuthProvider { get; set; }

    /// <summary>
    /// Gets or sets the primary administrator identifier.
    /// </summary>
    [UserExist(RoleNames = new string[] { Library.Constants.AdminsRoleName })]
    public int PrimaryAdministratorId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a valid profile is required to login.
    /// </summary>
    public bool RequireValidProfileAtLogin { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether captcha is required to login.
    /// </summary>
    public bool CaptchaLogin { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether captcha is required to retrieve a password.
    /// </summary>
    public bool CaptchaRetrievePassword { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether captcha is required to change a password.
    /// </summary>
    public bool CaptchaChangePassword { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to hide the login control.
    /// </summary>
    public bool HideLoginControl { get; set; }

    /// <summary>
    /// Gets or sets the user request ip header.
    /// </summary>
    public string UserRequestIPHeader { get; set; }
}
