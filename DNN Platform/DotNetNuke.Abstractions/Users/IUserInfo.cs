// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Users;

using System;

/// <summary>The UserInfo class provides Business Layer model for Users.</summary>
public interface IUserInfo
{
    /// <summary>Gets or sets the AffiliateId for this user.</summary>
    int AffiliateID { get; set; }

    /// <summary>Gets or sets the Display Name.</summary>
    string DisplayName { get; set; }

    /// <summary>Gets or sets the Email Address.</summary>
    string Email { get; set; }

    /// <summary>Gets or sets the First Name.</summary>
    string FirstName { get; set; }

    /// <summary>Gets or sets a value indicating whether the user has agreed to the terms and conditions.</summary>
    bool HasAgreedToTerms { get; set; }

    /// <summary>Gets or sets when the user last agreed to the terms and conditions.</summary>
    DateTime HasAgreedToTermsOn { get; set; }

    /// <summary>Gets a value indicating whether the user is in the portal's administrators role.</summary>
    bool IsAdmin { get; }

    /// <summary>Gets or sets a value indicating whether the User is deleted.</summary>
    bool IsDeleted { get; set; }

    /// <summary>Gets or sets a value indicating whether the User is a SuperUser.</summary>
    bool IsSuperUser { get; set; }

    /// <summary>Gets or sets the Last IP address used by user.</summary>
    string LastIPAddress { get; set; }

    /// <summary>Gets or sets the Last Name.</summary>
    string LastName { get; set; }

    /// <summary>Gets or sets the datetime that the PasswordResetToken is valid.</summary>
    DateTime PasswordResetExpiration { get; set; }

    /// <summary>Gets or sets the token created for resetting passwords.</summary>
    Guid PasswordResetToken { get; set; }

    /// <summary>Gets or sets the PortalId.</summary>
    int PortalID { get; set; }

    /// <summary>Gets or sets a value indicating whether the user has requested they be removed from the site.</summary>
    bool RequestsRemoval { get; set; }

    /// <summary>Gets or sets the roles.</summary>
    string[] Roles { get; set; }

    /// <summary>Gets or sets the User Id.</summary>
    int UserID { get; set; }

    /// <summary>Gets or sets the User Name.</summary>
    string Username { get; set; }

    /// <summary>Gets or sets the vanity url.</summary>
    string VanityUrl { get; set; }

    /// <summary>IsInRole determines whether the user is in the role passed.</summary>
    /// <param name="role">The role to check.</param>
    /// <returns>A Boolean indicating success or failure.</returns>
    bool IsInRole(string role);

    /// <summary>Gets current time in User's timezone.</summary>
    /// <returns>The local time for the user.</returns>
    DateTime LocalTime();

    /// <summary>Convert utc time in User's timezone.</summary>
    /// <param name="utcTime">Utc time to convert.</param>
    /// <returns>The local time for the user.</returns>
    DateTime LocalTime(DateTime utcTime);

    /// <summary>UpdateDisplayName updates the displayname to the format provided.</summary>
    /// <param name="format">The format to use.</param>
    void UpdateDisplayName(string format);
}
